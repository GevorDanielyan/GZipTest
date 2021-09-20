using GZipTest.Models;
using System;
using System.IO;

namespace GZipTest.Producer
{
    class Decompress : AbstractArchiver
    {
        public Decompress(Stream inputFileStream, Stream outputFileStream) : base(inputFileStream, outputFileStream)
        {
        }

        protected override void ReadInFile()
        {
            try
            {
                using (var binaryReader = new BinaryReader(InputFileStream))
                {
                    var fileSize = InputFileStream.Length;

                    const int intSize = 4;
                    var chunkId = 0;

                    while (fileSize > 0 && HasError == null)
                    {
                        var chunkSize = binaryReader.ReadInt32();
                        var bytes = binaryReader.ReadBytes(chunkSize);
                        InputQueue.Enqueue(new Chunk(chunkId++, bytes));
                        fileSize -= (chunkSize + intSize);
                        if (fileSize == 0)
                        {
                            InputQueue.ReadComplete();
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                Serilog.Log.Error($"(Your exception is :{error.Message}");
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
                        binaryWriter.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception error)
            {
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                Serilog.Log.Error($"(Your exception is :{error.Message}");
                HasError = error;
            }
        }

        protected override void Process(int processEventId)
        {
            try
            {
                while (InputQueue.Dequeue(out Chunk chunk) && HasError == null)
                {
                    var decompressedChunkData = GZip.GZip.DecompressByBlocks(chunk.Bytes);
                    if (decompressedChunkData == null) throw new OutOfMemoryException();
                    OutputDictionary.Add(chunk.Id, decompressedChunkData);
                }
                ProcessEvents[processEventId].Set();
            }
            catch (Exception error)
            {
                Serilog.Log.Debug($"(Your exception is :{error.Message}");
                Serilog.Log.Error($"(Your exception is :{error.Message}");
                HasError = error;
            }
        }
    }
}
