using System.Reflection;

using AppEngine.Accounting.Assignments;
using AppEngine.Accounting.Iso20022.Camt;
using AppEngine.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace AppEngine.Accounting;

public class AccountingRegistrar : ITypeRegistrar
{
    public void Register(IServiceCollection services, Assembly[] assemblies)
    {
        services.AddScoped<Camt053Parser>();

        services.AddTypesImplementingAsScoped<IPaymentAssignmentSource>(assemblies);
    }
}