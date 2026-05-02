using System.Collections.Generic;
using System.Linq;
using System.Xaml;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.UI.Xaml.Markup.Compiler.Parsing;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public abstract class BindPathStep
{
	private BindStatus bindStatus;

	private List<IBindAssignment> bindAssignments = new List<IBindAssignment>();

	private Dictionary<string, BindPathStep> children = new Dictionary<string, BindPathStep>();

	private List<int> distinctPhases;

	private Dictionary<string, BindPathStep> dependents = new Dictionary<string, BindPathStep>();

	private Dictionary<string, BindPathStep> dependencies = new Dictionary<string, BindPathStep>();

	public abstract string UniqueName { get; }

	public virtual XamlType ValueType { get; }

	public BindPathStep Parent { get; }

	public IEnumerable<BindPathStep> Parents
	{
		get
		{
			List<BindPathStep> list = new List<BindPathStep>();
			if (Parent != null)
			{
				list.AddRange(Parent.Parents);
				list.Add(Parent);
			}
			return list;
		}
	}

	public ApiInformation ApiInformation { get; }

	public bool UpdateNeedsBindingsVariable => Children.OfType<RootNamedElementStep>().Any((RootNamedElementStep step) => !string.IsNullOrEmpty(step.UpdateCallParamOverride));

	public string CodeName
	{
		get
		{
			string text = ((ApiInformation == null) ? "" : ("_" + (uint)ApiInformation.UniqueName.GetHashCode()));
			if (string.IsNullOrEmpty(Parent?.CodeName))
			{
				return UniqueName + text;
			}
			return Parent.CodeName + "_" + UniqueName + text;
		}
	}

	public BindStatus BindStatus
	{
		get
		{
			return bindStatus;
		}
		private set
		{
			bindStatus = value;
			if (Parent != null)
			{
				Parent.BindStatus |= value;
			}
			foreach (BindPathStep value2 in dependencies.Values)
			{
				value2.BindStatus |= bindStatus;
			}
		}
	}

	public IEnumerable<IBindAssignment> BindAssignments => bindAssignments;

	public bool RequiresChildNotification
	{
		get
		{
			if (IsTrackingSource)
			{
				if (!ValueType.ImplementsINotifyPropertyChanged() && !ValueType.ImplementsINotifyCollectionChanged() && !ValueType.ImplementsIObservableVector() && !ValueType.ImplementsIObservableMap())
				{
					return HasTrackingDPs;
				}
				return true;
			}
			return false;
		}
	}

	public bool NeedsUpdateChildListeners
	{
		get
		{
			if (IsIncludedInUpdate && RequiresChildNotification)
			{
				if (!(this is PropertyStep) && !(this is CastStep) && !(this is RootNamedElementStep) && !(this is RootStep) && !(this is ArrayIndexStep))
				{
					return this is MapIndexStep;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasTrackingDPs => TrackingSteps.OfType<DependencyPropertyStep>().Count() > 0;

	public bool IsTrackingSource => BindStatus.HasFlag(BindStatus.TracksSource);

	public IEnumerable<BindPathStep> Children => children.Values;

	public IEnumerable<BindPathStep> TrackingSteps => from c in Children.Concat(Dependents)
		where c.IsTrackingSource
		select c;

	public IEnumerable<BindPathStep> Dependents => dependents.Values;

	public IEnumerable<BindPathStep> Dependencies => dependencies.Values;

	internal IList<BindPathStep> ParentsAndSelf
	{
		get
		{
			IList<BindPathStep> list2;
			if (Parent == null)
			{
				IList<BindPathStep> list = new List<BindPathStep>();
				list2 = list;
			}
			else
			{
				list2 = Parent.ParentsAndSelf;
			}
			IList<BindPathStep> list3 = list2;
			list3.Add(this);
			return list3;
		}
	}

	public List<int> DistinctPhases
	{
		get
		{
			if (distinctPhases == null)
			{
				distinctPhases = new List<int> { 0 };
				foreach (IBindAssignment bindAssignment in BindAssignments)
				{
					int computedPhase = bindAssignment.ComputedPhase;
					if (!distinctPhases.Contains(computedPhase))
					{
						distinctPhases.Add(computedPhase);
					}
				}
				foreach (BindPathStep child in Children)
				{
					foreach (int distinctPhase in child.DistinctPhases)
					{
						if (!distinctPhases.Contains(distinctPhase))
						{
							distinctPhases.Add(distinctPhase);
						}
					}
				}
				distinctPhases.Sort();
			}
			return distinctPhases;
		}
	}

	public virtual bool ValueTypeIsConditional => (ValueType as IXamlTypeMeta).HasApiInformation;

	public virtual IEnumerable<IBindAssignment> AssociatedBindAssignments
	{
		get
		{
			foreach (IBindAssignment bindAssignment in BindAssignments)
			{
				yield return bindAssignment;
			}
			foreach (BindPathStep dependent in Dependents)
			{
				foreach (IBindAssignment bindAssignment2 in dependent.BindAssignments)
				{
					yield return bindAssignment2;
				}
			}
		}
	}

	public virtual bool IsValueRequired => false;

	public bool ImplementsINPC => ValueType.ImplementsINotifyPropertyChanged();

	public bool ImplementsINCC => ValueType.ImplementsINotifyCollectionChanged();

	public bool ImplementsINDEI => ValueType.ImplementsINotifyDataErrorInfo();

	public bool ImplementsIObservableVector => ValueType.ImplementsIObservableVector();

	public bool ImplementsIObservableMap => ValueType.ImplementsIObservableMap();

	public virtual bool IsIncludedInUpdate => BindStatus.HasFlag(BindStatus.HasBinding);

	public string PhaseList => string.Join(":", DistinctPhases);

	public virtual bool NeedsCheckForNull => ValueType.IsNullable;

	public string TryGetValueCodeName => $"TryGet_{CodeName}";

	public BindPathStep(XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
	{
		ValueType = valueType;
		Parent = parent;
		ApiInformation = apiInformation;
	}

	public void AddBindAssignment(IBindAssignment bindAssignment)
	{
		bindAssignments.Add(bindAssignment);
		BindStatus |= bindAssignment.BindStatus;
	}

	public void AddChild(BindPathStep step)
	{
		children[step.CodeName] = step;
	}

	public void AddDependent(BindPathStep dependent)
	{
		dependent.BindStatus |= BindStatus;
		dependents[dependent.CodeName] = dependent;
		dependent.dependencies[CodeName] = this;
	}

	public static BindPathStep Parse(string bindPath, ApiInformation apiInformation, IBindUniverse bindUniverse, IXamlTypeResolver resolver, IList<string> warnings)
	{
		AntlrInputStream input = new AntlrInputStream(bindPath);
		BindingPathLexer bindingPathLexer = new BindingPathLexer(input);
		CommonTokenStream input2 = new CommonTokenStream(bindingPathLexer);
		BindingPathParser bindingPathParser = new BindingPathParser(input2);
		bindingPathParser.RemoveErrorListeners();
		bindingPathParser.AddErrorListener(new ParseErrorListener());
		bindingPathParser.ErrorHandler = new Microsoft.UI.Xaml.Markup.Compiler.Parsing.BailErrorStrategy();
		BindingPathListener listener = new BindingPathListener(bindPath, apiInformation, bindUniverse, resolver)
		{
			Warnings = warnings
		};
		ParseTreeWalker parseTreeWalker = new ParseTreeWalker();
		BindingPathParser.PathContext pathContext = bindingPathParser.path();
		parseTreeWalker.Walk(listener, pathContext);
		bindingPathLexer.ConfirmInputFullyConsumed();
		return pathContext.PathStep;
	}
}
