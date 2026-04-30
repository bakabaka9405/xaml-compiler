using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Runtime;

internal abstract class XamlRuntime
{
	public abstract IAddLineInfo LineInfo { get; set; }

	public abstract object CreateInstance(XamlType xamlType, object[] args);

	public abstract object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args);

	public abstract object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property);

	public abstract bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance);

	public abstract bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter);

	public abstract bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type);

	public abstract string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance);

	public abstract T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance);

	public abstract object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent);

	public abstract XamlReader DeferredSave(IServiceProvider context, XamlValueConverter<XamlDeferringLoader> deferringLoader, object value);

	public object GetValue(object obj, XamlMember property)
	{
		return GetValue(obj, property, failIfWriteOnly: true);
	}

	public abstract object GetValue(object obj, XamlMember property, bool failIfWriteOnly);

	public abstract void SetValue(object obj, XamlMember property, object value);

	public abstract void SetUriBase(XamlType xamlType, object obj, Uri baseUri);

	public abstract void SetXmlInstance(object inst, XamlMember property, XData xData);

	public abstract void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType);

	public abstract void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key);

	public abstract IList<object> GetCollectionItems(object collection, XamlType collectionType);

	public abstract IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType);

	public abstract int AttachedPropertyCount(object instance);

	public abstract KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance);

	public abstract void SetConnectionId(object root, int connectionId, object instance);

	public abstract void InitializationGuard(XamlType xamlType, object obj, bool begin);

	public abstract object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider);

	public abstract ShouldSerializeResult ShouldSerialize(XamlMember member, object instance);

	public abstract TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter) where TConverterBase : class;
}
