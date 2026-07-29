namespace VlaBra.DevTools.AksInstalledApps.ConsoleApp
{
    public abstract class AksItemBase
    {
        protected AksItemBase(string? folder, string name)
        {
            Name = name;
            Folder = folder;
        }

        public string Name { get; }
        public string? Folder { get; }
    }
}
