namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MarkupExtensionReturnTypeAttribute : Attribute
{
	private Type _returnType;

	private Type _expressionType;

	public Type ReturnType => _returnType;

	[Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
	public Type ExpressionType => _expressionType;

	public MarkupExtensionReturnTypeAttribute(Type returnType)
	{
		_returnType = returnType;
	}

	[Obsolete("The expressionType argument is not used by the XAML parser. To specify the expected return type, use MarkupExtensionReturnTypeAttribute(Type). To specify custom handling for expression types, use XamlSetMarkupExtensionAttribute.")]
	public MarkupExtensionReturnTypeAttribute(Type returnType, Type expressionType)
	{
		_returnType = returnType;
		_expressionType = expressionType;
	}

	public MarkupExtensionReturnTypeAttribute()
	{
	}
}
