namespace VlaBra.DevTools.AksInstalledApps.ConsoleApp
{
    public class AksDeployment: AksItemBase
    {
        public AksDeployment(AksCustomer customer, AksApplication application, string? folder, string deploymentName, string? chartVersion, string? imageVersion) : base(folder, application.Name)
        {
            Customer = customer;
            Application = application;
            DeploymentName = deploymentName;
            ChartVersion = chartVersion;
            ImageVersion = imageVersion;
        }
               

        public string? ChartVersion { get; set; }
        public string? ImageVersion { get; set; }
        public string DeploymentName { get; }
        public AksCustomer Customer { get; }
        public AksApplication Application { get; }
    }
}
