using System;

namespace Antlr4.Runtime;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class RuleDependencyAttribute : Attribute
{
	private readonly Type _recognizer;

	private readonly int _rule;

	private readonly int _version;

	private readonly Dependents _dependents;

	public Type Recognizer => _recognizer;

	public int Rule => _rule;

	public int Version => _version;

	public Dependents Dependents => _dependents;

	public RuleDependencyAttribute(Type recognizer, int rule, int version)
	{
		_recognizer = recognizer;
		_rule = rule;
		_version = version;
		_dependents = Dependents.Self | Dependents.Parents;
	}

	public RuleDependencyAttribute(Type recognizer, int rule, int version, Dependents dependents)
	{
		_recognizer = recognizer;
		_rule = rule;
		_version = version;
		_dependents = dependents | Dependents.Self;
	}
}
