using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IValueSerializerContext : ITypeDescriptorContext, IServiceProvider
{
	ValueSerializer GetValueSerializerFor(Type type);

	ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor);
}
