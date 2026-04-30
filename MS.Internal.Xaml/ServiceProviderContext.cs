using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml;

internal class ServiceProviderContext : ITypeDescriptorContext, IServiceProvider, IXamlTypeResolver, IUriContext, IAmbientProvider, IXamlSchemaContextProvider, IRootObjectProvider, IXamlNamespaceResolver, IProvideValueTarget, IXamlNameResolver, IDestinationTypeProvider, IXamlLineInfo
{
	private ObjectWriterContext _xamlContext;

	IContainer ITypeDescriptorContext.Container => null;

	object ITypeDescriptorContext.Instance => null;

	PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

	Uri IUriContext.BaseUri
	{
		get
		{
			return _xamlContext.BaseUri;
		}
		set
		{
			throw new InvalidOperationException(SR.Get("MustNotCallSetter"));
		}
	}

	XamlSchemaContext IXamlSchemaContextProvider.SchemaContext => _xamlContext.SchemaContext;

	object IProvideValueTarget.TargetObject => _xamlContext.ParentInstance;

	object IProvideValueTarget.TargetProperty => ContextServices.GetTargetProperty(_xamlContext);

	object IRootObjectProvider.RootObject => _xamlContext.RootInstance;

	bool IXamlNameResolver.IsFixupTokenAvailable => !_xamlContext.NameResolutionComplete;

	public bool HasLineInfo
	{
		get
		{
			if (_xamlContext.LineNumber == 0)
			{
				return _xamlContext.LinePosition != 0;
			}
			return true;
		}
	}

	public int LineNumber => _xamlContext.LineNumber;

	public int LinePosition => _xamlContext.LinePosition;

	event EventHandler IXamlNameResolver.OnNameScopeInitializationComplete
	{
		add
		{
			_xamlContext.AddNameScopeInitializationCompleteSubscriber(value);
		}
		remove
		{
			_xamlContext.RemoveNameScopeInitializationCompleteSubscriber(value);
		}
	}

	public ServiceProviderContext(ObjectWriterContext context)
	{
		_xamlContext = context;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == typeof(IXamlTypeResolver))
		{
			return this;
		}
		if (serviceType == typeof(IUriContext))
		{
			return this;
		}
		if (serviceType == typeof(IAmbientProvider))
		{
			return this;
		}
		if (serviceType == typeof(IXamlSchemaContextProvider))
		{
			return this;
		}
		if (serviceType == typeof(IProvideValueTarget))
		{
			return this;
		}
		if (serviceType == typeof(IRootObjectProvider))
		{
			return this;
		}
		if (serviceType == typeof(IXamlNamespaceResolver))
		{
			return this;
		}
		if (serviceType == typeof(IXamlNameResolver))
		{
			return this;
		}
		if (serviceType == typeof(IXamlObjectWriterFactory))
		{
			return new XamlObjectWriterFactory(_xamlContext);
		}
		if (serviceType == typeof(IDestinationTypeProvider))
		{
			return this;
		}
		if (serviceType == typeof(IXamlLineInfo))
		{
			return this;
		}
		return null;
	}

	void ITypeDescriptorContext.OnComponentChanged()
	{
	}

	bool ITypeDescriptorContext.OnComponentChanging()
	{
		return false;
	}

	Type IXamlTypeResolver.Resolve(string qName)
	{
		return _xamlContext.ServiceProvider_Resolve(qName);
	}

	AmbientPropertyValue IAmbientProvider.GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
	{
		if (properties == null)
		{
			throw new ArgumentNullException("properties");
		}
		foreach (XamlMember xamlMember in properties)
		{
			if (xamlMember == null)
			{
				throw new ArgumentException(SR.Get("ValueInArrayIsNull", "properties"));
			}
		}
		return _xamlContext.ServiceProvider_GetFirstAmbientValue(ceilingTypes, properties);
	}

	object IAmbientProvider.GetFirstAmbientValue(params XamlType[] types)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		foreach (XamlType xamlType in types)
		{
			if (xamlType == null)
			{
				throw new ArgumentException(SR.Get("ValueInArrayIsNull", "types"));
			}
		}
		return _xamlContext.ServiceProvider_GetFirstAmbientValue(types);
	}

	IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
	{
		if (properties == null)
		{
			throw new ArgumentNullException("properties");
		}
		foreach (XamlMember xamlMember in properties)
		{
			if (xamlMember == null)
			{
				throw new ArgumentException(SR.Get("ValueInArrayIsNull", "properties"));
			}
		}
		return _xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, properties);
	}

	IEnumerable<object> IAmbientProvider.GetAllAmbientValues(params XamlType[] types)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		foreach (XamlType xamlType in types)
		{
			if (xamlType == null)
			{
				throw new ArgumentException(SR.Get("ValueInArrayIsNull", "types"));
			}
		}
		return _xamlContext.ServiceProvider_GetAllAmbientValues(types);
	}

	IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties)
	{
		if (properties == null)
		{
			throw new ArgumentNullException("properties");
		}
		foreach (XamlMember xamlMember in properties)
		{
			if (xamlMember == null)
			{
				throw new ArgumentException(SR.Get("ValueInArrayIsNull", "properties"));
			}
		}
		return _xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties);
	}

	string IXamlNamespaceResolver.GetNamespace(string prefix)
	{
		return _xamlContext.FindNamespaceByPrefix(prefix);
	}

	IEnumerable<NamespaceDeclaration> IXamlNamespaceResolver.GetNamespacePrefixes()
	{
		return _xamlContext.GetNamespacePrefixes();
	}

	object IXamlNameResolver.Resolve(string name)
	{
		bool isFullyInitialized;
		return _xamlContext.ResolveName(name, out isFullyInitialized);
	}

	object IXamlNameResolver.Resolve(string name, out bool isFullyInitialized)
	{
		return _xamlContext.ResolveName(name, out isFullyInitialized);
	}

	object IXamlNameResolver.GetFixupToken(IEnumerable<string> names)
	{
		return ((IXamlNameResolver)this).GetFixupToken(names, false);
	}

	object IXamlNameResolver.GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
	{
		if (_xamlContext.NameResolutionComplete)
		{
			return null;
		}
		NameFixupToken nameFixupToken = new NameFixupToken();
		nameFixupToken.CanAssignDirectly = canAssignDirectly;
		nameFixupToken.NeededNames.AddRange(names);
		if (nameFixupToken.CanAssignDirectly && nameFixupToken.NeededNames.Count != 1)
		{
			throw new ArgumentException(SR.Get("SimpleFixupsMustHaveOneName"), "names");
		}
		if (_xamlContext.CurrentType == null)
		{
			if (_xamlContext.ParentProperty == XamlLanguage.Initialization)
			{
				nameFixupToken.FixupType = FixupType.ObjectInitializationValue;
				nameFixupToken.Target.Instance = _xamlContext.GrandParentInstance;
				nameFixupToken.Target.InstanceWasGotten = _xamlContext.GrandParentIsObjectFromMember;
				nameFixupToken.Target.InstanceType = _xamlContext.GrandParentType;
				nameFixupToken.Target.Property = _xamlContext.GrandParentProperty;
			}
			else
			{
				nameFixupToken.FixupType = FixupType.PropertyValue;
				nameFixupToken.Target.Instance = _xamlContext.ParentInstance;
				nameFixupToken.Target.InstanceWasGotten = _xamlContext.ParentIsObjectFromMember;
				nameFixupToken.Target.InstanceType = _xamlContext.ParentType;
				nameFixupToken.Target.Property = _xamlContext.ParentProperty;
			}
		}
		else
		{
			nameFixupToken.FixupType = FixupType.MarkupExtensionRerun;
			nameFixupToken.Target.Instance = _xamlContext.ParentInstance;
			nameFixupToken.Target.InstanceWasGotten = _xamlContext.ParentIsObjectFromMember;
			nameFixupToken.Target.InstanceType = _xamlContext.ParentType;
			nameFixupToken.Target.Property = _xamlContext.ParentProperty;
		}
		if (nameFixupToken.CanAssignDirectly)
		{
			nameFixupToken.NameScopeDictionaryList.AddRange(_xamlContext.StackWalkOfNameScopes);
		}
		else
		{
			nameFixupToken.SavedContext = _xamlContext.GetSavedContext((nameFixupToken.FixupType != FixupType.MarkupExtensionRerun) ? SavedContextType.ReparseValue : SavedContextType.ReparseMarkupExtension);
		}
		return nameFixupToken;
	}

	IEnumerable<KeyValuePair<string, object>> IXamlNameResolver.GetAllNamesAndValuesInScope()
	{
		return _xamlContext.GetAllNamesAndValuesInScope();
	}

	public Type GetDestinationType()
	{
		return _xamlContext.GetDestinationType().UnderlyingType;
	}
}
