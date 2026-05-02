using System;
using System.Linq;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class Language
{
	private static Language[] languages = new Language[4]
	{
		new Language("C#", ".g.i.cs", ".g.cs", isManaged: true, isStringNullable: true, isExperimental: false, () => new CSharpAppPass1(), () => new CSharpPagePass2(), () => new CSharpPagePass1(), () => new CSharpPagePass2(), null, null, null, null, () => new CSharpTypeInfoPass2(), null, null),
		new Language("VB", ".g.i.vb", ".g.vb", isManaged: true, isStringNullable: true, isExperimental: false, () => new VisualBasicAppPass1(), () => new VisualBasicPagePass2(), () => new VisualBasicPagePass1(), () => new VisualBasicPagePass2(), null, null, null, null, () => new VisualBasicTypeInfoPass2(), null, null),
		new Language("C++", ".g.h", ".g.hpp", isManaged: false, isStringNullable: true, isExperimental: false, () => new MoComCppAppPass1(), () => new MoComCppAppPass2(), () => new MoComCppPagePass1(), () => new MoComCppPagePass2(), null, () => new CppWinRT_XamlMetaDataProviderPass2(), () => new MoComCppTypeInfoPass1(), () => new MoComCppTypeInfoPass1Impl(), () => new MoComCppTypeInfoPass2(), () => new MoComCppBindingInfoPass1(), () => new MoComCppBindingInfoPass2()),
		new Language("CppWinRT", ".xaml.g.h", ".xaml.g.hpp", isManaged: false, isStringNullable: false, isExperimental: false, () => new CppWinRT_AppPass1(), () => new CppWinRT_AppPass2(), () => new CppWinRT_PagePass1(), () => new CppWinRT_PagePass2(), () => new CppWinRT_XamlMetaDataProviderPass1(), () => new CppWinRT_XamlMetaDataProviderPass2(), () => new CppWinRT_TypeInfoPass1(), () => new CppWinRT_TypeInfoPass1Impl(), () => new CppWinRT_TypeInfoPass2(), () => new CppWinRT_BindingInfoPass1(), () => new CppWinRT_BindingInfoPass2())
	};

	public string Name { get; }

	public bool IsExperimental { get; }

	public bool IsManaged { get; }

	public bool IsNative { get; }

	public bool IsStringNullable { get; }

	public string Pass1Extension { get; }

	public string Pass2Extension { get; }

	public CodeGeneratorDelegate AppPass1CodeGenerator { get; }

	public CodeGeneratorDelegate AppPass2CodeGenerator { get; }

	public CodeGeneratorDelegate PagePass1CodeGenerator { get; }

	public CodeGeneratorDelegate PagePass2CodeGenerator { get; }

	public CodeGeneratorDelegate XamlMetaDataProviderPass1 { get; }

	public CodeGeneratorDelegate XamlMetaDataProviderPass2 { get; }

	public CodeGeneratorDelegate TypeInfoPass1CodeGenerator { get; }

	public CodeGeneratorDelegate TypeInfoPass1ImplCodeGenerator { get; }

	public CodeGeneratorDelegate TypeInfoPass2CodeGenerator { get; }

	public CodeGeneratorDelegate BindingInfoPass1CodeGenerator { get; }

	public CodeGeneratorDelegate BindingInfoPass2CodeGenerator { get; }

	public static Language Parse(string name)
	{
		Language language = languages.Where((Language l) => l.Name == name).FirstOrDefault();
		if (language == null)
		{
			throw new ArgumentOutOfRangeException(ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LanguageUnsupported, name));
		}
		return language;
	}

	private Language(string name, string pass1Extension, string pass2Extension, bool isManaged, bool isStringNullable, bool isExperimental, CodeGeneratorDelegate appPass1CodeGenerator, CodeGeneratorDelegate appPass2CodeGenerator, CodeGeneratorDelegate pagePass1CodeGenerator, CodeGeneratorDelegate pagePass2CodeGenerator, CodeGeneratorDelegate xamlMetaDataProviderPass1, CodeGeneratorDelegate xamlMetaDataProviderPass2, CodeGeneratorDelegate typeInfoPass1CodeGenerator, CodeGeneratorDelegate typeInfoPass1ImplCodeGenerator, CodeGeneratorDelegate typeInfoPass2CodeGenerator, CodeGeneratorDelegate bindingInfoPass1CodeGenerator, CodeGeneratorDelegate bindingInfoPass2CodeGenerator)
	{
		Name = name;
		IsExperimental = isExperimental;
		IsManaged = isManaged;
		IsNative = !isManaged;
		IsStringNullable = isStringNullable;
		Pass1Extension = pass1Extension;
		Pass2Extension = pass2Extension;
		AppPass1CodeGenerator = appPass1CodeGenerator;
		AppPass2CodeGenerator = appPass2CodeGenerator;
		PagePass1CodeGenerator = pagePass1CodeGenerator;
		PagePass2CodeGenerator = pagePass2CodeGenerator;
		XamlMetaDataProviderPass1 = xamlMetaDataProviderPass1;
		XamlMetaDataProviderPass2 = xamlMetaDataProviderPass2;
		TypeInfoPass1CodeGenerator = typeInfoPass1CodeGenerator;
		TypeInfoPass1ImplCodeGenerator = typeInfoPass1ImplCodeGenerator;
		TypeInfoPass2CodeGenerator = typeInfoPass2CodeGenerator;
		BindingInfoPass1CodeGenerator = bindingInfoPass1CodeGenerator;
		BindingInfoPass2CodeGenerator = bindingInfoPass2CodeGenerator;
	}
}
