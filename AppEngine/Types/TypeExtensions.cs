using System.Reflection;

namespace AppEngine.Types;

public static class TypeExtensions
{
    public static bool IsConcreteType(this Type serviceType)
    {
        if (!serviceType.GetTypeInfo().IsAbstract && !serviceType.IsArray && serviceType != typeof(object))
        {
            return !typeof(Delegate).IsAssignableFrom(serviceType);
        }

        return false;
    }


    internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
    {
        if (service.IsAssignableFrom(implementation))
        {
            return true;
        }

        if (service.IsGenericTypeDefinitionOf(implementation))
        {
            return true;
        }

        var interfaces = implementation.GetInterfaces();

        foreach (var t in interfaces)
        {
            if (IsGenericImplementationOf(t, service))
            {
                return true;
            }
        }

        var type = implementation.GetTypeInfo().BaseType ?? ((implementation != typeof(object)) ? typeof(object) : null);

        while (type != null)
        {
            if (IsGenericImplementationOf(type, service))
            {
                return true;
            }

            type = type.GetTypeInfo().BaseType;
        }

        return false;
    }

    private static bool IsGenericImplementationOf(Type type, Type serviceType)
    {
        if (!(type == serviceType) && !serviceType.IsVariantVersionOf(type))
        {
            if (type.GetTypeInfo().IsGenericType && serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return type.GetGenericTypeDefinition() == serviceType;
            }

            return false;
        }

        return true;
    }

    private static bool IsVariantVersionOf(this Type type, Type otherType)
    {
        if (type.GetTypeInfo().IsGenericType && otherType.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition())
        {
            return type.IsAssignableFrom(otherType);
        }

        return false;
    }

    internal static bool IsGenericTypeDefinitionOf(this Type genericTypeDefinition, Type typeToCheck)
    {
        if (typeToCheck.GetTypeInfo().IsGenericType)
        {
            return typeToCheck.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        return false;
    }

    public static bool ImplementsInterface<TInterface>(this Type type)
    {
        return typeof(TInterface).IsAssignableFrom(type);
    }

    public static bool HasAttribute<TAttribute>(this Type type)
        where TAttribute : Attribute
    {
        return type.GetCustomAttribute<TAttribute>() != null;
    }
}