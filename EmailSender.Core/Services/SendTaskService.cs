using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Data.Repositories;
using EmailSender.Models;

// ✅ 添加这一行，给自定义枚举起别名
using SendTaskStatus = EmailSender.Models.SendTaskStatus;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 发送任务服务
    /// 负责：任务创建、调度、启动/暂停/取消
    /// </summary>
    public class SendTaskService
    {
        private readonly SendTaskRepository    _taskRepo;
        private readonly EmailListService      _listService;
        private readonly SendEngineService     _engine;

        // 当前运行中的任务取消令牌
        private readonly Dictionary<int, CancellationTokenSource> _runningTasks
            = new Dictionary<int, CancellationTokenSource>();

        public SendTaskService(
            SendTaskRepository taskRepo,
            EmailListService listService,
            SendEngineService engine)
        {
            _taskRepo    = taskRepo;
            _listService = listService;
            _engine      = engine;
        }

        /// <summary>创建新任务</summary>
        public int CreateTask(SendTask task)
        {
            task.Status    = SendTaskStatus.Pending;
            task.CreatedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;
            task.Id        = _taskRepo.Add(task);
            return task.Id;
        }

        /// <summary>启动任务（异步，不阻塞UI）</summary>
        public async Task StartTaskAsync(
            int taskId,
            IProgress<SendProgressInfo> progress = null)
        {
            var task = _taskRepo.GetById(taskId)
                ?? throw new Exception($"任务 {taskId} 不存在");

            if (task.Status == SendTaskStatus.Running)
                throw new Exception("任务已在运行中");

            // 获取过滤后的邮件列表
            var emails = _listService.GetFiltered(
                task.ListId,
                excludeBlacklist: true,
                maxFailCount: task.RetryMax,
                excludeTaskId: taskId);

            task.TotalCount = emails.Count;
            _taskRepo.UpdateStatus(taskId, SendTaskStatus.Running);

            var cts = new CancellationTokenSource();
            _runningTasks[taskId] = cts;

            try
            {
                await _engine.ExecuteAsync(task, emails, progress, cts.Token);
                _taskRepo.UpdateStatus(taskId, SendTaskStatus.Done);
            }
            catch (OperationCanceledException)
            {
                _taskRepo.UpdateStatus(taskId, SendTaskStatus.Paused);
            }
            catch (Exception)
            {
                _taskRepo.UpdateStatus(taskId, SendTaskStatus.Failed);
                throw;
            }
            finally
            {
                _runningTasks.Remove(taskId);
            }
        }

        /// <summary>暂停/取消任务</summary>
        public void CancelTask(int taskId)
        {
            if (_runningTasks.TryGetValue(taskId, out var cts))
                cts.Cancel();
        }

        public List<SendTask> GetAll()         => _taskRepo.GetAll();
        public SendTask GetById(int id)        => _taskRepo.GetById(id);
        public void Delete(int id)             => _taskRepo.Delete(id);
    }

    /// <summary>发送进度信息</summary>
    public class SendProgressInfo
    {
        public int    Total     { get; set; }
        public int    Sent      { get; set; }
        public int    Success   { get; set; }
        public int    Failed    { get; set; }
        public string CurrentEmail { get; set; }
        public double Percent   => Total > 0 ? (double)Sent / Total * 100 : 0;
    }
}
