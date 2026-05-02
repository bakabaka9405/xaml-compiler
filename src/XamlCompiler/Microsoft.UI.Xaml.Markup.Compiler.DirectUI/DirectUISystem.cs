using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

public class DirectUISystem
{
	private List<DirectUIAssembly> systemAssemblies;

	private List<XamlTypeUniverse> xamlTypeUniverses;

	private List<DirectUIAssembly> platformAssemblies;

	private Lazy<Type> _Void;

	private Lazy<Type> _Object;

	private Lazy<Type> _String;

	private Lazy<Type> _Double;

	private Lazy<Type> _Int32;

	private Lazy<Type> _Boolean;

	private Lazy<Type> _Nullable;

	private Lazy<Type> _IReference;

	private Lazy<Type> _Type;

	private Lazy<Type> _frameworkTemplate;

	private Lazy<Type> _dataTemplate;

	private Lazy<Type> _controlTemplate;

	private Lazy<Type> _dependencyObject;

	private Lazy<Type> _dependencyProperty;

	private Lazy<Type> _frameworkElement;

	private Lazy<Type> _style;

	private Lazy<Type> _IComponentConnector;

	private Lazy<Type> _setter;

	private Lazy<Type> _binding;

	private Lazy<Type> _propertyPath;

	private Lazy<Type> _relativeSource;

	private Lazy<Type> _contentPropertyAttribute;

	private Lazy<Type> _inlineCollection;

	private Lazy<Type> _inline;

	private Lazy<Type> _lineBreak;

	private Lazy<Type> _uiElement;

	private Lazy<Type> _resourceDictionary;

	private Lazy<Type> _Deprecated;

	private Lazy<Type> _Delegate;

	private Lazy<Type> _flyoutBase;

	private Lazy<Type> _markupExtension;

	private Lazy<Type> _textBox;

	private Lazy<Type> _validationCommand;

	private Lazy<Type> _window;

	internal Type Void => _Void.Value;

	internal Type Object => _Object.Value;

	internal Type String => _String.Value;

	internal Type Double => _Double.Value;

	internal Type Int32 => _Int32.Value;

	internal Type Boolean => _Boolean.Value;

	internal Type Nullable => _Nullable.Value;

	internal Type IReference => _IReference.Value;

	internal Type Type => _Type.Value;

	internal Type FrameworkTemplate => _frameworkTemplate.Value;

	internal Type DataTemplate => _dataTemplate.Value;

	internal Type ControlTemplate => _controlTemplate.Value;

	internal Type DependencyObject => _dependencyObject.Value;

	internal Type DependencyProperty => _dependencyProperty.Value;

	internal Type FrameworkElement => _frameworkElement.Value;

	internal Type Style => _style.Value;

	internal Type IComponentConnector => _IComponentConnector.Value;

	internal Type Setter => _setter.Value;

	internal Type Binding => _binding.Value;

	internal Type PropertyPath => _propertyPath.Value;

	internal Type RelativeSource => _relativeSource.Value;

	internal Type ContentPropertyAttribute => _contentPropertyAttribute.Value;

	internal Type InlineCollection => _inlineCollection.Value;

	internal Type Inline => _inline.Value;

	internal Type LineBreak => _lineBreak.Value;

	internal Type UIElement => _uiElement.Value;

	internal Type ResourceDictionary => _resourceDictionary.Value;

	internal Type Deprecated => _Deprecated.Value;

	internal Type Delegate => _Delegate.Value;

	internal Type FlyoutBase => _flyoutBase.Value;

	internal Type MarkupExtension => _markupExtension.Value;

	internal Type TextBox => _textBox.Value;

	internal Type ValidationCommand => _validationCommand.Value;

	internal Type Window => _window.Value;

	internal List<DirectUIAssembly> DirectUIBaseAssemblies
	{
		get
		{
			if (systemAssemblies == null)
			{
				systemAssemblies = new List<DirectUIAssembly>();
			}
			return systemAssemblies;
		}
	}

	internal List<DirectUIAssembly> PlatformAssemblies
	{
		get
		{
			if (platformAssemblies == null)
			{
				platformAssemblies = new List<DirectUIAssembly>();
			}
			return platformAssemblies;
		}
	}

	internal List<XamlTypeUniverse> XamlTypeUniverses
	{
		get
		{
			if (xamlTypeUniverses == null)
			{
				xamlTypeUniverses = new List<XamlTypeUniverse>();
				foreach (DirectUIAssembly platformAssembly in PlatformAssemblies)
				{
					Assembly wrappedAssembly = platformAssembly.WrappedAssembly;
					IAssembly2 assembly = wrappedAssembly as IAssembly2;
					xamlTypeUniverses.Add(assembly.TypeUniverse as XamlTypeUniverse);
				}
			}
			return xamlTypeUniverses;
		}
	}

	public DirectUISystem(IList<Assembly> referenceAssemblies)
	{
		LoadCoreDirectUIAssemblies(referenceAssemblies);
		_Void = new Lazy<Type>(() => DirectUISystemGetType("System.Void"));
		_Object = new Lazy<Type>(() => DirectUISystemGetType("System.Object"));
		_String = new Lazy<Type>(() => DirectUISystemGetType("System.String"));
		_Double = new Lazy<Type>(() => DirectUISystemGetType("System.Double"));
		_Int32 = new Lazy<Type>(() => DirectUISystemGetType("System.Int32"));
		_Boolean = new Lazy<Type>(() => DirectUISystemGetType("System.Boolean"));
		_Nullable = new Lazy<Type>(() => DirectUISystemGetType("System.Nullable`1"));
		_IReference = new Lazy<Type>(() => DirectUISystemGetType("Windows.Foundation.IReference`1", mustExist: false));
		_Type = new Lazy<Type>(() => DirectUISystemGetType("System.Type"));
		_frameworkTemplate = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.FrameworkTemplate"));
		_dataTemplate = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.DataTemplate"));
		_controlTemplate = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Controls.ControlTemplate"));
		_dependencyObject = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.DependencyObject"));
		_dependencyProperty = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.DependencyProperty"));
		_frameworkElement = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.FrameworkElement"));
		_style = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Style"));
		_IComponentConnector = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Markup.IComponentConnector"));
		_setter = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Setter"));
		_binding = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Data.Binding"));
		_propertyPath = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.PropertyPath"));
		_relativeSource = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Data.RelativeSource"));
		_contentPropertyAttribute = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Markup.ContentPropertyAttribute"));
		_inlineCollection = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Documents.InlineCollection"));
		_inline = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Documents.Inline"));
		_lineBreak = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Documents.LineBreak"));
		_uiElement = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.UIElement"));
		_flyoutBase = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Controls.Primitives.FlyoutBase"));
		_resourceDictionary = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.ResourceDictionary"));
		_markupExtension = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Markup.MarkupExtension"));
		_textBox = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Controls.TextBox"));
		_Deprecated = new Lazy<Type>(() => DirectUISystemGetType("Windows.Foundation.Metadata.DeprecatedAttribute"));
		_Delegate = new Lazy<Type>(() => DirectUISystemGetType("System.Delegate"));
		_validationCommand = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Controls.InputValidationCommand"));
		_window = new Lazy<Type>(() => DirectUISystemGetType("Microsoft.UI.Xaml.Window"));
	}

	internal Type DirectUISystemGetType(string typeName, bool mustExist = true)
	{
		Type type = null;
		foreach (DirectUIAssembly directUIBaseAssembly in DirectUIBaseAssemblies)
		{
			type = directUIBaseAssembly.GetType(typeName);
			if (type != null && type.IsPublic)
			{
				break;
			}
		}
		_ = type == null;
		return type;
	}

	private void LoadCoreDirectUIAssemblies(IList<Assembly> referenceAssemblies)
	{
		foreach (DirectUIAssembly referenceAssembly in referenceAssemblies)
		{
			if (FileHelpers.IsPlatformAssembly(referenceAssembly) || FileHelpers.IsWinUIAssembly(referenceAssembly))
			{
				PlatformAssemblies.Add(referenceAssembly);
				DirectUIBaseAssemblies.Add(referenceAssembly);
			}
		}
		bool flag = false;
		foreach (DirectUIAssembly referenceAssembly2 in referenceAssemblies)
		{
			if (KS.EqIgnoreCase("mscorlib", referenceAssembly2.GetName().Name))
			{
				DirectUIBaseAssemblies.Add(referenceAssembly2);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DirectUIAssembly item = DirectUIAssembly.Wrap(typeof(string).Assembly);
			DirectUIBaseAssemblies.Add(item);
		}
	}

	private DirectUIAssembly GetSystemAssembly(string assemblyName)
	{
		foreach (DirectUIAssembly directUIBaseAssembly in DirectUIBaseAssemblies)
		{
			if (KS.EqIgnoreCase(assemblyName, directUIBaseAssembly.BaseName))
			{
				return directUIBaseAssembly;
			}
		}
		return null;
	}
}
