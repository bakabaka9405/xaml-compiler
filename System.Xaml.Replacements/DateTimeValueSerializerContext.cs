using System.ComponentModel;
using System.Windows.Markup;

namespace System.Xaml.Replacements;

internal class DateTimeValueSerializerContext : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
{
	public IContainer Container => null;

	public object Instance => null;

	public PropertyDescriptor PropertyDescriptor => null;

	public ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor)
	{
		return null;
	}

	public ValueSerializer GetValueSerializerFor(Type type)
	{
		return null;
	}

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return false;
	}

	public object GetService(Type serviceType)
	{
		return null;
	}
}
