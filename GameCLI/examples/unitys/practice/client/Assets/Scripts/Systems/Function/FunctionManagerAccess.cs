using System;
using Config;
using Oathx.Entity;

/// <summary>Resolves runtime types and delegates availability checks to the entity.</summary>
public static class FunctionManagerAccess
{
    /// <summary>Resolve the configured type without constructing a business system.</summary>
    /// <param name="row">The configured function.</param>
    /// <returns>A concrete runtime system type, or null for presentation-only entries.</returns>
    public static Type ResolveType(Config.FunctionSystem row)
    {
        if (row == null || string.IsNullOrWhiteSpace(row.ClassType))
            return null;

        var type = typeof(RuntimeSystem).Assembly.GetType(row.ClassType);

        return type != null && !type.IsAbstract && typeof(RuntimeSystem).IsAssignableFrom(type) ? type : null;
    }

    /// <summary>Check the configured gate for a requested runtime system.</summary>
    /// <param name="type">The target runtime type.</param>
    /// <returns>True for available systems and infrastructure that owns availability itself.</returns>
    public static bool CanActivate(Type type)
    {
        if (type == typeof(MasterSystem) || type == typeof(FunctionManagerSystem) || type == typeof(RedDotSystem))
            return true;

        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!entity)
            return false;

        foreach (var row in Excel.functionSystemConfig.Values)
        {
            if (ResolveType(row) == type && type != null)
                return entity.IsOpen(row.ID);
        }

        return false;
    }
}
