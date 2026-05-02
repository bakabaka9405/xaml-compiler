using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

public class DirectUIXamlType : XamlType, IXamlTypeMeta
{
	private List<XamlType> inlineCollectionAllowedContentTypes;

	private bool? isValueType;

	private bool? isCodeGenType;

	private bool? hasWinUIContract;

	private bool? isSignedChar;

	private bool? isEnumBaseType;

	private bool? isEnum;

	private bool? isTemplateType;

	private List<string> enumNamesList;

	private bool? isNullableGeneric;

	private Type nullableGenericInnerType;

	private bool? isDeprecated;

	private bool isHardDeprecated;

	private string deprecatedMessage;

	private bool? isExperimental;

	private MethodInfo addMethod;

	private MemberProxyMetadata frameworkTemplateProxyMetadata;

	private bool? _hasValueConverter;

	private bool? _hasMetadataProvider;

	private bool? _isStyle;

	private bool? _isBinding;

	private bool? _isPropertyPath;

	private CreateFromStringMethod _createFromStringMethod;

	private bool? _hasINotifyPropertyChanged;

	private bool? _hasINotifyCollectionChanged;

	private bool? _hasINotifyDataErrorInfo;

	private bool? _hasObservableVector;

	private bool? _hasObservableMap;

	private bool? _isDelegate;

	private bool? _isDerivedFromFrameworkTemplate;

	private bool? _isDerivedFromValidationCommand;

	private bool? _isDerivedFromResourceDictionary;

	private bool? _isDerivedFromUIElement;

	private bool? _isDerivedFromFlyoutBase;

	private bool? _isDerivedFromMarkupExtension;

	private bool? _isDerivedFromTextBox;

	internal static List<string> WindowsFoundationSystemTypes = new List<string> { "Point", "Size", "Rect", "Uri", "TimeSpan" };

	private static List<string> PrimitiveSystemTypes = new List<string>
	{
		"Object", "Double", "Single", "Int16", "Char16", "Int32", "Int64", "UInt32", "UInt64", "Boolean",
		"String"
	};

	private static List<string> _namesOfTypesWithConverters = new List<string>
	{
		"System.Double", "System.Boolean", "System.Numerics.Matrix3x2", "System.Numerics.Matrix4x4", "System.Numerics.Vector2", "System.Numerics.Vector3", "System.String", "System.TimeSpan", "System.Single", "Windows.Foundation.TimeSpan",
		"System.Int32", "System.EventHandler", "System.Type", "Windows.Foundation.Uri", "System.Uri", "Windows.Foundation.Numerics.Matrix3x2", "Windows.Foundation.Numerics.Matrix4x4", "Windows.Foundation.Numerics.Vector2", "Windows.Foundation.Numerics.Vector3", "Windows.Foundation.Point",
		"Windows.Foundation.Size", "Windows.Foundation.Rect", "Windows.Media.Playback.IMediaPlaybackSource", "Windows.UI.Color", "Windows.UI.Text.FontWeight", "Windows.UI.Text.FontStyle", "Windows.UI.Text.FontStretch", "Microsoft.UI.Xaml.RoutedEvent", "Microsoft.UI.Xaml.Thickness", "Microsoft.UI.Xaml.CornerRadius",
		"Microsoft.UI.Xaml.TextWrapping", "Microsoft.UI.Xaml.TextAlignment", "Microsoft.UI.Xaml.FontWeight", "Microsoft.UI.Xaml.FontStyle", "Microsoft.UI.Xaml.FontStretch", "Microsoft.UI.Xaml.Duration", "Microsoft.UI.Xaml.DependencyProperty", "Microsoft.UI.Xaml.DependencyProperty", "Microsoft.UI.Xaml.Visibility", "Microsoft.UI.Xaml.HorizontalAlignment",
		"Microsoft.UI.Xaml.PropertyPath", "Microsoft.UI.Xaml.TargetPropertyPath", "Microsoft.UI.Xaml.VerticalAlignment", "Microsoft.UI.Xaml.GridLength", "Microsoft.UI.Xaml.GridUnitType", "Microsoft.UI.Xaml.DurationType", "Microsoft.UI.Xaml.FlowDirection", "Microsoft.UI.Xaml.Automation.DockPosition", "Microsoft.UI.Xaml.Automation.ExpandCollapseState", "Microsoft.UI.Xaml.Automation.ScrollAmount",
		"Microsoft.UI.Xaml.Automation.RowOrColumnMajor", "Microsoft.UI.Xaml.Automation.ToggleState", "Microsoft.UI.Xaml.Automation.WindowVisualState", "Microsoft.UI.Xaml.Automation.WindowInteractionState", "Microsoft.UI.Xaml.Automation.SupportedTextSelection", "Microsoft.UI.Xaml.Automation.Peers.AutomationControlType", "Microsoft.UI.Xaml.Automation.Peers.AutomationEvents", "Microsoft.UI.Xaml.Automation.Peers.PatternInterface", "Microsoft.UI.Xaml.Automation.Peers.AutomationOrientation", "Microsoft.UI.Xaml.Automation.Text.TextUnit",
		"Microsoft.UI.Xaml.Automation.Text.TextPatternRangeEndpoint", "Microsoft.UI.Xaml.Controls.Orientation", "Microsoft.UI.Xaml.Controls.IncrementalLoadingTrigger", "Microsoft.UI.Xaml.Controls.StretchDirection", "Microsoft.UI.Xaml.Controls.ScrollBarVisibility", "Microsoft.UI.Xaml.Controls.ClickMode", "Microsoft.UI.Xaml.Controls.SelectionMode", "Microsoft.UI.Xaml.Controls.VirtualizationMode", "Microsoft.UI.Xaml.Controls.ScrollMode", "Microsoft.UI.Xaml.Controls.ZoomMode",
		"Microsoft.UI.Xaml.Controls.SnapPointsType", "Microsoft.UI.Xaml.Controls.IconElement", "Microsoft.UI.Xaml.Controls.SymbolIcon", "Microsoft.UI.Xaml.Controls.ColumnDefinition", "Microsoft.UI.Xaml.Controls.RowDefinition", "Microsoft.UI.Xaml.Controls.Primitives.GeneratorPosition", "Microsoft.UI.Xaml.Controls.Primitives.GeneratorDirection", "Microsoft.UI.Xaml.Controls.Primitives.ScrollEventType", "Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment", "Microsoft.UI.Xaml.Documents.Run",
		"Microsoft.UI.Xaml.Input.KeyboardNavigationMode", "Microsoft.UI.Xaml.Input.ManipulationModes", "Microsoft.UI.Xaml.Input.ModifierKeys", "Microsoft.UI.Xaml.Input.Key", "Microsoft.UI.Xaml.Input.InputScope", "Windows.UI.Xaml.Interop.TypeName", "Microsoft.UI.Xaml.Media.Color", "Microsoft.UI.Xaml.Media.FillRule", "Microsoft.UI.Xaml.Media.PenLineCap", "Microsoft.UI.Xaml.Media.PenLineJoin",
		"Microsoft.UI.Xaml.Media.SweepDirection", "Microsoft.UI.Xaml.Media.ColorInterpolationMode", "Microsoft.UI.Xaml.Media.GradientSpreadMethod", "Microsoft.UI.Xaml.Media.BrushMappingMode", "Microsoft.UI.Xaml.Media.AlignmentX", "Microsoft.UI.Xaml.Media.AlignmentY", "Microsoft.UI.Xaml.Media.Stretch", "Microsoft.UI.Xaml.Media.DoubleCollection", "Microsoft.UI.Xaml.Media.PointCollection", "Microsoft.UI.Xaml.Media.Transform",
		"Microsoft.UI.Xaml.Media.Brush", "Microsoft.UI.Xaml.Media.SolidColorBrush", "Microsoft.UI.Xaml.Media.Geometry", "Microsoft.UI.Xaml.Media.ImageSource", "Microsoft.UI.Xaml.Media.TransformGroup", "Microsoft.UI.Xaml.Media.MatrixTransform", "Microsoft.UI.Xaml.Media.Matrix", "Microsoft.UI.Xaml.Media.FontFamily", "Microsoft.UI.Xaml.Media.MediaCanPlayResponse", "Microsoft.UI.Xaml.Media.Video3DMode",
		"Microsoft.UI.Xaml.Media.CacheMode", "Microsoft.UI.Xaml.Media.Animation.FillBehavior", "Microsoft.UI.Xaml.Media.Animation.EasingMode", "Microsoft.UI.Xaml.Media.Animation.ClockState", "Microsoft.UI.Xaml.Media.Animation.RepeatBehavior", "Microsoft.UI.Xaml.Media.Animation.KeyTime", "Microsoft.UI.Xaml.Media.Animation.KeySpline", "Microsoft.UI.Xaml.Media.Animation.RepeatBehaviorType", "Microsoft.UI.Xaml.Media.Imaging.BitmapCreateOptions", "Microsoft.UI.Xaml.Media.Media3D.Matrix3D",
		"Microsoft.UI.Xaml.Vector3TransitionComponents"
	};

	public ApiInformation ApiInformation { get; }

	public bool HasApiInformation => ApiInformation != null;

	public Platform TargetPlatform { get; }

	public bool IsValueType
	{
		get
		{
			if (!isValueType.HasValue)
			{
				isValueType = LookupIsValueType();
			}
			return isValueType.Value;
		}
	}

	public bool IsCodeGenType
	{
		get
		{
			if (!isCodeGenType.HasValue)
			{
				isCodeGenType = LookupIsCodeGenType();
			}
			return isCodeGenType.Value;
		}
	}

	public bool HasWinUIContract
	{
		get
		{
			if (!hasWinUIContract.HasValue)
			{
				hasWinUIContract = LookupHasWinUIContract();
			}
			return hasWinUIContract.Value;
		}
	}

	public bool IsSignedChar
	{
		get
		{
			if (!isSignedChar.HasValue)
			{
				isSignedChar = LookupIsSignedChar();
			}
			return isSignedChar.Value;
		}
	}

	public bool IsEnumBaseType
	{
		get
		{
			if (!isEnumBaseType.HasValue)
			{
				isEnumBaseType = LookupIsEnumBaseType();
			}
			return isEnumBaseType.Value;
		}
	}

	public bool IsTemplateType
	{
		get
		{
			if (!isTemplateType.HasValue)
			{
				isTemplateType = LookupIsTemplateType();
			}
			return isTemplateType.Value;
		}
	}

	public bool IsEnum
	{
		get
		{
			if (!isEnum.HasValue)
			{
				isEnum = LookupIsEnum();
			}
			return isEnum.Value;
		}
	}

	public List<string> EnumNames
	{
		get
		{
			if (enumNamesList == null)
			{
				enumNamesList = LookupEnumNames();
			}
			return enumNamesList;
		}
	}

	public bool IsInvalidType
	{
		get
		{
			if (!IsSignedChar)
			{
				return IsEnumBaseType;
			}
			return true;
		}
	}

	public bool IsDeprecated
	{
		get
		{
			if (!isDeprecated.HasValue)
			{
				isDeprecated = LookupIsDeprecated();
			}
			return isDeprecated.Value;
		}
	}

	public bool IsHardDeprecated
	{
		get
		{
			if (!IsDeprecated)
			{
				return false;
			}
			return isHardDeprecated;
		}
	}

	public string DeprecatedMessage
	{
		get
		{
			if (!IsDeprecated)
			{
				return string.Empty;
			}
			return deprecatedMessage;
		}
	}

	public bool IsExperimental
	{
		get
		{
			if (!isExperimental.HasValue)
			{
				isExperimental = LookupIsExperimental();
			}
			return isExperimental.Value;
		}
	}

	public string AddMethodName
	{
		get
		{
			if (base.IsCollection || base.IsDictionary)
			{
				if (addMethod != null)
				{
					return addMethod.Name;
				}
				return "Add";
			}
			return null;
		}
	}

	public bool IsValueConverter
	{
		get
		{
			if (!_hasValueConverter.HasValue)
			{
				_hasValueConverter = base.UnderlyingType.GetInterface("Microsoft.UI.Xaml.Data.IValueConverter") != null;
			}
			return _hasValueConverter.Value;
		}
	}

	public bool IsMetadataProvider
	{
		get
		{
			if (!_hasMetadataProvider.HasValue)
			{
				_hasMetadataProvider = base.UnderlyingType.GetInterface("Microsoft.UI.Xaml.Markup.IXamlMetadataProvider") != null;
			}
			return _hasMetadataProvider.Value;
		}
	}

	public bool IsAssignableToStyle
	{
		get
		{
			if (!_isStyle.HasValue)
			{
				_isStyle = ((DirectUISchemaContext)base.SchemaContext).DirectUISystem.Style.IsAssignableFrom(base.UnderlyingType);
			}
			return _isStyle.Value;
		}
	}

	public bool IsAssignableToBinding
	{
		get
		{
			if (!_isBinding.HasValue)
			{
				_isBinding = ((DirectUISchemaContext)base.SchemaContext).DirectUISystem.Binding.IsAssignableFrom(base.UnderlyingType);
			}
			return _isBinding.Value;
		}
	}

	public bool IsAssignableToPropertyPath
	{
		get
		{
			if (!_isPropertyPath.HasValue)
			{
				_isPropertyPath = ((DirectUISchemaContext)base.SchemaContext).DirectUISystem.PropertyPath.IsAssignableFrom(base.UnderlyingType);
			}
			return _isPropertyPath.Value;
		}
	}

	public bool ImplementsINotifyPropertyChanged
	{
		get
		{
			if (!_hasINotifyPropertyChanged.HasValue)
			{
				_hasINotifyPropertyChanged = HasInterface("System.ComponentModel.INotifyPropertyChanged") || HasInterface("Microsoft.UI.Xaml.Data.INotifyPropertyChanged");
			}
			return _hasINotifyPropertyChanged.Value;
		}
	}

	public bool ImplementsINotifyDataErrorInfo
	{
		get
		{
			if (!_hasINotifyDataErrorInfo.HasValue)
			{
				_hasINotifyDataErrorInfo = HasInterface("System.ComponentModel.INotifyDataErrorInfo") || HasInterface("Microsoft.UI.Xaml.Data.INotifyDataErrorInfo");
			}
			return _hasINotifyDataErrorInfo.Value;
		}
	}

	public bool ImplementsINotifyCollectionChanged
	{
		get
		{
			if (!_hasINotifyCollectionChanged.HasValue)
			{
				_hasINotifyCollectionChanged = HasInterface("System.Collections.Specialized.INotifyCollectionChanged") || HasInterface("Microsoft.UI.Xaml.Interop.INotifyCollectionChanged");
			}
			return _hasINotifyCollectionChanged.Value;
		}
	}

	public bool ImplementsIObservableVector
	{
		get
		{
			if (!_hasObservableVector.HasValue)
			{
				_hasObservableVector = HasInterface("Windows.Foundation.Collections.IObservableVector`1");
			}
			return _hasObservableVector.Value;
		}
	}

	public bool ImplementsIObservableMap
	{
		get
		{
			if (!_hasObservableMap.HasValue)
			{
				_hasObservableMap = HasInterface("Windows.Foundation.Collections.IObservableMap`2");
			}
			return _hasObservableMap.Value;
		}
	}

	public bool IsDelegate
	{
		get
		{
			if (!_isDelegate.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDelegate = base.UnderlyingType.IsSubclassOf(directUISchemaContext.DirectUISystem.Delegate);
			}
			return _isDelegate.Value;
		}
	}

	public bool IsDerivedFromFrameworkTemplate
	{
		get
		{
			if (!_isDerivedFromFrameworkTemplate.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDerivedFromFrameworkTemplate = directUISchemaContext.DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromFrameworkTemplate.Value;
		}
	}

	public bool IsDerivedFromValidationCommand
	{
		get
		{
			if (!_isDerivedFromValidationCommand.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDerivedFromValidationCommand = directUISchemaContext.DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromFrameworkTemplate.Value;
		}
	}

	public bool IsDerivedFromResourceDictionary
	{
		get
		{
			if (!_isDerivedFromResourceDictionary.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDerivedFromResourceDictionary = directUISchemaContext.DirectUISystem.ResourceDictionary.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromResourceDictionary.Value;
		}
	}

	public bool IsDerivedFromUIElement
	{
		get
		{
			if (!_isDerivedFromUIElement.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDerivedFromUIElement = directUISchemaContext.DirectUISystem.UIElement.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromUIElement.Value;
		}
	}

	public bool IsDerivedFromFlyoutBase
	{
		get
		{
			if (!_isDerivedFromFlyoutBase.HasValue)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				_isDerivedFromFlyoutBase = directUISchemaContext.DirectUISystem.FlyoutBase.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromFlyoutBase.Value;
		}
	}

	public bool IsDerivedFromMarkupExtension
	{
		get
		{
			if (!_isDerivedFromMarkupExtension.HasValue)
			{
				Type type = (base.SchemaContext as DirectUISchemaContext)?.DirectUISystem.MarkupExtension;
				_isDerivedFromMarkupExtension = !(type == null) && type.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromMarkupExtension.Value;
		}
	}

	public bool IsDerivedFromTextBox
	{
		get
		{
			if (!_isDerivedFromTextBox.HasValue)
			{
				Type type = (base.SchemaContext as DirectUISchemaContext)?.DirectUISystem.TextBox;
				_isDerivedFromTextBox = !(type == null) && type.IsAssignableFrom(base.UnderlyingType);
			}
			return _isDerivedFromTextBox.Value;
		}
	}

	private IDirectUIXamlLanguage DirectUIXamlLanguage
	{
		get
		{
			DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
			return directUISchemaContext.DirectUIXamlLanguage;
		}
	}

	private DirectUISystem DirectUISystem
	{
		get
		{
			DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
			return directUISchemaContext.DirectUISystem;
		}
	}

	private MemberProxyMetadata FrameworkTemplateProxyMetadata
	{
		get
		{
			if (frameworkTemplateProxyMetadata == null)
			{
				frameworkTemplateProxyMetadata = new MemberProxyMetadata("Template", DirectUIXamlLanguage.Object);
				frameworkTemplateProxyMetadata.DeferringLoader = new XamlValueConverter<XamlDeferringLoader>(DirectUISystem.Object, DirectUIXamlLanguage.UIElement);
			}
			return frameworkTemplateProxyMetadata;
		}
	}

	public CreateFromStringMethod CreateFromStringMethod
	{
		get
		{
			if (_createFromStringMethod == null)
			{
				string text = LookupCreateFromStringMethod();
				if (!string.IsNullOrEmpty(text))
				{
					if (text.HasAtLeastTwo('.'))
					{
						_createFromStringMethod = new CreateFromStringMethod(text);
					}
					else
					{
						_createFromStringMethod = new CreateFromStringMethod(this, text);
					}
				}
				else
				{
					_createFromStringMethod = new CreateFromStringMethod();
				}
			}
			return _createFromStringMethod;
		}
	}

	public DirectUIXamlType(Type underlyingType, XamlSchemaContext schemaContext)
		: this(underlyingType, schemaContext, null, Platform.Any)
	{
	}

	public DirectUIXamlType(Type underlyingType, XamlSchemaContext schemaContext, ApiInformation apiInformation, Platform targetPlatform)
		: base(underlyingType, schemaContext)
	{
		ApiInformation = apiInformation;
		TargetPlatform = targetPlatform;
	}

	public DirectUIXamlType(string name, IList<XamlType> typeArgs, XamlSchemaContext schemaContext)
		: base(name, typeArgs, schemaContext)
	{
	}

	public bool HasInterface(string fullName)
	{
		Type underlyingType = base.UnderlyingType;
		if (fullName == underlyingType.Namespace + "." + underlyingType.Name)
		{
			return true;
		}
		return underlyingType.GetInterface(fullName) != null;
	}

	public XamlMember LookupMember_SkipReadOnlyCheck(string propertyName)
	{
		return LookupMember(propertyName, skipReadOnlyCheck: true);
	}

	public bool IsNullableGeneric(out Type innerType)
	{
		if (!isNullableGeneric.HasValue)
		{
			isNullableGeneric = LookupIsNullableGeneric(out nullableGenericInnerType);
		}
		innerType = nullableGenericInnerType;
		return isNullableGeneric.Value;
	}

	protected virtual bool LookupIsSignedChar()
	{
		if (base.UnderlyingType == null)
		{
			return false;
		}
		string fullName = base.UnderlyingType.FullName;
		if (!(fullName == "System.SByte"))
		{
			return fullName == "System.Int8";
		}
		return true;
	}

	protected virtual bool LookupIsEnumBaseType()
	{
		if (base.UnderlyingType == null)
		{
			return false;
		}
		string fullName = base.UnderlyingType.FullName;
		return fullName == "System.Enum";
	}

	protected virtual bool LookupIsValueType()
	{
		if (base.UnderlyingType == null)
		{
			return false;
		}
		return base.UnderlyingType.IsValueType;
	}

	private CustomAttributeData GetDirectAttribute(string name)
	{
		return GetAttribute(name, inherited: false);
	}

	private CustomAttributeData GetAttribute(string attrName, bool inherited = true)
	{
		return GetAttribute(base.UnderlyingType, attrName, inherited);
	}

	private static CustomAttributeData GetAttribute(Type type, string attrName, bool inherited = true)
	{
		CustomAttributeData result = null;
		if (type != null)
		{
			result = ReflectionHelper.FindAttributeByTypeName(type, inherited, attrName);
		}
		return result;
	}

	private bool CheckDeprecationAttribute(string attrName, string defaultMessage)
	{
		CustomAttributeData attribute = GetAttribute(attrName);
		if (attribute != null)
		{
			Type declaringType = attribute.Constructor.DeclaringType;
			deprecatedMessage = ReflectionHelper.GetAttributeConstructorArgument(attribute, 0, null) as string;
			if (string.IsNullOrWhiteSpace(deprecatedMessage))
			{
				deprecatedMessage = defaultMessage;
			}
			if (attrName.Equals("Windows.Foundation.Metadata.DeprecatedAttribute") && (int)ReflectionHelper.GetAttributeConstructorArgument(attribute, 1, null) != 0)
			{
				isHardDeprecated = true;
			}
			return true;
		}
		return false;
	}

	protected virtual bool LookupIsDeprecated()
	{
		bool flag = CheckDeprecationAttribute("Windows.Foundation.Metadata.DeprecatedAttribute", "Deprecated");
		if (!flag)
		{
			return CheckDeprecationAttribute("System.ObsoleteAttribute", "Obsolete");
		}
		return flag;
	}

	protected virtual bool LookupIsExperimental()
	{
		return GetAttribute("Windows.Foundation.Metadata.ExperimentalAttribute") != null;
	}

	protected virtual bool LookupIsTemplateType()
	{
		return DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType);
	}

	protected virtual bool LookupIsCodeGenType()
	{
		if (base.UnderlyingType == null)
		{
			return false;
		}
		if (HasWinUIContract)
		{
			return false;
		}
		string fullName = base.UnderlyingType.FullName;
		if (fullName.StartsWith("Windows.Foundation."))
		{
			string item = fullName.Substring("Windows.Foundation.".Length);
			if (WindowsFoundationSystemTypes.Contains(item))
			{
				return false;
			}
		}
		if (fullName.StartsWith("System."))
		{
			string item2 = fullName.Substring("System.".Length);
			if (PrimitiveSystemTypes.Contains(item2))
			{
				return false;
			}
		}
		if (fullName.StartsWith("Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes"))
		{
			return false;
		}
		return true;
	}

	protected bool LookupIsEnum()
	{
		if (base.UnderlyingType == null)
		{
			return false;
		}
		return base.UnderlyingType.IsEnum;
	}

	protected List<string> LookupEnumNames()
	{
		List<string> list = null;
		if (IsEnum)
		{
			list = new List<string>();
			string[] names = Enum.GetNames(base.UnderlyingType);
			foreach (string item in names)
			{
				list.Add(item);
			}
		}
		return list;
	}

	protected override XamlType LookupBaseType()
	{
		Type underlyingType = base.UnderlyingType;
		if (underlyingType != null && underlyingType.BaseType != null)
		{
			if (underlyingType.BaseType.FullName == "System.__ComObject" || underlyingType.BaseType.FullName == "System.Runtime.InteropServices.WindowsRuntime.RuntimeClass")
			{
				return base.SchemaContext.GetXamlType(DirectUISystem.Object);
			}
			return base.SchemaContext.GetXamlType(underlyingType.BaseType);
		}
		return null;
	}

	protected override XamlCollectionKind LookupCollectionKind()
	{
		if (base.UnderlyingType.IsArray)
		{
			return XamlCollectionKind.Array;
		}
		if (base.UnderlyingType.IsInterface && GetCollectionKind(base.UnderlyingType, out var collectionKind))
		{
			return collectionKind;
		}
		Type collectionReleventInterface = GetCollectionReleventInterface();
		if (collectionReleventInterface != null && GetCollectionKind(collectionReleventInterface, out collectionKind))
		{
			return collectionKind;
		}
		return XamlCollectionKind.None;
	}

	protected override XamlType LookupItemType()
	{
		XamlType xamlType = base.LookupItemType();
		if (xamlType == null)
		{
			Type type = DirectUISystem.Object;
			if (base.IsCollection && addMethod != null)
			{
				ParameterInfo[] parameters = addMethod.GetParameters();
				if (parameters.Length == 1)
				{
					type = parameters[0].ParameterType;
				}
			}
			if (base.IsDictionary && addMethod != null)
			{
				ParameterInfo[] parameters2 = addMethod.GetParameters();
				if (parameters2.Length == 2)
				{
					type = parameters2[1].ParameterType;
				}
			}
			xamlType = base.SchemaContext.GetXamlType(type);
		}
		return xamlType;
	}

	protected override XamlType LookupKeyType()
	{
		XamlType xamlType = base.LookupKeyType();
		if (xamlType == null)
		{
			Type type = DirectUISystem.Object;
			if (base.IsDictionary && addMethod != null)
			{
				ParameterInfo[] parameters = addMethod.GetParameters();
				if (parameters.Length == 2)
				{
					type = parameters[0].ParameterType;
				}
			}
			xamlType = base.SchemaContext.GetXamlType(type);
		}
		return xamlType;
	}

	protected override bool LookupIsConstructible()
	{
		if (base.UnderlyingType == null)
		{
			return true;
		}
		if (!base.LookupIsConstructible())
		{
			return false;
		}
		if (base.ConstructionRequiresArguments)
		{
			return false;
		}
		if (IsValueType)
		{
			return false;
		}
		return true;
	}

	protected override XamlMember LookupAttachableMember(string name)
	{
		XamlMember xamlMember = base.LookupAttachableMember(name);
		if (xamlMember != null)
		{
			xamlMember = new DirectUIXamlMember(xamlMember.Name, xamlMember.Invoker, base.SchemaContext, ApiInformation);
		}
		return xamlMember;
	}

	protected override IEnumerable<XamlMember> LookupAllAttachableMembers()
	{
		List<XamlMember> list = new List<XamlMember>();
		if (base.UnderlyingType != null)
		{
			MethodInfo[] methods = base.UnderlyingType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			HashSet<string> hashSet = new HashSet<string>();
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.IsPublic && (IsAttachablePropertySetter(methodInfo, out var name) || IsAttachablePropertyGetter(methodInfo, out name)) && !hashSet.Contains(name))
				{
					hashSet.Add(name);
				}
			}
			foreach (string item in hashSet)
			{
				XamlMember xamlMember = LookupAttachableMember(item);
				if (xamlMember != null && xamlMember.Type.UnderlyingType != DirectUISystem.Void)
				{
					list.Add(xamlMember);
				}
			}
		}
		if (list.Count != 0)
		{
			return list;
		}
		return null;
	}

	protected override XamlMember LookupMember(string propertyName, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = base.LookupMember(propertyName, skipReadOnlyCheck);
		if (xamlMember == null)
		{
			if (IsFrameworkTemplateProperty(propertyName))
			{
				xamlMember = new ProxyDirectUIXamlMember(FrameworkTemplateProxyMetadata, this);
			}
		}
		else
		{
			DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
			if (xamlMember.IsEvent)
			{
				EventInfo eventInfo = xamlMember.UnderlyingMember as EventInfo;
				if (eventInfo != null && directUISchemaContext != null)
				{
					xamlMember = new DirectUIXamlMember(eventInfo, directUISchemaContext, ApiInformation);
				}
			}
			else
			{
				PropertyInfo propertyInfo = xamlMember.UnderlyingMember as PropertyInfo;
				if (propertyInfo != null && directUISchemaContext != null)
				{
					xamlMember = new DirectUIXamlMember(propertyInfo, directUISchemaContext, ApiInformation);
				}
			}
		}
		return xamlMember;
	}

	protected override IEnumerable<XamlMember> LookupAllMembers()
	{
		List<XamlMember> list = new List<XamlMember>();
		if (base.UnderlyingType != null)
		{
			if (DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType))
			{
				XamlMember propertyOrUnknown = GetPropertyOrUnknown("Template", skipReadOnlyCheck: false);
				list.Add(propertyOrUnknown);
			}
			DirectUISchemaContext schemaContext = base.SchemaContext as DirectUISchemaContext;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
			PropertyInfo[] properties = base.UnderlyingType.GetProperties(bindingAttr);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				DirectUIXamlMember item = new DirectUIXamlMember(propertyInfo, schemaContext, ApiInformation);
				list.Add(item);
			}
			EventInfo[] events = base.UnderlyingType.GetEvents(bindingAttr);
			EventInfo[] array2 = events;
			foreach (EventInfo eventInfo in array2)
			{
				DirectUIXamlMember item2 = new DirectUIXamlMember(eventInfo, schemaContext, ApiInformation);
				list.Add(item2);
			}
		}
		return list;
	}

	protected override XamlMember LookupContentProperty()
	{
		CustomAttributeData customAttributeData = ReflectionHelper.FindAttributeByTypeName(base.UnderlyingType, inherit: true, DirectUISystem.ContentPropertyAttribute.FullName);
		XamlMember xamlMember = null;
		if (customAttributeData != null)
		{
			Type declaringType = customAttributeData.Constructor.DeclaringType;
			if (ReflectionHelper.GetAttributeConstructorArgument(customAttributeData, -1, "Name") is string propertyName)
			{
				xamlMember = GetPropertyOrUnknown(propertyName, skipReadOnlyCheck: false);
			}
		}
		if (xamlMember == null && DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType))
		{
			xamlMember = GetPropertyOrUnknown("Template", skipReadOnlyCheck: false);
		}
		return xamlMember;
	}

	protected override IList<XamlType> LookupAllowedContentTypes()
	{
		if (base.UnderlyingType == DirectUISystem.InlineCollection)
		{
			if (inlineCollectionAllowedContentTypes == null)
			{
				inlineCollectionAllowedContentTypes = new List<XamlType>();
				inlineCollectionAllowedContentTypes.Add(base.SchemaContext.GetXamlType(DirectUISystem.Inline));
				inlineCollectionAllowedContentTypes.Add(base.SchemaContext.GetXamlType(DirectUISystem.String));
				inlineCollectionAllowedContentTypes.Add(base.SchemaContext.GetXamlType(DirectUISystem.UIElement));
			}
			return inlineCollectionAllowedContentTypes;
		}
		return null;
	}

	protected override XamlType LookupMarkupExtensionReturnType()
	{
		if (base.UnderlyingType == DirectUISystem.RelativeSource)
		{
			return this;
		}
		return DirectUIXamlLanguage.Object;
	}

	protected override bool LookupIsMarkupExtension()
	{
		if (!(base.UnderlyingType == DirectUISystem.Binding) && !(base.UnderlyingType == DirectUISystem.RelativeSource))
		{
			if (DirectUISystem.MarkupExtension != null)
			{
				return DirectUISystem.MarkupExtension.IsAssignableFrom(base.UnderlyingType);
			}
			return false;
		}
		return true;
	}

	protected override bool LookupIsNameScope()
	{
		return false;
	}

	protected override bool LookupIsXData()
	{
		return false;
	}

	protected override bool LookupTrimSurroundingWhitespace()
	{
		return base.UnderlyingType == DirectUISystem.LineBreak;
	}

	protected override bool LookupIsWhitespaceSignificantCollection()
	{
		return base.UnderlyingType == DirectUISystem.InlineCollection;
	}

	protected override XamlMember LookupAliasedProperty(XamlDirective directive)
	{
		if (directive == XamlLanguage.Key)
		{
			if (base.UnderlyingType == DirectUISystem.Style)
			{
				return LookupMember("TargetType", skipReadOnlyCheck: true);
			}
		}
		else if (directive == XamlLanguage.Lang)
		{
			if (DirectUISystem.FrameworkElement.IsAssignableFrom(base.UnderlyingType) || DirectUISystem.Inline.IsAssignableFrom(base.UnderlyingType))
			{
				return LookupMember("Language", skipReadOnlyCheck: true);
			}
		}
		else if (directive == XamlLanguage.Name && DirectUISystem.FrameworkElement.IsAssignableFrom(base.UnderlyingType))
		{
			return LookupMember("Name", skipReadOnlyCheck: true);
		}
		XamlMember result = null;
		try
		{
			result = base.LookupAliasedProperty(directive);
		}
		catch (TypeLoadException)
		{
		}
		return result;
	}

	protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		XamlValueConverter<TypeConverter> xamlValueConverter = FindTypeConverter(base.UnderlyingType);
		if (xamlValueConverter == null && IsNullableGeneric(out var innerType))
		{
			xamlValueConverter = FindTypeConverter(innerType);
		}
		return xamlValueConverter;
	}

	protected XamlValueConverter<TypeConverter> FindTypeConverter(Type underlyingType)
	{
		if (underlyingType.IsEnum)
		{
			return new XamlValueConverter<TypeConverter>(typeof(DirectUINativeTypeConverter), this);
		}
		if (_namesOfTypesWithConverters.Contains(underlyingType.FullName))
		{
			return new XamlValueConverter<TypeConverter>(typeof(DirectUINativeTypeConverter), this);
		}
		if (CreateFromStringMethod.Exists)
		{
			return new XamlValueConverter<TypeConverter>(typeof(DirectUINativeTypeConverter), this);
		}
		return null;
	}

	protected override bool LookupIsNullable()
	{
		if (base.UnderlyingType == null)
		{
			return base.LookupIsNullable();
		}
		if (this.IsString())
		{
			return DirectUIXamlLanguage.IsStringNullable;
		}
		Type innerType;
		if (base.UnderlyingType.IsValueType)
		{
			return IsNullableGeneric(out innerType);
		}
		return true;
	}

	protected virtual bool LookupIsNullableGeneric(out Type innerType)
	{
		innerType = null;
		if (base.UnderlyingType == null || !base.UnderlyingType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = base.UnderlyingType.GetGenericTypeDefinition();
		if (genericTypeDefinition == DirectUISystem.Nullable || genericTypeDefinition == DirectUISystem.IReference)
		{
			Type[] genericArguments = base.UnderlyingType.GetGenericArguments();
			innerType = genericArguments[0];
			return true;
		}
		return false;
	}

	private bool IsFrameworkTemplateProperty(string propertyName)
	{
		if (KS.Eq(propertyName, "Template") && DirectUISystem.FrameworkTemplate.IsAssignableFrom(base.UnderlyingType))
		{
			return true;
		}
		return false;
	}

	private Type GetCollectionReleventInterface()
	{
		List<Type> list = new List<Type>();
		Type[] interfaces = base.UnderlyingType.GetInterfaces();
		Type[] array = interfaces;
		foreach (Type type in array)
		{
			string text = (type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName);
			if (text == "System.Collections.Generic.IDictionary`2" || text == "Windows.Foundation.Collections.IMap`2")
			{
				return type;
			}
		}
		Type[] array2 = interfaces;
		foreach (Type type2 in array2)
		{
			string text2 = (type2.IsGenericType ? type2.GetGenericTypeDefinition().FullName : type2.FullName);
			if (text2 == "System.Collections.Generic.ICollection`1" || text2 == "Windows.Foundation.Collections.IVector`1")
			{
				return type2;
			}
		}
		return null;
	}

	private bool GetCollectionKind(Type type, out XamlCollectionKind collectionKind)
	{
		collectionKind = XamlCollectionKind.None;
		string text = (type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName);
		string methodName = string.Empty;
		int num = 0;
		switch (text)
		{
		case "System.Collections.Generic.ICollection`1":
			collectionKind = XamlCollectionKind.Collection;
			methodName = "Add";
			num = 1;
			break;
		case "Windows.Foundation.Collections.IVector`1":
			collectionKind = XamlCollectionKind.Collection;
			methodName = "Append";
			num = 1;
			break;
		case "System.Collections.Generic.IDictionary`2":
			collectionKind = XamlCollectionKind.Dictionary;
			methodName = "Add";
			num = 2;
			break;
		case "Windows.Foundation.Collections.IMap`2":
			collectionKind = XamlCollectionKind.Dictionary;
			methodName = "Insert";
			num = 2;
			break;
		}
		if (collectionKind != XamlCollectionKind.None)
		{
			addMethod = GetMethodWithNParameters(type, methodName, num, out var hasMoreThanOne);
			if (hasMoreThanOne)
			{
				DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
				directUISchemaContext.SchemaErrors.Add(new XamlSchemaError_AmbiguousCollectionAdd(base.UnderlyingType.FullName, methodName, num));
			}
			return addMethod != null;
		}
		return false;
	}

	private MethodInfo GetMethodWithNParameters(Type type, string methodName, int paramCount, out bool hasMoreThanOne)
	{
		MethodInfo methodInfo = null;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
		MemberInfo[] member = type.GetMember(methodName, MemberTypes.Method, bindingAttr);
		if (member != null)
		{
			MemberInfo[] array = member;
			foreach (MemberInfo memberInfo in array)
			{
				MethodInfo methodInfo2 = (MethodInfo)memberInfo;
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				if (parameters != null && parameters.Length == paramCount)
				{
					if (methodInfo != null)
					{
						hasMoreThanOne = true;
						return methodInfo;
					}
					methodInfo = methodInfo2;
				}
			}
		}
		hasMoreThanOne = false;
		return methodInfo;
	}

	private bool IsAttachablePropertyGetter(MethodInfo methodInfo, out string name)
	{
		name = null;
		if (!KS.StartsWith(methodInfo.Name, "Get"))
		{
			return false;
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length != 1 || methodInfo.ReturnType == typeof(void))
		{
			return false;
		}
		if (methodInfo.IsGenericMethod || methodInfo.ReturnType.IsGenericParameter || parameters[0].ParameterType.IsGenericType)
		{
			return false;
		}
		name = methodInfo.Name.Substring("Get".Length);
		return true;
	}

	private bool IsAttachablePropertySetter(MethodInfo methodInfo, out string name)
	{
		name = null;
		if (!KS.StartsWith(methodInfo.Name, "Set"))
		{
			return false;
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length != 2)
		{
			return false;
		}
		if (methodInfo.IsGenericMethod || methodInfo.ReturnType.IsGenericParameter || parameters[0].ParameterType.IsGenericType || parameters[1].ParameterType.IsGenericType)
		{
			return false;
		}
		name = methodInfo.Name.Substring("Set".Length);
		return true;
	}

	private XamlMember GetPropertyOrUnknown(string propertyName, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = (skipReadOnlyCheck ? LookupMember(propertyName, skipReadOnlyCheck: true) : GetMember(propertyName));
		if (xamlMember == null)
		{
			xamlMember = new DirectUIXamlMember(propertyName, this);
		}
		return xamlMember;
	}

	private string LookupCreateFromStringMethod()
	{
		foreach (CustomAttributeData customAttributeDatum in ReflectionHelper.GetCustomAttributeData(base.UnderlyingType, inherit: false, "Windows.Foundation.Metadata.CreateFromStringAttribute"))
		{
			foreach (CustomAttributeNamedArgument namedArgument in customAttributeDatum.NamedArguments)
			{
				if (namedArgument.MemberName.Equals("MethodName", StringComparison.InvariantCultureIgnoreCase))
				{
					return namedArgument.TypedValue.Value.ToString();
				}
			}
		}
		return string.Empty;
	}

	internal static bool LookupHasWinUIContract(Type type)
	{
		CustomAttributeData attribute = GetAttribute(type, "Windows.Foundation.Metadata.ContractVersionAttribute", inherited: false);
		if (attribute != null)
		{
			try
			{
				Type type2 = attribute?.ConstructorArguments?[0].Value as Type;
				if (type2 != null)
				{
					return type2.FullName == "Microsoft.UI.Xaml.WinUIContract";
				}
			}
			catch (TypeLoadException)
			{
			}
		}
		return false;
	}

	private bool LookupHasWinUIContract()
	{
		return LookupHasWinUIContract(base.UnderlyingType);
	}
}
