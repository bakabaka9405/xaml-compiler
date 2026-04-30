using System;

namespace Antlr4.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class RuleVersionAttribute : Attribute
{
	private readonly int _version;

	public int Version => _version;

	public RuleVersionAttribute(int version)
	{
		_version = version;
	}
}
