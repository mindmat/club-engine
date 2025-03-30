var builder = DistributedApplication.CreateBuilder(args);

//var cs = builder.AddConnectionString("devdb");

var sql = builder.AddSqlServer("sql",port: 62422)
                 //.WithConnectionStringRedirection(cs.Resource);
                 .WithDataBindMount(source: @"C:\SqlServer\Data")
                 .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("database", "club-engine");

var migrations = builder.AddProject<Projects.ClubEngine_MigrationService>("migrations")
                        .WithReference(db)
                        .WaitFor(db);

var apiService = builder.AddProject<Projects.ClubEngine_ApiService>("api")
                        .WithReference(db)
                        .WaitFor(db)
                        .WaitFor(migrations);


//builder.AddNpmApp()

builder.Build().Run();