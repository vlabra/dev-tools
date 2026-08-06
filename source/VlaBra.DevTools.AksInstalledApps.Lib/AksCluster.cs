namespace VlaBra.DevTools.AksInstalledApps
{
    public class AksCluster : AksItemBase, IReadOnlyCollection<AksCustomer>
    {
        private readonly List<AksCustomer> _customers = new List<AksCustomer>();

        public AksCluster(AksCollection collection, string? folder, string name) : base(folder, name)
        {
            Collection = collection;
        }

        public AksCollection Collection { get; }

        internal AksCustomer AddCustomer(string? path, string name)
        {
            var customer = new AksCustomer(this, path, name);
            _customers.Add(customer);
            return customer;
        }

        public int Count => _customers.Count;

        public IEnumerator<AksCustomer> GetEnumerator()
        {
            return _customers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
