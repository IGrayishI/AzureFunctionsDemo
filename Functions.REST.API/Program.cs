using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

const string connectionUri = "Connectionstring";
var settings = MongoClientSettings.FromConnectionString(connectionUri);
// Set the ServerApi field of the settings object to set the version of the Stable API on the client
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
// Create a new client and connect to the server
var client = new MongoClient(settings);

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IMongoClient>(client);
    })
    .Build();

host.Run();
