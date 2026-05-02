using System;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal class XbfXamlMember : IXbfMember
{
	private DirectUIXamlMember _xamlMember;

	private IXmpXamlType _xmp;

	public bool IsAttachable => _xamlMember.IsAttachable;

	public bool IsDependencyProperty => _xamlMember.IsDependencyProperty;

	public bool IsReadOnly => _xamlMember.IsReadOnly;

	public string Name => _xamlMember.Name;

	public IXbfType TargetType => _xmp.GetXmpXamlType((DirectUIXamlType)_xamlMember.TargetType);

	public IXbfType Type => _xmp.GetXmpXamlType((DirectUIXamlType)_xamlMember.Type);

	public XbfXamlMember(DirectUIXamlMember member, IXmpXamlType xmp)
	{
		_xamlMember = member;
		_xmp = xmp;
	}

	public void SetValue(object instance, object value)
	{
		throw new NotImplementedException();
	}

	public object GetValue(object instance)
	{
		throw new NotImplementedException();
	}
}
