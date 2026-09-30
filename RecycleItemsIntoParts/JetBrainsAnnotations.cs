using System;

namespace JetBrains.Annotations
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class PublicAPIAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class UsedImplicitlyAttribute : Attribute
    {
    }
}
