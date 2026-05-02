using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public abstract class SemanticContext
{
	public class Predicate : SemanticContext
	{
		public readonly int ruleIndex;

		public readonly int predIndex;

		public readonly bool isCtxDependent;

		protected internal Predicate()
		{
			ruleIndex = -1;
			predIndex = -1;
			isCtxDependent = false;
		}

		public Predicate(int ruleIndex, int predIndex, bool isCtxDependent)
		{
			this.ruleIndex = ruleIndex;
			this.predIndex = predIndex;
			this.isCtxDependent = isCtxDependent;
		}

		public override bool Eval<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			RuleContext localctx = (isCtxDependent ? parserCallStack : null);
			return parser.Sempred(localctx, ruleIndex, predIndex);
		}

		public override int GetHashCode()
		{
			int hash = MurmurHash.Initialize();
			hash = MurmurHash.Update(hash, ruleIndex);
			hash = MurmurHash.Update(hash, predIndex);
			hash = MurmurHash.Update(hash, isCtxDependent ? 1 : 0);
			return MurmurHash.Finish(hash, 3);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Predicate))
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			Predicate predicate = (Predicate)obj;
			if (ruleIndex == predicate.ruleIndex && predIndex == predicate.predIndex)
			{
				return isCtxDependent == predicate.isCtxDependent;
			}
			return false;
		}

		public override string ToString()
		{
			return "{" + ruleIndex + ":" + predIndex + "}?";
		}
	}

	public class PrecedencePredicate : SemanticContext, IComparable<PrecedencePredicate>
	{
		public readonly int precedence;

		protected internal PrecedencePredicate()
		{
			precedence = 0;
		}

		public PrecedencePredicate(int precedence)
		{
			this.precedence = precedence;
		}

		public override bool Eval<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			return parser.Precpred(parserCallStack, precedence);
		}

		public override SemanticContext EvalPrecedence<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			if (parser.Precpred(parserCallStack, precedence))
			{
				return None;
			}
			return null;
		}

		public virtual int CompareTo(PrecedencePredicate o)
		{
			return precedence - o.precedence;
		}

		public override int GetHashCode()
		{
			int num = 1;
			return 31 * num + precedence;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PrecedencePredicate))
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			PrecedencePredicate precedencePredicate = (PrecedencePredicate)obj;
			return precedence == precedencePredicate.precedence;
		}

		public override string ToString()
		{
			return "{" + precedence + ">=prec}?";
		}
	}

	public abstract class Operator : SemanticContext
	{
		[NotNull]
		public abstract ICollection<SemanticContext> Operands { get; }
	}

	public class AND : Operator
	{
		[NotNull]
		public readonly SemanticContext[] opnds;

		public override ICollection<SemanticContext> Operands => Arrays.AsList(opnds);

		public AND(SemanticContext a, SemanticContext b)
		{
			HashSet<SemanticContext> hashSet = new HashSet<SemanticContext>();
			if (a is AND)
			{
				hashSet.UnionWith(((AND)a).opnds);
			}
			else
			{
				hashSet.Add(a);
			}
			if (b is AND)
			{
				hashSet.UnionWith(((AND)b).opnds);
			}
			else
			{
				hashSet.Add(b);
			}
			IList<PrecedencePredicate> list = FilterPrecedencePredicates(hashSet);
			if (list.Count > 0)
			{
				PrecedencePredicate item = list.Min();
				hashSet.Add(item);
			}
			opnds = hashSet.ToArray();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is AND))
			{
				return false;
			}
			AND aND = (AND)obj;
			return Arrays.Equals(opnds, aND.opnds);
		}

		public override int GetHashCode()
		{
			return MurmurHash.HashCode(opnds, typeof(AND).GetHashCode());
		}

		public override bool Eval<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			SemanticContext[] array = opnds;
			foreach (SemanticContext semanticContext in array)
			{
				if (!semanticContext.Eval(parser, parserCallStack))
				{
					return false;
				}
			}
			return true;
		}

		public override SemanticContext EvalPrecedence<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			bool flag = false;
			IList<SemanticContext> list = new List<SemanticContext>();
			SemanticContext[] array = opnds;
			foreach (SemanticContext semanticContext in array)
			{
				SemanticContext semanticContext2 = semanticContext.EvalPrecedence(parser, parserCallStack);
				flag = flag || semanticContext2 != semanticContext;
				if (semanticContext2 == null)
				{
					return null;
				}
				if (semanticContext2 != None)
				{
					list.Add(semanticContext2);
				}
			}
			if (!flag)
			{
				return this;
			}
			if (list.Count == 0)
			{
				return None;
			}
			SemanticContext semanticContext3 = list[0];
			for (int j = 1; j < list.Count; j++)
			{
				semanticContext3 = AndOp(semanticContext3, list[j]);
			}
			return semanticContext3;
		}

		public override string ToString()
		{
			return Utils.Join("&&", opnds);
		}
	}

	public class OR : Operator
	{
		[NotNull]
		public readonly SemanticContext[] opnds;

		public override ICollection<SemanticContext> Operands => Arrays.AsList(opnds);

		public OR(SemanticContext a, SemanticContext b)
		{
			HashSet<SemanticContext> hashSet = new HashSet<SemanticContext>();
			if (a is OR)
			{
				hashSet.UnionWith(((OR)a).opnds);
			}
			else
			{
				hashSet.Add(a);
			}
			if (b is OR)
			{
				hashSet.UnionWith(((OR)b).opnds);
			}
			else
			{
				hashSet.Add(b);
			}
			IList<PrecedencePredicate> list = FilterPrecedencePredicates(hashSet);
			if (list.Count > 0)
			{
				PrecedencePredicate item = list.Max();
				hashSet.Add(item);
			}
			opnds = hashSet.ToArray();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is OR))
			{
				return false;
			}
			OR oR = (OR)obj;
			return Arrays.Equals(opnds, oR.opnds);
		}

		public override int GetHashCode()
		{
			return MurmurHash.HashCode(opnds, typeof(OR).GetHashCode());
		}

		public override bool Eval<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			SemanticContext[] array = opnds;
			foreach (SemanticContext semanticContext in array)
			{
				if (semanticContext.Eval(parser, parserCallStack))
				{
					return true;
				}
			}
			return false;
		}

		public override SemanticContext EvalPrecedence<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack)
		{
			bool flag = false;
			IList<SemanticContext> list = new List<SemanticContext>();
			SemanticContext[] array = opnds;
			foreach (SemanticContext semanticContext in array)
			{
				SemanticContext semanticContext2 = semanticContext.EvalPrecedence(parser, parserCallStack);
				flag = flag || semanticContext2 != semanticContext;
				if (semanticContext2 == None)
				{
					return None;
				}
				if (semanticContext2 != null)
				{
					list.Add(semanticContext2);
				}
			}
			if (!flag)
			{
				return this;
			}
			if (list.Count == 0)
			{
				return null;
			}
			SemanticContext semanticContext3 = list[0];
			for (int j = 1; j < list.Count; j++)
			{
				semanticContext3 = OrOp(semanticContext3, list[j]);
			}
			return semanticContext3;
		}

		public override string ToString()
		{
			return Utils.Join("||", opnds);
		}
	}

	public static readonly SemanticContext None = new Predicate();

	public abstract bool Eval<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack) where ATNInterpreter : ATNSimulator;

	public virtual SemanticContext EvalPrecedence<Symbol, ATNInterpreter>(Recognizer<Symbol, ATNInterpreter> parser, RuleContext parserCallStack) where ATNInterpreter : ATNSimulator
	{
		return this;
	}

	public static SemanticContext AndOp(SemanticContext a, SemanticContext b)
	{
		if (a == null || a == None)
		{
			return b;
		}
		if (b == null || b == None)
		{
			return a;
		}
		AND aND = new AND(a, b);
		if (aND.opnds.Length == 1)
		{
			return aND.opnds[0];
		}
		return aND;
	}

	public static SemanticContext OrOp(SemanticContext a, SemanticContext b)
	{
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}
		if (a == None || b == None)
		{
			return None;
		}
		OR oR = new OR(a, b);
		if (oR.opnds.Length == 1)
		{
			return oR.opnds[0];
		}
		return oR;
	}

	private static IList<PrecedencePredicate> FilterPrecedencePredicates(HashSet<SemanticContext> collection)
	{
		if (!collection.OfType<PrecedencePredicate>().Any())
		{
			Collections.EmptyList<PrecedencePredicate>();
		}
		List<PrecedencePredicate> list = collection.OfType<PrecedencePredicate>().ToList();
		collection.ExceptWith(list);
		return list;
	}
}
