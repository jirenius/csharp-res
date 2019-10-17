using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    internal class Work
    {
        public string ResourceName { get; }
        private Queue<Func<Task>> callbacks;

        public Work(string resourceName, Func<Task> callback)
        {
            this.ResourceName = resourceName;
            this.callbacks = new Queue<Func<Task>>(4);
            this.callbacks.Enqueue(callback);
        }

        public void AddTask(Func<Task> callback)
        {
            callbacks.Enqueue(callback);
        }

        public Func<Task> NextTask()
        {
            if (callbacks.Count == 0)
            {
                return null;
            }

            return callbacks.Dequeue();
        }
    }
}
