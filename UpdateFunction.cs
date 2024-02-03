using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System;

namespace CosmosDbCrudFunctions
{
    public static class UpdateFunction
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDbEndpointUri");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");
        private static readonly string DatabaseName = "ToDoList";
        private static readonly string ContainerName = "Items";
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        [Function("UpdateItem")]
        public static async Task<HttpResponseData> UpdateItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "items/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateItem");
            logger.LogInformation($"Updating item with ID: {id}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            data.id = id; // Ensure the ID is set correctly for the item to be updated

            Container container = cosmosClient.GetContainer(DatabaseName, ContainerName);
            var responseItem = await container.UpsertItemAsync(data, new PartitionKey(id));

            var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            okResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Convert the object to a JSON string.
            string jsonResponse = JsonConvert.SerializeObject(responseItem.Resource);

            // Use the static type for the body content.
            await okResponse.WriteStringAsync(jsonResponse);
            return okResponse;
        }
    }
}
