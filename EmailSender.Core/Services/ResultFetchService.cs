using System;
using System.Linq;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 发送结果拉取服务
    /// 负责：主动查询 SendCloud 发送日志，更新本地发送记录和任务统计
    /// </summary>
    public class ResultFetchService
    {
        private readonly SendCloudApiClient   _sendCloud;
        private readonly SendRecordRepository _recordRepo;
        private readonly SendTaskRepository   _taskRepo;
        private readonly BlacklistRepository  _blacklist;

        public ResultFetchService(
            SendCloudApiClient sendCloud,
            SendRecordRepository recordRepo,
            SendTaskRepository taskRepo,
            BlacklistRepository blacklist)
        {
            _sendCloud  = sendCloud;
            _recordRepo = recordRepo;
            _taskRepo   = taskRepo;
            _blacklist  = blacklist;
        }

        /// <summary>拉取指定任务的未处理发送结果</summary>
        public async Task<int> FetchPendingResultsAsync(
            int taskId,
            IProgress<(int current, int total)> progress = null)
        {
            var pending = _recordRepo.GetPendingFetch(taskId, 200);
            if (pending.Count == 0) return 0;

            // 查询最近3天的日志
            var startDate = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            var endDate   = DateTime.Now.ToString("yyyy-MM-dd");
            var logMap    = await _sendCloud.QuerySendLogAsync(startDate, endDate);

            int updated = 0;
            for (int i = 0; i < pending.Count; i++)
            {
                var record = pending[i];
                if (string.IsNullOrEmpty(record.SendCloudMsgId)) continue;

                if (logMap.TryGetValue(record.SendCloudMsgId, out var statusStr))
                {
                    var status = ParseSendCloudStatus(statusStr);
                    _recordRepo.MarkFetched(record.Id, status);

                    // 硬退信自动加黑名单
                    if (status == SendStatus.Bounced)
                        _blacklist.Add(new Blacklist
                        {
                            Email  = record.Email,
                            Type   = BlacklistType.HardBounce,
                            Reason = "SendCloud HardBounce",
                            TaskId = taskId,
                            SendRecordId = record.Id
                        });

                    // 垃圾举报自动加黑名单
                    if (status == SendStatus.Spam)
                        _blacklist.Add(new Blacklist
                        {
                            Email  = record.Email,
                            Type   = BlacklistType.SpamReport,
                            Reason = "SendCloud SpamReport",
                            TaskId = taskId,
                            SendRecordId = record.Id
                        });

                    updated++;
                }
                progress?.Report((i + 1, pending.Count));
            }

            // 重新统计并更新任务
            await RefreshTaskStatsAsync(taskId);
            return updated;
        }

        private async Task RefreshTaskStatsAsync(int taskId)
        {
            var success = _recordRepo.GetCountByStatus(taskId, SendStatus.Delivered)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Opened)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Clicked);
            var fail    = _recordRepo.GetCountByStatus(taskId, SendStatus.Bounced)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Failed);
            var open    = _recordRepo.GetCountByStatus(taskId, SendStatus.Opened);
            var click   = _recordRepo.GetCountByStatus(taskId, SendStatus.Clicked);
            var bounce  = _recordRepo.GetCountByStatus(taskId, SendStatus.Bounced);

            _taskRepo.UpdateStats(taskId, success, fail, open, click, bounce);
            await Task.CompletedTask;
        }

        private SendStatus ParseSendCloudStatus(string s)
        {
            return s?.ToLower() switch
            {
                "delivered"   => SendStatus.Delivered,
                "opened"      => SendStatus.Opened,
                "clicked"     => SendStatus.Clicked,
                "bounced"     => SendStatus.Bounced,
                "spam"        => SendStatus.Spam,
                "unsubscribe" => SendStatus.Spam,
                "failed"      => SendStatus.Failed,
                _             => SendStatus.Sent
            };
        }
    }
}
