using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Shared.Models;
using System.Text.Json;

namespace Shared.Extensions
{
    public static class AmazonExtension
    {
        public static RegionEndpoint region => RegionEndpoint.USEast1;
        public static string SqsQueueUrl  => "https://sqs.us-east-1.amazonaws.com/491085419902/";
        public static string SnsQueueUrl => "";
                

        public static async Task SalvarAsync(this Pedido pedido)
        {
            var client = new AmazonDynamoDBClient(region);
            var context = new DynamoDBContext(client);
            await context.SaveAsync(pedido);
        }
        public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
        {
            var client = new AmazonDynamoDBClient(region);
            var context = new DynamoDBContext(client);

            var doc = Document.FromAttributeMap(dictionary);
            return context.FromDocument<T>(doc);
        }

        public static Dictionary<string, AttributeValue> ToDynamoDbAttributes(this Dictionary<string, DynamoDBEvent.AttributeValue> obj)
        {
            var attributes = Document.FromJson(obj.ToJson()).ToAttributeMap();
            return attributes;
        }

        public static async Task EnviarParaFila(EnumFilasSQS fila, Pedido pedido)
        {
            var json = JsonSerializer.Serialize(pedido);
            var client = new AmazonSQSClient(region);
            var request = new SendMessageRequest
            {
                QueueUrl = $"{SqsQueueUrl}{fila}",
                MessageBody = json
            };

            await client.SendMessageAsync(request);
        }

        public static async Task EnviarParaFila(EnumFilasSNS fila, Pedido pedido)
        {
            // Implementar
            await Task.CompletedTask;
        }
    }
}
