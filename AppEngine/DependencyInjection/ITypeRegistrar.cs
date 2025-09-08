using Microsoft.Extensions.DependencyInjection;

namespace AppEngine.DependencyInjection;

public interface ITypeRegistrar
{
    public void Register(IServiceCollection services);
}