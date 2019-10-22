
using Serilog;
using System;

namespace Lamp.Core.Common.Logger.Implement
{
    public class SeriLogger : ILogger
    {
        private Serilog.Core.Logger _logger;
        public SeriLogger()
        {
            _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        public void Debug(string info)
        {
            _logger.Debug(info);
        }

        public void Error(string info, Exception ex)
        {
            _logger.Error(ex, info);
        }

        public void Info(string info)
        {
            _logger.Information(info);
        }

        public void Warn(string info)
        {
            _logger.Warning(info);
        }
    }
}
