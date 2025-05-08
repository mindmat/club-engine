var builder = DistributedApplication.CreateBuilder(args);

//var cs = builder.AddConnectionString("devdb");

var sql = builder.AddSqlServer("sql", port: 62422)
                 //.WithConnectionStringRedirection(cs.Resource);
                 .WithDataBindMount(source: @"C:\SqlServer\Data")
                 .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("database", "club-engine");

var migrations = builder.AddProject<Projects.ClubEngine_MigrationService>("migrations")
                        .WithReference(db)
                        .WaitFor(db);

var serviceBus = builder.AddAzureServiceBus("service-bus")
                        .RunAsEmulator();
var queue = serviceBus.AddServiceBusQueue("CommandQueue");

//var keyVault = builder.AddAzureKeyVault("key-vault");

var apiService = builder.AddProject<Projects.ClubEngine_ApiService>("api")
                        .WithReference(db)
                        .WaitFor(db)
                        .WaitFor(migrations)
                        .WithReference(serviceBus)
                        .WaitFor(queue);
//.WithReference(keyVault)
//.WaitFor(keyVault);


//builder.AddNpmApp()

builder.Build().Run();