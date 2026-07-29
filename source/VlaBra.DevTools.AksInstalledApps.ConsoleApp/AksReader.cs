using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace VlaBra.DevTools.AksInstalledApps.ConsoleApp
{

    public class AksReader
    {
        public static IEnumerable<AksApplication> MakeDefaultApplications ()
        {
            yield return new AksApplication("ART", "Alarm rationalization Tool", "alarm-rationalization-tool");
            yield return new AksApplication("E2", "Event Explorer", "event-explorer");
            yield return new AksApplication("EB", "Event Broker", "eventbroker");
            yield return new AksApplication("DB", "Data Broker", "databroker");
            yield return new AksApplication("IAM", "Intelligent Alarm Management", "iam");
        }

        private readonly List<AksApplication> _applications;
        private readonly Dictionary<string, AksApplication> _applicationsByChart;

        public AksReader(bool addDefaultAlarmInsightApps, ICollection<AksApplication>? applications = null) 
        { 
            _applications = new List<AksApplication>();

            if (addDefaultAlarmInsightApps)
            {
                _applications.AddRange(MakeDefaultApplications());
            }
            
            if (applications != null && applications.Count > 0)
            {
                _applications.AddRange(applications);
            }

            if (_applications.Count == 0)
            {
                throw new ArgumentException("At least one application must be provided.", nameof(applications));
            }

            _applicationsByChart = new Dictionary<string, AksApplication>();
            foreach (var application in _applications)
            {
                foreach (var chartName in application.ChartNames)
                {
                    if (_applicationsByChart.ContainsKey(chartName))
                    {
                        throw new ArgumentException($"Duplicate chart name '{chartName}' found in application '{application.Name}'. Chart names must be unique across all applications.");
                    }
                    _applicationsByChart[chartName] = application;
                }
            }
        }

        public List<AksCollection> ProcessCollections(string[] collectionFolders)
        {
            var collections = new List<AksCollection>();

            foreach (var collectionFolder in collectionFolders)
            {
                ProcessCollections(collections, collectionFolder);
            }
            return collections;
        }

        private void ProcessCollections(List<AksCollection> collections, string collectionFolder)
        {
            var collectionName = Path.GetFileName(collectionFolder);
            var collection = new AksCollection(collectionFolder, collectionName);
            collections.Add(collection);
            
            var deploymentsPath = Path.Combine(collectionFolder, "deployments");

            if (!Directory.Exists(deploymentsPath))
            {
                return;
            }

            foreach (var clusterFolder in Directory.GetDirectories(deploymentsPath))
            {
                ProcessCluster(collection, clusterFolder);
            }
        }

        private void ProcessCluster(AksCollection collection, string clusterFolder)
        {
            var clusterName = Path.GetFileName(clusterFolder);
            if (IgnoreCluster(clusterName))
            {
                return;
            }

            var cluster = collection.AddCluster(clusterFolder, clusterName);

            var customersFolder = Path.Combine(clusterFolder, "customers");

            if (!Directory.Exists(customersFolder))
            {
                return;
            }

            foreach (var customerFolder in Directory.GetDirectories(customersFolder))
            {
                ProcessCustomer(cluster, customerFolder);
            }

        }



        private void ProcessCustomer(AksCluster cluster, string customerFolder)
        {
            var customerName = Path.GetFileName(customerFolder);
            if (IgnoreCustomer(customerName))
            {
                return;
            }

            var customer = cluster.AddCustomer(customerFolder, customerName);

            foreach (var applicationFolder in Directory.GetDirectories(customerFolder))
            {
                ProcessApplicationRecursive(customer, customerFolder, applicationFolder, 0);
            }
        }



        private void ProcessApplicationRecursive(AksCustomer customer, string customerFolder, string applicationFolder, int depth)
        {
            var deploymentLastName = Path.GetFileName(applicationFolder);
            if (IgnoreDeployment(deploymentLastName))
            {
                return;
            }

            var deploymentName = applicationFolder.Substring(customerFolder.Length + 1);

            var chartYamlPath = Directory
                .EnumerateFiles(applicationFolder, "*.yaml", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), "chart.yaml", StringComparison.OrdinalIgnoreCase));

            var valuesYamlPath = Directory
                .EnumerateFiles(applicationFolder, "*.yaml", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), "values.yaml", StringComparison.OrdinalIgnoreCase));

            if (chartYamlPath != null)
            {
                ProcessApplication(customer, applicationFolder, deploymentName, chartYamlPath, valuesYamlPath);
            }
            else
            {
                foreach (var subFolder in Directory.GetDirectories(applicationFolder))
                {
                    ProcessApplicationRecursive(customer, customerFolder, subFolder, depth + 1);
                }
            }
        }

        private void ProcessApplication(AksCustomer customer, string folder, string deploymentName, string chartYamlPath, string? valuesYamlPath)
        {
            var charts = ProcessChartsYaml(chartYamlPath);
              
            foreach (var chart in charts) 
            {
                if (_applicationsByChart.TryGetValue(chart.ChartName, out var application))
                {
                    customer.AddDeployment(application, folder, deploymentName, chart.Version, null);
                }
            }

            //var imageVersion = ProcessValuesYaml(charts, valuesYamlPath);


        }

        //private void ProcessValuesYaml(IEnumerable<(string? ChartName, string? Version)> charts, string? valuesYamlPath)
        //{
        //    if (valuesYamlPath == null)
        //    {
        //        return;
        //    }

        //    using var reader = new StreamReader(valuesYamlPath);
        //    var yaml = new YamlStream();
        //    yaml.Load(reader);

        //    if (yaml.Documents.Count == 0)
        //    {
        //        yield break;
        //    }

        //    if (yaml.Documents[0].RootNode is not YamlMappingNode root)
        //    {
        //        yield break;
        //    }


        //    if (!root.Children.TryGetValue(new YamlScalarNode(imageName), out var imageNameNode)
        //        || imageNameNode is not YamlMappingNode imageNameMapping)
        //    {
        //        return null;
        //    }

        //    if (!imageNameMapping.Children.TryGetValue(new YamlScalarNode("image"), out var imageNode)
        //        || imageNode is not YamlMappingNode imageMapping)
        //    {
        //        return null;
        //    }

        //    if (!imageMapping.Children.TryGetValue(new YamlScalarNode("tag"), out var tagNode)
        //        || tagNode is not YamlScalarNode tagScalar)
        //    {
        //        return null;
        //    }

        //    return tagScalar.Value;
        //}

        private IEnumerable<(string ChartName, string? Version)> ProcessChartsYaml(string chartYamlPath)
        {
            YamlStream yaml;

            try
            {
                using var reader = new StreamReader(chartYamlPath);
                yaml = new YamlStream();
                yaml.Load(reader);
            }
            catch (Exception ex)
            {
             //   Console.WriteLine($"Error processing chart.yaml at '{chartYamlPath}': {ex.Message}");
                yield break;
            }

            if (yaml.Documents.Count == 0)
            {
                yield break;
            }

            if (yaml.Documents[0].RootNode is not YamlMappingNode root)
            {
                yield break;
            }

            if (!root.Children.TryGetValue(new YamlScalarNode("dependencies"), out var depsNode)
                || depsNode is not YamlSequenceNode dependencies)
            {
                yield break;
            }

            foreach (var depNode in dependencies)
            {
                if (depNode is not YamlMappingNode dep)
                {
                    continue;
                }

                var chartName = dep.Children.TryGetValue(new YamlScalarNode("name"), out var nameNode)
                    ? ((YamlScalarNode)nameNode).Value
                    : null;

                var version = dep.Children.TryGetValue(new YamlScalarNode("version"), out var versionNode)
                    ? ((YamlScalarNode)versionNode).Value
                    : null;

                if (!string.IsNullOrWhiteSpace(chartName) && !string.IsNullOrWhiteSpace(version) && !IgnoreChart(chartName))
                {
                    yield return (chartName, version);
                }
            }
        }

        private bool IgnoreCluster(string name)
        {
            switch (name.Trim().ToLower())
            {
                case "argo":
                case "argodeploy":
                    return true;
                default:
                    return false;
            }
        }

        private bool IgnoreCustomer(string name)
        {
            switch (name.Trim().ToLower())
            {
                case "argo":
                case "argodeploy":
                    return true;
                default:
                    return false;
            }
        }

        private bool IgnoreDeployment(string name)
        {
            switch (name.Trim().ToLower())
            {
                case "argo":
                case "argodeploy":
                    return true;
                default:
                    return false;
            }
        }

        private bool IgnoreChart([NotNullWhen(false)]string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }
            
            switch (name.Trim().ToLower())
            {
                case "argo":
                case "argodeploy":
                    return true;
                default:
                    return false;
            }
        }
    }
}
