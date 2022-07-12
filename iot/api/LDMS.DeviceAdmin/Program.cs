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

void SelectOptions()
{
    Console.WriteLine("Select one of Options:");
    Console.WriteLine("1. List device Registries");
    Console.WriteLine("2. Select region" + $" [current region is {CloudIOTApis.SelectedRegionId}]");
    Console.WriteLine("3. Open an existing Registry" + $" [current registry is {CloudIOTApis.SelectedRegistryId}]");
    Console.WriteLine($"4. List Devices in Registry, '{CloudIOTApis.SelectedRegistryId}'");
    Console.WriteLine($"5. Create a New Device in Registry, '{CloudIOTApis.SelectedRegistryId}'");
    Console.WriteLine($"6. Open an existing Device in Registry,'{CloudIOTApis.SelectedRegistryId}'");
    Console.WriteLine($"7. Send Command to device, '{CloudIOTApis.SelectedDeviceId}'");
    Console.WriteLine($"8. Update device,'{CloudIOTApis.SelectedDeviceId}''s config");
    Console.WriteLine("9. Exit");
    Console.WriteLine("");
}
while (true)
{
    SelectOptions();
    string? selectedOption = Console.ReadLine();
    if (selectedOption == null)
        continue;
    if (selectedOption == "9")
    {
        Console.WriteLine("Goodbye!");
        break;
    }
    switch (selectedOption)
    {
        case "1":
            Console.WriteLine($"1. List device registries. Current project: '{CloudIOTApis.ProjectId}'");
            Registry.DisplayRegistries();
            break;
        case "2":
            Console.WriteLine($"2. Select region.Current project: '{CloudIOTApis.ProjectId}'");
            string? regionId = Console.ReadLine();
            if(regionId != null)
                CloudIOTApis.SelectedRegionId = regionId;
            else
                Console.WriteLine("Invalid Region Id");
            break;
        case "3":
            Console.WriteLine($"3. Open an existing Registry. Selected Region: '{CloudIOTApis.SelectedRegionId}'");
            string? registryId = Console.ReadLine();
            if (registryId != null)
            {
                CloudIOTApis.SelectedRegistryId = registryId;
                try
                {
                    DeviceRegistry deviceRegistry = CloudIOTApis.GetRegistry();
                    CloudIOTApis.DumpObjectProps(typeof(DeviceRegistry), deviceRegistry);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            else
                Console.WriteLine("Invalid Region Id");
            break;
        case "4":
            Console.WriteLine($"4. List devices in Registry.Selected Registry: '{CloudIOTApis.SelectedRegistryId}'");

            List<Device> devices = CloudIOTApis.GetDevices();
            Console.WriteLine($"Found { devices.Count} device(s)");

            foreach (var device in devices)
            {
                CloudIOTApis.DumpObjectProps(typeof(Device), device);
            }
            break;
        case "5":
            Console.WriteLine("5. Create a New Device in the opend Registry");

            CloudIOTApis.DeviceIDToCreate = "Immucor-replicant-" + Guid.NewGuid(); // Default
            string? newdeviceId = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(newdeviceId))
            {
                CloudIOTApis.DeviceIDToCreate = newdeviceId;
            }
            Console.WriteLine("Creating a device");
            CloudIOTApis.CreateDevice();

            break;
        case "6":
            Console.WriteLine("6. Open an existing Device in current Registry. Enter Device ID:");
            string? deviceId = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(deviceId))
            {
                CloudIOTApis.SelectedDeviceId = deviceId;
            }
            Device openedDevice = CloudIOTApis.GetDevice();
            CloudIOTApis.DumpObjectProps(typeof(Device), openedDevice);

            break;
        case "7":
            Console.WriteLine("7. Send Command to opened device");

            break;
        case "8":
            Console.WriteLine("8. Update device config");
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
    internal static readonly string ApiCertPath = "../../../pki/roots.pem";
    internal static readonly string DeviceCertPath = "../../../pki/rsa_cert.pem";
    internal static readonly string ServiceAccount = "serviceAccount:ivdiot-service-account@ivdiot.iam.gserviceaccount.com";
    //internal static readonly string DeviceUniqueId = Guid.NewGuid().ToString();
    internal static readonly string DefaultRegionId = RegionIdUS;
    internal static readonly string DefaultRegistryId = "immucor-device-registry";

    internal static string SelectedRegionId = DefaultRegionId;
    internal static string SelectedRegistryId = DefaultRegistryId;
    internal static string SelectedDeviceId = "Not Selected Yet";

    internal static string DeviceIDToCreate = "0";
    static CloudIOTApis()
    {
        
    }

    internal static List<DeviceRegistry> GetRegistries()
    {
        return (List<DeviceRegistry>)CloudIotSample.GetRegistries(ProjectId, SelectedRegionId);
    }
    internal static List<DeviceRegistry> GetRegistries(string regionId)
    {
        return (List<DeviceRegistry>)CloudIotSample.GetRegistries(ProjectId, regionId);
    }

    internal static DeviceRegistry GetRegistry()
    {
        return (DeviceRegistry)CloudIotSample.GetRegistry(ProjectId, SelectedRegionId, SelectedRegistryId);
    }

    internal static List<Device> GetDevices()
    {
        var devices = CloudIotSample.ListDevices(ProjectId, SelectedRegionId, SelectedRegistryId);

        if(devices == null)
        {
            return new List<Device>();
        }
        return (List<Device>)devices;
    }

    internal static Device CreateDevice()
    {
        return (Device)CloudIotSample.CreateRsaDevice(ProjectId, SelectedRegionId, SelectedRegistryId, DeviceIDToCreate, DeviceCertPath);
    }

    internal static Device GetDevice()
    {
        return (Device) CloudIotSample.GetDevice(ProjectId, SelectedRegionId, SelectedRegistryId, SelectedDeviceId);
    }

    internal static void DumpObjectProps(Type objectType, object obj)
    {
        if (obj != null)
        {
            foreach (var prop in objectType.GetProperties())
            {
                var value = prop.GetValue(obj);
                Console.WriteLine(prop.Name + " : " + value);
            }
        }
        else
        {
            Console.WriteLine($"{objectType} is null");
        }
    }
}