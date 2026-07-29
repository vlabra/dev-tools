namespace VlaBra.DevTools.AksInstalledApps.ConsoleApp
{
    public class AksApplication
    {
        public AksApplication(string name, string fullName, params string[] chartNames)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name cannot be null or whitespace.", nameof(fullName));
            }
            if (chartNames == null || chartNames.Length == 0)
            {
                throw new ArgumentException("Image names cannot be null or empty array.", nameof(chartNames));
            }

            Name = name;
            FullName = fullName;
            ChartNames = chartNames;       
        }

        public string Name { get;  }
        public string FullName { get; }
        public string[] ChartNames { get;  }
    }
}
