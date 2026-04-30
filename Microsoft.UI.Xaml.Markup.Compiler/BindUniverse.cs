using System;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BindUniverse : IBindUniverse
{
	private IEnumerable<string> distinctConvertersUsed;

	private readonly string parentClassShortName;

	public IList<BindUniverse> Children = new List<BindUniverse>();

	public BindUniverse Parent;

	public Dictionary<string, BindPathStep> BindPathSteps = new Dictionary<string, BindPathStep>();

	internal List<BindAssignment> BindAssignments = new List<BindAssignment>();

	internal List<BoundEventAssignment> BoundEventAssignments = new List<BoundEventAssignment>();

	internal List<ConnectionIdElement> BoundElements = new List<ConnectionIdElement>();

	internal List<ConnectionIdElement> OuterScopeBoundElements = new List<ConnectionIdElement>();

	internal IList<ConnectionIdElement> UnloadableBindingSourceElements = new List<ConnectionIdElement>();

	private IList<ConnectionIdElement> directParentsOfUnloadables = new List<ConnectionIdElement>();

	internal Dictionary<int, List<PhaseAssignment>> PhaseAssignments = new Dictionary<int, List<PhaseAssignment>>();

	public bool NeededForOuterScopeElement;

	private IEnumerable<FieldDefinition> rootFieldDefinitions;

	public BindPathStep RootStep { get; private set; }

	public IList<ConnectionIdElement> NamedElements { get; }

	internal ConnectionIdElement RootElement { get; private set; }

	internal bool IsFileRoot { get; private set; }

	internal XamlType DataRootType { get; private set; }

	public BindPathStep ElementRootStep { get; private set; }

	public string BindingsClassName => $"{parentClassShortName}_obj{RootElement.ConnectionId}_Bindings";

	public string BindingsTrackingClassName => $"{parentClassShortName}_obj{RootElement.ConnectionId}_BindingsTracking";

	public bool HasBindings => BindPathSteps.Values.Count > 0;

	public bool HasBindAssignments => BindAssignments.Count > 0;

	public bool HasBoundEventAssignments => BoundEventAssignments.Count > 0;

	public bool HasFunctionBindings => BindPathSteps.Where((KeyValuePair<string, BindPathStep> s) => s.Value is FunctionStep).Count() > 0;

	internal bool NeedsIDataTemplateExtension
	{
		get
		{
			if (!IsFileRoot)
			{
				return !RootElement.Type.IsDerivedFromControlTemplate();
			}
			return false;
		}
	}

	internal bool NeedsIDataTemplateComponent => !IsFileRoot;

	internal IEnumerable<string> DistinctConvertersUsed
	{
		get
		{
			if (distinctConvertersUsed == null)
			{
				distinctConvertersUsed = (from bindAssignment in BindAssignments
					where bindAssignment.BindStatus.HasFlag(BindStatus.HasConverter)
					select bindAssignment.Converter).Distinct();
			}
			return distinctConvertersUsed;
		}
	}

	public IEnumerable<BindPathStep> TryGetValueSteps
	{
		get
		{
			Dictionary<string, BindPathStep> dictionary = new Dictionary<string, BindPathStep>();
			foreach (FunctionStep item in from s in BindPathSteps.Values.OfType<FunctionStep>()
				where s.RequiresSafeParameterRetrieval
				select s)
			{
				foreach (FunctionPathParam item2 in from p in item.Parameters.OfType<FunctionPathParam>()
					where p.HasTryGetValue
					select p)
				{
					AddTryGetValueStep(dictionary, item2.Path);
				}
			}
			return dictionary.Values;
		}
	}

	internal IEnumerable<ConnectionIdElement> ElementsWithDisconnectCase => ElementsWithConnectCase.Where((ConnectionIdElement ele) => ele.CanBeInstantiatedLater);

	internal IEnumerable<ConnectionIdElement> ElementsWithBoundLoadAssignments => BoundElements.Where((ConnectionIdElement ele) => ele.BindAssignments.OfType<BoundLoadAssignment>().Any());

	public IEnumerable<ConnectionIdElement> ElementsWithConnectCase => from e in BoundElements.Union(OuterScopeBoundElements).Union(UnloadableBindingSourceElements).Union(directParentsOfUnloadables)
			.Distinct()
		orderby e.ConnectionId
		select e;

	public IEnumerable<ConnectionIdElement> ElementsWithConnectCaseInLocalScope => ElementsWithConnectCase.Where((ConnectionIdElement ele) => ele.BindUniverse == this);

	internal bool NeedsCompleteUpdate
	{
		get
		{
			if (!HasFunctionBindings && !ElementsWithBoundLoadAssignments.Any())
			{
				return UnloadableBindingSourceElements.Any();
			}
			return true;
		}
	}

	public bool NeedsCppBindingTrackingClass => BindPathSteps.Values.Where((BindPathStep s) => s.ValueType.ImplementsIObservableVector() || s.ValueType.ImplementsIObservableMap()).Any();

	public bool NeedsBindingsTracking
	{
		get
		{
			if (!RootStep.IsTrackingSource && !BindPathSteps.Values.OfType<StaticRootStep>().Any())
			{
				if (ElementRootStep != null)
				{
					return ElementRootStep.IsTrackingSource;
				}
				return false;
			}
			return true;
		}
	}

	public IEnumerable<BindPathStep> INDEIPathSteps => BindPathSteps.Values.Where((BindPathStep step) => step.IsIncludedInUpdate && step.IsTrackingSource && step.ImplementsINDEI);

	public BindPathStep MakeOrGetRootStepOutOfScope()
	{
		if (ElementRootStep == null)
		{
			ElementRootStep = new RootStep(DataRootType, isElementRoot: true);
		}
		return ElementRootStep;
	}

	internal BindUniverse(ConnectionIdElement rootElement, XamlType dataRootType, bool isFileRoot, string classShortName)
	{
		RootElement = rootElement;
		IsFileRoot = isFileRoot;
		DataRootType = dataRootType;
		parentClassShortName = classShortName;
		NamedElements = new List<ConnectionIdElement>();
	}

	private void ProcessRootNamedElementSteps(Version targetPlatformMinVersion, List<XamlCompileErrorBase> issues)
	{
		foreach (RootNamedElementStep step in BindPathSteps.Values.OfType<RootNamedElementStep>())
		{
			for (BindUniverse bindUniverse = this; bindUniverse != null; bindUniverse = bindUniverse.Parent)
			{
				if (bindUniverse != this)
				{
					bindUniverse.NeededForOuterScopeElement = true;
				}
				IEnumerable<ConnectionIdElement> source = bindUniverse.RootElement.ElementAndAllChildren.Where((ConnectionIdElement e) => e.ElementName == step.FieldName);
				if (source.Any())
				{
					ConnectionIdElement connectionIdElement = source.First();
					if (bindUniverse == this)
					{
						connectionIdElement.RootNamedElementStep = step;
						if (!connectionIdElement.BindUniverse.IsFileRoot && !BoundElements.Contains(connectionIdElement))
						{
							BoundElements.Add(connectionIdElement);
						}
						if (connectionIdElement.CanBeInstantiatedLater)
						{
							UnloadableBindingSourceElements.Add(connectionIdElement);
						}
						break;
					}
					connectionIdElement.IsUsedByOtherScopes = true;
					if (!OuterScopeBoundElements.Contains(connectionIdElement))
					{
						OuterScopeBoundElements.Add(connectionIdElement);
					}
					NeededForOuterScopeElement = true;
					if (!BoundElements.Contains(connectionIdElement))
					{
						BoundElements.Add(connectionIdElement);
					}
					if (!bindUniverse.BoundElements.Contains(connectionIdElement))
					{
						bindUniverse.BoundElements.Add(connectionIdElement);
					}
					break;
				}
			}
		}
	}

	internal IEnumerable<XamlCompileErrorBase> Parse(XamlClassCodeInfo classCodeInfo, Version targetPlatformMinVersion)
	{
		List<XamlCompileErrorBase> list = new List<XamlCompileErrorBase>();
		if (IsFileRoot)
		{
			rootFieldDefinitions = classCodeInfo.FieldDeclarations;
		}
		RootStep = new RootStep(DataRootType);
		BindPathSteps[""] = RootStep;
		foreach (BindAssignment bindAssignment in BindAssignments)
		{
			IEnumerable<XamlCompileErrorBase> collection = bindAssignment.ParsePath();
			list.AddRange(collection);
		}
		foreach (BoundEventAssignment boundEventAssignment in BoundEventAssignments)
		{
			IEnumerable<XamlCompileErrorBase> collection2 = boundEventAssignment.ParsePath();
			list.AddRange(collection2);
		}
		ProcessRootNamedElementSteps(targetPlatformMinVersion, list);
		foreach (ConnectionIdElement potentialParent in RootElement.AllChildren)
		{
			if (potentialParent.Children.Any((ConnectionIdElement c) => ElementsWithBoundLoadAssignments.Any((ConnectionIdElement e) => c == e)) && ElementsWithBoundLoadAssignments.Any((ConnectionIdElement e) => e.ElementAndAllChildren.Any((ConnectionIdElement c) => c == potentialParent)))
			{
				directParentsOfUnloadables.Add(potentialParent);
			}
		}
		return list;
	}

	internal void AddPhase(PhaseAssignment phase)
	{
		if (!PhaseAssignments.TryGetValue(phase.Phase, out var value))
		{
			value = new List<PhaseAssignment>();
			PhaseAssignments.Add(phase.Phase, value);
		}
		value.Add(phase);
	}

	internal int GetNextPhase(int currentPhase)
	{
		int num = -1;
		foreach (int key in PhaseAssignments.Keys)
		{
			if (key > currentPhase && (num == -1 || key < num))
			{
				num = key;
			}
		}
		return num;
	}

	public BindPathStep EnsureUniquePathStep(BindPathStep step)
	{
		string codeName = step.CodeName;
		if (BindPathSteps.ContainsKey(codeName))
		{
			return BindPathSteps[codeName];
		}
		BindPathSteps[codeName] = step;
		step.Parent?.AddChild(step);
		return step;
	}

	private void AddTryGetValueStep(Dictionary<string, BindPathStep> steps, BindPathStep step)
	{
		if (step.IsIncludedInUpdate)
		{
			steps[step.TryGetValueCodeName] = step;
			if (step.Parent != null)
			{
				AddTryGetValueStep(steps, step.Parent);
			}
		}
	}

	public XamlType GetNamedElementType(string name, out string objectCodeName)
	{
		for (BindUniverse bindUniverse = this; bindUniverse != null; bindUniverse = bindUniverse.Parent)
		{
			ConnectionIdElement connectionIdElement = bindUniverse.NamedElements?.Where((ConnectionIdElement e) => e.ElementName == name).FirstOrDefault();
			if (connectionIdElement != null)
			{
				objectCodeName = connectionIdElement.ObjectCodeName;
				return connectionIdElement.Type;
			}
		}
		objectCodeName = null;
		return null;
	}

	public XamlType GetNamedFieldType(string name)
	{
		return (rootFieldDefinitions?.Where((FieldDefinition f) => f.FieldName == name).FirstOrDefault())?.FieldXamlType;
	}
}
