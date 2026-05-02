using System.Collections.Generic;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class XamlCodeGenerator
{
	private Language _language;

	private bool _isPass1;

	private XamlProjectInfo _projectInfo;

	private XamlSchemaCodeInfo _schemaInfo;

	public XamlCodeGenerator(Language language, bool isPass1, XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo)
	{
		_language = language;
		_isPass1 = isPass1;
		_projectInfo = projectInfo;
		_schemaInfo = schemaInfo;
	}

	public List<FileNameAndContentPair> GenerateCodeBehind(XamlClassCodeInfo codeInfo, out IEnumerable<FileNameAndChecksumPair> xamlFilesChecksumPairs)
	{
		xamlFilesChecksumPairs = null;
		CodeGeneratorDelegate codeGeneratorDelegate = ((!codeInfo.IsApplication) ? (_isPass1 ? _language.PagePass1CodeGenerator : _language.PagePass2CodeGenerator) : (_isPass1 ? _language.AppPass1CodeGenerator : _language.AppPass2CodeGenerator));
		if (codeGeneratorDelegate == null)
		{
			return null;
		}
		T4Base t4Base = codeGeneratorDelegate();
		PageDefinition pageDefinition = new PageDefinition(_projectInfo, _schemaInfo)
		{
			CodeInfo = codeInfo
		};
		t4Base.SetModel(_projectInfo, _schemaInfo, pageDefinition);
		string contents = t4Base.TransformText();
		xamlFilesChecksumPairs = pageDefinition.XamlFileFullPathAndCheckSums;
		string fileName = codeInfo.BaseFileName + (_isPass1 ? _language.Pass1Extension : _language.Pass2Extension);
		List<FileNameAndContentPair> list = new List<FileNameAndContentPair>();
		list.Add(new FileNameAndContentPair(fileName, contents));
		return list;
	}

	public List<FileNameAndContentPair> GenerateTypeInfo(ClassName appXamlInfo)
	{
		CodeGeneratorDelegate codeGenDelegate = (_isPass1 ? _language.TypeInfoPass1CodeGenerator : _language.TypeInfoPass2CodeGenerator);
		string text = GenerateTypeInfoCode(codeGenDelegate, appXamlInfo);
		if (text == null)
		{
			return null;
		}
		List<FileNameAndContentPair> list = new List<FileNameAndContentPair>();
		string text2 = "XamlTypeInfo" + (_isPass1 ? _language.Pass1Extension : _language.Pass2Extension);
		if (!_isPass1 && text2.EndsWith(".g.hpp"))
		{
			text2 = "XamlTypeInfo.g.cpp";
		}
		list.Add(new FileNameAndContentPair(text2, text));
		if (_isPass1)
		{
			text = GenerateTypeInfoCode(_language.XamlMetaDataProviderPass1, appXamlInfo);
			if (text != null)
			{
				text2 = "XamlMetaDataProvider.h";
				list.Add(new FileNameAndContentPair(text2, text));
			}
			text = GenerateTypeInfoCode(_language.XamlMetaDataProviderPass2, appXamlInfo);
			if (text != null)
			{
				text2 = "XamlLibMetadataProvider.g.cpp";
				list.Add(new FileNameAndContentPair(text2, text));
			}
			text = GenerateTypeInfoCode(_language.TypeInfoPass1ImplCodeGenerator, appXamlInfo);
			if (text != null)
			{
				text2 = "XamlTypeInfo.Impl.g.cpp";
				list.Add(new FileNameAndContentPair(text2, text));
			}
		}
		return list;
	}

	public List<FileNameAndContentPair> GenerateBindingInfo(Dictionary<string, XamlType> observableVectorTypes, Dictionary<string, XamlType> observableMapTypes, Dictionary<string, XamlMember> bindingSetters, bool eventBindingUsed)
	{
		List<FileNameAndContentPair> list = new List<FileNameAndContentPair>();
		string fileName = "XamlBindingInfo" + (_isPass1 ? _language.Pass1Extension : _language.Pass2Extension);
		T4Base t4Base = (_isPass1 ? _language.BindingInfoPass1CodeGenerator() : _language.BindingInfoPass2CodeGenerator());
		t4Base.SetModel(_projectInfo, _schemaInfo, new BindingInfoDefinition(_projectInfo, _schemaInfo)
		{
			ObservableVectorTypes = observableVectorTypes,
			ObservableMapTypes = observableMapTypes,
			BindingSetters = bindingSetters,
			EventBindingUsed = eventBindingUsed
		});
		string text = t4Base.TransformText();
		if (text != null)
		{
			list.Add(new FileNameAndContentPair(fileName, text));
		}
		return list;
	}

	private string GenerateTypeInfoCode(CodeGeneratorDelegate codeGenDelegate, ClassName appXamlInfo)
	{
		if (codeGenDelegate == null)
		{
			return null;
		}
		T4Base t4Base = codeGenDelegate();
		t4Base.SetModel(_projectInfo, _schemaInfo, new TypeInfoDefinition(_projectInfo, _schemaInfo)
		{
			AppXamlInfo = appXamlInfo
		});
		return t4Base.TransformText();
	}
}
