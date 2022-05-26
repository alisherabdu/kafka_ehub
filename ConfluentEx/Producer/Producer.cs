using Confluent.Kafka;
using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using Microsoft.Extensions.Configuration;


class Producer {

    static void Main(string[] args)
    {
        ProduceMessageToAzureEventHubWithAvro(args).Wait();
        //ProduceMessageToConfluentCloudCluster(args);
    }
    
    static async Task ProduceMessageToAzureEventHubWithAvro(string[] args)
    {
        
        IConfiguration configuration = new ConfigurationBuilder()
            .AddIniFile(args[0])
            .Build();
        
        string tenantId = configuration.GetValue<string>("azure.tenantid");
        string clientId = configuration.GetValue<string>("azure.clientid");
        string clientSecret = configuration.GetValue<string>("azure.clientsecret");
        string schemaName = configuration.GetValue<string>("eventhub.schema");
        string connectionString = configuration.GetValue<string>("eventhub.connectionstring");
        string endpoint = configuration.GetValue<string>("eventhub.endpoint");
        
        var credentials = new ClientSecretCredential(tenantId,clientId,clientSecret);
        var client = new SchemaRegistryClient(endpoint, credentials);
        var serializer = new SchemaRegistryAvroSerializer(client, schemaName, new SchemaRegistryAvroSerializerOptions { AutoRegisterSchemas = true });
        
        var employee = new Employee { Age = 42, Name = "Caketown" };
        EventData eventData = (EventData) await serializer.SerializeAsync(employee, messageType: typeof(EventData));
        
        // the schema Id will be included as a parameter of the content type
        Console.WriteLine(eventData.ContentType);
        
        // the serialized Avro data will be stored in the EventBody
        Console.WriteLine(eventData.EventBody);
 
        //await using var producer = new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, credential);
        await using var producer = new EventHubProducerClient(connectionString);
        await producer.SendAsync(new EventData[] { eventData });
        
    }


    //can be used for AZURE Event Hub also by just changing the properties file
    static void ProduceMessageToConfluentCloudCluster(string[] args)
    {
        if (args.Length != 1) {
            Console.WriteLine("Please provide the configuration file path as a command line argument");
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddIniFile(args[0])
            .Build();

        string topic = "test_topic";

        string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
        string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

        using (var producer = new ProducerBuilder<string, string>(
            configuration.AsEnumerable()).Build())
        {
            var numProduced = 0;
            const int numMessages = 10;
            for (int i = 0; i < numMessages; ++i)
            {
                Random rnd = new Random();
                var user = users[rnd.Next(users.Length)];
                var item = items[rnd.Next(items.Length)];

                producer.Produce(topic, new Message<string, string> { Key = user, Value = item },
                    (deliveryReport) =>
                    {
                        if (deliveryReport.Error.Code != ErrorCode.NoError) {
                            Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                        }
                        else {
                            Console.WriteLine($"Produced event to topic {topic}: key = {user,-10} value = {item}");
                            numProduced += 1;
                        }
                    });
            }

            producer.Flush(TimeSpan.FromSeconds(10));
            Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
        }
    }
}