using GZipTest.Models;
using System;
using System.IO;

namespace GZipTest.Producer
{
    public class Compress : AbstractArchiver
    {
        public Compress(Stream inputFileStream, Stream outputFileStream) : base(inputFileStream, outputFileStream)
        {
        }


        protected override void ReadInFile()
        {
            try
            {
                var fileSize = InputFileStream.Length;
                using (var binaryReader = new BinaryReader(InputFileStream))
                {
                    var chunkId = 0;
                    while (fileSize > 0 && HasError == null)
                    {
                        var currentChunkSize = fileSize > Constants.Constants.ChunkSize ? Constants.Constants.ChunkSize : fileSize;
                        var bytes = binaryReader.ReadBytes((int)currentChunkSize);
                        InputQueue.Enqueue(new Chunk(chunkId++, bytes));
                        fileSize -= currentChunkSize;
                        if (fileSize == 0)
                        {
                            InputQueue.ReadComplete();
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Serilog.Log.Error($"(Your exception is :{error.Message}");
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                HasError = error;
            }
        }

        protected override void Process(int processEventId)
        {
            try
            {
                while (InputQueue.Dequeue(out Chunk chunk) && HasError == null)
                {
                    var compressChunk = GZip.GZip.CompressByBlocks(chunk.Bytes);
                    if (compressChunk == null) throw new OutOfMemoryException();
                    OutputDictionary.Add(chunk.Id, compressChunk);
                }
                ProcessEvents[processEventId].Set();
            }
            catch (Exception error)
            {
                Serilog.Log.Error($"(Your exception is :{error.Message}");
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                HasError = error;
            }
        }

        protected override void WriteOutFile()
        {
            try
            {
                using (var binaryWriter = new BinaryWriter(OutputFileStream))
                {
                    while (OutputDictionary.GetValueByKey(out var data) && HasError == null)
                    {
                        binaryWriter.Write(data.Length);
                        binaryWriter.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception error)
            {
                Serilog.Log.Error($"(Your exception is :{error.Message}");
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                HasError = error;
            }
        }
    }
}
