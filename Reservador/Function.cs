using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Shared.Models;
using Shared;
using System.Text.Json;
using Shared.Extensions;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Reservador;

public class Function
{
    private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

    public Function()
    {
        _amazonDynamoDBClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed message {message.Body}");

        var pedido = JsonSerializer.Deserialize<Pedido>(message.Body);
        pedido!.Status = StatusDoPedido.Reservado;

        foreach (var produto in pedido.Produtos)
        {
            try
            {
                await BaixarEstoque(produto.Id, produto.Quantidade);
                produto.Reservado = true;
                context.Logger.LogLine($"Produto baixado do estoque {produto.Id} - {produto.Nome}");
            }
            catch (ConditionalCheckFailedException)
            {
                pedido.JustificativaDeCancelamento = $"Produto indisponível no estoque {produto.Id} - {produto.Nome}";
                pedido.Cancelado = true;
                context.Logger.LogLine($"Erro: {pedido.JustificativaDeCancelamento}");
                break;
            }
        }

        if (pedido.Cancelado)
        {
            foreach (var produto in pedido.Produtos)
            {
                if (produto.Reservado)
                {
                    await DevolverAoEstoque(produto.Id, produto.Quantidade);
                    produto.Reservado = false;
                    context.Logger.LogLine($"Produto devolvido ao estoque {produto.Id} - {produto.Nome}");
                }
            }

            await AmazonExtension.EnviarParaFila(EnumFilasSNS.falha, pedido);
            await pedido.SalvarAsync();
        }
        else
        {
            await AmazonExtension.EnviarParaFila(EnumFilasSQS.reservado, pedido);
            await pedido.SalvarAsync();
        }
        await Task.CompletedTask;
    }

    private async Task DevolverAoEstoque(string id, int quantidade)
    {
        var request = new UpdateItemRequest
        {
            TableName = "estoque",
            ReturnValues = "NONE",
            Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue{ S = id } }
                },
            UpdateExpression = "SET Quantidade = (Quantidade - :quantidadeDoPedido)",
            ConditionExpression = "Quantidade >= :quantidadeDoPedido",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":quantidadeDoPedido", new AttributeValue { N = quantidade.ToString() } }
                }
        };

        await _amazonDynamoDBClient.UpdateItemAsync(request);
    }

    private async Task BaixarEstoque(string id, int quantidade)
    {
        var request = new UpdateItemRequest
        {
            TableName = "estoque",
            ReturnValues = "NONE",
            Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue{ S = id } }
                },
            UpdateExpression = "SET Quantidade = (Quantidade + :quantidadeDoPedido)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":quantidadeDoPedido", new AttributeValue { N = quantidade.ToString() } }
                }
        };

        await _amazonDynamoDBClient.UpdateItemAsync(request);
    }
}