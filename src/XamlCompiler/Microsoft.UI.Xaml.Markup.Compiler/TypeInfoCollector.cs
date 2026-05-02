using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.RootLog;
using Microsoft.UI.Xaml.Markup.Compiler.Tracing;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class TypeInfoCollector
{
	private DirectUISchemaContext _schemaContext;

	private XamlSchemaCodeInfo _schemaInfo;

	private RootLogBuilder _rootLogBuilder;

	private Platform _targPlat;

	private bool _enableBindingDiagnostics;

	internal ClassName AppXamlInfo { get; set; }

	public XamlSchemaCodeInfo SchemaInfo => _schemaInfo;

	public Roots RootLog => _rootLogBuilder.GetRoots();

	public TypeInfoCollector(DirectUISchemaContext schemaContext, Platform targPlat, bool enableBindingDiagnostics)
	{
		_schemaContext = schemaContext;
		_targPlat = targPlat;
		_enableBindingDiagnostics = enableBindingDiagnostics;
		_schemaInfo = new XamlSchemaCodeInfo();
		_rootLogBuilder = new RootLogBuilder();
	}

	public void Collect(XamlDomObject domRoot)
	{
		XamlDomIterator xamlDomIterator = new XamlDomIterator(domRoot);
		foreach (XamlDomObject item in xamlDomIterator.DescendantsAndSelf())
		{
			if (DomHelper.IsObjectInvalidForPlatform(item, _targPlat))
			{
				continue;
			}
			if (!item.IsGetObject)
			{
				XamlType type = item.Type;
				if (type.IsUnknown)
				{
					throw new ArgumentOutOfRangeException(type.Name);
				}
				if (type.UnderlyingType == null)
				{
					continue;
				}
				DirectUIXamlType directUIXamlType = (DirectUIXamlType)item.Type;
				if (directUIXamlType.IsAssignableToStyle)
				{
					if (!CollectSettersInStyle(item))
					{
						_schemaContext.SchemaErrors.Add(new XamlCompilerErrorProcessingStyle(item));
					}
					continue;
				}
				if (directUIXamlType.IsCodeGenType)
				{
					_schemaInfo.AddTypeAndProperties(directUIXamlType);
				}
				_rootLogBuilder.AddTypeBuilder(directUIXamlType);
				if (directUIXamlType.IsAssignableToBinding)
				{
					CollectBindingCtorParam(item);
				}
				foreach (XamlDomMember memberNode in item.MemberNodes)
				{
					if (memberNode.Member.IsDirective)
					{
						continue;
					}
					if (memberNode.Member.IsUnknown)
					{
						throw new ArgumentOutOfRangeException(memberNode.Member.Name);
					}
					DirectUIXamlMember directUIXamlMember = (DirectUIXamlMember)memberNode.Member;
					if (directUIXamlMember.IsAttachable)
					{
						DirectUIXamlType directUIXamlType2 = (DirectUIXamlType)directUIXamlMember.DeclaringType;
						if (directUIXamlType2.IsCodeGenType)
						{
							_schemaInfo.AddTypeAndProperties(directUIXamlType2);
						}
					}
					if (_schemaContext.DirectUISystem.Type.IsAssignableFrom(memberNode.Member.Type.UnderlyingType))
					{
						string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode);
						if (!string.IsNullOrEmpty(stringValueOfProperty))
						{
							XamlType xamlType = item.ResolveXmlName(stringValueOfProperty);
							if (xamlType != null)
							{
								_schemaInfo.AddTypeAndProperties(xamlType);
							}
						}
					}
					AddMemberToRootLog(directUIXamlType, directUIXamlMember);
					if (_schemaContext.DirectUISystem.PropertyPath.IsAssignableFrom(memberNode.Member.Type.UnderlyingType))
					{
						CollectPropertyPath(memberNode);
					}
					else if (ShouldTreatAsPropertyPath(memberNode))
					{
						CheckForPropertyPathAotWarnings(item);
						CollectPropertyPath(memberNode);
					}
				}
			}
			else
			{
				XamlType type2 = item.Type;
				if (type2.IsUnknown)
				{
					throw new ArgumentOutOfRangeException(type2.Name);
				}
				XamlDomMember parent = item.Parent;
				string name = parent.Member.DeclaringType.Name;
				string name2 = parent.Member.Name;
			}
		}
	}

	public void AddTypeToRootLog(DirectUIXamlType duiXamlType)
	{
		_rootLogBuilder.AddTypeBuilder(duiXamlType);
	}

	public void AddMetadataAndBindableTypes(List<Assembly> loadedAssemblies, Assembly localAssembly)
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_SearchIxmpAndBindableStart);
		FindAllMetadataAndBindableTypes(loadedAssemblies, localAssembly, out var otherProviders, out var bindableTypes);
		int num = otherProviders.FindIndex((Type p) => p.FullName == "Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider");
		if (num != -1)
		{
			Type item = otherProviders[num];
			otherProviders.RemoveAt(num);
			otherProviders.Insert(0, item);
		}
		_schemaInfo.OtherMetadataProviders = (from p in otherProviders
			select new DirectUIXamlType(p, _schemaContext) into p
			where !DomHelper.IsLocalType(p)
			select new TypeForCodeGen(p)).ToList();
		foreach (Type item2 in bindableTypes)
		{
			DirectUIXamlType type = new DirectUIXamlType(item2, _schemaContext);
			InternalTypeEntry internalTypeEntry = _schemaInfo.AddBindableType(type);
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_SearchIxmpAndBindableEnd);
	}

	public void AddAllConstructibleTypesFromLocalAssembly(Assembly localAssembly)
	{
		_schemaInfo.TypeInfoReflectionEnabled = true;
	}

	private void AddMemberToRootLog(DirectUIXamlType duiType, DirectUIXamlMember duiMember)
	{
		if (duiMember.IsAttachable)
		{
			DirectUIXamlType duiType2 = (DirectUIXamlType)duiMember.DeclaringType;
			RootTypeBuilder rootTypeBuilder = _rootLogBuilder.AddTypeBuilder(duiType2);
			if (duiMember.HasPublicGetter)
			{
				rootTypeBuilder.AddMethod("Get" + duiMember.Name);
			}
			if (duiMember.HasPublicSetter)
			{
				rootTypeBuilder.AddMethod("Set" + duiMember.Name);
			}
		}
		else if (duiMember.IsEvent)
		{
			_rootLogBuilder.AddEvent(duiType, duiMember.Name);
		}
		else
		{
			_rootLogBuilder.AddProperty(duiType, duiMember.Name);
		}
	}

	private bool CollectSettersInStyle(XamlDomObject StyleObject)
	{
		XamlDomMember memberNode = StyleObject.GetMemberNode("TargetType");
		if (memberNode == null)
		{
			return false;
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode);
		if (stringValueOfProperty == null)
		{
			return false;
		}
		XamlType xamlType = StyleObject.ResolveXmlName(stringValueOfProperty);
		if (xamlType == null)
		{
			return false;
		}
		DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
		if (directUIXamlType != null && directUIXamlType.IsCodeGenType)
		{
			_schemaInfo.AddTypeAndProperties(directUIXamlType);
		}
		_rootLogBuilder.AddTypeBuilder(directUIXamlType);
		XamlDomMember memberNode2 = StyleObject.GetMemberNode("Setters");
		if (memberNode2 == null || memberNode2.Item == null)
		{
			return true;
		}
		if (!(memberNode2.Item is XamlDomObject xamlDomObject))
		{
			return false;
		}
		XamlDomMember memberNode3 = xamlDomObject.GetMemberNode(XamlLanguage.Items);
		if (memberNode3 == null || memberNode3.Items.Count == 0)
		{
			return true;
		}
		foreach (XamlDomItem item in memberNode3.Items)
		{
			if (item is XamlDomObject xamlDomObject2 && !xamlDomObject2.Type.IsUnknown && xamlDomObject2.Type.UnderlyingType.IsAssignableFrom(_schemaContext.DirectUISystem.Setter))
			{
				CollectSingleSetter(xamlType, xamlDomObject2);
			}
		}
		return true;
	}

	private bool CollectSingleSetter(XamlType xamlTargetType, XamlDomObject domSetterObject)
	{
		XamlDomMember memberNode = domSetterObject.GetMemberNode("Property");
		if (memberNode == null)
		{
			memberNode = domSetterObject.GetMemberNode("Target");
			if (memberNode == null)
			{
				return false;
			}
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(memberNode);
		if (stringValueOfProperty == null)
		{
			return false;
		}
		XamlMember xamlMember = domSetterObject.ResolveMemberName(xamlTargetType, stringValueOfProperty);
		if (xamlMember == null)
		{
			return false;
		}
		XamlDomMember memberNode2 = domSetterObject.GetMemberNode("Value");
		if (memberNode2 == null)
		{
			return false;
		}
		if (memberNode2.Item == null)
		{
			return false;
		}
		DirectUIXamlType directUIXamlType = (DirectUIXamlType)xamlMember.DeclaringType;
		if (directUIXamlType.IsCodeGenType)
		{
			InternalTypeEntry usingType = _schemaInfo.AddType(directUIXamlType);
			_schemaInfo.AddMember(usingType, xamlMember);
		}
		DirectUIXamlMember duiMember = (DirectUIXamlMember)xamlMember;
		AddMemberToRootLog(directUIXamlType, duiMember);
		return true;
	}

	private void CollectBindingCtorParam(XamlDomObject domBinding)
	{
		XamlDomMember memberNode = domBinding.GetMemberNode(XamlLanguage.PositionalParameters);
		if (memberNode != null)
		{
			XamlDomItem item = memberNode.Item;
			if (memberNode.Item is XamlDomValue { Value: string value })
			{
				CheckForPropertyPathAotWarnings(domBinding);
				CollectPropertyPath(value, domBinding);
			}
		}
	}

	private void CheckForPropertyPathAotWarnings(XamlDomObject domBinding)
	{
		if (!_enableBindingDiagnostics)
		{
			return;
		}
		bool flag = false;
		XamlDomMember memberNode = domBinding.GetMemberNode(SuppressXamlTrimWarningsDirective.Value);
		bool result = default(bool);
		if (memberNode != null && ((memberNode.Item is XamlDomValue xamlDomValue) ? xamlDomValue.Value : null) is string value && bool.TryParse(value, out result) && result)
		{
			flag = true;
		}
		if (!flag)
		{
			XamlDomMember xamlDomMember = null;
			XamlDomObject xamlDomObject = domBinding;
			while (xamlDomMember == null && xamlDomObject != null)
			{
				xamlDomMember = DomHelper.GetDataTypeMember(xamlDomObject, getDirectiveOnly: true);
				xamlDomObject = xamlDomObject.Parent?.Parent;
			}
			if (xamlDomMember != null)
			{
				string stringValueOfProperty = DomHelper.GetStringValueOfProperty(xamlDomMember);
				if (!string.IsNullOrEmpty(stringValueOfProperty))
				{
					XamlType xamlType = domBinding.ResolveXmlName(stringValueOfProperty);
					if (xamlType != null && (HasBindableAttribute(xamlType.UnderlyingType) || HasGeneratedBindableCustomPropertyAttribute(xamlType.UnderlyingType)))
					{
						flag = true;
					}
				}
			}
		}
		if (!flag)
		{
			_schemaContext.SchemaWarnings.Add(new XamlBindingAotCompatibilityWarning(domBinding));
		}
	}

	private void CollectPropertyPath(XamlDomMember pathProperty)
	{
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(pathProperty);
		if (stringValueOfProperty != null)
		{
			XamlDomObject parent = pathProperty.Parent;
			CollectPropertyPath(stringValueOfProperty, parent);
		}
	}

	private void CollectPropertyPath(string pathString, XamlDomObject domBinding)
	{
		if (!PropertyPathParser.Parse(pathString, out var qualifiedProperties, out var names))
		{
			return;
		}
		if (names != null)
		{
			foreach (string item in names)
			{
				_rootLogBuilder.AddPropertyPathName(item);
			}
		}
		if (qualifiedProperties == null)
		{
			return;
		}
		foreach (string item2 in qualifiedProperties)
		{
			XamlMember xamlMember = domBinding.ResolveMemberName(item2);
			DirectUIXamlType duiType = (DirectUIXamlType)xamlMember.DeclaringType;
			DirectUIXamlMember duiMember = (DirectUIXamlMember)xamlMember;
			AddMemberToRootLog(duiType, duiMember);
		}
	}

	private void FindAllMetadataAndBindableTypes(IList<Assembly> loadedAssemblies, Assembly localAssembly, out List<Type> otherProviders, out List<Type> bindableTypes)
	{
		otherProviders = new List<Type>();
		bindableTypes = new List<Type>();
		foreach (Assembly loadedAssembly in loadedAssemblies)
		{
			Type[] array = null;
			try
			{
				array = loadedAssembly.GetTypes();
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				if (ex is ReflectionTypeLoadException { LoaderExceptions: var loaderExceptions } ex2)
				{
					array = ex2.Types;
				}
			}
			bool flag = loadedAssembly.GetName().ContentType == AssemblyContentType.WindowsRuntime;
			Type[] array2 = array;
			foreach (Type type in array2)
			{
				try
				{
					if (type == null || !type.IsPublic || type.FullName.StartsWith("ABI.") || (!flag && (!type.IsClass || type.IsAbstract)))
					{
						continue;
					}
					if (HasBindableAttribute(type))
					{
						if (type.IsGenericType)
						{
							_schemaContext.SchemaErrors.Add(new XamlSchemaError_BindableNotSupportedOnGeneric(type.FullName));
						}
						else
						{
							bindableTypes.Add(type);
						}
					}
					if (loadedAssembly != localAssembly && HasInterface(type, "Microsoft.UI.Xaml.Markup.IXamlMetadataProvider") && !DerivesFromBaseType(type, "Microsoft.UI.Xaml.Application"))
					{
						otherProviders.Add(type);
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private bool HasBindableAttribute(Type type)
	{
		using (IEnumerator<CustomAttributeData> enumerator = Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(type, inherit: false, "Windows.UI.Xaml.Data.BindableAttribute").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				CustomAttributeData current = enumerator.Current;
				return true;
			}
		}
		using (IEnumerator<CustomAttributeData> enumerator2 = Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(type, inherit: false, "Microsoft.UI.Xaml.Data.BindableAttribute").GetEnumerator())
		{
			if (enumerator2.MoveNext())
			{
				CustomAttributeData current2 = enumerator2.Current;
				return true;
			}
		}
		return false;
	}

	private static bool HasGeneratedBindableCustomPropertyAttribute(Type type)
	{
		using (IEnumerator<CustomAttributeData> enumerator = Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(type, inherit: false, "WinRT.GeneratedBindableCustomPropertyAttribute").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				CustomAttributeData current = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	private bool HasInterface(Type type, string InterfaceName)
	{
		Type type2 = null;
		try
		{
			type2 = type.GetInterface(InterfaceName);
		}
		catch (Exception)
		{
		}
		return type2 != null;
	}

	private bool DerivesFromBaseType(Type type, string baseTypeName)
	{
		Type type2 = type;
		while (type2 != null)
		{
			if (type2.FullName == baseTypeName)
			{
				return true;
			}
			type2 = type2.BaseType;
		}
		return false;
	}

	private bool ShouldTreatAsPropertyPath(XamlDomMember domMember)
	{
		string text = null;
		if (domMember != null && domMember.Member != null && domMember.Member.UnderlyingMember != null)
		{
			text = domMember.Member.UnderlyingMember.Name;
		}
		switch (text)
		{
		case "DisplayMemberPath":
			if (!(domMember.Member.DeclaringType.UnderlyingType.FullName == "Microsoft.UI.Xaml.Controls.ItemsControl"))
			{
				return domMember.Member.DeclaringType.UnderlyingType.FullName == "Microsoft.UI.Xaml.Controls.ListPickerFlyout";
			}
			return true;
		case "TextMemberPath":
			return domMember.Member.DeclaringType.UnderlyingType.FullName == "Microsoft.UI.Xaml.Controls.AutoSuggestBox";
		case "SelectedValuePath":
			return domMember.Member.DeclaringType.UnderlyingType.FullName == "Microsoft.UI.Xaml.Controls.Primitives.Selector";
		default:
			return false;
		}
	}
}
