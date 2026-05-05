using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Core.Helpers;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 核心发送引擎
    /// 支持：SendCloud / Gmail / Hotmail / SMTP 四种通道
    /// 特性：多线程并发、发送间隔、失败重试、进度上报、取消支持
    /// </summary>
    public class SendEngineService
    {
        private readonly SendCloudApiClient      _sendCloud;
        private readonly SenderAccountRepository _accountRepo;
        private readonly SendRecordRepository    _recordRepo;
        private readonly EmailAddressRepository  _addrRepo;
        private readonly BlacklistRepository     _blacklist;
        private readonly TemplateRenderer        _renderer;

        public SendEngineService(
            SendCloudApiClient sendCloud,
            SenderAccountRepository accountRepo,
            SendRecordRepository recordRepo,
            EmailAddressRepository addrRepo,
            BlacklistRepository blacklist,
            TemplateRenderer renderer)
        {
            _sendCloud   = sendCloud;
            _accountRepo = accountRepo;
            _recordRepo  = recordRepo;
            _addrRepo    = addrRepo;
            _blacklist   = blacklist;
            _renderer    = renderer;
        }

        /// <summary>执行发送任务</summary>
        public async Task ExecuteAsync(
            SendTask task,
            List<EmailRecipient> emails,
            IProgress<SendProgressInfo> progress,
            CancellationToken ct)
        {
            var account   = _accountRepo.GetById(task.AccountId)
                ?? throw new Exception($"发送账户 {task.AccountId} 不存在");
            var semaphore = new SemaphoreSlim(task.ThreadCount, task.ThreadCount);
            var taskList  = new List<Task>();
            var info      = new SendProgressInfo { Total = emails.Count };
            var lockObj   = new object();

            foreach (var email in emails)
            {
                ct.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(ct);

                var emailCopy = email; // 闭包捕获
                var t = Task.Run(async () =>
                {
                    try
                    {
                        await SendOneAsync(task, account, emailCopy, ct);
                        lock (lockObj) { info.Success++; info.Sent++; }
                        _addrRepo.UpdateLastSentAt(emailCopy.Email);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        lock (lockObj) { info.Failed++; info.Sent++; }
                        // 记录失败，更新失败次数
                        _addrRepo.UpdateFailCount(emailCopy.Email,
                            emailCopy.FailCount + 1);
                        // 失败次数超限自动加黑名单
                        if (emailCopy.FailCount + 1 >= task.RetryMax)
                            _blacklist.Add(new Blacklist
                            {
                                Email  = emailCopy.Email,
                                Type   = BlacklistType.RepeatedFail,
                                Reason = ex.Message,
                                TaskId = task.Id
                            });
                    }
                    finally
                    {
                        lock (lockObj) { info.CurrentEmail = emailCopy.Email; }
                        progress?.Report(info);
                        semaphore.Release();
                    }
                }, ct);

                taskList.Add(t);
                await Task.Delay(task.IntervalSeconds * 1000, ct);
            }

            await Task.WhenAll(taskList);

            // 更新任务统计
            _recordRepo.GetCountByStatus(task.Id, SendStatus.Sent);
        }

        private async Task SendOneAsync(
            SendTask task, SenderAccount account,
            EmailRecipient addr, CancellationToken ct)
        {
            var record = new SendRecord
            {
                TaskId         = task.Id,
                EmailAddressId = addr.Id,
                Email          = addr.Email,
                SendStatus     = SendStatus.Pending,
                SentAt         = DateTime.Now,
            };

            try
            {
                switch (task.Channel)
                {
                    case SendChannel.SendCloud:
                        record.SendCloudMsgId = await SendViaSendCloudAsync(
                            task, account, addr);
                        break;
                    case SendChannel.Gmail:
                    case SendChannel.Hotmail:
                        await SendViaOAuthSmtpAsync(task, account, addr);
                        break;
                    case SendChannel.SMTP:
                        await SendViaSmtpAsync(task, account, addr);
                        break;
                }
                record.SendStatus = SendStatus.Sent;
            }
            catch (Exception ex)
            {
                record.SendStatus    = SendStatus.Failed;
                record.ErrorMessage  = ex.Message;
                throw;
            }
            finally
            {
                _recordRepo.Add(record);
            }
        }

        private async Task<string> SendViaSendCloudAsync(
            SendTask task, SenderAccount account, EmailRecipient addr)
        {
            var vars = new Dictionary<string, string>
            {
                ["first_name"] = addr.FirstName ?? "",
                ["last_name"]  = addr.LastName  ?? "",
                ["company"]    = addr.Company   ?? "",
                ["email"]      = addr.Email,
            };
            return await _sendCloud.SendByTemplateAsync(
                addr.Email, $"{addr.FirstName} {addr.LastName}".Trim(),
                account.ApiUser, account.SmtpFromEmail ?? account.OAuthEmail,
                account.SmtpFromName ?? account.Name, vars);
        }

        private async Task SendViaOAuthSmtpAsync(
            SendTask task, SenderAccount account, EmailRecipient addr)
        {
            // OAuth Token 刷新由 OAuthHelper 处理
            var smtpHost = task.Channel == SendChannel.Gmail
                ? "smtp.gmail.com" : "smtp.office365.com";
            var smtpPort = 587;

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl             = true;
                client.UseDefaultCredentials = false;
                client.Credentials           = new NetworkCredential(
                    account.OAuthEmail, account.OAuthToken);

                var msg = BuildMailMessage(task, account, addr);
                await client.SendMailAsync(msg);
            }
        }

        private async Task SendViaSmtpAsync(
            SendTask task, SenderAccount account, EmailRecipient addr)
        {
            using (var client = new SmtpClient(account.SmtpHost, account.SmtpPort))
            {
                client.EnableSsl             = account.SmtpUseSsl;
                client.UseDefaultCredentials = false;
                client.Credentials           = new NetworkCredential(
                    account.SmtpUser, account.SmtpPassword);

                var msg = BuildMailMessage(task, account, addr);
                await client.SendMailAsync(msg);
            }
        }

        private MailMessage BuildMailMessage(
            SendTask task, SenderAccount account, EmailRecipient addr)
        {
            // 从 Data 层获取模版内容（由调用方注入，此处简化）
            var html = _renderer.Render("{{html_body}}", addr);
            var msg  = new MailMessage
            {
                From       = new MailAddress(
                    account.SmtpFromEmail ?? account.OAuthEmail,
                    account.SmtpFromName  ?? account.Name),
                Subject    = _renderer.Render("{{subject}}", addr),
                Body       = html,
                IsBodyHtml = true,
            };
            msg.To.Add(new MailAddress(addr.Email,
                $"{addr.FirstName} {addr.LastName}".Trim()));
            return msg;
        }
    }
}
