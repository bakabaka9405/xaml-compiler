using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

[ComImport]
[Guid("d0aa6fc8-087f-46cf-b36a-7e68f8295ceb")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IXbfMember
{
	bool IsAttachable
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsDependencyProperty
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	bool IsReadOnly
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get;
	}

	string Name
	{
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	IXbfType TargetType
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	IXbfType Type
	{
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[return: MarshalAs(UnmanagedType.IInspectable)]
	object GetValue([MarshalAs(UnmanagedType.IInspectable)] object instance);

	void SetValue([MarshalAs(UnmanagedType.IInspectable)] object instance, [MarshalAs(UnmanagedType.IInspectable)] object value);
}
