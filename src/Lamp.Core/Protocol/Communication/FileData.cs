using System;

namespace Lamp.Core.Protocol.Communication
{
    [Serializable]
    public class FileData
    {
        public FileData()
        {
        }

        public FileData(string name, string data)
        {
            FileName = name;
            Data = data;
        }

        public string FileName { get; set; }
        public string Data { get; set; }
    }
}
