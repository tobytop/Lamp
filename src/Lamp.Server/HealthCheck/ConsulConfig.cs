namespace Lamp.Server.HealthCheck
{
    public class ConsulConfig
    {
        public int CheckInterval { get; set; } = 10;
        public int CriticalInterval { get; set; } = 30;
        //public static readonly TimeSpan BlacklistPeriod = TimeSpan.FromMinutes(2);
        //public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
        //public static readonly TimeSpan CriticalInterval = TimeSpan.FromSeconds(30);
    }
}
