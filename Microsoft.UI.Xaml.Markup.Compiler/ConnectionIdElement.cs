using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[ContentProperty("FieldDefinition")]
internal class ConnectionIdElement
{
	private List<EventAssignment> eventAssignments;

	private List<BindAssignment> bindAssignments;

	private List<BoundEventAssignment> boundEventAssignments;

	public ApiInformation ApiInformation { get; }

	public PhaseAssignment PhaseAssignment { get; }

	public bool IsBoundNamedTemplateElement
	{
		get
		{
			if (HasRootNamedElementStep)
			{
				return IsTemplateChild;
			}
			return false;
		}
	}

	public bool IsUsedByOtherScopes { get; set; }

	public bool HasRewritableAttributes
	{
		get
		{
			if (!HasEventAssignments && !HasBoundEventAssignments && !HasBindAssignments && !HasFieldDefinition && !IsBoundNamedTemplateElement && !IsBindingRoot && !IsUsedByOtherScopes)
			{
				if (Type.IsDerivedFromFrameworkTemplate())
				{
					return BindUniverse.NeededForOuterScopeElement;
				}
				return false;
			}
			return true;
		}
	}

	public LineNumberInfo LineNumberInfo { get; set; }

	public XamlFileCodeInfo ParentFileCodeInfo { get; private set; }

	public XamlType Type { get; set; }

	public int ConnectionId { get; }

	public string ElementName { get; }

	public FieldDefinition FieldDefinition { get; set; }

	public bool HasFieldDefinition => FieldDefinition != null;

	public RootNamedElementStep RootNamedElementStep { get; set; }

	public bool HasRootNamedElementStep => RootNamedElementStep != null;

	public BindUniverse BindUniverse { get; set; }

	public bool HasEventAssignments
	{
		get
		{
			if (eventAssignments != null)
			{
				return eventAssignments.Count > 0;
			}
			return false;
		}
	}

	public bool CanBeInstantiatedLater { get; private set; }

	public bool IsUnloadableRoot { get; private set; }

	public string DefaultBindMode { get; private set; }

	public IList<ConnectionIdElement> Children { get; }

	public List<EventAssignment> EventAssignments
	{
		get
		{
			if (eventAssignments == null)
			{
				eventAssignments = new List<EventAssignment>();
			}
			return eventAssignments;
		}
	}

	public IEnumerable<BindAssignment> InputPropertyBindAssignments => TwoWayBindAssignments.Where((BindAssignment ba) => ba.IsInputPropertyAssignment);

	public bool IsValueRequired
	{
		get
		{
			if (TryGetValidationContextStep(out var bindStep))
			{
				return bindStep.IsValueRequired;
			}
			return false;
		}
	}

	public bool HasBindAssignments
	{
		get
		{
			if (bindAssignments != null)
			{
				return bindAssignments.Count > 0;
			}
			return false;
		}
	}

	public IList<BindAssignment> BindAssignments
	{
		get
		{
			if (bindAssignments == null)
			{
				bindAssignments = new List<BindAssignment>();
			}
			return bindAssignments;
		}
	}

	public IEnumerable<BindAssignment> TwoWayBindAssignments => BindAssignments.Where((BindAssignment ba) => ba.IsTrackingTarget);

	public bool IsBindingRoot
	{
		get
		{
			if (BindUniverse == null)
			{
				return false;
			}
			if (BindUniverse.RootElement == this && !BindUniverse.RootElement.Type.IsDerivedFromDataTemplate())
			{
				if (!BindUniverse.HasBindAssignments && !BindUniverse.HasBoundEventAssignments)
				{
					return BindUniverse.NeededForOuterScopeElement;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsBindingFileRoot
	{
		get
		{
			if (IsBindingRoot)
			{
				return BindUniverse.IsFileRoot;
			}
			return false;
		}
	}

	public bool IsTemplateChild
	{
		get
		{
			if (BindUniverse == null)
			{
				return false;
			}
			XamlType type = BindUniverse.RootElement.Type;
			if (!type.IsDerivedFromControlTemplate())
			{
				return type.IsDerivedFromDataTemplate();
			}
			return true;
		}
	}

	public bool NeedsConnectCase
	{
		get
		{
			if (FieldDefinition == null)
			{
				return EventAssignments.Count > 0;
			}
			return true;
		}
	}

	public bool HasPhase => PhaseAssignment != null;

	public bool HasBoundEventAssignments
	{
		get
		{
			if (boundEventAssignments != null)
			{
				return boundEventAssignments.Count > 0;
			}
			return false;
		}
	}

	public List<BoundEventAssignment> BoundEventAssignments
	{
		get
		{
			if (boundEventAssignments == null)
			{
				boundEventAssignments = new List<BoundEventAssignment>();
			}
			return boundEventAssignments;
		}
	}

	public bool IsWeakRef
	{
		get
		{
			if (IsBindingRoot)
			{
				return !Type.IsDerivedFromControlTemplate();
			}
			return false;
		}
	}

	public bool NeedsNullCheckBeforeSetValue
	{
		get
		{
			if (!IsWeakRef)
			{
				return CanBeInstantiatedLater;
			}
			return true;
		}
	}

	public string ObjectCodeName => $"obj{ConnectionId}";

	public string ElementCodeName => $"element{ConnectionId}";

	public string ElementTemplatedParentCodeName => $"templatedParent{BindUniverse.RootElement.ConnectionId}";

	public XamlType TemplatedParentType => BindUniverse.DataRootType;

	public LanguageSpecificString ReferenceExpression
	{
		get
		{
			if (!IsWeakRef)
			{
				return new LanguageSpecificString(() => "this->" + ObjectCodeName, () => ObjectCodeName ?? "", () => "this." + ObjectCodeName, () => "Me." + ObjectCodeName);
			}
			return new LanguageSpecificString(() => "this->" + ObjectCodeName + ".Resolve<" + Type.CppCXName(IncludeHatIfApplicable: false) + ">()", () => "this->" + ObjectCodeName + ".get()", () => $"(this.{ObjectCodeName}.Target as {Type.CSharpName()})", () => $"TryCast(Me.{ObjectCodeName}.Target, {Type.VBName()})");
		}
	}

	internal string LineNumberAndXamlFile => $"{ParentFileCodeInfo.ApparentRelativePath} line {LineNumberInfo.StartLineNumber}";

	public IEnumerable<ConnectionIdElement> ElementAndAllChildren
	{
		get
		{
			yield return this;
			foreach (ConnectionIdElement allChild in AllChildren)
			{
				yield return allChild;
			}
		}
	}

	public IEnumerable<ConnectionIdElement> AllChildren
	{
		get
		{
			foreach (ConnectionIdElement item in Children.SelectMany((ConnectionIdElement c) => c.ElementAndAllChildren).Distinct())
			{
				yield return item;
			}
		}
	}

	private ConnectionIdElement(XamlDomObject domObject, BindUniverse bindUniverse, XamlFileCodeInfo fileCodeInfo, XamlClassCodeInfo classCodeInfo, XamlType dataRootType)
	{
		Type = domObject.Type;
		ApiInformation = domObject.ApiInformation;
		ConnectionId = classCodeInfo.NextConnectionId;
		LineNumberInfo = new LineNumberInfo(domObject);
		ParentFileCodeInfo = fileCodeInfo;
		Children = new List<ConnectionIdElement>();
		DefaultBindMode = DomHelper.GetDefaultBindMode(domObject);
		XamlDomMember aliasedMemberNode = DomHelper.GetAliasedMemberNode(domObject, XamlLanguage.Name, forcePass1Eval: true);
		if (aliasedMemberNode != null)
		{
			ElementName = DomHelper.GetStringValueOfProperty(aliasedMemberNode);
		}
		if (bindUniverse == null)
		{
			bindUniverse = new BindUniverse(this, dataRootType, !DomHelper.UnderANamescope(domObject) && !DomHelper.IsDerivedFromControlTemplate(domObject) && !DomHelper.IsDerivedFromDataTemplate(domObject), classCodeInfo.ClassName.ShortName);
		}
		BindUniverse = bindUniverse;
		if (!string.IsNullOrEmpty(ElementName))
		{
			BindUniverse.NamedElements.Add(this);
		}
		foreach (XamlDomMember item in domObject.MemberNodes.Where((XamlDomMember mem) => DomHelper.IsPhaseMember(mem.Member)))
		{
			PhaseAssignment = new PhaseAssignment(item, this);
			bindUniverse.AddPhase(PhaseAssignment);
			fileCodeInfo.HasPhaseAssignments = true;
		}
		foreach (XamlDomMember item2 in domObject.MemberNodes.Where((XamlDomMember mem) => mem.Items.Count == 1 && mem.Item is XamlDomObject && !mem.Member.IsEvent && domObject.SchemaContext is DirectUISchemaContext && ((XamlDomObject)mem.Item).Type == ((DirectUISchemaContext)domObject.SchemaContext).DirectUIXamlLanguage.BindExtension))
		{
			BindAssignment bindAssignment = BindAssignment.Create(item2, bindUniverse, this);
			BindAssignments.Add(bindAssignment);
			bindUniverse.BindAssignments.Add(bindAssignment);
			fileCodeInfo.BindStatus |= bindAssignment.BindStatus;
		}
		CanBeInstantiatedLater = DomHelper.CanBeInstantiatedLater(domObject);
		IsUnloadableRoot = DomHelper.HasLoadOrDeferLoadStrategyMember(domObject);
	}

	public ConnectionIdElement(XamlDomObject domObject, BindUniverse bindUniverse, XamlFileCodeInfo fileCodeInfo, XamlClassCodeInfo classCodeInfo, XamlType dataRootType, bool skipFieldDefinition)
		: this(domObject, bindUniverse, fileCodeInfo, classCodeInfo, dataRootType)
	{
		Type = domObject.Type;
		if (!skipFieldDefinition && !string.IsNullOrEmpty(ElementName))
		{
			FieldDefinition = new FieldDefinition(domObject);
		}
		foreach (XamlDomMember item3 in domObject.MemberNodes.Where((XamlDomMember member) => member.Member.IsEvent))
		{
			if (item3.Member.IsAttachable)
			{
				throw new ArgumentException("Attached Events are not supported");
			}
			if (item3.Item is XamlDomObject && domObject.SchemaContext is DirectUISchemaContext && ((XamlDomObject)item3.Item).Type == ((DirectUISchemaContext)domObject.SchemaContext).DirectUIXamlLanguage.BindExtension)
			{
				BoundEventAssignment item = new BoundEventAssignment(item3, BindUniverse, this);
				BoundEventAssignments.Add(item);
				BindUniverse.BoundEventAssignments.Add(item);
				fileCodeInfo.BindStatus |= BindStatus.HasEventBinding;
			}
			else
			{
				EventAssignment item2 = new EventAssignment(item3);
				EventAssignments.Add(item2);
				fileCodeInfo.HasEventAssignments = true;
			}
		}
	}

	public ConnectionIdElement(XamlDomObject domObject, BindUniverse bindUniverse, XamlFileCodeInfo fileCodeInfo, XamlClassCodeInfo classCodeInfo, XamlType dataRootType, bool skipFieldDefinition, string clrPath)
		: this(domObject, bindUniverse, fileCodeInfo, classCodeInfo, dataRootType)
	{
		Type = null;
		if (!skipFieldDefinition && !string.IsNullOrEmpty(ElementName))
		{
			FieldDefinition = new FieldDefinition(domObject, clrPath);
		}
	}

	public bool TryGetValidationContextStep(out PropertyStep bindStep)
	{
		PropertyStep propertyStep = null;
		if (Type.ImplementsIInputValidationControl())
		{
			foreach (BindAssignment inputPropertyBindAssignment in InputPropertyBindAssignments)
			{
				if (inputPropertyBindAssignment.PathStep is PropertyStep propertyStep2)
				{
					propertyStep = propertyStep2;
				}
				else if (inputPropertyBindAssignment.PathStep is FunctionStep functionStep)
				{
					IEnumerable<FunctionPathParam> source = from p in functionStep.Parameters.OfType<FunctionPathParam>()
						where p.Path is PropertyStep
						select p;
					propertyStep = ((!source.Any((FunctionPathParam param) => param.Path.IsValueRequired)) ? (source.FirstOrDefault()?.Path as PropertyStep) : (source.First((FunctionPathParam param) => param.Path.IsValueRequired).Path as PropertyStep));
				}
			}
		}
		bindStep = propertyStep;
		return propertyStep != null;
	}

	public LanguageSpecificString GetMemberGetExpression(BindAssignmentBase bindAssignment)
	{
		if (!bindAssignment.IsAttachable)
		{
			return new LanguageSpecificString(() => $"{ReferenceExpression.CppCXName()}->{bindAssignment.MemberName}", () => ReferenceExpression.CppWinRTName() + "." + bindAssignment.MemberName + "()", () => $"{ReferenceExpression.CSharpName()}.{bindAssignment.MemberName}", () => $"{ReferenceExpression.VBName()}.{bindAssignment.MemberName}");
		}
		return new LanguageSpecificString(() => $"{bindAssignment.MemberDeclaringType.CppCXName(IncludeHatIfApplicable: false)}::Get{bindAssignment.MemberName}({ReferenceExpression.CppCXName()})", () => bindAssignment.MemberDeclaringType.CppWinRTName() + "::Get" + bindAssignment.MemberName + "(" + ReferenceExpression.CppWinRTName() + ")", () => $"{bindAssignment.MemberDeclaringType.CSharpName()}.Get{bindAssignment.MemberName}({ReferenceExpression.CSharpName()})", () => $"{bindAssignment.MemberDeclaringType.VBName()}.Get{bindAssignment.MemberName}({ReferenceExpression.VBName()})");
	}
}
