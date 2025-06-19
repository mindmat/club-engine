using System.Diagnostics;

using AppEngine.Authentication;
using AppEngine.Authentication.Users;
using AppEngine.Authorization.UsersInPartition;
using AppEngine.DataAccess;

using ClubEngine.ApiService.Clubs;
using ClubEngine.ApiService.Members.Memberships;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Trace;

namespace ClubEngine.MigrationService;

public class Worker(IServiceProvider serviceProvider,
                    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const            string         ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource     = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);

            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Set<Club>().AnyAsync(cancellationToken))
        {
            return;
        }

        var club = new Club
                   {
                       Id = Guid.Parse("AD4D61CD-7EA3-4571-9412-1274834503B6"),
                       Acronym = "swima",
                       Name = "Swingmachine",
                       MembershipTypes =
                       [
                           new MembershipType
                           {
                               Id = Guid.Parse("F750C7B8-4AF6-43EC-897C-38457863788A"),
                               FallbackName = "Aktiv",
                               AnnualFee = 150,
                               Color = "#6ee7b7"
                           },
                           new MembershipType
                           {
                               Id = Guid.Parse("DADBB6D5-F1C5-4B9D-A585-CCA92BAA9E43"),
                               FallbackName = "Passiv",
                               AnnualFee = 150,
                               Color = "#a78bfa"
                           },
                           new MembershipType
                           {
                               Id = Guid.Parse("48AF80FD-E5F0-48AA-8314-97C27C547E92"),
                               FallbackName = "Ehrenmitglied",
                               AnnualFee = 0,
                               Color = "#7dd3fc"
                           },
                       ],
                       Users =
                       [
                           new UserInPartition
                           {
                               Id = Guid.Parse("F7FC36DA-A3B7-40F6-83F9-1BC045422332"),
                               Role = UserInPartitionRole.Admin,
                               User = new User
                                      {
                                          Id = Guid.Parse("7D95B912-7513-4EAE-A010-441941F91578"),
                                          IdentityProvider = IdentityProvider.Auth0,
                                          IdentityProviderUserIdentifier = "google-oauth2|114830392522716751150",
                                          FirstName = "Mathias",
                                          LastName = "Minder",
                                          Email = "mathias.minder@gmail.com",
                                          AvatarUrl = "https://lh3.googleusercontent.com/a/ACg8ocKF15jWv7tIOjXXoqw1Vnt9DSsRrh5MaemRYY2x4Hr8BrTOGA_l=s96-c"
                                      }
                           }
                       ]
                   };

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            dbContext.Set<Club>().Add(club);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}