using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    internal class TimerQueue<T>
    {
        private Action<T> callback;

        public TimerQueue(Action<T> callback)
        {
            this.callback = callback;        
        }
    }
}
