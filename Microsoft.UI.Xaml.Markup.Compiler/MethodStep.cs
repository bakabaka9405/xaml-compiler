using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class MethodStep : BindPathStep
{
	private MemberInfo[] methodInfos;

	private readonly IList<Parameter> parameters;

	public override bool IsIncludedInUpdate => false;

	public override string UniqueName { get; }

	public override XamlType ValueType { get; }

	public bool IsOverloaded { get; }

	public bool IsStatic { get; }

	public string MethodName { get; }

	public XamlType OwnerType { get; }

	public IReadOnlyList<Parameter> Parameters => parameters as IReadOnlyList<Parameter>;

	public MethodStep(MemberInfo[] memberInfos, XamlType ownerType, BindPathStep parent, ApiInformation apiInformation)
		: this(memberInfos, 0u, ownerType, parent, apiInformation)
	{
	}

	private MethodStep(MemberInfo[] memberInfos, uint selectedOverload, XamlType ownerType, BindPathStep parent, ApiInformation apiInformation)
		: base(null, parent, apiInformation)
	{
		if (memberInfos.Length < 1)
		{
			throw new ArgumentException("methodInfos must not be empty");
		}
		MethodInfo methodInfo = memberInfos[selectedOverload] as MethodInfo;
		methodInfos = memberInfos;
		MethodName = methodInfo.Name;
		IsOverloaded = memberInfos.Length > 1;
		IsStatic = methodInfo.IsStatic;
		OwnerType = ownerType;
		ValueType = OwnerType.SchemaContext.GetXamlType(methodInfo.ReturnType);
		UniqueName = string.Format("M_{0}{1}", MethodName, (selectedOverload == 0) ? "" : selectedOverload.ToString());
		parameters = (from p in methodInfo.GetParameters()
			select new Parameter(p)).ToList();
	}

	public MethodStep GetOverload(int parameterCount)
	{
		for (uint num = 0u; num < methodInfos.Length; num++)
		{
			MethodInfo methodInfo = methodInfos[num] as MethodInfo;
			if (methodInfo.GetParameters().Length == parameterCount)
			{
				return new MethodStep(methodInfos, num, OwnerType, base.Parent, base.ApiInformation);
			}
		}
		throw new ArgumentException();
	}
}
