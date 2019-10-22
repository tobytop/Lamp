namespace Lamp.Core.Protocol.Server
{
    public class RegisterServer
    {
        public RegisterServer(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}
