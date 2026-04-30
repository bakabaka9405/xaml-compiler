namespace System.Xaml;

public abstract class XamlDeferringLoader
{
	public abstract object Load(XamlReader xamlReader, IServiceProvider serviceProvider);

	public abstract XamlReader Save(object value, IServiceProvider serviceProvider);
}
