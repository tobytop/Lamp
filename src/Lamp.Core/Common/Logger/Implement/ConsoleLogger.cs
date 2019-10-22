using System;

namespace Lamp.Core.Common.Logger.Implement
{
    public class ConsoleLogger : ILogger
    {
        public void Debug(string info)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} DEBUG {info}");
        }

        public void Error(string info, Exception ex)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ERROR {info},{ex.Message}");
        }

        public void Info(string info)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} INFO  {info}");
        }

        public void Warn(string info)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} WARN  {info}");
        }
    }
}
