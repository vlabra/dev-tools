using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VlaBra.DevTools.NiceCopilotChat
{
    /// <summary>
    /// Resolves command-line path arguments into a de-duplicated list of markdown <see cref="FileInfo"/> instances.
    /// </summary>
    public static class InputPathResolver
    {
        private const string RecursiveSuffixBackslash = "\\**";
        private const string RecursiveSuffixForwardSlash = "/**";
        private const string MarkdownSearchPattern = "*.md";

        /// <summary>
        /// Resolves the given path arguments into a de-duplicated list of <see cref="FileInfo"/> instances.
        /// Throws <see cref="InvalidOperationException"/> with a descriptive message on the first invalid path.
        /// </summary>
        public static List<FileInfo> Resolve(IEnumerable<string> paths)
        {
            var filesByFullName = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                foreach (var file in ResolveSinglePath(path))
                {
                    filesByFullName[file.FullName] = file;
                }
            }

            return filesByFullName.Values.ToList();
        }

        private static IEnumerable<FileInfo> ResolveSinglePath(string path)
        {
            if (path.EndsWith(RecursiveSuffixBackslash, StringComparison.Ordinal)
                || path.EndsWith(RecursiveSuffixForwardSlash, StringComparison.Ordinal))
            {
                var folderPath = path[..^RecursiveSuffixBackslash.Length];

                if (!Directory.Exists(folderPath))
                {
                    throw new InvalidOperationException($"Path '{path}' is invalid: folder '{folderPath}' does not exist.");
                }

                return EnumerateMarkdownFiles(folderPath, SearchOption.AllDirectories);
            }

            if (File.Exists(path))
            {
                return new[] { new FileInfo(path) };
            }

            if (Directory.Exists(path))
            {
                return EnumerateMarkdownFiles(path, SearchOption.TopDirectoryOnly);
            }

            throw new InvalidOperationException($"Path '{path}' is invalid: it is not an existing file or folder.");
        }

        private static IEnumerable<FileInfo> EnumerateMarkdownFiles(string folderPath, SearchOption searchOption)
        {
            return new DirectoryInfo(folderPath).EnumerateFiles(MarkdownSearchPattern, searchOption);
        }
    }
}
