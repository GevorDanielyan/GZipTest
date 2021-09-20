using System;
using System.IO;

namespace GZipTest.Helpers
{
    public static class InputValidator
    {
        public static bool ValidateInputArgs(string[] args)
        {
            var infoFileIn = new FileInfo(args[1]);
            var infoFileOut = new FileInfo(args[2]);

            if (args?.Length != 3)
            {
                Console.WriteLine("Incorrect number of parameters");
                return false;
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                Console.WriteLine("Invalid first argument choose of [compress/decompress]");
            }

            if (!infoFileIn.Exists || infoFileIn.Length == 0)
            {
                Console.WriteLine("Invalid second argument, you must specify the file path if it exists");
                return false;
            }

            if (infoFileOut.Exists)
            {
                Console.WriteLine("Invalid third argument, you must specify the file path if it exists");
                return false;
            }

            return true;
        }
    }
}
