using System;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

[ComImport]
[Guid("a50fc345-4c61-411b-8a68-13da7b7c4ee4")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IXbfType
{
	IXbfType BaseType
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	IXbfMember ContentProperty
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	string FullName
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	bool IsArray
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsCollection
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsConstructible
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsDictionary
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsMarkupExtension
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsBindable
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	IXbfType ItemType
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	IXbfType KeyType
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	IXbfType BoxedType
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	Type UnderlyingType { get; }

	[return: MarshalAs(UnmanagedType.IInspectable)]
	object ActivateInstance();

	[return: MarshalAs(UnmanagedType.IInspectable)]
	object CreateFromString([MarshalAs(UnmanagedType.BStr)] string value);

	[return: MarshalAs(UnmanagedType.Interface)]
	IXbfMember GetMember([MarshalAs(UnmanagedType.BStr)] string name);

	void AddToVector([MarshalAs(UnmanagedType.IInspectable)] object instance, [MarshalAs(UnmanagedType.IInspectable)] object value);

	void AddToMap([MarshalAs(UnmanagedType.IInspectable)] object instance, [MarshalAs(UnmanagedType.IInspectable)] object key, [MarshalAs(UnmanagedType.IInspectable)] object value);

	void RunInitializer();
}
