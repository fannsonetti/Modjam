using System.Reflection;
using S1API.Items;

namespace MoreWeapons.Utils;

internal static class S1ApiHelpers
{
    internal static ScheduleOne.Equipping.Equippable ResolveEquippable(Equippable wrapper)
    {
        if (wrapper == null)
            return null;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var property in wrapper.GetType().GetProperties(flags))
        {
            if (typeof(ScheduleOne.Equipping.Equippable).IsAssignableFrom(property.PropertyType))
                return property.GetValue(wrapper) as ScheduleOne.Equipping.Equippable;
        }

        foreach (var field in wrapper.GetType().GetFields(flags))
        {
            if (typeof(ScheduleOne.Equipping.Equippable).IsAssignableFrom(field.FieldType))
                return field.GetValue(wrapper) as ScheduleOne.Equipping.Equippable;
        }

        return null;
    }

    internal static StorableItemDefinition WrapDefinition(ScheduleOne.ItemFramework.StorableItemDefinition definition)
    {
        if (definition == null)
            return null;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var constructor in typeof(StorableItemDefinition).GetConstructors(flags))
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(definition))
                return constructor.Invoke(new object[] { definition }) as StorableItemDefinition;
        }

        foreach (var method in typeof(StorableItemDefinition).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!method.Name.Contains("Wrap") && !method.Name.Contains("From"))
                continue;

            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(definition))
                return method.Invoke(null, new object[] { definition }) as StorableItemDefinition;
        }

        return null;
    }
}
