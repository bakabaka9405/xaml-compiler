using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionStep : BindPathStep
{
	private readonly string paramsHashCode;

	public List<FunctionParam> Parameters { get; }

	public MethodStep Method { get; }

	public override string UniqueName => $"{Method.UniqueName}_{paramsHashCode}";

	public bool RequiresSafeParameterRetrieval => true;

	public override bool ValueTypeIsConditional
	{
		get
		{
			if (!base.ValueTypeIsConditional)
			{
				return (from p in Parameters.OfType<FunctionPathParam>()
					where p.Path.ValueTypeIsConditional
					select p).Any();
			}
			return true;
		}
	}

	public override bool IsValueRequired => (from p in Parameters.OfType<FunctionPathParam>()
		where p.Path.IsValueRequired
		select p).Any();

	public FunctionStep(MethodStep method, ApiInformation apiInformation)
		: base(method.ValueType, method.Parent, apiInformation)
	{
		Method = method;
		Parameters = new List<FunctionParam>();
		paramsHashCode = "";
	}

	public FunctionStep(MethodStep method, IEnumerable<FunctionParam> parameters, ApiInformation apiInformation)
		: this(method, apiInformation)
	{
		Parameters.AddRange(parameters);
		paramsHashCode = ((uint)string.Concat(Parameters.Select((FunctionParam p) => p.CodeName).ToArray()).GetHashCode()).ToString();
		foreach (FunctionPathParam item in Parameters.OfType<FunctionPathParam>())
		{
			item.Path.AddDependent(this);
		}
	}
}
