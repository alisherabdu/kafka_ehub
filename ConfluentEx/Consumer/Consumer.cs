using Confluent.Kafka;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;

class Consumer {

    static void Main(string[] args)
    {
        //ConsumeMessageToAzureEventHubWithAvro(args).Wait();
        ConsumeMessageToConfluentCloudCluster(args);
    }

    static async Task ConsumeMessageToAzureEventHubWithAvro(string[] args)
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
        var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;//"customgroupname";
        
        var credentials = new ClientSecretCredential(tenantId,clientId,clientSecret);
        var client = new SchemaRegistryClient(endpoint, credentials);
        var serializer = new SchemaRegistryAvroSerializer(client, schemaName, new SchemaRegistryAvroSerializerOptions { AutoRegisterSchemas = true });

        //await using var consumer = new EventHubConsumerClient(consumerGroup, fullyQualifiedNamespace, eventHubName, credential);
        await using var consumer = new EventHubConsumerClient(consumerGroup,connectionString);
        await foreach (PartitionEvent receivedEvent in consumer.ReadEventsAsync())
        {
            Console.WriteLine(receivedEvent.Data.ContentType);
                Employee deserialized =
                    (Employee) await serializer.DeserializeAsync(receivedEvent.Data, typeof(Employee),
                        CancellationToken.None);
                Console.WriteLine(deserialized.Age);
                Console.WriteLine(deserialized.Name);
                //break;
        }
    }
    static void ConsumeMessageToConfluentCloudCluster(string[] args)
    {
        if (args.Length != 1) {
            Console.WriteLine("Please provide the configuration file path as a command line argument");
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddIniFile(args[0])
            .Build();
        //configuration["auto.offset.reset"] = "earliest";

        const string topic = "test_topic";

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        using (var consumer = new ConsumerBuilder<string, string>(
            configuration.AsEnumerable()).Build())
        {
            consumer.Subscribe(topic);
            try {
                while (true) {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                }
            }
            catch (OperationCanceledException) {
                // Ctrl-C was pressed.
            }
            finally{
                consumer.Close();
            }
        }
    }
}