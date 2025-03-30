using System.Reflection;

namespace AppEngine.Types;

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetTypesImplementing(this IEnumerable<Assembly> assemblies, Type serviceType)
    {
        var serviceTypeLocal = serviceType;

        return from assembly in assemblies.Distinct()
            where !assembly.IsDynamic
            from type in assembly.GetTypes()
            where type.IsConcreteType()
            //where options2.IncludeGenericTypeDefinitions || !type.IsGenericTypeDefinition()
            where TypeExtensions.ServiceIsAssignableFromImplementation(serviceTypeLocal, type)
            //let ctor = SelectImplementationTypeConstructorOrNull(type)
            //where (object)ctor == null || options2.IncludeDecorators || !Types.IsDecorator(serviceType2, ctor)
            //where (object)ctor == null || options2.IncludeComposites || !Types.IsComposite(serviceType2, ctor)
            select type;
    }
}