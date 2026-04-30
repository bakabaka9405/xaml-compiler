namespace System.Xaml;

public class XamlSchemaContextSettings
{
	public bool SupportMarkupExtensionsWithDuplicateArity { get; set; }

	public bool FullyQualifyAssemblyNamesInClrNamespaces { get; set; }

	public XamlSchemaContextSettings()
	{
	}

	public XamlSchemaContextSettings(XamlSchemaContextSettings settings)
	{
		if (settings != null)
		{
			SupportMarkupExtensionsWithDuplicateArity = settings.SupportMarkupExtensionsWithDuplicateArity;
			FullyQualifyAssemblyNamesInClrNamespaces = settings.FullyQualifyAssemblyNamesInClrNamespaces;
		}
	}
}
