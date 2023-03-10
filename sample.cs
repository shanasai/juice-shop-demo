using System;

using System.Threading.Tasks;

using System.Collections.Generic;

using System.Net;

using Microsoft.Azure.Cosmos;



namespace CosmosGettingStartedDotnetCoreTutorial

{

  public class Program

  {

    private static readonly string EndpointUri = "https://cosmosdb-xyz.documents.azure.com:443/";

    // The primary key for the Azure Cosmos account.

    private static readonly string PrimaryKey = "OLPSLLojzW6BURAmhvboprZudeYHPcOUVDeSUNTVZGii4PaCNbPVdWp51X5cwTkqhkxqpoxPRp1qCCixgimQlA==";



    // The Cosmos client instance

    private CosmosClient cosmosClient;



    // The database we will create

    private Database database;



    // The container we will create.

    private Container container;



    // The name of the database and container we will create

    private string databaseId = "FamilyDatabase";

    private string containerId = "FamilyContainer";



    public static async Task Main(string[] args)

    {

      try

      {

        Console.WriteLine("Beginning operations...\n");

        Program p = new Program();

        await p.GetStartedDemoAsync();

      }

      catch (CosmosException de)

      {

        Exception baseException = de.GetBaseException();

        Console.WriteLine("{0} error occurred: {1}\n", de.StatusCode, de);

      }

      catch (Exception e)

      {

        Console.WriteLine("Error: {0}\n", e);

      }

      finally

      {

        Console.WriteLine("End of demo, press any key to exit.");

        Console.ReadKey();

      }

    }



    /*

      Entry point to call methods that operate on Azure Cosmos DB resources in this sample

    */

    public async Task GetStartedDemoAsync()

    {

      // Create a new instance of the Cosmos Client

      this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

      await this.CreateDatabaseAsync();

      await this.CreateContainerAsync();

      await this.AddItemsToContainerAsync();

      await this.QueryItemsAsync();

      await this.ReplaceFamilyItemAsync();

      await this.DeleteFamilyItemAsync();

      await this.DeleteDatabaseAndCleanupAsync();

    }



    /*

    Create the database if it does not exist

    */

    private async Task CreateDatabaseAsync()

    {

      // Create a new database

      this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

      Console.WriteLine("Created Database: {0}\n", this.database.Id);

    }



    /*

    Create the container if it does not exist. 

    Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.

    */

    private async Task CreateContainerAsync()

    {

      // Create a new container

      this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/LastName");

      Console.WriteLine("Created Container: {0}\n", this.container.Id);

    }



    /*

    Add Family items to the container

    */

    private async Task AddItemsToContainerAsync()

    {

      // Create a family object for the Andersen family

      Family andersenFamily = new Family

      {

        Id = "Andersen.1",

        LastName = "Andersen",

        Parents = new Parent[]

        {

          new Parent { FirstName = "Thomas" },

          new Parent { FirstName = "Mary Kay" }

        },

        Children = new Child[]

        {

          new Child

          {

            FirstName = "Henriette Thaulow",

            Gender = "female",

            Grade = 5,

            Pets = new Pet[]

            {

              new Pet { GivenName = "Fluffy" }

            }

          }

        },

        Address = new Address { State = "WA", County = "King", City = "Seattle" },

        IsRegistered = false

      };



      try

      {

        // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"

        var andersenFamilyResponse = await this.container.CreateItemAsync<Family>(andersenFamily, new PartitionKey(andersenFamily.LastName));



        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.

        Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);

      }

      catch (Exception)

      {



        Console.WriteLine("Resource already exists");

      }





       

       

      // Read the item to see if it exists. Note ReadItemAsync will not throw an exception if an item does not exist. Instead, we check the StatusCode property off the response object. 

      //ItemResponse<Family> andersenFamilyResponse = await this.container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.LastName));



      //if (andersenFamilyResponse.StatusCode == HttpStatusCode.NotFound)

      //{

         

      //}

      //else

      //{

      //  Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);

      //}



      // Create a family object for the Wakefield family

      Family wakefieldFamily = new Family

      {

        Id = "Wakefield.7",

        LastName = "Wakefield",

        Parents = new Parent[]

        {

          new Parent { FamilyName = "Wakefield", FirstName = "Robin" },

          new Parent { FamilyName = "Miller", FirstName = "Ben" }

        },

        Children = new Child[]

        {

          new Child

          {

            FamilyName = "Merriam",

            FirstName = "Jesse",

            Gender = "female",

            Grade = 8,

            Pets = new Pet[]

            {

              new Pet { GivenName = "Goofy" },

              new Pet { GivenName = "Shadow" }

            }

          },

          new Child

          {

            FamilyName = "Miller",

            FirstName = "Lisa",

            Gender = "female",

            Grade = 1

          }

        },

        Address = new Address { State = "NY", County = "Manhattan", City = "NY" },

        IsRegistered = true

      };

      try

      {

        // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"

        var wakefieldFamilyResponse = await this.container.CreateItemAsync<Family>(wakefieldFamily, new PartitionKey(wakefieldFamily.LastName));



        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.

        Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);

      }

      catch (Exception)

      {



        Console.WriteLine("Resource already exists");

      }







      // Read the item to see if it exists

      //ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>(wakefieldFamily.Id, new PartitionKey(wakefieldFamily.LastName));



      //if (wakefieldFamilyResponse.StatusCode == HttpStatusCode.NotFound)

      //{

         

      //}

      //else

      //{

      //  Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);

      //}

    }



    /*

    Run a query (using Azure Cosmos DB SQL syntax) against the container

    */

    private async Task QueryItemsAsync()

    {

      var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";



      Console.WriteLine("Running query: {0}\n", sqlQueryText);



      QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

      FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);



      List<Family> families = new List<Family>();



      while (queryResultSetIterator.HasMoreResults)

      {

        FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        foreach (Family family in currentResultSet)

        {

          families.Add(family);

          Console.WriteLine("\tRead {0}\n", family);

        }

      }

    }



    /*

    Update an item in the container

    */

    private async Task ReplaceFamilyItemAsync()

    {

      ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>("Wakefield.7", new PartitionKey("Wakefield"));

      var itemBody = wakefieldFamilyResponse.Resource;



      // update registration status from false to true

      itemBody.IsRegistered = true;

      // update grade of child

      itemBody.Children[0].Grade = 6;



      // replace the item with the updated content

      wakefieldFamilyResponse = await this.container.ReplaceItemAsync<Family>(itemBody, itemBody.Id, new PartitionKey(itemBody.LastName));

      Console.WriteLine("Updated Family [{0},{1}]\n. Body is now: {2}\n", itemBody.LastName, itemBody.Id, wakefieldFamilyResponse.Resource);

    }



    /*

    Delete an item in the container

    */

    private async Task DeleteFamilyItemAsync()

    {

      var partitionKeyValue = "Wakefield";

      var familyId = "Wakefield.7";



      // Delete an item. Note we must provide the partition key value and id of the item to delete

      ItemResponse<Family> wakefieldFamilyResponse = await this.container.DeleteItemAsync<Family>(familyId, new PartitionKey(partitionKeyValue));

      Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, familyId);

    }



    /*

    Delete the database and dispose of the Cosmos Client instance

    */

    private async Task DeleteDatabaseAndCleanupAsync()

    {

      DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();

      // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();



      Console.WriteLine("Deleted Database: {0}\n", this.databaseId);



      //Dispose of CosmosClient

      this.cosmosClient.Dispose();

    }

  }

}
