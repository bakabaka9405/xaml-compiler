using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;
using Antlr4.Runtime.Misc;

namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal class BindingPathListener : BindingPathBaseListener
{
	private readonly string pathExpression;

	private IXamlTypeResolver resolver;

	private IBindUniverse bindUniverse;

	private readonly ApiInformation apiInformation;

	public IList<string> Warnings { get; set; }

	public BindingPathListener([NotNull] string pathExpression, ApiInformation apiInformation, [NotNull] IBindUniverse bindUniverse, [NotNull] IXamlTypeResolver resolver)
	{
		this.apiInformation = apiInformation;
		this.pathExpression = pathExpression;
		this.resolver = resolver;
		this.bindUniverse = bindUniverse;
	}

	public override void ExitPathIdentifier([NotNull] BindingPathParser.PathIdentifierContext context)
	{
		string text = context.IDENTIFIER().GetText();
		BindPathStep step = ResolveNameOnRoot(text);
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathDotIdentifier([NotNull] BindingPathParser.PathDotIdentifierContext context)
	{
		BindPathStep bindPathStep = EnsureNotFunction(context.path().PathStep);
		string text = context.IDENTIFIER().GetText();
		BindPathStep step = ResolveNameOnType(text, bindPathStep.ValueType, bindPathStep);
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathStaticIdentifier([NotNull] BindingPathParser.PathStaticIdentifierContext context)
	{
		string text = context.IDENTIFIER().GetText();
		BindPathStep bindPathStep = CreateStaticRootStep(context.static_type());
		BindPathStep step = ResolveNameOnType(text, bindPathStep.ValueType, bindPathStep);
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathIndexer([NotNull] BindingPathParser.PathIndexerContext context)
	{
		BindPathStep bindPathStep = EnsureNotFunction(context.path().PathStep);
		string text = context.Digits().GetText();
		if (!int.TryParse(text, out var result))
		{
			throw new ParseException(ErrorMessages.ExpectingDigit);
		}
		if (!bindPathStep.ValueType.IsIntegerIndexable())
		{
			throw new ParseException(ErrorMessages.UnexpectedArrayIndexer);
		}
		ArrayIndexStep step = new ArrayIndexStep(result, bindPathStep.ValueType.ItemType, bindPathStep, apiInformation);
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathStringIndexer([NotNull] BindingPathParser.PathStringIndexerContext context)
	{
		BindPathStep bindPathStep = EnsureNotFunction(context.path().PathStep);
		string key = UnescapeAndStripQuotes(context.QuotedString().GetText());
		if (!bindPathStep.ValueType.IsStringIndexable())
		{
			throw new ParseException(ErrorMessages.UnexpectedArrayIndexer);
		}
		MapIndexStep step = new MapIndexStep(key, bindPathStep.ValueType.ItemType, bindPathStep, apiInformation);
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathDotAttached([NotNull] BindingPathParser.PathDotAttachedContext context)
	{
		BindingPathParser.Static_typeContext static_typeContext = context.attached_expr().static_type();
		string text;
		string text2;
		if (static_typeContext != null)
		{
			text = static_typeContext.GetText();
			text2 = context.attached_expr().IDENTIFIER()[0].GetText();
		}
		else
		{
			text = context.attached_expr().IDENTIFIER()[0].GetText();
			text2 = context.attached_expr().IDENTIFIER()[1].GetText();
		}
		XamlType xamlType = resolver.ResolveXmlName(text);
		if (xamlType == null)
		{
			throw new ParseException(ErrorMessages.TypeNotFound, text);
		}
		XamlMember attachableMember = xamlType.GetAttachableMember(text2);
		BindPathStep parent = EnsureNotFunction(context.path().PathStep);
		BindPathStep step;
		if (null != attachableMember && attachableMember.IsNameValid)
		{
			step = new AttachedPropertyStep(text2, attachableMember.Type, xamlType, parent, apiInformation);
		}
		else
		{
			XamlType directPropertyOrFieldType = GetDirectPropertyOrFieldType(xamlType, text2);
			if (!(directPropertyOrFieldType != null))
			{
				throw new ParseException(ErrorMessages.PropertyNotFound, text2, text);
			}
			BindPathStep parent2 = bindUniverse.EnsureUniquePathStep(new CastStep(xamlType, parent, apiInformation));
			PropertyStep propertyStep = new PropertyStep(text2, directPropertyOrFieldType, parent2, apiInformation);
			step = propertyStep;
		}
		context.PathStep = bindUniverse.EnsureUniquePathStep(step);
	}

	public override void ExitPathCastInvalid([NotNull] BindingPathParser.PathCastInvalidContext context)
	{
		throw new ParseException(ErrorMessages.CastCannotStartWithAttachedProperty);
	}

	public override void ExitPathCast([NotNull] BindingPathParser.PathCastContext context)
	{
		context.PathStep = CreateCastStep(context.cast_expr(), bindUniverse.RootStep);
	}

	public override void ExitPathCastPath([NotNull] BindingPathParser.PathCastPathContext context)
	{
		context.PathStep = CreateCastStep(context.cast_expr(), EnsureNotFunction(context.path().PathStep));
	}

	public override void ExitPathCastPathParen([NotNull] BindingPathParser.PathCastPathParenContext context)
	{
		context.PathStep = CreateCastStep(context.cast_expr(), EnsureNotFunction(context.path().PathStep));
	}

	public override void ExitPathFunction([NotNull] BindingPathParser.PathFunctionContext context)
	{
		string text = context.function().IDENTIFIER().GetText();
		BindPathStep bindPathStep = ResolveNameOnRoot(text);
		BindingPathParser.Function_paramContext[] paramArray = context.function().function_param();
		context.PathStep = CreateFunctionStep(bindPathStep as MethodStep, paramArray);
	}

	public override void ExitPathStaticFuction([NotNull] BindingPathParser.PathStaticFuctionContext context)
	{
		BindPathStep bindPathStep = CreateStaticRootStep(context.static_type());
		string text = context.function().IDENTIFIER().GetText();
		BindPathStep bindPathStep2 = ResolveNameOnType(text, bindPathStep.ValueType, bindPathStep);
		BindingPathParser.Function_paramContext[] paramArray = context.function().function_param();
		context.PathStep = CreateFunctionStep(bindPathStep2 as MethodStep, paramArray);
	}

	public override void ExitPathPathToFunction([NotNull] BindingPathParser.PathPathToFunctionContext context)
	{
		BindPathStep bindPathStep = EnsureNotFunction(context.path().PathStep);
		string text = context.function().IDENTIFIER().GetText();
		BindPathStep bindPathStep2 = ResolveNameOnType(text, bindPathStep.ValueType, bindPathStep);
		BindingPathParser.Function_paramContext[] paramArray = context.function().function_param();
		context.PathStep = CreateFunctionStep(bindPathStep2 as MethodStep, paramArray);
	}

	public override void ExitFunctionParameterInvalid([NotNull] BindingPathParser.FunctionParameterInvalidContext context)
	{
		throw new ParseException(ErrorMessages.FunctionAsParameter);
	}

	public override void ExitFunctionParamPath([NotNull] BindingPathParser.FunctionParamPathContext context)
	{
		BindPathStep pathStep = context.path().PathStep;
		context.Param = new FunctionPathParam(pathStep);
	}

	public override void ExitFunctionParamBool([NotNull] BindingPathParser.FunctionParamBoolContext context)
	{
		string text = context.boolean_value().GetText();
		context.Param = new FunctionBoolParam(bool.Parse(text.Replace("x:", string.Empty)));
	}

	public override void ExitFunctionParamNumber([NotNull] BindingPathParser.FunctionParamNumberContext context)
	{
		string text = context.decimal_value().GetText();
		context.Param = new FunctionNumberParam(text);
	}

	public override void ExitFunctionParamString([NotNull] BindingPathParser.FunctionParamStringContext context)
	{
		string value = UnescapeAndStripQuotes(context.QuotedString().GetText());
		context.Param = new FunctionStringParam(value);
	}

	public override void ExitFunctionParamNullValue([NotNull] BindingPathParser.FunctionParamNullValueContext context)
	{
		context.Param = new FunctionNullValueParam();
	}

	private BindPathStep CreateStaticRootStep(BindingPathParser.Static_typeContext static_type)
	{
		string text = static_type.namespace_qualifier()?.GetText();
		string text2 = static_type.IDENTIFIER().GetText();
		if (text != null)
		{
			text2 = text + text2;
		}
		XamlType xamlType = resolver.ResolveXmlName(text2);
		if (xamlType == null)
		{
			throw new ParseException(ErrorMessages.TypeNotFound, text2);
		}
		return bindUniverse.EnsureUniquePathStep(new StaticRootStep(xamlType, apiInformation));
	}

	private BindPathStep CreateCastStep(BindingPathParser.Cast_exprContext castExp, BindPathStep parentStep)
	{
		BindingPathParser.Static_typeContext static_typeContext = castExp.static_type();
		string text = ((static_typeContext != null) ? static_typeContext.GetText() : castExp.IDENTIFIER().GetText());
		XamlType xamlType = resolver.ResolveXmlName(text);
		if (xamlType == null)
		{
			throw new ParseException(ErrorMessages.TypeNotFound, text);
		}
		XamlType valueType = parentStep.ValueType;
		if (!resolver.CanInlineConvert(valueType, xamlType))
		{
			throw new ParseException(ErrorMessages.InvalidCast, valueType, xamlType);
		}
		return bindUniverse.EnsureUniquePathStep(new CastStep(xamlType, parentStep, apiInformation));
	}

	private BindPathStep CreateFunctionStep(MethodStep method, BindingPathParser.Function_paramContext[] paramArray)
	{
		if (method == null)
		{
			throw new ParseException(ErrorMessages.ExpectingMethod);
		}
		List<FunctionParam> list = new List<FunctionParam>();
		foreach (BindingPathParser.Function_paramContext function_paramContext in paramArray)
		{
			list.Add(function_paramContext.Param);
		}
		if (list.Count() != method.Parameters.Count())
		{
			if (!method.IsOverloaded)
			{
				throw new ParseException(ErrorMessages.MissmatchedParameterCount);
			}
			try
			{
				method = method.GetOverload(list.Count());
				method = bindUniverse.EnsureUniquePathStep(method) as MethodStep;
			}
			catch (ArgumentException)
			{
				throw new ParseException(ErrorMessages.NoMatchingOverload, list.Count());
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Parameter parameter = method.Parameters[j];
			FunctionParam functionParam = list[j];
			if (parameter.IsOut || parameter.ParameterType.IsByRef)
			{
				throw new ParseException(ErrorMessages.UnsuportedOutParameter, parameter.Position + 1);
			}
			XamlType xamlType = resolver.ResolveType(parameter.ParameterType);
			try
			{
				functionParam.SetParameterInfo(parameter, xamlType);
			}
			catch (ArgumentException)
			{
				throw new ParseException(ErrorMessages.InvalidParameter, parameter.Position + 1);
			}
			if (parameter is FunctionPathParam functionPathParam && !resolver.CanAssignDirectlyTo(functionPathParam.ValueType, xamlType))
			{
				throw new ParseException(ErrorMessages.InvalidParameter, parameter.Position + 1);
			}
		}
		if (method.Parent is StaticRootStep && !method.IsStatic)
		{
			throw new ParseException(ErrorMessages.ExpectingStaticFunction, method.MethodName, method.OwnerType.Name);
		}
		return bindUniverse.EnsureUniquePathStep(new FunctionStep(method, list, apiInformation));
	}

	private XamlType GetDirectPropertyOrFieldType(XamlType sourceType, string propertyName)
	{
		XamlMember member = sourceType.GetMember(propertyName);
		if (member != null && member.IsNameValid && member.UnderlyingMember.MemberType == MemberTypes.Property)
		{
			return member.Type;
		}
		PropertyInfo property = sourceType.UnderlyingType.GetProperty(propertyName);
		if (property != null)
		{
			return sourceType.SchemaContext.GetXamlType(property.PropertyType);
		}
		FieldInfo field = sourceType.UnderlyingType.GetField(propertyName);
		if (field != null)
		{
			return sourceType.SchemaContext.GetXamlType(field.FieldType);
		}
		return null;
	}

	private static string UnescapeAndStripQuotes(string quotedString)
	{
		string text = quotedString;
		if ((quotedString.StartsWith("'") && quotedString.EndsWith("'")) || (quotedString.StartsWith("\"") && quotedString.EndsWith("\"")))
		{
			text = quotedString.Substring(1, quotedString.Length - 2);
		}
		return text.Replace("^'", "'").Replace("^\"", "\"");
	}

	private BindPathStep EnsureNotFunction(BindPathStep step)
	{
		if (step is FunctionStep)
		{
			throw new ParseException(ErrorMessages.FunctionNotLeaf);
		}
		return step;
	}

	private BindPathStep ResolveNameOnRoot(string name)
	{
		BindPathStep bindPathStep = null;
		BindPathStep bindPathStep2 = null;
		string objectCodeName;
		XamlType namedElementType = bindUniverse.GetNamedElementType(name, out objectCodeName);
		bool flag = bindUniverse.GetNamedFieldType(name) != null;
		if (namedElementType != null)
		{
			string updateParamOverride = (flag ? null : objectCodeName);
			BindPathStep parent = (flag ? bindUniverse.RootStep : bindUniverse.MakeOrGetRootStepOutOfScope());
			bindPathStep2 = new RootNamedElementStep(name, namedElementType, parent, apiInformation, updateParamOverride);
		}
		try
		{
			bindPathStep = ResolveNameOnType(name, bindUniverse.RootStep.ValueType, bindUniverse.RootStep);
		}
		catch (ParseException)
		{
			if (bindPathStep2 == null)
			{
				throw;
			}
			if (bindUniverse.RootStep.ValueType.GetMember(name) != null)
			{
				Warnings?.Add(string.Format(ErrorMessages.UnbindableMemberConflict, name, bindUniverse.RootStep.ValueType.UnderlyingType.FullName));
			}
		}
		if (bindPathStep != null && bindPathStep2 != null && !flag)
		{
			Warnings?.Add(string.Format(ErrorMessages.UsingNamedElement, name, bindUniverse.RootStep.ValueType.UnderlyingType.FullName));
		}
		if (flag)
		{
			return bindPathStep2 ?? bindPathStep;
		}
		return bindPathStep ?? bindPathStep2;
	}

	private BindPathStep ResolveNameOnType(string name, XamlType type, BindPathStep parentStep)
	{
		MemberInfo[] members = GetMembers(type.UnderlyingType, name);
		if (members.Length == 0)
		{
			throw new ParseException(ErrorMessages.PropertyNotFound, name, type.Name);
		}
		switch (members[0].MemberType)
		{
		case MemberTypes.Property:
		{
			PropertyInfo propertyInfo = members[0] as PropertyInfo;
			XamlType xamlType2 = type.SchemaContext.GetXamlType(propertyInfo.PropertyType);
			if (type.UnderlyingType.IsDependencyProperty(name))
			{
				return new DependencyPropertyStep(name, xamlType2, parentStep, apiInformation);
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if (getMethod == null)
			{
				throw new ParseException(ErrorMessages.PropertyWithoutGet, name, type.Name);
			}
			if (!getMethod.IsStatic && parentStep is StaticRootStep)
			{
				throw new ParseException(ErrorMessages.ExpectingStaticProperty, name, type.Name);
			}
			return new PropertyStep(name, xamlType2, parentStep, apiInformation);
		}
		case MemberTypes.Field:
		{
			XamlType xamlType = type.SchemaContext.GetXamlType(((FieldInfo)members[0]).FieldType);
			return new FieldStep(name, xamlType, parentStep, apiInformation);
		}
		case MemberTypes.Method:
			return new MethodStep(members, type, parentStep, apiInformation);
		default:
		{
			string text = members[0].MemberType.ToString();
			throw new ArgumentException("Unexpected member type '" + text + "' when binding to '" + name + "'");
		}
		}
	}

	private MemberInfo[] GetMembers(Type type, string name)
	{
		MemberInfo[] member = type.GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);
		if (member.Length == 0)
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				member = type2.GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty);
				if (member.Length != 0)
				{
					return member;
				}
			}
		}
		return member;
	}
}
