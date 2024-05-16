using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Authentication;


string connectionString =
  Environment.GetEnvironmentVariable(@"ConnectionString");
MongoClientSettings settings = MongoClientSettings.FromUrl(
  new MongoUrl(connectionString)
);
settings.SslSettings =
  new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
var mongoClient = new MongoClient(settings);


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IMongoClient>(mongoClient);
    })
    .Build();

host.Run();
