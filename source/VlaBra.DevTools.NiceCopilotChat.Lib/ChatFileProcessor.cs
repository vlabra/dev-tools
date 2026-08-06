using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VlaBra.DevTools.NiceCopilotChat
{
    /// <summary>
    /// Reads an exported Copilot chat markdown file, cleans it up line by line and overwrites it in place.
    /// </summary>
    public class ChatFileProcessor
    {
        private const string UserPromptOpenTag = "<user_prompt>";
        private const string UserPromptCloseTag = "</user_prompt>";
        private static readonly string[] StartPrefixes = { "User:", "## :question:", "**USER:**" };
        private static readonly string[] EndPrefixes = { "GitHub Copilot:", "## GitHub Copilot", "**ASSISTANT:**" };

        public void ProcessFile(FileInfo file, bool writeOutput = true)
        {
            Console.WriteLine($"Processing {file.FullName} ...");

            var inputLines = File.ReadAllLines(file.FullName);

            var outputLines = new List<string>();
            var cacheLines = new List<string>();
            var lastLineEmpty = true;
            var lineNumber = 0;
            var anyChanges = false;

            foreach (var line in inputLines)
            {
                lineNumber++;

                if (line.Trim() == UserPromptOpenTag || line.Trim() == UserPromptCloseTag)
                {
                    anyChanges = true;
                    continue;
                }
                else if (cacheLines.Count > 0 && EndPrefixes.Any(p => line.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (cacheLines.Count > 0)
                    {
                        anyChanges = true;
                        Console.WriteLine($"  - line {lineNumber} - AI response detected");
                        ProcessCache(cacheLines, outputLines, lastLineEmpty);
                        cacheLines.Clear();
                    }
                    else
                    {
                        Console.WriteLine($"  - line {lineNumber} - AI response detected without previous user prompt (ignored)");
                        outputLines.Add(line);
                    }
                    outputLines.Add(line);
                }
                else if (cacheLines.Count > 0)
                {
                    cacheLines.Add(line);
                }
                else if (StartPrefixes.FirstOrDefault(p => line.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)) is string matchedStartPrefix)
                {
                    Console.WriteLine($"  - line {lineNumber} - User prompt detected");
                    cacheLines.Add(line[matchedStartPrefix.Length..]);
                }
                else
                {
                    outputLines.Add(line);
                }

                lastLineEmpty = string.IsNullOrWhiteSpace(line);
            }

            if (cacheLines.Count > 0)
            {
                ProcessCache(cacheLines, outputLines, lastLineEmpty);
                cacheLines.Clear();
            }

            if (!anyChanges)
            {
                Console.WriteLine($"  - Writing output skipped - No changes detected.");
                return;
            }
            else if (writeOutput)
            {
                Console.WriteLine($"  - Writing output ...");
                File.WriteAllLines(file.FullName, outputLines);
            }
            else
            {
                Console.WriteLine($"  - Writing output skipped - DRY RUN.");
            }
        }

        /// <summary>
        /// TODO: real formatting logic to be implemented later.
        /// For now, passes the cached lines through into the output as-is.
        /// </summary>
        private void ProcessCache(List<string> cacheLines, List<string> outputLines, bool lastLineEmpty)
        {
            if (cacheLines.Count == 0)
            {
                return;
            }

            if (!lastLineEmpty)
            {
                outputLines.Add(string.Empty);
            }

            outputLines.Add("> # USER PROMPT");
            
            if (!string.IsNullOrWhiteSpace(cacheLines[0]))
            {
                outputLines.Add(">");
            }
            
            outputLines.AddRange(cacheLines.Select(line => $"> {line}"));

            if (!string.IsNullOrWhiteSpace(cacheLines[^1]))
            {
                outputLines.Add(">");
            }

            outputLines.Add("> ---");
            outputLines.Add(string.Empty);
        }
    }
}
