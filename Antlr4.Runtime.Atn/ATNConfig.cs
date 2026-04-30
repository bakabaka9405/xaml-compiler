using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class ATNConfig
{
	private class SemanticContextATNConfig : ATNConfig
	{
		[NotNull]
		private readonly SemanticContext semanticContext;

		public override SemanticContext SemanticContext => semanticContext;

		public SemanticContextATNConfig(SemanticContext semanticContext, ATNState state, int alt, PredictionContext context)
			: base(state, alt, context)
		{
			this.semanticContext = semanticContext;
		}

		public SemanticContextATNConfig(SemanticContext semanticContext, ATNConfig c, ATNState state, PredictionContext context)
			: base(c, state, context)
		{
			this.semanticContext = semanticContext;
		}
	}

	private class ActionATNConfig : ATNConfig
	{
		private readonly LexerActionExecutor lexerActionExecutor;

		private readonly bool passedThroughNonGreedyDecision;

		public override LexerActionExecutor ActionExecutor => lexerActionExecutor;

		public override bool PassedThroughNonGreedyDecision => passedThroughNonGreedyDecision;

		public ActionATNConfig(LexerActionExecutor lexerActionExecutor, ATNState state, int alt, PredictionContext context, bool passedThroughNonGreedyDecision)
			: base(state, alt, context)
		{
			this.lexerActionExecutor = lexerActionExecutor;
			this.passedThroughNonGreedyDecision = passedThroughNonGreedyDecision;
		}

		protected internal ActionATNConfig(LexerActionExecutor lexerActionExecutor, ATNConfig c, ATNState state, PredictionContext context, bool passedThroughNonGreedyDecision)
			: base(c, state, context)
		{
			if (c.SemanticContext != SemanticContext.None)
			{
				throw new NotSupportedException();
			}
			this.lexerActionExecutor = lexerActionExecutor;
			this.passedThroughNonGreedyDecision = passedThroughNonGreedyDecision;
		}
	}

	private class ActionSemanticContextATNConfig : SemanticContextATNConfig
	{
		private readonly LexerActionExecutor lexerActionExecutor;

		private readonly bool passedThroughNonGreedyDecision;

		public override LexerActionExecutor ActionExecutor => lexerActionExecutor;

		public override bool PassedThroughNonGreedyDecision => passedThroughNonGreedyDecision;

		public ActionSemanticContextATNConfig(LexerActionExecutor lexerActionExecutor, SemanticContext semanticContext, ATNState state, int alt, PredictionContext context, bool passedThroughNonGreedyDecision)
			: base(semanticContext, state, alt, context)
		{
			this.lexerActionExecutor = lexerActionExecutor;
			this.passedThroughNonGreedyDecision = passedThroughNonGreedyDecision;
		}

		public ActionSemanticContextATNConfig(LexerActionExecutor lexerActionExecutor, SemanticContext semanticContext, ATNConfig c, ATNState state, PredictionContext context, bool passedThroughNonGreedyDecision)
			: base(semanticContext, c, state, context)
		{
			this.lexerActionExecutor = lexerActionExecutor;
			this.passedThroughNonGreedyDecision = passedThroughNonGreedyDecision;
		}
	}

	private const int SuppressPrecedenceFilter = int.MinValue;

	[NotNull]
	private readonly ATNState state;

	private int altAndOuterContextDepth;

	[NotNull]
	private PredictionContext context;

	public ATNState State => state;

	public int Alt => altAndOuterContextDepth & 0xFFFFFF;

	public virtual PredictionContext Context
	{
		get
		{
			return context;
		}
		set
		{
			context = value;
		}
	}

	public bool ReachesIntoOuterContext => OuterContextDepth != 0;

	public virtual int OuterContextDepth
	{
		get
		{
			return (altAndOuterContextDepth >>> 24) & 0x7F;
		}
		set
		{
			int val = value;
			val = Math.Min(val, 127);
			altAndOuterContextDepth = (val << 24) | (altAndOuterContextDepth & -2130706433);
		}
	}

	public virtual LexerActionExecutor ActionExecutor => null;

	public virtual SemanticContext SemanticContext => SemanticContext.None;

	public virtual bool PassedThroughNonGreedyDecision => false;

	public bool PrecedenceFilterSuppressed
	{
		get
		{
			return (altAndOuterContextDepth & int.MinValue) != 0;
		}
		set
		{
			if (value)
			{
				altAndOuterContextDepth |= int.MinValue;
			}
			else
			{
				altAndOuterContextDepth &= int.MaxValue;
			}
		}
	}

	protected internal ATNConfig(ATNState state, int alt, PredictionContext context)
	{
		this.state = state;
		altAndOuterContextDepth = alt;
		this.context = context;
	}

	protected internal ATNConfig(ATNConfig c, ATNState state, PredictionContext context)
	{
		this.state = state;
		altAndOuterContextDepth = c.altAndOuterContextDepth;
		this.context = context;
	}

	public static ATNConfig Create(ATNState state, int alt, PredictionContext context)
	{
		return Create(state, alt, context, SemanticContext.None, null);
	}

	public static ATNConfig Create(ATNState state, int alt, PredictionContext context, SemanticContext semanticContext)
	{
		return Create(state, alt, context, semanticContext, null);
	}

	public static ATNConfig Create(ATNState state, int alt, PredictionContext context, SemanticContext semanticContext, LexerActionExecutor lexerActionExecutor)
	{
		if (semanticContext != SemanticContext.None)
		{
			if (lexerActionExecutor != null)
			{
				return new ActionSemanticContextATNConfig(lexerActionExecutor, semanticContext, state, alt, context, passedThroughNonGreedyDecision: false);
			}
			return new SemanticContextATNConfig(semanticContext, state, alt, context);
		}
		if (lexerActionExecutor != null)
		{
			return new ActionATNConfig(lexerActionExecutor, state, alt, context, passedThroughNonGreedyDecision: false);
		}
		return new ATNConfig(state, alt, context);
	}

	public ATNConfig Clone()
	{
		return Transform(State, checkNonGreedy: false);
	}

	public ATNConfig Transform(ATNState state, bool checkNonGreedy)
	{
		return Transform(state, context, SemanticContext, checkNonGreedy, ActionExecutor);
	}

	public ATNConfig Transform(ATNState state, SemanticContext semanticContext, bool checkNonGreedy)
	{
		return Transform(state, context, semanticContext, checkNonGreedy, ActionExecutor);
	}

	public ATNConfig Transform(ATNState state, PredictionContext context, bool checkNonGreedy)
	{
		return Transform(state, context, SemanticContext, checkNonGreedy, ActionExecutor);
	}

	public ATNConfig Transform(ATNState state, LexerActionExecutor lexerActionExecutor, bool checkNonGreedy)
	{
		return Transform(state, context, SemanticContext, checkNonGreedy, lexerActionExecutor);
	}

	private ATNConfig Transform(ATNState state, PredictionContext context, SemanticContext semanticContext, bool checkNonGreedy, LexerActionExecutor lexerActionExecutor)
	{
		bool flag = checkNonGreedy && CheckNonGreedyDecision(this, state);
		if (semanticContext != SemanticContext.None)
		{
			if (lexerActionExecutor != null || flag)
			{
				return new ActionSemanticContextATNConfig(lexerActionExecutor, semanticContext, this, state, context, flag);
			}
			return new SemanticContextATNConfig(semanticContext, this, state, context);
		}
		if (lexerActionExecutor != null || flag)
		{
			return new ActionATNConfig(lexerActionExecutor, this, state, context, flag);
		}
		return new ATNConfig(this, state, context);
	}

	private static bool CheckNonGreedyDecision(ATNConfig source, ATNState target)
	{
		if (!source.PassedThroughNonGreedyDecision)
		{
			if (target is DecisionState)
			{
				return ((DecisionState)target).nonGreedy;
			}
			return false;
		}
		return true;
	}

	public virtual ATNConfig AppendContext(int context, PredictionContextCache contextCache)
	{
		PredictionContext predictionContext = Context.AppendContext(context, contextCache);
		return Transform(State, predictionContext, checkNonGreedy: false);
	}

	public virtual ATNConfig AppendContext(PredictionContext context, PredictionContextCache contextCache)
	{
		PredictionContext predictionContext = Context.AppendContext(context, contextCache);
		return Transform(State, predictionContext, checkNonGreedy: false);
	}

	public virtual bool Contains(ATNConfig subconfig)
	{
		if (state.stateNumber != subconfig.State.stateNumber || Alt != subconfig.Alt || !SemanticContext.Equals(subconfig.SemanticContext))
		{
			return false;
		}
		Stack<PredictionContext> stack = new Stack<PredictionContext>();
		Stack<PredictionContext> stack2 = new Stack<PredictionContext>();
		stack.Push(Context);
		stack2.Push(subconfig.Context);
		while (stack.Count > 0)
		{
			PredictionContext predictionContext = stack.Pop();
			PredictionContext predictionContext2 = stack2.Pop();
			if (predictionContext == predictionContext2)
			{
				return true;
			}
			if (predictionContext.Size < predictionContext2.Size)
			{
				return false;
			}
			if (predictionContext2.IsEmpty)
			{
				return predictionContext.HasEmpty;
			}
			for (int i = 0; i < predictionContext2.Size; i++)
			{
				int num = predictionContext.FindReturnState(predictionContext2.GetReturnState(i));
				if (num < 0)
				{
					return false;
				}
				stack.Push(predictionContext.GetParent(num));
				stack2.Push(predictionContext2.GetParent(i));
			}
		}
		return false;
	}

	public override bool Equals(object o)
	{
		if (!(o is ATNConfig))
		{
			return false;
		}
		return Equals((ATNConfig)o);
	}

	public virtual bool Equals(ATNConfig other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (State.stateNumber == other.State.stateNumber && Alt == other.Alt && ReachesIntoOuterContext == other.ReachesIntoOuterContext && Context.Equals(other.Context) && SemanticContext.Equals(other.SemanticContext) && PrecedenceFilterSuppressed == other.PrecedenceFilterSuppressed && PassedThroughNonGreedyDecision == other.PassedThroughNonGreedyDecision)
		{
			return EqualityComparer<LexerActionExecutor>.Default.Equals(ActionExecutor, other.ActionExecutor);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize(7);
		hash = MurmurHash.Update(hash, State.stateNumber);
		hash = MurmurHash.Update(hash, Alt);
		hash = MurmurHash.Update(hash, ReachesIntoOuterContext ? 1 : 0);
		hash = MurmurHash.Update(hash, Context);
		hash = MurmurHash.Update(hash, SemanticContext);
		hash = MurmurHash.Update(hash, PassedThroughNonGreedyDecision ? 1 : 0);
		hash = MurmurHash.Update(hash, ActionExecutor);
		return MurmurHash.Finish(hash, 7);
	}

	public virtual string ToDotString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("digraph G {\n");
		stringBuilder.Append("rankdir=LR;\n");
		HashSet<PredictionContext> hashSet = new HashSet<PredictionContext>();
		Stack<PredictionContext> stack = new Stack<PredictionContext>();
		stack.Push(Context);
		hashSet.Add(Context);
		while (stack.Count > 0)
		{
			PredictionContext predictionContext = stack.Pop();
			for (int i = 0; i < predictionContext.Size; i++)
			{
				stringBuilder.Append("  s").Append(RuntimeHelpers.GetHashCode(predictionContext));
				stringBuilder.Append("->");
				stringBuilder.Append("s").Append(RuntimeHelpers.GetHashCode(predictionContext.GetParent(i)));
				stringBuilder.Append("[label=\"").Append(predictionContext.GetReturnState(i)).Append("\"];\n");
				if (hashSet.Add(predictionContext.GetParent(i)))
				{
					stack.Push(predictionContext.GetParent(i));
				}
			}
		}
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return ToString(null, showAlt: true, showContext: false);
	}

	public virtual string ToString(IRecognizer recog, bool showAlt)
	{
		return ToString(recog, showAlt, showContext: true);
	}

	public virtual string ToString(IRecognizer recog, bool showAlt, bool showContext)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = ((!showContext) ? new string[1] { "?" } : Context.ToStrings(recog, State.stateNumber));
		bool flag = true;
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append('(');
			stringBuilder.Append(State);
			if (showAlt)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(Alt);
			}
			if (Context != null)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(value);
			}
			if (SemanticContext != null && SemanticContext != SemanticContext.None)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(SemanticContext);
			}
			if (ReachesIntoOuterContext)
			{
				stringBuilder.Append(",up=").Append(OuterContextDepth);
			}
			stringBuilder.Append(')');
		}
		return stringBuilder.ToString();
	}
}
