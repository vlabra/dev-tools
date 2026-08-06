using System;
using VlaBra.DevTools.NiceCopilotChat;

namespace VlaBra.DevTools.NiceCopilotChat.ConsoleApp
{
    internal class Program
    {
        static int Main(string[] args)
        {
            bool doWriteOutput = false;
            if (args.Length > 0 && (args[^1] == "--write-output" || args[^1] == "--write"))
            {
                doWriteOutput = true;
                args = args[..^1];
            }

            if (args.Length == 0)
            {
                Console.Error.WriteLine("Error: No paths provided. Usage: <program> <path1> [path2 ...]");
                return 1;
            }

            List<FileInfo> files;

            try
            {
                files = InputPathResolver.Resolve(args);
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }

            var processor = new ChatFileProcessor();

            foreach (var file in files)
            {
                processor.ProcessFile(file, doWriteOutput);
            }

            return 0;
        }
    }
}
