using System;
using System.Linq.Expressions;
using System.Reflection;
using Oathx.Entity;

/// <summary>Builds lowercase storage keys from system, entity and member metadata.</summary>
public static class StorageKey
{
    /// <summary>Build a key for a direct public entity property or field.</summary>
    /// <typeparam name="TSystem">Owning system whose name ends in System.</typeparam>
    /// <typeparam name="TEntity">Entity that owns the stored member.</typeparam>
    /// <param name="member">Direct member selector, without evaluating its value.</param>
    /// <returns>The lowercase system.class.member key.</returns>
    public static string For<TSystem, TEntity>(Expression<Func<TEntity, object>> member)
        where TSystem : RuntimeSystem
        where TEntity : XEntity
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        Expression body = member.Body;
        if (body is UnaryExpression conversion && conversion.NodeType == ExpressionType.Convert)
            body = conversion.Operand;

        if (!(body is MemberExpression access) || access.Expression != member.Parameters[0])
            throw new ArgumentException("Select a direct entity property or field.", nameof(member));

        bool valid = access.Member is PropertyInfo property && property.GetMethod != null
            && property.GetMethod.IsPublic && !property.GetMethod.IsStatic
            || access.Member is FieldInfo field && field.IsPublic && !field.IsStatic;
        if (!valid)
            throw new ArgumentException("Select a public instance property or field.", nameof(member));

        string systemName = typeof(TSystem).Name;
        const string suffix = "System";
        if (!systemName.EndsWith(suffix, StringComparison.Ordinal) || systemName.Length == suffix.Length)
            throw new ArgumentException("The owning system type must end in System.");

        systemName = systemName.Substring(0, systemName.Length - suffix.Length);

        return string.Concat(systemName, ".", typeof(TEntity).Name, ".", access.Member.Name).ToLowerInvariant();
    }
}
