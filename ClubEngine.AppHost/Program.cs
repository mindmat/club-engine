var builder = DistributedApplication.CreateBuilder(args);

//var cs = builder.AddConnectionString("devdb");

//SetupLocalDev(builder);
SetupProdDev(builder);

//builder.AddNpmApp()

builder.Build()
       .Run();

return;

void SetupLocalDev(IDistributedApplicationBuilder distributedApplicationBuilder)
{
    var sql = distributedApplicationBuilder.AddSqlServer("sql", port: 62422)
                                           //.WithConnectionStringRedirection(cs.Resource);
                                           .WithDataBindMount(source: @"C:\SqlServer\Data")
                                           .WithLifetime(ContainerLifetime.Persistent);

    var db = sql.AddDatabase("database", "club-engine");

    var insights = distributedApplicationBuilder.AddAzureApplicationInsights("club-engine-insights");

    var migrations = distributedApplicationBuilder.AddProject<Projects.ClubEngine_MigrationService>("migrations")
                                                  .WithReference(db)
                                                  .WithReference(insights)
                                                  .WaitFor(db);

    var serviceBus = distributedApplicationBuilder.AddAzureServiceBus("service-bus")
                                                  .RunAsEmulator();
    var queue = serviceBus.AddServiceBusQueue("CommandQueue");

//var keyVault = builder.AddAzureKeyVault("key-vault");

    var apiService = distributedApplicationBuilder.AddProject<Projects.ClubEngine_ApiService>("api")
                                                  .WithReference(db)
                                                  .WaitFor(db)
                                                  .WaitFor(migrations)
                                                  .WithReference(serviceBus)
                                                  .WaitFor(queue)
                                                  .WithReference(insights);
//.WithReference(keyVault)
//.WaitFor(keyVault);
}

void SetupProdDev(IDistributedApplicationBuilder distributedApplicationBuilder)
{
    var dbConnectionString = builder.AddConnectionString("database");
    var serviceBusConnectionString = builder.AddConnectionString("messaging");

    //var insights = distributedApplicationBuilder.AddAzureApplicationInsights("club-engine-insights");

    var migrations = distributedApplicationBuilder.AddProject<Projects.ClubEngine_MigrationService>("migrations")
                                                  .WithReference(dbConnectionString);

//var keyVault = builder.AddAzureKeyVault("key-vault");

    var apiService = distributedApplicationBuilder.AddProject<Projects.ClubEngine_ApiService>("api")
                                                  .WithReference(dbConnectionString)
                                                  .WaitFor(migrations)
                                                  .WithReference(serviceBusConnectionString);
    //.WithReference(insights);
//.WithReference(keyVault)
//.WaitFor(keyVault);
    ;
}