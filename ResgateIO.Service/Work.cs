using System;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    internal class Work
    {
        public string ResourceName { get; }
        private Queue<Action> callbacks;

        public Work(string resourceName, Action callback)
        {
            this.ResourceName = resourceName;
            this.callbacks = new Queue<Action>(4);
            this.callbacks.Enqueue(callback);
        }

        public void AddTask(Action callback)
        {
            callbacks.Enqueue(callback);
        }

        public Action NextTask()
        {
            if (callbacks.Count == 0)
            {
                return null;
            }

            return callbacks.Dequeue();
        }
    }
}
