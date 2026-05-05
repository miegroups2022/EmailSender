using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 模版服务
    /// 负责：从 meetby 同步模版、推送到 SendCloud、触发 AI 分析
    /// </summary>
    public class TemplateService
    {
        private readonly MeetbyApiClient        _meetby;
        private readonly SendCloudApiClient     _sendCloud;
        private readonly EmailTemplateRepository _repo;
        private readonly AiAnalysisService      _ai;

        public TemplateService(
            MeetbyApiClient meetby,
            SendCloudApiClient sendCloud,
            EmailTemplateRepository repo,
            AiAnalysisService ai)
        {
            _meetby    = meetby;
            _sendCloud = sendCloud;
            _repo      = repo;
            _ai        = ai;
        }

        /// <summary>从 meetby 拉取模版并同步到本地数据库</summary>
        public async Task<List<EmailTemplate>> SyncFromMeetbyAsync()
        {
            var remoteList = await _meetby.GetTemplatesAsync();
            var synced     = new List<EmailTemplate>();

            foreach (var remote in remoteList)
            {
                var all      = _repo.GetAll();
                var existing = all.Find(t => t.MeetbyTemplateId == remote.MeetbyTemplateId);

                if (existing == null)
                {
                    remote.Id = _repo.Add(remote);
                    synced.Add(remote);
                }
                else
                {
                    // 更新内容但保留本地 AI 分析和 SendCloud 同步状态
                    existing.Name      = remote.Name;
                    existing.Subject   = remote.Subject;
                    existing.HtmlBody  = remote.HtmlBody;
                    existing.TextBody  = remote.TextBody;
                    existing.FromName  = remote.FromName;
                    existing.FromEmail = remote.FromEmail;
                    existing.NeedResync = true;
                    _repo.Update(existing);
                    synced.Add(existing);
                }
            }
            return synced;
        }

        /// <summary>将指定模版推送到 SendCloud</summary>
        public async Task SyncToSendCloudAsync(int templateId)
        {
            var template = _repo.GetById(templateId)
                ?? throw new Exception($"模版 {templateId} 不存在");

            var sendCloudId = await _sendCloud.UploadTemplateAsync(template);
            _repo.MarkSynced(templateId, sendCloudId);
        }

        /// <summary>对模版进行 AI 反垃圾分析并保存结果</summary>
        public async Task AnalyzeWithAiAsync(int templateId)
        {
            var template = _repo.GetById(templateId)
                ?? throw new Exception($"模版 {templateId} 不存在");

            var result = await _ai.AnalyzeAsync(template.Subject, template.HtmlBody);

            template.AiScore       = result.Score;
            template.AiIssues      = result.IssuesJson;
            template.AiSuggestions = result.SuggestionsJson;
            template.AiAnalyzedAt  = DateTime.Now;
            _repo.Update(template);
        }

        public List<EmailTemplate> GetAll()   => _repo.GetAll();
        public EmailTemplate GetById(int id)  => _repo.GetById(id);

        public void SaveLocal(EmailTemplate t)
        {
            if (t.Id == 0) _repo.Add(t);
            else           _repo.Update(t);
        }

        public void Delete(int id) => _repo.Delete(id);
    }
}
