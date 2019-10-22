using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lamp.Service.Test1
{
    public class MonitorTest
    {
        public Mutex MyMutex { get; set; }
        public MonitorTest(string Name)
        {
            MyMutex = new Mutex(true, Name);
        }
    }
}
