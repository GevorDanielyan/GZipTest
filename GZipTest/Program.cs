using GZipTest.Helpers;
using GZipTest.Producer;
using System.IO;
using System;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (InputValidator.ValidateInputArgs(args))
            {
                using var sourceStream = new FileStream(args[1], FileMode.Open, FileAccess.Read);
                using var destinationStream = new FileStream(args[2], FileMode.Create, FileAccess.Write);
                AbstractArchiver archiver;
                switch (args[0])
                {
                    case "compress":
                        archiver = new Compress(sourceStream, destinationStream);
                        archiver.Run();
                        break;
                    case "decompress":
                        archiver = new Decompress(sourceStream, destinationStream);
                        archiver.Run();
                        break;
                }

            }
            Console.ReadKey();
        }
    }
}
