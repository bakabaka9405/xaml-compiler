using System.Diagnostics;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[DebuggerDisplay("{Type.Name}  {DeclaringType.Name}.{Name}")]
internal class InternalXamlUserMemberInfo
{
	public string Name { get; set; }

	public InternalTypeEntry Type { get; set; }

	public bool IsValueType { get; set; }

	public bool IsString { get; set; }

	public bool IsSignedChar { get; set; }

	public bool IsInvalidType { get; set; }

	public bool IsEnum { get; set; }

	public bool IsDeprecated { get; set; }

	public InternalTypeEntry DeclaringType { get; set; }

	public bool IsDependencyProperty { get; set; }

	public bool HasPublicSetter { get; set; }

	public bool HasPublicGetter { get; set; }

	public bool IsAttachable { get; set; }

	public InternalTypeEntry TargetType { get; set; }

	public bool IsEvent { get; set; }

	public void Init(XamlMember xamlMember, XamlSchemaCodeInfo schemaInfo)
	{
		Type = schemaInfo.AddReturnTypeStub(xamlMember.Type);
		IsAttachable = xamlMember.IsAttachable;
		IsEvent = xamlMember.IsEvent;
		DirectUIXamlMember directUIXamlMember = xamlMember as DirectUIXamlMember;
		if (IsEvent)
		{
			HasPublicGetter = false;
			HasPublicSetter = false;
		}
		else
		{
			HasPublicSetter = ((directUIXamlMember == null) ? xamlMember.IsWritePublic : directUIXamlMember.HasPublicSetter);
			HasPublicGetter = ((directUIXamlMember == null) ? xamlMember.IsReadPublic : directUIXamlMember.HasPublicGetter);
		}
		IsDependencyProperty = !(directUIXamlMember == null) && directUIXamlMember.IsDependencyProperty;
		DirectUIXamlType directUIXamlType = xamlMember.Type as DirectUIXamlType;
		IsValueType = !(directUIXamlType == null) && directUIXamlType.IsValueType;
		IsString = !(directUIXamlType == null) && directUIXamlType.IsString();
		IsSignedChar = !(directUIXamlType == null) && directUIXamlType.IsSignedChar;
		IsInvalidType = !(directUIXamlType == null) && directUIXamlType.IsInvalidType;
		IsEnum = !(directUIXamlType == null) && directUIXamlType.UnderlyingType.IsEnum;
		IsDeprecated = !(directUIXamlMember == null) && directUIXamlMember.IsDeprecated;
		IsDeprecated |= !(directUIXamlType == null) && directUIXamlType.IsDeprecated;
		DirectUIXamlType directUIXamlType2 = directUIXamlMember.DeclaringType as DirectUIXamlType;
		IsDeprecated |= !(directUIXamlType2 == null) && directUIXamlType2.IsDeprecated;
		if (xamlMember.IsAttachable && xamlMember.TargetType != null)
		{
			TargetType = schemaInfo.AddTypeAndProperties(xamlMember.TargetType);
		}
	}
}
