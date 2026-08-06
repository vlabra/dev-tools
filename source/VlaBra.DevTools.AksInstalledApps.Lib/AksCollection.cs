namespace VlaBra.DevTools.AksInstalledApps
{
    public class AksCollection : AksItemBase, IReadOnlyCollection<AksCluster>
    {
        private readonly List<AksCluster> _clusters = new List<AksCluster>();

        public AksCollection(string? folder, string name) : base(folder, name)
        {
        }

        public AksCluster AddCluster(string? path, string name)
        {
            var cluster = new AksCluster(this, path, name);
            _clusters.Add(cluster);
            return cluster;
        }

        public int Count => _clusters.Count;

        public IEnumerator<AksCluster> GetEnumerator()
        {
            return _clusters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
