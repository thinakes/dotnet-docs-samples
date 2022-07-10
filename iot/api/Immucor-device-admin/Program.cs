using GoogleCloudSamples;
using Google.Apis.CloudIot.v1.Data;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("Google IOT Core Project: " + CloudIOTApis.ProjectId);
Console.WriteLine("");

Registry.DisplayRegistries();

Console.WriteLine("");

Console.WriteLine("Select one of Options: ");
Console.WriteLine("1. List device Registries");
Console.WriteLine("2. Open an existing Registry");
Console.WriteLine("3. List Devices in Registry");
Console.WriteLine("4. Create a New Device in the opend Registry");
Console.WriteLine("5. Open an existing Device in current Registry");
Console.WriteLine("6. Send Command to opened device");
Console.WriteLine("7. Update device config");
Console.WriteLine("8. Exit");
Console.WriteLine("");

while (true)
{
    Console.Write("Option: ");
    string? selectedOption = Console.ReadLine();
    if (selectedOption == null)
        continue;
    if (selectedOption == "8")
    {
        Console.WriteLine("Goodbye!");
        break;
    }
    switch (selectedOption)
    {
        case "1":
            Console.WriteLine("1. List device registries");
            Registry.DisplayRegistries();
            break;
        case "2":
            Console.WriteLine("2. Open an existing Registry");
            break;
        case "3":
            Console.WriteLine("3. List devices in Registry");
            break;
        case "4":
            Console.WriteLine("4. Create a New Device in the opend Registry");
            break;
        case "5":
            Console.WriteLine("5. Open an existing Device in current Registry");
            break;
        case "6":
            Console.WriteLine("6. Send Command to opened device");
            break;
        case "7":
            Console.WriteLine("7. Update device config");
            break;
    }
}

class Registry
{
    internal static void DisplayRegistries()
    {
        string[] regions = { CloudIOTApis.RegionIdASIA, CloudIOTApis.RegionIdEU, CloudIOTApis.RegionIdUS };
        foreach (string region in regions)
        {
            Console.WriteLine(region + ":");
        
            foreach (DeviceRegistry dr in CloudIOTApis.GetRegistries(region))
            {
               Console.WriteLine(dr.Name);
               
            }
            Console.WriteLine("-------------------------------------------");
        }
    }
}

static class CloudIOTApis
{
    internal static readonly string RegionIdUS = "us-central1";
    internal static readonly string RegionIdEU = "europe-west1";
    internal static readonly string RegionIdASIA = "asia-east1";

    internal static readonly string ProjectId = "ivdiot";
    internal static readonly string PrivateKeyPath = "../../../pki/rsa_private.pem";
    internal static readonly string CertPath = "../../../pki/roots.pem";
    internal static readonly string ServiceAccount = "serviceAccount:ivdiot-service-account@ivdiot.iam.gserviceaccount.com";
    //internal static readonly string DeviceUniqueId = Guid.NewGuid().ToString();
    internal static readonly string RegistryId = "immucor-device-registry";

    static CloudIOTApis()
    {
        
    }

    internal static List<DeviceRegistry> GetRegistries(string regionId)
    {
        return (List<DeviceRegistry>)CloudIotSample.GetRegistries(ProjectId, regionId);
    }

}