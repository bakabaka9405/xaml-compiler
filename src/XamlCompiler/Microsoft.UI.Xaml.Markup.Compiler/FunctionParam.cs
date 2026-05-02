using System;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public abstract class FunctionParam : Parameter
{
	public XamlType ValueType { get; private set; }

	public abstract string UniqueName { get; }

	public virtual bool HasTryGetValue => false;

	public virtual string TryGetValueCodeName
	{
		get
		{
			throw new NotImplementedException("This step does not have a TryGetValueCodeName. Should it?");
		}
	}

	public virtual string CodeName => UniqueName;

	public virtual XamlType AssignmentType => ValueType;

	protected abstract void ValidateParameter(Parameter paramInfo);

	public void SetParameterInfo(Parameter paramInfo, XamlType valueType)
	{
		if (valueType == null)
		{
			throw new ArgumentException("FunctionParam valueType cannot be null", "valueType");
		}
		ValidateParameter(paramInfo);
		CopyFrom(paramInfo);
		ValueType = valueType;
	}
}
