using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using System.Xml.Serialization;

namespace MS.Internal.Xaml.Runtime;

internal class ClrObjectRuntime : XamlRuntime
{
	private bool _ignoreCanConvert;

	private bool _isWriter;

	public override IAddLineInfo LineInfo { get; set; }

	public ClrObjectRuntime(XamlRuntimeSettings settings, bool isWriter)
	{
		if (settings != null)
		{
			_ignoreCanConvert = settings.IgnoreCanConvert;
		}
		_isWriter = isWriter;
	}

	private static Exception UnwrapTargetInvocationException(Exception e)
	{
		if (e is TargetInvocationException && e.InnerException != null)
		{
			return e.InnerException;
		}
		return e;
	}

	public override object CreateInstance(XamlType xamlType, object[] args)
	{
		if (xamlType.IsUnknown)
		{
			throw CreateException(SR.Get("CannotCreateBadType", xamlType.Name));
		}
		try
		{
			return CreateInstanceWithCtor(xamlType, args);
		}
		catch (MissingMethodException innerException)
		{
			throw CreateException(SR.Get("NoConstructor", xamlType.UnderlyingType), innerException);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("ConstructorInvocation", xamlType.UnderlyingType), UnwrapTargetInvocationException(ex));
		}
	}

	protected virtual object CreateInstanceWithCtor(XamlType xamlType, object[] args)
	{
		return xamlType.Invoker.CreateInstance(args);
	}

	public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
	{
		Type underlyingType = xamlType.UnderlyingType;
		if (underlyingType == null)
		{
			throw CreateException(SR.Get("CannotResolveTypeForFactoryMethod", xamlType, methodName));
		}
		string text = underlyingType.ToString() + "." + methodName;
		object obj = null;
		try
		{
			obj = InvokeFactoryMethod(underlyingType, methodName, args);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("MethodInvocation", text), UnwrapTargetInvocationException(ex));
		}
		if (obj == null)
		{
			throw CreateException(SR.Get("FactoryReturnedNull", text));
		}
		return obj;
	}

	protected virtual object InvokeFactoryMethod(Type type, string methodName, object[] args)
	{
		MethodInfo factoryMethod = GetFactoryMethod(type, methodName, args, BindingFlags.Static | BindingFlags.Public);
		return SafeReflectionInvoker.InvokeMethod(factoryMethod, null, args);
	}

	protected MethodInfo GetFactoryMethod(Type type, string methodName, object[] args, BindingFlags flags)
	{
		MethodInfo methodInfo = null;
		if (args == null || args.Length == 0)
		{
			methodInfo = type.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
		}
		if (methodInfo == null)
		{
			MemberInfo[] member = type.GetMember(methodName, MemberTypes.Method, flags);
			MethodBase[] array = member as MethodBase[];
			if (array == null)
			{
				array = new MethodBase[member.Length];
				Array.Copy(member, array, member.Length);
			}
			methodInfo = (MethodInfo)BindToMethod(flags, array, args);
		}
		return methodInfo;
	}

	protected MethodBase BindToMethod(BindingFlags bindingFlags, MethodBase[] candidates, object[] args)
	{
		object state;
		return Type.DefaultBinder.BindToMethod(bindingFlags, candidates, ref args, null, null, null, out state);
	}

	public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
	{
		if (ts == BuiltInValueConverter.String || ts == BuiltInValueConverter.Object)
		{
			return value;
		}
		return CreateObjectWithTypeConverter(serviceContext, ts, value);
	}

	public override bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
	{
		try
		{
			return serializer.CanConvertToString(instance, context);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("TypeConverterFailed2", instance, typeof(string)), ex);
		}
	}

	public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
	{
		try
		{
			return converter.CanConvertFrom(context, typeof(T));
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("CanConvertFromFailed", typeof(T), converter.GetType()), ex);
		}
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
	{
		try
		{
			return converter.CanConvertTo(context, type);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("CanConvertToFailed", type, converter.GetType()), ex);
		}
	}

	public override string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
	{
		try
		{
			return serializer.ConvertToString(instance, context);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("TypeConverterFailed2", instance, typeof(string)), ex);
		}
	}

	public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
	{
		try
		{
			return (T)converter.ConvertTo(context, TypeConverterHelper.InvariantEnglishUS, instance, typeof(T));
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("TypeConverterFailed2", instance, typeof(T)), ex);
		}
	}

	public override object GetValue(object obj, XamlMember property, bool failIfWriteOnly)
	{
		try
		{
			if (property.IsDirective)
			{
				return CreateInstance(property.Type, null);
			}
			if (!failIfWriteOnly)
			{
				try
				{
					return GetValue(property, obj);
				}
				catch (NotSupportedException)
				{
					return null;
				}
			}
			return GetValue(property, obj);
		}
		catch (Exception ex2)
		{
			if (CriticalExceptions.IsCriticalException(ex2))
			{
				throw;
			}
			throw CreateException(SR.Get("GetValue", property), UnwrapTargetInvocationException(ex2));
		}
	}

	protected virtual object GetValue(XamlMember member, object obj)
	{
		return member.Invoker.GetValue(obj);
	}

	public override void SetValue(object inst, XamlMember property, object value)
	{
		try
		{
			if (!property.IsDirective)
			{
				SetValue(property, inst, value);
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("SetValue", property), UnwrapTargetInvocationException(ex));
		}
	}

	protected virtual void SetValue(XamlMember member, object obj, object value)
	{
		member.Invoker.SetValue(obj, value);
	}

	public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
	{
		try
		{
			collectionType.Invoker.AddToCollection(collection, value);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("AddCollection", collectionType), UnwrapTargetInvocationException(ex));
		}
	}

	public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
	{
		try
		{
			dictionaryType.Invoker.AddToDictionary(collection, key, value);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("AddDictionary", dictionaryType), UnwrapTargetInvocationException(ex));
		}
	}

	public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
	{
		IEnumerator items = GetItems(collection, collectionType);
		try
		{
			List<object> list = new List<object>();
			while (items.MoveNext())
			{
				list.Add(items.Current);
			}
			return list;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("GetItemsException", collectionType), ex);
		}
	}

	public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
	{
		IEnumerator items = GetItems(dictionary, dictionaryType);
		try
		{
			if (items is IDictionaryEnumerator enumerator)
			{
				return DictionaryEntriesFromIDictionaryEnumerator(enumerator);
			}
			Type underlyingType = dictionaryType.KeyType.UnderlyingType;
			Type underlyingType2 = dictionaryType.ItemType.UnderlyingType;
			Type type = typeof(KeyValuePair<, >).MakeGenericType(underlyingType, underlyingType2);
			Type type2 = typeof(IEnumerator<>).MakeGenericType(type);
			if (type2.IsAssignableFrom(items.GetType()))
			{
				MethodInfo method = typeof(ClrObjectRuntime).GetMethod("DictionaryEntriesFromIEnumeratorKvp", BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo methodInfo = method.MakeGenericMethod(underlyingType, underlyingType2);
				return (IEnumerable<DictionaryEntry>)methodInfo.Invoke(null, new object[1] { items });
			}
			return DictionaryEntriesFromIEnumerator(items);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("GetItemsException", dictionaryType), ex);
		}
	}

	public override int AttachedPropertyCount(object instance)
	{
		try
		{
			return AttachablePropertyServices.GetAttachedPropertyCount(instance);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("APSException", instance));
		}
	}

	public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
	{
		try
		{
			KeyValuePair<AttachableMemberIdentifier, object>[] array = null;
			int attachedPropertyCount = AttachablePropertyServices.GetAttachedPropertyCount(instance);
			if (attachedPropertyCount > 0)
			{
				array = new KeyValuePair<AttachableMemberIdentifier, object>[attachedPropertyCount];
				AttachablePropertyServices.CopyPropertiesTo(instance, array, 0);
			}
			return array;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("APSException", instance));
		}
	}

	public override void SetConnectionId(object root, int connectionId, object instance)
	{
		try
		{
			if (root is IComponentConnector componentConnector)
			{
				componentConnector.Connect(connectionId, instance);
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("SetConnectionId"), ex);
		}
	}

	public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
	{
		try
		{
			if (obj is ISupportInitialize supportInitialize)
			{
				if (begin)
				{
					supportInitialize.BeginInit();
				}
				else
				{
					supportInitialize.EndInit();
				}
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("InitializationGuard", xamlType), ex);
		}
	}

	public override object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider)
	{
		try
		{
			return me.ProvideValue(serviceProvider);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("ProvideValue", me.GetType()), ex);
		}
	}

	public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
	{
		try
		{
			if (obj is IUriContext uriContext)
			{
				uriContext.BaseUri = baseUri;
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("AddDictionary", xamlType), ex);
		}
	}

	public override void SetXmlInstance(object inst, XamlMember property, XData xData)
	{
		object value = GetValue(inst, property, failIfWriteOnly: true);
		if (!(value is IXmlSerializable xmlSerializable))
		{
			throw CreateException(SR.Get("XmlDataNull", property.Name));
		}
		if (!(xData.XmlReader is XmlReader reader))
		{
			throw new XamlInternalException(SR.Get("XmlValueNotReader", property.Name));
		}
		try
		{
			xmlSerializable.ReadXml(reader);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("SetXmlInstance", property), ex);
		}
	}

	public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter)
	{
		return converter.ConverterInstance;
	}

	public override object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent)
	{
		try
		{
			XamlDeferringLoader converterInstance = GetConverterInstance(deferringLoader);
			if (converterInstance == null)
			{
				throw new XamlObjectWriterException(SR.Get("DeferringLoaderInstanceNull", deferringLoader));
			}
			return converterInstance.Load(deferredContent, serviceContext);
		}
		catch (Exception ex)
		{
			if (deferredContent is IXamlIndexingReader { CurrentIndex: >=0 } xamlIndexingReader)
			{
				xamlIndexingReader.CurrentIndex = -1;
			}
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlException)
			{
				throw;
			}
			throw CreateException(SR.Get("DeferredLoad"), ex);
		}
	}

	public override XamlReader DeferredSave(IServiceProvider serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, object value)
	{
		try
		{
			XamlDeferringLoader converterInstance = GetConverterInstance(deferringLoader);
			if (converterInstance == null)
			{
				throw new XamlObjectWriterException(SR.Get("DeferringLoaderInstanceNull", deferringLoader));
			}
			return converterInstance.Save(value, serviceContext);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is XamlException)
			{
				throw;
			}
			throw CreateException(SR.Get("DeferredSave"), ex);
		}
	}

	public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
	{
		try
		{
			return member.Invoker.ShouldSerializeValue(instance);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("ShouldSerializeFailed", member));
		}
	}

	private object CreateObjectWithTypeConverter(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value)
	{
		TypeConverter converterInstance = GetConverterInstance(ts);
		if (converterInstance != null)
		{
			if (_ignoreCanConvert && value.GetType() == typeof(string))
			{
				return converterInstance.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
			}
			if (converterInstance.CanConvertFrom(value.GetType()))
			{
				return converterInstance.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
			}
			return value;
		}
		return value;
	}

	protected virtual Delegate CreateDelegate(Type delegateType, object target, string methodName)
	{
		return SafeReflectionInvoker.CreateDelegate(delegateType, target, methodName);
	}

	internal XamlRuntimeSettings GetSettings()
	{
		return new XamlRuntimeSettings
		{
			IgnoreCanConvert = _ignoreCanConvert
		};
	}

	private XamlException CreateException(string message)
	{
		return CreateException(message, null);
	}

	private XamlException CreateException(string message, Exception innerException)
	{
		XamlException ex = ((!_isWriter) ? ((XamlException)new XamlObjectReaderException(message, innerException)) : ((XamlException)new XamlObjectWriterException(message, innerException)));
		if (LineInfo == null)
		{
			return ex;
		}
		return LineInfo.WithLineInfo(ex);
	}

	private IEnumerator GetItems(object collection, XamlType collectionType)
	{
		IEnumerator items;
		try
		{
			items = collectionType.Invoker.GetItems(collection);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw CreateException(SR.Get("GetItemsException", collectionType), UnwrapTargetInvocationException(ex));
		}
		if (items == null)
		{
			throw CreateException(SR.Get("GetItemsReturnedNull", collectionType));
		}
		return items;
	}

	private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIDictionaryEnumerator(IDictionaryEnumerator enumerator)
	{
		while (enumerator.MoveNext())
		{
			yield return enumerator.Entry;
		}
	}

	private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumerator(IEnumerator enumerator)
	{
		while (enumerator.MoveNext())
		{
			yield return (DictionaryEntry)enumerator.Current;
		}
	}

	private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumeratorKvp<TKey, TValue>(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
	{
		while (enumerator.MoveNext())
		{
			yield return new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);
		}
	}
}
