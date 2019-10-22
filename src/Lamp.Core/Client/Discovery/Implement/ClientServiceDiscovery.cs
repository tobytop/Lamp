using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Server;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Core.Client.Discovery.Implement
{
    public class ClientServiceDiscovery : IClientServiceDiscovery
    {
        private readonly List<Func<Task<ServerDesc>>> _routesGetters;

        private readonly ConcurrentQueue<ServerDesc> _routes;

        private readonly int _updateJobIntervalMinute;
        private readonly ILogger _logger;
        public ClientServiceDiscovery(ILogger logger, int updateJobIntervalMinute = 1)
        {
            if (updateJobIntervalMinute == 0 || updateJobIntervalMinute > 60)
            {
                throw new ArgumentOutOfRangeException($"updateJobIntervalMinute must between 1 and 60, current is {updateJobIntervalMinute}");
            }
            _updateJobIntervalMinute = updateJobIntervalMinute;
            _routesGetters = new List<Func<Task<ServerDesc>>>();
            _routes = new ConcurrentQueue<ServerDesc>();
            _logger = logger;
        }

        public async Task RunInInit()
        {
            await UpdateRoutes();
            await RunUpdateJob();
        }

        private async Task RunUpdateJob()
        {
            string cron = $"0 0/{_updateJobIntervalMinute} * * * ?";
            if (_updateJobIntervalMinute == 60)
            {
                cron = $"0 0 0/1 * * ?";
            }

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            IJobDetail jobDetail = JobBuilder.Create<MonitorJob>().WithIdentity("MonitorJob", "Lamp.Client.UpdateServiceJob").Build();
            jobDetail.JobDataMap.Put("serviceDiscovery", this);
            jobDetail.JobDataMap.Put("logger", _logger);
            IOperableTrigger trigger = new CronTriggerImpl("MonitorJob", "Lamp.Client.UpdateServiceJob", cron);
            await scheduler.ScheduleJob(jobDetail, trigger);
            await scheduler.Start();

        }

        private void ClearQueue<T>(ConcurrentQueue<T> queue)
        {
            foreach (T q in queue)
            {
                queue.TryDequeue(out T tmpQ);
            }
        }

        private void QueueAdd<T>(ConcurrentQueue<T> queue, List<T> elems)
        {
            foreach (T elem in elems)
            {
                if (!queue.Any(x => x.Equals(elem)))
                {
                    queue.Enqueue(elem);
                }
            }
        }


        public void AddRoutesGetter(Func<Task<ServerDesc>> getter)
        {
            _routesGetters.Add(getter);
        }

        public Task<List<ServerAddress>> GetAddressAsync()
        {
            return Task.FromResult(_routes.Select(x => x.ServerAddress).ToList());
        }

        public Task<List<ServerDesc>> GetRoutesAsync()
        {
            return Task.FromResult(_routes.ToList());
        }

        public Task UpdateServerHealthAsync(List<ServerAddress> addresses)
        {
            foreach (ServerAddress addr in addresses)
            {
                _routes.Where(x => x.ServerAddress.Code == addr.Code).ToList()
                    .ForEach(x => x.ServerAddress.IsHealth = addr.IsHealth);
            }

            return Task.CompletedTask;
        }
        private async Task UpdateRoutes()
        {
            List<ServerDesc> routes = new List<ServerDesc>();
            foreach (Func<Task<ServerDesc>> routesGetter in _routesGetters)
            {
                ServerDesc server = await routesGetter();
                if (server != null && server.ServiceDescriptor.Any())
                {
                    routes.Add(server);
                }
            }
            // merge service and its address by service id
            ClearQueue(_routes);
            foreach (ServerDesc route in routes)
            {
                _routes.Enqueue(route);
            }
        }


        [DisallowConcurrentExecution]
        private class MonitorJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                ClientServiceDiscovery serviceDiscovery = context.JobDetail.JobDataMap.Get("serviceDiscovery") as ClientServiceDiscovery;
                ILogger logger = context.JobDetail.JobDataMap.Get("logger") as ILogger;
                logger.Debug("******* start update services job *******");
                serviceDiscovery?.UpdateRoutes();
                return Task.CompletedTask;
            }
        }
    }
}
