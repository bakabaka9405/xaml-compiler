using System.Xaml;

namespace System.Windows.Markup;

[ContentProperty("Name")]
public class Reference : MarkupExtension
{
	[ConstructorArgument("name")]
	public string Name { get; set; }

	public Reference()
	{
	}

	public Reference(string name)
	{
		Name = name;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		if (!(serviceProvider.GetService(typeof(IXamlNameResolver)) is IXamlNameResolver xamlNameResolver))
		{
			throw new InvalidOperationException(SR.Get("MissingNameResolver"));
		}
		if (string.IsNullOrEmpty(Name))
		{
			throw new InvalidOperationException(SR.Get("MustHaveName"));
		}
		object obj = xamlNameResolver.Resolve(Name);
		if (obj == null)
		{
			string[] names = new string[1] { Name };
			obj = xamlNameResolver.GetFixupToken(names, canAssignDirectly: true);
		}
		return obj;
	}
}
