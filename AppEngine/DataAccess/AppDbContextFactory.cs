using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppEngine.DataAccess;

//public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
//{
//    public AppDbContext CreateDbContext(string[] args)
//    {
//        Console.WriteLine("AppDbContextFactory");
//        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer()
//                                                                        .EnableSensitiveDataLogging();

//        return new AppDbContext(optionsBuilder.Options);
//    }
//}