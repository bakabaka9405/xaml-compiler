using System.Xaml;

namespace System.Windows.Markup;

public class XamlSetMarkupExtensionEventArgs : XamlSetValueEventArgs
{
	public MarkupExtension MarkupExtension => base.Value as MarkupExtension;

	public IServiceProvider ServiceProvider { get; private set; }

	internal XamlType CurrentType { get; set; }

	internal object TargetObject { get; private set; }

	public XamlSetMarkupExtensionEventArgs(XamlMember member, MarkupExtension value, IServiceProvider serviceProvider)
		: base(member, value)
	{
		ServiceProvider = serviceProvider;
	}

	internal XamlSetMarkupExtensionEventArgs(XamlMember member, MarkupExtension value, IServiceProvider serviceProvider, object targetObject)
		: this(member, value, serviceProvider)
	{
		TargetObject = targetObject;
	}

	public override void CallBase()
	{
		if (!(CurrentType != null))
		{
			return;
		}
		XamlType baseType = CurrentType.BaseType;
		if (baseType != null)
		{
			CurrentType = baseType;
			if (baseType.SetMarkupExtensionHandler != null)
			{
				baseType.SetMarkupExtensionHandler(TargetObject, this);
			}
		}
	}
}
