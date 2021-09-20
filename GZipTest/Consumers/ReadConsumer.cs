using GZipTest.Models;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Consumers
{
    public class ReadConsumer
    {
        private readonly object _locker = new object();
        private bool _complete = false;
        private Queue<Chunk> _queue = new Queue<Chunk>();

        public void Enqueue(Chunk chunk)
        {
            lock (_locker)
            {
                _queue.Enqueue(chunk);
                Monitor.PulseAll(_locker);
            }
        }

        public bool Dequeue(out Chunk chunk)
        {
            lock (_locker)
            {
                while (_queue.Count == 0)
                {
                    if (_complete)
                    {
                        chunk = new Chunk();
                        return false;
                    }

                    Monitor.Wait(_locker);
                }
                chunk = _queue.Dequeue();
                Monitor.PulseAll(_locker);
                return true;
            }
        }

        public void ReadComplete()
        {
            lock (_locker)
            {
                _complete = true;
                Monitor.PulseAll(_locker);
            }
        }
    }
}
