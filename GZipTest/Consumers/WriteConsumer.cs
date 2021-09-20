using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest.Consumers
{
    public class WriteConsumer
    {
        private readonly object _locker = new object();
        private ConcurrentDictionary<int, byte[]> _dictionary = new ConcurrentDictionary<int, byte[]>();
        private bool _completed = false;
        private int _index = 0;

        public void Add(int chunkId, byte[] bytes)
        {
            lock (_locker)
            {
                _dictionary.TryAdd(chunkId, bytes);
                Monitor.PulseAll(_locker);
            }
        }

        public bool GetValueByKey(out byte[] data)
        {
            lock (_locker)
            {
                while (!_dictionary.ContainsKey(_index))
                {
                    if (_completed)
                    {
                        data = new byte[0];
                        return false;
                    }
                    Monitor.Wait(_locker);
                }
                data = _dictionary[_index++];
                Monitor.PulseAll(_locker);
                return true;
            }


        }

        public void SetCompleted()
        {
            lock (_locker)
            {
                _completed = true;
                Monitor.PulseAll(_locker);
            }
        }
    }
}
