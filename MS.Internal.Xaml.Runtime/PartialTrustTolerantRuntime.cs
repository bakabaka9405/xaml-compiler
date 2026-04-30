using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Runtime;

internal class PartialTrustTolerantRuntime : XamlRuntime
{
	private bool _memberAccessPermissionDenied;

	private ClrObjectRuntime _transparentRuntime;

	private ClrObjectRuntime _elevatedRuntime;

	private XamlAccessLevel _accessLevel;

	private XamlSchemaContext _schemaContext;

	public override IAddLineInfo LineInfo
	{
		get
		{
			return _transparentRuntime.LineInfo;
		}
		set
		{
			_transparentRuntime.LineInfo = value;
			if (_elevatedRuntime != null)
			{
				_elevatedRuntime.LineInfo = value;
			}
		}
	}

	private bool MemberAccessPermissionDenied
	{
		get
		{
			return _memberAccessPermissionDenied;
		}
		set
		{
			_memberAccessPermissionDenied = value;
			if (value)
			{
				EnsureElevatedRuntime();
			}
		}
	}

	public PartialTrustTolerantRuntime(XamlRuntimeSettings runtimeSettings, XamlAccessLevel accessLevel, XamlSchemaContext schemaContext)
	{
		_transparentRuntime = new ClrObjectRuntime(runtimeSettings, isWriter: true);
		_accessLevel = accessLevel;
		_schemaContext = schemaContext;
	}

	public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
	{
		_transparentRuntime.Add(collection, collectionType, value, valueXamlType);
	}

	public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
	{
		_transparentRuntime.AddToDictionary(collection, dictionaryType, value, valueXamlType, key);
	}

	public override object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider)
	{
		return _transparentRuntime.CallProvideValue(me, serviceProvider);
	}

	public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
	{
		if (!MemberAccessPermissionDenied || ts.IsPublic || !IsDefaultConverter(ts))
		{
			try
			{
				return _transparentRuntime.CreateFromValue(serviceContext, ts, value, property);
			}
			catch (MissingMethodException)
			{
				EnsureElevatedRuntime();
			}
			catch (MethodAccessException)
			{
				MemberAccessPermissionDenied = true;
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.CreateFromValue(serviceContext, ts, value, property);
	}

	public override int AttachedPropertyCount(object instance)
	{
		return _transparentRuntime.AttachedPropertyCount(instance);
	}

	public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
	{
		return _transparentRuntime.GetAttachedProperties(instance);
	}

	public override bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
	{
		return _transparentRuntime.CanConvertToString(context, serializer, instance);
	}

	public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
	{
		return _transparentRuntime.CanConvertFrom<T>(context, converter);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
	{
		return _transparentRuntime.CanConvertTo(context, converter, type);
	}

	public override string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
	{
		return _transparentRuntime.ConvertToString(context, serializer, instance);
	}

	public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
	{
		return _transparentRuntime.ConvertToValue<T>(context, converter, instance);
	}

	public override object CreateInstance(XamlType xamlType, object[] args)
	{
		if (!MemberAccessPermissionDenied || xamlType.IsPublic || !HasDefaultInvoker(xamlType))
		{
			try
			{
				return _transparentRuntime.CreateInstance(xamlType, args);
			}
			catch (XamlException ex)
			{
				if (ex.InnerException is MethodAccessException)
				{
					MemberAccessPermissionDenied = true;
				}
				else
				{
					if (!(ex.InnerException is MissingMethodException))
					{
						throw;
					}
					EnsureElevatedRuntime();
				}
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.CreateInstance(xamlType, args);
	}

	public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
	{
		if (!MemberAccessPermissionDenied || xamlType.IsPublic)
		{
			try
			{
				return _transparentRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
			}
			catch (XamlException ex)
			{
				if (ex.InnerException is MethodAccessException)
				{
					MemberAccessPermissionDenied = true;
				}
				else
				{
					if (!(ex.InnerException is MissingMethodException))
					{
						throw;
					}
					EnsureElevatedRuntime();
				}
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
	}

	public override object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent)
	{
		if (!MemberAccessPermissionDenied || deferringLoader.IsPublic || !IsDefaultConverter(deferringLoader))
		{
			try
			{
				return _transparentRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
			}
			catch (XamlException ex)
			{
				if (ex.InnerException is MissingMethodException)
				{
					EnsureElevatedRuntime();
				}
				else
				{
					if (!(ex.InnerException is MethodAccessException))
					{
						throw;
					}
					MemberAccessPermissionDenied = true;
				}
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
	}

	public override XamlReader DeferredSave(IServiceProvider context, XamlValueConverter<XamlDeferringLoader> deferringLoader, object value)
	{
		if (!MemberAccessPermissionDenied || deferringLoader.IsPublic || !IsDefaultConverter(deferringLoader))
		{
			try
			{
				return _transparentRuntime.DeferredSave(context, deferringLoader, value);
			}
			catch (XamlException ex)
			{
				if (ex.InnerException is MissingMethodException)
				{
					EnsureElevatedRuntime();
				}
				else
				{
					if (!(ex.InnerException is MethodAccessException))
					{
						throw;
					}
					MemberAccessPermissionDenied = true;
				}
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.DeferredSave(context, deferringLoader, value);
	}

	public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter)
	{
		if (!MemberAccessPermissionDenied || converter.IsPublic || !IsDefaultConverter(converter))
		{
			try
			{
				return _transparentRuntime.GetConverterInstance(converter);
			}
			catch (MissingMethodException)
			{
				EnsureElevatedRuntime();
			}
			catch (MethodAccessException)
			{
				MemberAccessPermissionDenied = true;
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.GetConverterInstance(converter);
	}

	public override object GetValue(object obj, XamlMember property, bool failIfWriteOnly)
	{
		if (!MemberAccessPermissionDenied || property.IsReadPublic || !HasDefaultInvoker(property))
		{
			try
			{
				return _transparentRuntime.GetValue(obj, property, failIfWriteOnly);
			}
			catch (XamlException ex)
			{
				if (!(ex.InnerException is MethodAccessException))
				{
					throw;
				}
				MemberAccessPermissionDenied = true;
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		return _elevatedRuntime.GetValue(obj, property, failIfWriteOnly);
	}

	public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
	{
		_transparentRuntime.InitializationGuard(xamlType, obj, begin);
	}

	public override void SetConnectionId(object root, int connectionId, object instance)
	{
		_transparentRuntime.SetConnectionId(root, connectionId, instance);
	}

	public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
	{
		_transparentRuntime.SetUriBase(xamlType, obj, baseUri);
	}

	public override void SetValue(object obj, XamlMember property, object value)
	{
		if (!MemberAccessPermissionDenied || property.IsWritePublic || !HasDefaultInvoker(property))
		{
			try
			{
				_transparentRuntime.SetValue(obj, property, value);
				return;
			}
			catch (XamlException ex)
			{
				if (!(ex.InnerException is MethodAccessException))
				{
					throw;
				}
				MemberAccessPermissionDenied = true;
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		_elevatedRuntime.SetValue(obj, property, value);
	}

	public override void SetXmlInstance(object inst, XamlMember property, XData xData)
	{
		if (!MemberAccessPermissionDenied || property.IsReadPublic)
		{
			try
			{
				_transparentRuntime.SetXmlInstance(inst, property, xData);
				return;
			}
			catch (XamlException ex)
			{
				if (!(ex.InnerException is MethodAccessException))
				{
					throw;
				}
				MemberAccessPermissionDenied = true;
			}
			catch (SecurityException)
			{
				MemberAccessPermissionDenied = true;
			}
		}
		_elevatedRuntime.SetXmlInstance(inst, property, xData);
	}

	public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
	{
		return _transparentRuntime.ShouldSerialize(member, instance);
	}

	public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
	{
		return _transparentRuntime.GetCollectionItems(collection, collectionType);
	}

	public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
	{
		return _transparentRuntime.GetDictionaryItems(dictionary, dictionaryType);
	}

	[SecuritySafeCritical]
	private void EnsureElevatedRuntime()
	{
		if (_elevatedRuntime == null)
		{
			_elevatedRuntime = new DynamicMethodRuntime(_transparentRuntime.GetSettings(), _schemaContext, _accessLevel);
			_elevatedRuntime.LineInfo = LineInfo;
		}
	}

	private static bool HasDefaultInvoker(XamlType xamlType)
	{
		return xamlType.Invoker.GetType() == typeof(XamlTypeInvoker);
	}

	private static bool HasDefaultInvoker(XamlMember xamlMember)
	{
		return xamlMember.Invoker.GetType() == typeof(XamlMemberInvoker);
	}

	private static bool IsDefaultConverter<TConverterBase>(XamlValueConverter<TConverterBase> converter) where TConverterBase : class
	{
		return converter.GetType() == typeof(XamlValueConverter<TConverterBase>);
	}
}
