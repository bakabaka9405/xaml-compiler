using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class ParserInterpreter : Parser
{
	private readonly string _grammarFileName;

	private readonly ATN _atn;

	protected internal readonly BitSet pushRecursionContextStates;

	private readonly string[] _ruleNames;

	[NotNull]
	private readonly IVocabulary vocabulary;

	private readonly Stack<Tuple<ParserRuleContext, int>> _parentContextStack = new Stack<Tuple<ParserRuleContext, int>>();

	public override ATN Atn => _atn;

	public override IVocabulary Vocabulary => vocabulary;

	public override string[] RuleNames => _ruleNames;

	public override string GrammarFileName => _grammarFileName;

	protected internal virtual ATNState AtnState => _atn.states[base.State];

	public ParserInterpreter(string grammarFileName, IVocabulary vocabulary, IEnumerable<string> ruleNames, ATN atn, ITokenStream input)
		: base(input)
	{
		_grammarFileName = grammarFileName;
		_atn = atn;
		_ruleNames = ruleNames.ToArray();
		this.vocabulary = vocabulary;
		pushRecursionContextStates = new BitSet(atn.states.Count);
		foreach (ATNState state in atn.states)
		{
			if (state is StarLoopEntryState && ((StarLoopEntryState)state).precedenceRuleDecision)
			{
				pushRecursionContextStates.Set(state.stateNumber);
			}
		}
		Interpreter = new ParserATNSimulator(this, atn);
	}

	public virtual ParserRuleContext Parse(int startRuleIndex)
	{
		RuleStartState ruleStartState = _atn.ruleToStartState[startRuleIndex];
		InterpreterRuleContext interpreterRuleContext = new InterpreterRuleContext(null, -1, startRuleIndex);
		if (ruleStartState.isPrecedenceRule)
		{
			EnterRecursionRule(interpreterRuleContext, ruleStartState.stateNumber, startRuleIndex, 0);
		}
		else
		{
			EnterRule(interpreterRuleContext, ruleStartState.stateNumber, startRuleIndex);
		}
		while (true)
		{
			ATNState atnState = AtnState;
			StateType stateType = atnState.StateType;
			if (stateType == StateType.RuleStop)
			{
				if (RuleContext.IsEmpty)
				{
					break;
				}
				VisitRuleStopState(atnState);
				continue;
			}
			try
			{
				VisitState(atnState);
			}
			catch (RecognitionException ex)
			{
				base.State = _atn.ruleToStopState[atnState.ruleIndex].stateNumber;
				Context.exception = ex;
				ErrorHandler.ReportError(this, ex);
				ErrorHandler.Recover(this, ex);
			}
		}
		if (ruleStartState.isPrecedenceRule)
		{
			ParserRuleContext ruleContext = RuleContext;
			Tuple<ParserRuleContext, int> tuple = _parentContextStack.Pop();
			UnrollRecursionContexts(tuple.Item1);
			return ruleContext;
		}
		ExitRule();
		return interpreterRuleContext;
	}

	public override void EnterRecursionRule(ParserRuleContext localctx, int state, int ruleIndex, int precedence)
	{
		_parentContextStack.Push(Tuple.Create(RuleContext, localctx.invokingState));
		base.EnterRecursionRule(localctx, state, ruleIndex, precedence);
	}

	protected internal virtual void VisitState(ATNState p)
	{
		int num;
		if (p.NumberOfTransitions > 1)
		{
			ErrorHandler.Sync(this);
			num = Interpreter.AdaptivePredict(base.TokenStream, ((DecisionState)p).decision, RuleContext);
		}
		else
		{
			num = 1;
		}
		Transition transition = p.Transition(num - 1);
		switch (transition.TransitionType)
		{
		case TransitionType.Epsilon:
			if (pushRecursionContextStates.Get(p.stateNumber) && !(transition.target is LoopEndState))
			{
				InterpreterRuleContext localctx2 = new InterpreterRuleContext(_parentContextStack.Peek().Item1, _parentContextStack.Peek().Item2, RuleContext.RuleIndex);
				PushNewRecursionContext(localctx2, _atn.ruleToStartState[p.ruleIndex].stateNumber, RuleContext.RuleIndex);
			}
			break;
		case TransitionType.Atom:
			Match(((AtomTransition)transition).token);
			break;
		case TransitionType.Range:
		case TransitionType.Set:
		case TransitionType.NotSet:
			if (!transition.Matches(base.TokenStream.La(1), 1, 65535))
			{
				ErrorHandler.RecoverInline(this);
			}
			MatchWildcard();
			break;
		case TransitionType.Wildcard:
			MatchWildcard();
			break;
		case TransitionType.Rule:
		{
			RuleStartState ruleStartState = (RuleStartState)transition.target;
			int ruleIndex = ruleStartState.ruleIndex;
			InterpreterRuleContext localctx = new InterpreterRuleContext(RuleContext, p.stateNumber, ruleIndex);
			if (ruleStartState.isPrecedenceRule)
			{
				EnterRecursionRule(localctx, ruleStartState.stateNumber, ruleIndex, ((RuleTransition)transition).precedence);
			}
			else
			{
				EnterRule(localctx, transition.target.stateNumber, ruleIndex);
			}
			break;
		}
		case TransitionType.Predicate:
		{
			PredicateTransition predicateTransition = (PredicateTransition)transition;
			if (!Sempred(RuleContext, predicateTransition.ruleIndex, predicateTransition.predIndex))
			{
				throw new FailedPredicateException(this);
			}
			break;
		}
		case TransitionType.Action:
		{
			ActionTransition actionTransition = (ActionTransition)transition;
			Action(RuleContext, actionTransition.ruleIndex, actionTransition.actionIndex);
			break;
		}
		case TransitionType.Precedence:
			if (!Precpred(RuleContext, ((PrecedencePredicateTransition)transition).precedence))
			{
				throw new FailedPredicateException(this, $"precpred(_ctx, {((PrecedencePredicateTransition)transition).precedence})");
			}
			break;
		default:
			throw new NotSupportedException("Unrecognized ATN transition type.");
		}
		base.State = transition.target.stateNumber;
	}

	protected internal virtual void VisitRuleStopState(ATNState p)
	{
		RuleStartState ruleStartState = _atn.ruleToStartState[p.ruleIndex];
		if (ruleStartState.isPrecedenceRule)
		{
			Tuple<ParserRuleContext, int> tuple = _parentContextStack.Pop();
			UnrollRecursionContexts(tuple.Item1);
			base.State = tuple.Item2;
		}
		else
		{
			ExitRule();
		}
		RuleTransition ruleTransition = (RuleTransition)_atn.states[base.State].Transition(0);
		base.State = ruleTransition.followState.stateNumber;
	}
}
