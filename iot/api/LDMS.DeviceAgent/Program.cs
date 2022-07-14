using GoogleCloudSamples;
using Google.Apis.CloudIot.v1.Data;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

Console.WriteLine("Starting LDMS.DeviceAgent....");
Console.WriteLine("Current Product is : ivdiot ");

SetDeviceId();
SetDeviceRegion();
SetDeviceRegistry();

// Setup the client

CloudIOTApis.SetupMqttClient();

//Run the client
while (true)
{
    SelectOptions();
    string? selectedOption = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(selectedOption))
        continue;
    if (selectedOption == "5")
    {
        CloudIOTApis.DisconnectFromBridge();
        Console.WriteLine("Goodbye!");
        break;
    }

    switch(selectedOption)
    {
        case "1":
            CloudIOTApis.ConnectToMQTTBridge();
            break;
        case "2":
            {
                Console.WriteLine("Enter Event Message to Send");
                string? eventMessage = Console.ReadLine();
                Console.WriteLine("How may events to send?");
                string? numevents = Console.ReadLine();
                CloudIOTApis.PublishEvent(eventMessage, Convert.ToInt32(numevents));

                break;
            }
        case "3":
            {
                Console.WriteLine("Enter Device state to Send");
                string? deviceState = Console.ReadLine();
                Console.WriteLine("How State message to send?");
                string? numevents = Console.ReadLine();
                CloudIOTApis.PublishDeviceState(deviceState, Convert.ToInt32(numevents));

                break;
            }
        case "4":

            break;
        
    }

}




void SelectOptions()
{
    Console.WriteLine("Select one of Options:");
    Console.WriteLine("1. Connect to LDMS");
    Console.WriteLine("2. Send Device Event to LDMS :");
    Console.WriteLine("3. Send Device State to LDMS :");
    Console.WriteLine("4. Disconnect from LDMS :");
    Console.WriteLine("5. Exit");
    Console.WriteLine("");
}

static void SetDeviceId()
{
    Console.WriteLine("Set Device Id: [ Must match one of the registered device ]");
    string? deviceId = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(deviceId))
    {
        CloudIOTApis.SelectedDeviceId = deviceId;
    }
}

static void SetDeviceRegion()
{
    Console.WriteLine("Set Device Region: [ Must match one of the Region as per the LDMS portal ]");
    string? regionId = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(regionId))
    {
        CloudIOTApis.SelectedRegionId = regionId;
    }
}

static void SetDeviceRegistry()
{
    Console.WriteLine("Set Device Registry: [ Must match one of the Registry in the region as per the LDMS portal ]");
    string? registryId = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(registryId))
    {
        CloudIOTApis.SelectedRegistryId = registryId;
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
    internal static readonly string DefaultRegistryId = "immucor-device-registry-na";

    internal static string SelectedRegionId = DefaultRegionId;
    internal static string SelectedRegistryId = DefaultRegistryId;
    internal static string SelectedDeviceId = "immucor-replicant-0";

    internal static string DeviceIDToCreate = "0";

    private static MqttClient _mqttClient;
    private static string _mqttClientId;
    private static string _jwtpassword;
    private static string _messageType = "events";
    private static string _mqttBridgeHostName = "mqtt.googleapis.com";
    private static int _mqttBridgePort = 443; //8883;
    private static int _jwtExpiresMin = 20;
    private static int _waitTime = 10;


    static CloudIOTApis()
    {

    }

    internal static void SetupMqttClient()
    {
        _mqttClient = CloudIotMqttExample.GetClient(ProjectId,
                SelectedRegionId,
                SelectedRegistryId,
                SelectedDeviceId,
                ApiCertPath,
                _mqttBridgeHostName,
                _mqttBridgePort);

        // Create our MQTT client. The client_id is a unique string
        // that identifies this device.For Google Cloud IoT Core,
        // it must be in the format below.
        _mqttClientId = $"projects/{ProjectId}" +
            $"/locations/{SelectedRegionId}" +
            $"/registries/{SelectedRegistryId}" +
            $"/devices/{SelectedDeviceId}";
    }

    internal static void ConnectToMQTTBridge()
    {
        if (_mqttClient.IsConnected)
        {
            _mqttClient.ConnectionClosed -= _mqttClient_ConnectionClosed;
            _mqttClient.MqttMsgPublishReceived -= _mqttClient_MqttMsgPublishReceived;
            _mqttClient.MqttMsgPublished -= _mqttClient_MqttMsgPublished;
            _mqttClient.MqttMsgSubscribed -= _mqttClient_MqttMsgSubscribed;
            _mqttClient.MqttMsgUnsubscribed -= _mqttClient_MqttMsgUnsubscribed;

            _mqttClient.Disconnect();
        }

        // With Google Cloud IoT Core, the username field is ignored,
        // and the password field is used to transmit a JWT to authorize
        // the device.
        _jwtpassword = CloudIotMqttExample.CreateJwtRsa(ProjectId, PrivateKeyPath);

        double initialConnectIntervalMillis = 0.5;
        double maxConnectIntervalMillis = 6;
        double maxConnectRetryTimeElapsedMillis = 900;
        double intervalMultiplier = 1.5;

        double retryIntervalMs = initialConnectIntervalMillis;
        double totalRetryTimeMs = 0;

        // Both connect and publish operations may fail. If they do,
        // allow retries but with an exponential backoff time period.
        while (!_mqttClient.IsConnected &&
            totalRetryTimeMs < maxConnectRetryTimeElapsedMillis)
        {
            try
            {
                // Connect to the Google MQTT bridge.
                _mqttClient.Connect(_mqttClientId, "unused", _jwtpassword);
                _mqttClient.ConnectionClosed += _mqttClient_ConnectionClosed;
                _mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;
                _mqttClient.MqttMsgPublished += _mqttClient_MqttMsgPublished;
                _mqttClient.MqttMsgSubscribed += _mqttClient_MqttMsgSubscribed;
                _mqttClient.MqttMsgUnsubscribed += _mqttClient_MqttMsgUnsubscribed;
                
            }
            catch (AggregateException aggExceps)
            {
                printExceptions(aggExceps);
                Console.WriteLine("Retrying in " + retryIntervalMs
                    + " seconds.");
                System.Threading.Thread.Sleep((int)retryIntervalMs);
                totalRetryTimeMs += retryIntervalMs;
                retryIntervalMs *= intervalMultiplier;
                if (retryIntervalMs > maxConnectIntervalMillis)
                {
                    retryIntervalMs = maxConnectIntervalMillis;
                }
            }
            catch (uPLibrary.Networking.M2Mqtt.Exceptions.MqttCommunicationException mqttComException)
            {
                Console.WriteLine(mqttComException.Message);
            }
        }
        //Finally subscribe to command and config topics
        SetupMqttTopics();
    }


    public static void DisconnectFromBridge()
    {
        if(_mqttClient.IsConnected)
        {
            _mqttClient.Disconnect();
        }
    }

    private static object SetupMqttTopics()
    {
        // The configuration topic is used for acknowledged changes.
        string mqttConfigTopic = $"/devices/{SelectedDeviceId}/config";
        // The commands topic is used for frequent, transitory, updates.
        string mqttCommandTopic = $"/devices/{SelectedDeviceId}/commands/#";
        string mqttErrorTopic = $"/devices/{SelectedDeviceId}/errors";

        string[] topics = new string[] {
                mqttConfigTopic,
                mqttCommandTopic,
                mqttErrorTopic
            };

        byte[] qosLevels = new byte[] {
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, // config topic, Qos *1*
                MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, // command topic, Qos 0
                MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE // error topic, Qos 0
            };
        Console.WriteLine("Subscribing to {0}", mqttCommandTopic);

        _mqttClient.Unsubscribe(topics);
        _mqttClient.Subscribe(topics, qosLevels);
        
        return 0;
    }

    internal static void PublishEvent(string? eventMessage,int numEvents)
    {
        if(!_mqttClient.IsConnected)
        {
            SetupMqttClient();
            ConnectToMQTTBridge();
        }

        // Publish to the events or state topic based on the flag.
        string sub_topic = "events";

        // The MQTT topic that this device will publish telemetry data to.
        // The MQTT topic name is required to be in the format below.
        // Note that this is not the same as the device registry's
        // Cloud Pub/Sub topic.
        string mqttTopic = $"/devices/{SelectedDeviceId}/{sub_topic}";

        Console.WriteLine("Publishing Events..");

        for (var i = 1; i <= numEvents; ++i)
        {
            Console.Write(".");
            string payload = $"{i}.Event '{ eventMessage }' From {SelectedRegionId}-{SelectedRegistryId}-{SelectedDeviceId}";
            var BinaryData = Encoding.Unicode.GetBytes(payload);
            // Publish "payload" to the MQTT topic. qos=1 means at least
            // once delivery. Cloud IoT Core also supports qos=0 for at
            // most once delivery.
            _mqttClient.Publish(mqttTopic, BinaryData,
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
             // Send telemetry events every second
             System.Threading.Thread.Sleep(1000);
        }
    }

    internal static void PublishDeviceState(string? stateMessage, int numEvents)
    {
        if (!_mqttClient.IsConnected)
        {
            SetupMqttClient();
            ConnectToMQTTBridge();
        }

        // Publish to the events or state topic based on the flag.
        string sub_topic = "state";

        // The MQTT topic that this device will publish telemetry data to.
        // The MQTT topic name is required to be in the format below.
        // Note that this is not the same as the device registry's
        // Cloud Pub/Sub topic.
        string mqttTopic = $"/devices/{SelectedDeviceId}/{sub_topic}";

        Console.WriteLine("Publishing States..");

        for (var i = 1; i <= numEvents; ++i)
        {
            Console.Write(".");
            string payload = $"{i}.Event '{ stateMessage }' From {SelectedRegionId}-{SelectedRegistryId}-{SelectedDeviceId}";
            var BinaryData = Encoding.Unicode.GetBytes(payload);

            // Publish "payload" to the MQTT topic. qos=1 means at least
            // once delivery. Cloud IoT Core also supports qos=0 for at
            // most once delivery.
            _mqttClient.Publish(mqttTopic, BinaryData,
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            // Send telemetry events every second

            System.Threading.Thread.Sleep(1000);
        }
    }


    internal static void printExceptions(AggregateException exceps)
    {
        exceps.Handle((ex) =>
        {
            if (ex is MqttClientException)
            {
                Console.WriteLine("Client Exception"
                + ex.InnerException.Message);
            }
            else if (ex is MqttCommunicationException)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine("An error occured {0}",
                        ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine("An error occured {0}", ex.Message);
                }
            }
            return false;
        }
        );
    }
    private static void _mqttClient_ConnectionClosed(object sender, EventArgs e)
    {
        /*if (!_mqttClient.IsConnected)
        {
            SetupMqttClient();
            ConnectToMQTTBridge();
        }*/
    }

    private static void _mqttClient_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private static void _mqttClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private static void _mqttClient_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }
    private static void _mqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        // handle message received
        var output = $"Received { Encoding.UTF8.GetString(e.Message)}" +
            $" on topic {e.Topic} with Qos {e.QosLevel}";
        Console.WriteLine(output);
    }
    
}
