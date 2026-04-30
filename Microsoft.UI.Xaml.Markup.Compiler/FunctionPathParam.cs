using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionPathParam : FunctionParam
{
	public BindPathStep Path { get; }

	public override string UniqueName => Path.UniqueName;

	public override string CodeName => Path.CodeName;

	public override bool HasTryGetValue => true;

	public override string TryGetValueCodeName => Path.TryGetValueCodeName;

	public override XamlType AssignmentType => Path.ValueType;

	public FunctionPathParam(BindPathStep value)
	{
		Path = value;
	}

	protected override void ValidateParameter(Parameter paramInfo)
	{
	}
}
