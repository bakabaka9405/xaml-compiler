using System;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

public class DirectUIXamlMember : XamlMember, IXamlMemberMeta
{
	private XamlType _eventArgType;

	private bool? _isDependencyProperty;

	private bool? _isIndexer;

	private bool? _hasPublicGetter;

	private bool? _hasPublicSetter;

	private bool? _isDeprecated;

	private bool? _isTemplate;

	private bool _isHardDeprecated;

	private string _deprecatedMessage;

	private bool? isExperimental;

	private MethodInfo _attachablePropertySetter;

	private MethodInfo _attachablePropertyGetter;

	public ApiInformation ApiInformation { get; set; }

	public bool IsDependencyProperty
	{
		get
		{
			if (!_isDependencyProperty.HasValue)
			{
				_isDependencyProperty = LookupIsDependencyProperty();
			}
			return _isDependencyProperty.Value;
		}
	}

	public bool IsDeprecated
	{
		get
		{
			if (!_isDeprecated.HasValue)
			{
				_isDeprecated = LookupIsDeprecated();
			}
			return _isDeprecated.Value;
		}
	}

	public bool IsHardDeprecated
	{
		get
		{
			if (!IsDeprecated)
			{
				return false;
			}
			return _isHardDeprecated;
		}
	}

	public string DeprecatedMessage
	{
		get
		{
			if (!IsDeprecated)
			{
				return string.Empty;
			}
			return _deprecatedMessage;
		}
	}

	public bool IsExperimental
	{
		get
		{
			if (!isExperimental.HasValue)
			{
				isExperimental = LookupIsExperimental();
			}
			return isExperimental.Value;
		}
	}

	public XamlType EventArgumentType
	{
		get
		{
			if (!base.IsEvent)
			{
				return null;
			}
			if (_eventArgType == null)
			{
				_eventArgType = LookupEventArgType();
			}
			return _eventArgType;
		}
	}

	public bool IsTemplate
	{
		get
		{
			if (!_isTemplate.HasValue)
			{
				_isTemplate = LookupIsTemplate();
			}
			return _isTemplate.Value;
		}
	}

	public bool IsIndexer
	{
		get
		{
			if (!_isIndexer.HasValue)
			{
				_isIndexer = LookupIsIndexer();
			}
			return _isIndexer.Value;
		}
	}

	public bool HasPublicGetter
	{
		get
		{
			if (!_hasPublicGetter.HasValue)
			{
				_hasPublicGetter = LookupHasPublicGetter();
			}
			return _hasPublicGetter.Value;
		}
	}

	public bool HasPublicSetter
	{
		get
		{
			if (!_hasPublicSetter.HasValue)
			{
				_hasPublicSetter = LookupHasPublicSetter();
			}
			return _hasPublicSetter.Value;
		}
	}

	public DirectUIXamlMember(PropertyInfo propertyInfo, DirectUISchemaContext schemaContext, ApiInformation apiInformation)
		: base(propertyInfo, schemaContext)
	{
		ApiInformation = apiInformation;
	}

	public DirectUIXamlMember(EventInfo eventInfo, DirectUISchemaContext schemaContext, ApiInformation apiInformation)
		: base(eventInfo, schemaContext)
	{
		ApiInformation = apiInformation;
	}

	public DirectUIXamlMember(string name, DirectUIXamlType declaringType, bool isAttachable, ApiInformation apiInformation)
		: base(name, declaringType, isAttachable)
	{
		ApiInformation = apiInformation;
	}

	public DirectUIXamlMember(string name, DirectUIXamlType declaringType)
		: base(name, declaringType, isAttachable: false)
	{
		ApiInformation = declaringType.ApiInformation;
	}

	public DirectUIXamlMember(string attachablePropertyName, XamlMemberInvoker invoker, XamlSchemaContext schemaContext, ApiInformation apiInformation)
		: base(attachablePropertyName, invoker.UnderlyingGetter, invoker.UnderlyingSetter, schemaContext, invoker)
	{
		_attachablePropertyGetter = invoker.UnderlyingGetter;
		_attachablePropertySetter = invoker.UnderlyingSetter;
		ApiInformation = apiInformation;
	}

	protected virtual bool LookupIsTemplate()
	{
		DirectUIXamlType directUIXamlType = base.DeclaringType as DirectUIXamlType;
		if (directUIXamlType != null && directUIXamlType.IsTemplateType)
		{
			return directUIXamlType.ContentProperty == this;
		}
		return false;
	}

	protected virtual bool LookupIsIndexer()
	{
		PropertyInfo propertyInfo = base.UnderlyingMember as PropertyInfo;
		if (propertyInfo != null)
		{
			ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
			return indexParameters.Length != 0;
		}
		return false;
	}

	protected virtual bool LookupIsDependencyProperty()
	{
		DirectUISchemaContext directUISchemaContext = base.DeclaringType.SchemaContext as DirectUISchemaContext;
		Type underlyingType = base.DeclaringType.UnderlyingType;
		return underlyingType.IsDependencyProperty(base.Name);
	}

	private CustomAttributeData GetAttribute(string attrName)
	{
		CustomAttributeData customAttributeData = null;
		if (!base.IsAttachable)
		{
			PropertyInfo propertyInfo = base.UnderlyingMember as PropertyInfo;
			if (propertyInfo != null)
			{
				MethodInfo setMethod = propertyInfo.GetSetMethod();
				if (setMethod != null)
				{
					customAttributeData = ReflectionHelper.FindAttributeByTypeName(setMethod, attrName);
				}
				if (customAttributeData == null)
				{
					customAttributeData = ReflectionHelper.FindAttributeByTypeName(propertyInfo, attrName);
				}
			}
		}
		else if (_attachablePropertySetter != null)
		{
			customAttributeData = ReflectionHelper.FindAttributeByTypeName(_attachablePropertySetter, attrName);
		}
		return customAttributeData;
	}

	private bool CheckDeprecationAttribute(string attrName, string defaultMessage)
	{
		CustomAttributeData attribute = GetAttribute(attrName);
		if (attribute != null)
		{
			Type declaringType = attribute.Constructor.DeclaringType;
			_deprecatedMessage = ReflectionHelper.GetAttributeConstructorArgument(attribute, 0, null) as string;
			if (string.IsNullOrWhiteSpace(_deprecatedMessage))
			{
				_deprecatedMessage = defaultMessage;
			}
			if (attrName.Equals("Windows.Foundation.Metadata.DeprecatedAttribute") && (int)ReflectionHelper.GetAttributeConstructorArgument(attribute, 1, null) != 0)
			{
				_isHardDeprecated = true;
			}
			return true;
		}
		return false;
	}

	protected virtual bool LookupIsDeprecated()
	{
		bool flag = CheckDeprecationAttribute("Windows.Foundation.Metadata.DeprecatedAttribute", "Deprecated");
		if (!flag)
		{
			return CheckDeprecationAttribute("System.ObsoleteAttribute", "Obsolete");
		}
		return flag;
	}

	protected virtual bool LookupIsExperimental()
	{
		return GetAttribute("Windows.Foundation.Metadata.ExperimentalAttribute") != null;
	}

	protected virtual XamlType LookupEventArgType()
	{
		XamlType result = null;
		if (base.IsEvent)
		{
			Type underlyingType = base.Type.UnderlyingType;
			if (underlyingType.BaseType == typeof(MulticastDelegate))
			{
				MethodInfo method = underlyingType.GetMethod("Invoke");
				if (method != null)
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters != null && parameters.Length == 2)
					{
						Type parameterType = parameters[1].ParameterType;
						result = base.DeclaringType.SchemaContext.GetXamlType(parameterType);
					}
				}
			}
		}
		return result;
	}

	protected virtual bool LookupHasPublicGetter()
	{
		PropertyInfo propertyInfo = base.UnderlyingMember as PropertyInfo;
		if (propertyInfo == null)
		{
			return base.IsReadPublic;
		}
		return propertyInfo.GetGetMethod() != null;
	}

	protected virtual bool LookupHasPublicSetter()
	{
		PropertyInfo propertyInfo = base.UnderlyingMember as PropertyInfo;
		if (propertyInfo == null)
		{
			return base.IsWritePublic;
		}
		return propertyInfo.GetSetMethod() != null;
	}

	protected override XamlType LookupType()
	{
		XamlType xamlType = base.LookupType();
		DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
		if (directUIXamlType != null)
		{
			return xamlType;
		}
		DirectUISchemaContext directUISchemaContext = base.DeclaringType.SchemaContext as DirectUISchemaContext;
		return directUISchemaContext.GetXamlType(xamlType.UnderlyingType);
	}
}
