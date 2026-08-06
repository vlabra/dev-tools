namespace VlaBra.DevTools.AksInstalledApps
{
    public class AksCustomer : AksItemBase, IReadOnlyCollection<AksDeployment>
    {
        private readonly List<AksDeployment> _applications = new List<AksDeployment>();
        
        public AksCustomer(AksCluster cluster, string? folder, string name) : base(folder, name)
        {
            Cluster = cluster;
        }
        
        public AksCluster Cluster { get; }
        public int Count => _applications.Count;

        internal AksDeployment AddDeployment(AksApplication application, string? path, string deploymentName, string? chartVersion, string? imageVersion)
        {
            var deployment = new AksDeployment(this, application, path, deploymentName, chartVersion, imageVersion); 
            _applications.Add(deployment);
            return deployment;
        }

        public IEnumerator<AksDeployment> GetEnumerator()
        {
            return _applications.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
