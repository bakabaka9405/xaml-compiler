using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BoundEventAssignment : BindAssignmentBase
{
	private IEnumerable<Parameter> parameters;

	public IEnumerable<Parameter> Parameters
	{
		get
		{
			if (parameters == null)
			{
				parameters = MemberType.UnderlyingType.GetInvokeParameters();
			}
			return parameters;
		}
	}

	public string EventHandlerCodeName => base.ConnectionIdElement.ObjectCodeName + base.MemberName;

	public BoundEventAssignment(XamlDomMember bindMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
		: base(bindMember, bindUniverse, connectionIdElement)
	{
	}

	public IEnumerable<XamlCompileErrorBase> ParsePath()
	{
		List<XamlCompileErrorBase> list = new List<XamlCompileErrorBase>();
		try
		{
			List<string> list2 = new List<string>();
			base.PathStep = ParseBindPath(list2);
			foreach (string item in list2)
			{
				list.Add(new XamlXBindParseWarning(bindItem, item));
			}
		}
		catch (CompiledBindingParseException ex)
		{
			list.Add(new XamlXBindParseError(bindItem, ex));
			return list;
		}
		ValidateEvent(list, base.PathStep);
		return list;
	}

	private bool ValidateEvent(IList<XamlCompileErrorBase> issues, BindPathStep leafStep)
	{
		if (leafStep.Parent != null)
		{
			foreach (BindPathStep item in leafStep.Parent.ParentsAndSelf)
			{
				if (item is MethodStep)
				{
					issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BoundEventAssignment_NonLeafMethod, ((MethodStep)item).MethodName)));
					return false;
				}
			}
		}
		if (leafStep is PropertyStep || leafStep is RootNamedElementStep)
		{
			if (leafStep.ValueType.IsDelegate() && leafStep.ValueType.Name == MemberType.Name)
			{
				return true;
			}
			issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BoundEventAssignment_NonDelegateProperty, base.MemberName, MemberType.Name)));
			return false;
		}
		if (leafStep is MethodStep)
		{
			MethodStep methodStep = leafStep as MethodStep;
			if (methodStep.IsOverloaded)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BoundEventAssignment_NoOverloads)));
				return false;
			}
			MethodInfo method = MemberType.UnderlyingType.GetMethod("Invoke");
			ParameterInfo[] array = method.GetParameters();
			if (methodStep.Parameters.Count != 0 && methodStep.Parameters.Count != array.Length)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BoundEventAssignment_InvalidSignature, base.MemberName)));
				return false;
			}
			for (int i = 0; i < methodStep.Parameters.Count; i++)
			{
				if (array[i].ParameterType != methodStep.Parameters[i].ParameterType && !array[i].ParameterType.IsSubclassOf(methodStep.Parameters[i].ParameterType))
				{
					issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BoundEventAssignment_SignatureMismatch, base.MemberName, i, methodStep.Parameters[i].ParameterType.Name, array[i].ParameterType.Name)));
					return false;
				}
			}
		}
		return true;
	}
}
