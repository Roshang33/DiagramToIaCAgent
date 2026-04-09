using Azure.Data.Tables;

namespace TerraformAgent.Persistence.Storage;

public static class TableStorageFactory
{
    public static TableClient Create(string connectionString, string tableName)
    {
        var serviceClient = new TableServiceClient(connectionString);

        var tableClient = serviceClient.GetTableClient(tableName);

        tableClient.CreateIfNotExists();

        return tableClient;
    }
}