using GZipTest.Consumers;
using GZipTest.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest.Producer
{
    public abstract class AbstractArchiver
    {
        protected Stream InputFileStream { get; set; }

        protected Stream OutputFileStream { get; set; }

        protected ReadConsumer InputQueue { get; set; }

        protected WriteConsumer OutputDictionary { get; set; }

        protected AutoResetEvent[] ProcessEvents = new AutoResetEvent[CoreManager.GetCoreCount()];

        protected Exception HasError = null;

        protected AbstractArchiver(Stream inputFileStream, Stream outputFileStream)
        {
            InputFileStream = inputFileStream;
            OutputFileStream = outputFileStream;
            InputQueue = new ReadConsumer();
            OutputDictionary = new WriteConsumer();
        }

        public void Run()
        {
            var readingThread = new Thread(new ThreadStart(ReadInFile));
            var compressingThreads = new List<Thread>();
            for (var i = 0; i < CoreManager.GetCoreCount(); i++)
            {
                var j = i;
                ProcessEvents[j] = new AutoResetEvent(false);
                compressingThreads.Add(new Thread(() => Process(j)));
            }
            var writingThread = new Thread(new ThreadStart(WriteOutFile));

            readingThread.Start();

            foreach (var compressThread in compressingThreads)
            {
                compressThread.Start();
            }

            writingThread.Start();

            WaitHandle.WaitAll(ProcessEvents);
            OutputDictionary.SetCompleted();

            writingThread.Join();
            if (HasError != null)
            {
                Serilog.Log.Debug(HasError.Message);
                Serilog.Log.Error(HasError.Message);
                
            }
            else
            {
                Serilog.Log.Information("Completed");
            }
        }


        protected abstract void ReadInFile();

        protected abstract void Process(int processEventId);

        protected abstract void WriteOutFile();
    }
}
