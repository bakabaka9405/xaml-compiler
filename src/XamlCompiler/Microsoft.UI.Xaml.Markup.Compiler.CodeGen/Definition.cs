using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class Definition
{
	private IEnumerable<TypeGenInfo> _typeInfos;

	public XamlProjectInfo ProjectInfo { get; }

	public XamlSchemaCodeInfo SchemaInfo { get; }

	public IEnumerable<TypeGenInfo> TypeInfos
	{
		get
		{
			if (_typeInfos == null)
			{
				Dictionary<string, TypeGenInfo> dictionary = new Dictionary<string, TypeGenInfo>();
				foreach (InternalTypeEntry item in SchemaInfo.TypeTable)
				{
					dictionary.Add(item.StandardName, new TypeGenInfo(item, ProjectInfo.GenerateIncrementalTypeInfo));
				}
				foreach (InternalXamlUserTypeInfo item2 in SchemaInfo.UserTypeInfo)
				{
					dictionary[item2.StandardName].UserTypeInfo = item2;
				}
				_typeInfos = dictionary.Values.OrderBy((TypeGenInfo x) => x.StandardName.Length);
			}
			return _typeInfos;
		}
	}

	public Definition(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo)
	{
		ProjectInfo = projectInfo;
		SchemaInfo = schemaInfo;
	}
}
