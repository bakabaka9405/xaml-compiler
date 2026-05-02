using System;

namespace Antlr4.Runtime.Misc;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
public sealed class NullableAttribute : Attribute
{
}
