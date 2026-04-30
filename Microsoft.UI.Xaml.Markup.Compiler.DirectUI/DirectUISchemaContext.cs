using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class DirectUISchemaContext : XamlSchemaContext
{
	internal static List<string> DirectUI2010Paths = new List<string>
	{
		"Microsoft.UI.Xaml", "Microsoft.UI.Xaml.Automation", "Microsoft.UI.Xaml.Automation.Peers", "Microsoft.UI.Xaml.Automation.Provider", "Microsoft.UI.Xaml.Controls", "Microsoft.UI.Xaml.Controls.Primitives", "Microsoft.UI.Xaml.Data", "Microsoft.UI.Xaml.Documents", "Microsoft.UI.Xaml.Input", "Microsoft.UI.Xaml.Interop",
		"Microsoft.UI.Xaml.Markup", "Microsoft.UI.Xaml.Media", "Microsoft.UI.Xaml.Media.Animation", "Microsoft.UI.Xaml.Media.Imaging", "Microsoft.UI.Xaml.Media.Media3D", "Microsoft.UI.Xaml.Navigation", "Microsoft.UI.Xaml.Resources", "Microsoft.UI.Xaml.Shapes", "Microsoft.UI.Xaml.Threading", "Windows.UI",
		"Windows.UI.Text"
	};

	private static ReadOnlyCollection<string> DirectUIXamlNamespaces = new ReadOnlyCollection<string>(new string[2] { "http://schemas.microsoft.com/winfx/2006/xaml/presentation", "http://schemas.microsoft.com/windows/2010/directui" });

	private Assembly localAssembly;

	private DirectUISystem directUISystem;

	private IDirectUIXamlLanguage directUIXamlLanguage;

	private List<string> systemExtraReferenceItems;

	private IEnumerable<DirectUIAssembly> systemExtraAssemblies;

	private ISet<string> staticLibraryAssemblies;

	private Dictionary<string, Dictionary<string, ProxyDirectUIXamlType>> proxyTypes;

	private List<Assembly> managedProjectionAssemblies = new List<Assembly>();

	private Dictionary<Type, XamlType> masterTypeTable = new Dictionary<Type, XamlType>();

	private Dictionary<string, XamlType> uiXamlCache = new Dictionary<string, XamlType>();

	private Dictionary<string, List<string>> usingNamespaces = new Dictionary<string, List<string>>();

	private Dictionary<string, XamlType> masterTypeTableByFullName = new Dictionary<string, XamlType>();

	private Dictionary<string, XamlType> domFullTypeNameCache = new Dictionary<string, XamlType>();

	private Lazy<List<XamlCompileWarning>> warningMessages = new Lazy<List<XamlCompileWarning>>(() => new List<XamlCompileWarning>());

	private Lazy<List<XamlCompileError>> errorMessages = new Lazy<List<XamlCompileError>>(() => new List<XamlCompileError>());

	private string windowsSdkPath;

	internal List<XamlCompileWarning> SchemaWarnings => warningMessages.Value;

	internal List<XamlCompileError> SchemaErrors => errorMessages.Value;

	internal TypeResolver TypeResolver { get; set; }

	internal DirectUISystem DirectUISystem => directUISystem;

	internal IDirectUIXamlLanguage DirectUIXamlLanguage => directUIXamlLanguage;

	internal Dictionary<string, Dictionary<string, ProxyDirectUIXamlType>> ProxyTypes
	{
		get
		{
			if (proxyTypes == null)
			{
				Dictionary<string, ProxyDirectUIXamlType> value = new Dictionary<string, ProxyDirectUIXamlType>
				{
					{
						TypeProxyMetadata.NullExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.NullExtension, this)
					},
					{
						TypeProxyMetadata.StaticResourceExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.StaticResourceExtension, this)
					},
					{
						TypeProxyMetadata.ThemeResourceExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.ThemeResourceExtension, this)
					},
					{
						TypeProxyMetadata.CustomResourceExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.CustomResourceExtension, this)
					},
					{
						TypeProxyMetadata.TemplateBindingExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.TemplateBindingExtension, this)
					},
					{
						TypeProxyMetadata.BindExtension.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.BindExtension, this)
					},
					{
						TypeProxyMetadata.Properties.Name,
						new ProxyDirectUIXamlType(TypeProxyMetadata.Properties, this)
					}
				};
				proxyTypes = new Dictionary<string, Dictionary<string, ProxyDirectUIXamlType>>
				{
					{ "http://schemas.microsoft.com/winfx/2006/xaml/presentation", value },
					{ "http://schemas.microsoft.com/windows/2010/directui", value }
				};
			}
			return proxyTypes;
		}
	}

	public DirectUISchemaContext(IEnumerable<Assembly> referenceAssemblies, List<string> systemExtraReferenceItems, Assembly localAssembly, ISet<string> staticLibraryAssemblies, string sdkPath, bool isStringNullable)
		: base(DirectUIAssembly.Wrap(referenceAssemblies))
	{
		if (referenceAssemblies == null)
		{
			throw new ArgumentNullException("referenceAssemblies");
		}
		windowsSdkPath = sdkPath;
		directUISystem = new DirectUISystem(base.ReferenceAssemblies);
		directUIXamlLanguage = new DirectUIXamlLanguage(this, isStringNullable);
		this.systemExtraReferenceItems = systemExtraReferenceItems;
		this.localAssembly = localAssembly;
		this.staticLibraryAssemblies = staticLibraryAssemblies;
		foreach (DirectUIAssembly referenceAssembly in base.ReferenceAssemblies)
		{
			AssemblyName name = referenceAssembly.GetName();
			if (KS.EqIgnoreCase(name.Name, "System.Runtime.WindowsRuntime.UI.Xaml"))
			{
				managedProjectionAssemblies.Add(referenceAssembly);
			}
			else if (KS.EqIgnoreCase(name.Name, "System.Runtime.WindowsRuntime"))
			{
				managedProjectionAssemblies.Add(referenceAssembly);
			}
		}
	}

	internal DirectUISchemaContext()
	{
	}

	public override XamlType GetXamlType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		type = EnsureIsLmrType(type);
		if (!masterTypeTable.TryGetValue(type, out var value))
		{
			value = new DirectUIXamlType(type, this);
			masterTypeTable.Add(type, value);
			if (masterTypeTableByFullName.ContainsKey(type.FullName))
			{
				SchemaErrors.Add(new XamlErrorDuplicateType(type.FullName));
			}
			else
			{
				masterTypeTableByFullName.Add(type.FullName, value);
			}
		}
		return value;
	}

	public override ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
	{
		return LookupAllXamlTypes(xamlNamespace);
	}

	public bool IsLocalAssembly(DirectUIAssembly asm)
	{
		if (asm != null)
		{
			if (asm.WrappedAssembly == localAssembly)
			{
				return true;
			}
			if (staticLibraryAssemblies != null && staticLibraryAssemblies.Contains(asm.Location))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public override XamlDirective GetXamlDirective(string xamlNamespace, string name)
	{
		if (name == "SuppressXamlTrimWarnings" && xamlNamespace == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			return SuppressXamlTrimWarningsDirective.Value;
		}
		XamlDirective xamlDirective = base.GetXamlDirective(xamlNamespace, name);
		if (xamlDirective == XamlLanguage.Arguments || xamlDirective == XamlLanguage.AsyncRecords || xamlDirective == XamlLanguage.ClassAttributes || xamlDirective == XamlLanguage.ClassModifier || xamlDirective == XamlLanguage.Code || xamlDirective == XamlLanguage.Members || xamlDirective == XamlLanguage.Subclass || xamlDirective == XamlLanguage.SynchronousMode || xamlDirective == XamlLanguage.TypeArguments || xamlDirective == XamlLanguage.FactoryMethod)
		{
			xamlDirective = null;
		}
		return xamlDirective;
	}

	internal XamlType GetXamlType(string fullName)
	{
		if (!masterTypeTableByFullName.TryGetValue(fullName, out var value))
		{
			value = null;
			Type typeByFullName = TypeResolver.GetTypeByFullName(fullName);
			if (typeByFullName != null)
			{
				value = GetXamlType(typeByFullName);
			}
		}
		return value;
	}

	internal XamlType GetProxyType(string xamlNamespace, string name)
	{
		if (ProxyTypes.TryGetValue(xamlNamespace, out var value))
		{
			if (value.TryGetValue(name, out var value2))
			{
				return value2;
			}
			string typeExtensionName = GetTypeExtensionName(name);
			if (value.TryGetValue(typeExtensionName, out value2))
			{
				return value2;
			}
		}
		return null;
	}

	protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		string key = $"{xamlNamespace}.{name}";
		XamlType xamlType = null;
		ApiInformation apiInformation = null;
		Platform platform = Platform.Any;
		if (domFullTypeNameCache.ContainsKey(key))
		{
			return domFullTypeNameCache[key];
		}
		if (KS.ContainsString(XamlLanguage.XamlNamespaces, xamlNamespace))
		{
			xamlType = DirectUIXamlLanguage.LookupXamlObjects(name);
		}
		else
		{
			string text = xamlNamespace;
			if (xamlNamespace.IsConditionalNamespace())
			{
				try
				{
					ConditionalNamespace conditionalNamespace = ConditionalNamespace.Parse(xamlNamespace);
					text = conditionalNamespace.UnconditionalNamespace;
					apiInformation = conditionalNamespace.ApiInfo;
					platform = conditionalNamespace.PlatConditional;
				}
				catch (ParseException)
				{
					return null;
				}
			}
			if (KS.ContainsString(DirectUIXamlNamespaces, text))
			{
				xamlType = GetDirectUIXamlType(name);
				if (xamlType == null)
				{
					return GetProxyType(text, name);
				}
			}
			else
			{
				xamlType = GetXamlTypeFromUsing(text, name, typeArguments);
			}
		}
		if (xamlType != null)
		{
			if (!(xamlType is DirectUIXamlType) || !(xamlType.UnderlyingType is MetadataOnlyCommonType))
			{
				xamlType = GetXamlType(xamlType.UnderlyingType);
			}
			DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
			if ((apiInformation != null || platform != Platform.Any) && directUIXamlType != null)
			{
				directUIXamlType = new DirectUIXamlType(xamlType.UnderlyingType, this, apiInformation, platform);
				xamlType = directUIXamlType;
			}
			domFullTypeNameCache[key] = xamlType;
		}
		return xamlType;
	}

	private ICollection<XamlType> LookupAllXamlTypes(string xamlNamespace)
	{
		List<XamlType> list = new List<XamlType>();
		if (!KS.ContainsString(DirectUIXamlNamespaces, xamlNamespace))
		{
			throw new NotImplementedException(ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_GetAllXamlTypeNotImpl, xamlNamespace));
		}
		if (directUISystem.PlatformAssemblies.Count == 0)
		{
			SchemaErrors.Add(new XamlSchemaError_WRTAssembliesMissing());
			return null;
		}
		foreach (DirectUIAssembly platformAssembly in directUISystem.PlatformAssemblies)
		{
			Type[] types = platformAssembly.GetTypes();
			Type[] array = types;
			foreach (Type type in array)
			{
				int length = type.FullName.LastIndexOf('.');
				string s = type.FullName.Substring(0, length);
				if (KS.ContainsString(DirectUI2010Paths, s))
				{
					XamlType xamlType = GetXamlType(type);
					list.Add(xamlType);
				}
			}
		}
		return list;
	}

	private Type EnsureIsLmrType(Type type)
	{
		Type type2 = type as MetadataOnlyCommonType;
		if (type2 == null)
		{
			foreach (XamlTypeUniverse xamlTypeUniverse in directUISystem.XamlTypeUniverses)
			{
				type2 = xamlTypeUniverse.FindType(type.FullName);
				if (type2 != null)
				{
					break;
				}
			}
		}
		return type2;
	}

	private XamlType GetXamlTypeFromUsing(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		if (!xamlNamespace.HasUsingPrefix())
		{
			return null;
		}
		string path = xamlNamespace.StripUsingPrefix();
		XamlType xamlTypeFromAssembliesAndPath = GetXamlTypeFromAssembliesAndPath(base.ReferenceAssemblies, path, name);
		if (xamlTypeFromAssembliesAndPath == null)
		{
			if (systemExtraAssemblies == null && systemExtraReferenceItems != null)
			{
				List<Assembly> list = new List<Assembly>();
				foreach (string systemExtraReferenceItem in systemExtraReferenceItems)
				{
					Assembly assembly = CompileXamlInternal.TryLoadAssembly(systemExtraReferenceItem);
					if (assembly != null)
					{
						list.Add(assembly);
					}
				}
				systemExtraAssemblies = DirectUIAssembly.Wrap(list);
			}
			if (systemExtraAssemblies != null)
			{
				xamlTypeFromAssembliesAndPath = GetXamlTypeFromAssembliesAndPath(systemExtraAssemblies, path, name);
			}
		}
		return xamlTypeFromAssembliesAndPath;
	}

	private XamlType GetDirectUIXamlType(string name)
	{
		if (directUISystem.PlatformAssemblies.Count == 0)
		{
			SchemaErrors.Add(new XamlSchemaError_WRTAssembliesMissing());
			return null;
		}
		if (uiXamlCache.TryGetValue(name, out var value))
		{
			return value;
		}
		Type directUIType = TypeResolver.GetDirectUIType(name);
		if (directUIType != null)
		{
			value = GetXamlType(directUIType);
		}
		if (value == null)
		{
			switch (name)
			{
			case "Point":
			case "Rect":
			case "Size":
				foreach (DirectUIAssembly platformAssembly in directUISystem.PlatformAssemblies)
				{
					value = GetXamlTypeFromAsmAndPath(platformAssembly, "Windows.Foundation", name);
					if (value == null)
					{
						value = GetXamlTypeFromAssembliesAndPath(managedProjectionAssemblies, "Windows.Foundation", name);
						if (value != null)
						{
							break;
						}
					}
				}
				break;
			}
		}
		if (value == null && managedProjectionAssemblies.Count > 0)
		{
			value = GetXamlTypeFromAssembliesAndPaths(managedProjectionAssemblies, DirectUI2010Paths, name);
		}
		if (value == null)
		{
			switch (name)
			{
			default:
				value = GetXamlTypeFromAssembliesAndPaths(base.ReferenceAssemblies, DirectUI2010Paths, name);
				break;
			case "StaticResourceExtension":
			case "ThemeResourceExtension":
			case "TemplateBindingExtension":
			case "BindingExtension":
				break;
			}
		}
		uiXamlCache.Add(name, value);
		return value;
	}

	private XamlType GetXamlTypeFromAssembliesAndPaths(IEnumerable<Assembly> asmList, IEnumerable<string> pathList, string name)
	{
		foreach (Assembly asm in asmList)
		{
			XamlType xamlType = null;
			if (FileHelpers.IsFacadeWinmd(asm, windowsSdkPath))
			{
				continue;
			}
			foreach (string path in pathList)
			{
				xamlType = GetXamlTypeFromAsmAndPath(asm, path, name);
				if (xamlType != null)
				{
					return xamlType;
				}
			}
		}
		return null;
	}

	private XamlType GetXamlTypeFromAssembliesAndPath(IEnumerable<Assembly> asmList, string path, string name)
	{
		foreach (Assembly asm in asmList)
		{
			XamlType xamlType = null;
			if (!FileHelpers.IsFacadeWinmd(asm, windowsSdkPath))
			{
				xamlType = GetXamlTypeFromAsmAndPath(asm, path, name);
				if (xamlType != null)
				{
					return xamlType;
				}
			}
		}
		return null;
	}

	private XamlType GetXamlTypeFromAsmAndPath(Assembly asm, string path, string name)
	{
		string name2 = path + "." + name;
		Type type = asm.GetType(name2);
		if (type != null && (type.IsPublic || IsLocalAssembly(asm as DirectUIAssembly)))
		{
			return GetXamlType(type);
		}
		return null;
	}

	private static string GetTypeExtensionName(string typeName)
	{
		return typeName + "Extension";
	}
}
