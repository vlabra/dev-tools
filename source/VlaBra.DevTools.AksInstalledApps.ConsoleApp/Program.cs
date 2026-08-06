using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VlaBra.DevTools.AksInstalledApps;

namespace VlaBra.DevTools.AksInstalledApps.ConsoleApp
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Error: No folders provided. Usage: <program> <folder1> [folder2 ...] [-all] [-apps <app1> [app2 ...]]");
                return 1;
            }

            var folders = new List<string>();
            var writeAll = false;
            var appFilter = new List<string>();

            int i = 0;
            while (i < args.Length && !args[i].StartsWith('-'))
            {
                folders.Add(args[i] == "." ? Directory.GetCurrentDirectory() : args[i]);
                i++;
            }

            while (i < args.Length)
            {
                switch (args[i].ToLower())
                {
                    case "-all":
                        writeAll = true;
                        i++;
                        break;
                    case "-apps":
                        i++;
                        while (i < args.Length && !args[i].StartsWith('-'))
                        {
                            appFilter.Add(args[i]);
                            i++;
                        }
                        break;
                    default:
                        Console.Error.WriteLine($"Warning: Unknown parameter '{args[i]}' ignored.");
                        i++;
                        break;
                }
            }

            if (folders.Count == 0)
            {
                Console.Error.WriteLine("Error: No folders provided.");
                return 1;
            }

            var reader = new AksReader(true);
            var collections = reader.ProcessCollections(folders.ToArray());

            if (writeAll)
                WriteAll(collections, appFilter);
            else
                WriteNonEmpty(collections, appFilter);

            return 0;
        }

        private static bool MatchesFilter(AksDeployment deployment, List<string> appFilter)
        {
            return appFilter.Count == 0
                || appFilter.Any(f => string.Equals(f, deployment.Name, StringComparison.OrdinalIgnoreCase));
        }

        private static void WriteAll(List<AksCollection> collections, List<string> appFilter)
        {
            foreach (var collection in collections)
            {
                Console.WriteLine($"{collection.Name}");
                foreach (var cluster in collection)
                {
                    if (cluster.Count == 0) continue;

                    Console.WriteLine($"  > {cluster.Name}");
                    foreach (var customer in cluster)
                    {
                        Console.WriteLine($"     * {customer.Name}");
                        foreach (var deployment in customer)
                        {
                            if (!MatchesFilter(deployment, appFilter)) continue;
                            Console.WriteLine($"       - {deployment.Name} [{deployment.ChartVersion}{(string.IsNullOrWhiteSpace(deployment.ImageVersion) ? "" : $", Image {deployment.ImageVersion}")}]");
                        }
                    }
                }
            }
        }

        private static void WriteNonEmpty(List<AksCollection> collections, List<string> appFilter)
        {
            foreach (var collection in collections)
            {
                var collectionWritten = false;
                foreach (var cluster in collection)
                {
                    var clusterWritten = false;
                    if (cluster.Count == 0) continue;

                    foreach (var customer in cluster)
                    {
                        var customerWritten = false;
                        foreach (var deployment in customer)
                        {
                            if (!MatchesFilter(deployment, appFilter)) continue;

                            if (!collectionWritten)
                            {
                                Console.WriteLine($"{collection.Name}");
                                collectionWritten = true;
                            }

                            if (!clusterWritten)
                            {
                                Console.WriteLine($"  > {cluster.Name}");
                                clusterWritten = true;
                            }

                            if (!customerWritten)
                            {
                                Console.WriteLine($"     * {customer.Name}");
                                customerWritten = true;
                            }                            

                            Console.WriteLine($"       - {deployment.Name} [{deployment.ChartVersion}{(string.IsNullOrWhiteSpace(deployment.ImageVersion) ? "" : $", Image {deployment.ImageVersion}")}]");
                        }
                    }
                }
            }
        }
    }
}
