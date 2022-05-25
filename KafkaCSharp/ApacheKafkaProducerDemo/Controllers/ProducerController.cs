using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using System.Net;

namespace ApacheKafkaProducerDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProducerController: ControllerBase {
    private readonly string bootstrapServers = "localhost:9092";
    private readonly string topic = "demotopic";

    [HttpPost]
    public async Task <IActionResult> Post([FromBody] OrderRequest orderRequest) {
        string message = JsonSerializer.Serialize(orderRequest);
        return Ok(await SendOrderRequest(topic, message));
    }
    
    private async Task <bool> SendOrderRequest(string topic, string message) {
        ProducerConfig config = new ProducerConfig {
            BootstrapServers = bootstrapServers,
            ClientId = Dns.GetHostName()
        };

        try {
            using(var producer = new ProducerBuilder<Null, string> (config).Build()) {
                var result = await producer.ProduceAsync
                (topic, new Message <Null, string> {Value = message});

                Debug.WriteLine($"Delivery Timestamp:{result.Timestamp.UtcDateTime}");
                return await Task.FromResult(true);
            }
    } catch (Exception ex) {
        Console.WriteLine($"Error occured: {ex.Message}");
    }

    return await Task.FromResult(false);
}
}

public class OrderRequest
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }

