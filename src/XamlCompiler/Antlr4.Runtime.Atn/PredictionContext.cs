using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public abstract class PredictionContext
{
	public sealed class IdentityHashMap : Dictionary<PredictionContext, PredictionContext>
	{
		public IdentityHashMap()
			: base((IEqualityComparer<PredictionContext>)IdentityEqualityComparator.Instance)
		{
		}
	}

	public sealed class IdentityEqualityComparator : EqualityComparer<PredictionContext>
	{
		public static readonly IdentityEqualityComparator Instance = new IdentityEqualityComparator();

		private IdentityEqualityComparator()
		{
		}

		public override int GetHashCode(PredictionContext obj)
		{
			return obj.GetHashCode();
		}

		public override bool Equals(PredictionContext a, PredictionContext b)
		{
			return a == b;
		}
	}

	[NotNull]
	public static readonly PredictionContext EmptyLocal = EmptyPredictionContext.LocalContext;

	[NotNull]
	public static readonly PredictionContext EmptyFull = EmptyPredictionContext.FullContext;

	public const int EmptyLocalStateKey = int.MinValue;

	public const int EmptyFullStateKey = int.MaxValue;

	private const int InitialHash = 1;

	private readonly int cachedHashCode;

	public abstract int Size { get; }

	public abstract bool IsEmpty { get; }

	public abstract bool HasEmpty { get; }

	protected internal PredictionContext(int cachedHashCode)
	{
		this.cachedHashCode = cachedHashCode;
	}

	protected internal static int CalculateEmptyHashCode()
	{
		int hash = MurmurHash.Initialize(1);
		return MurmurHash.Finish(hash, 0);
	}

	protected internal static int CalculateHashCode(PredictionContext parent, int returnState)
	{
		int hash = MurmurHash.Initialize(1);
		hash = MurmurHash.Update(hash, parent);
		hash = MurmurHash.Update(hash, returnState);
		return MurmurHash.Finish(hash, 2);
	}

	protected internal static int CalculateHashCode(PredictionContext[] parents, int[] returnStates)
	{
		int hash = MurmurHash.Initialize(1);
		foreach (PredictionContext value in parents)
		{
			hash = MurmurHash.Update(hash, value);
		}
		foreach (int value2 in returnStates)
		{
			hash = MurmurHash.Update(hash, value2);
		}
		return MurmurHash.Finish(hash, 2 * parents.Length);
	}

	public abstract int GetReturnState(int index);

	public abstract int FindReturnState(int returnState);

	[return: NotNull]
	public abstract PredictionContext GetParent(int index);

	protected internal abstract PredictionContext AddEmptyContext();

	protected internal abstract PredictionContext RemoveEmptyContext();

	public static PredictionContext FromRuleContext(ATN atn, RuleContext outerContext)
	{
		return FromRuleContext(atn, outerContext, fullContext: true);
	}

	public static PredictionContext FromRuleContext(ATN atn, RuleContext outerContext, bool fullContext)
	{
		if (outerContext.IsEmpty)
		{
			if (!fullContext)
			{
				return EmptyLocal;
			}
			return EmptyFull;
		}
		PredictionContext predictionContext = ((outerContext.Parent == null) ? (fullContext ? EmptyFull : EmptyLocal) : FromRuleContext(atn, outerContext.Parent, fullContext));
		ATNState aTNState = atn.states[outerContext.invokingState];
		RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
		return predictionContext.GetChild(ruleTransition.followState.stateNumber);
	}

	private static PredictionContext AddEmptyContext(PredictionContext context)
	{
		return context.AddEmptyContext();
	}

	private static PredictionContext RemoveEmptyContext(PredictionContext context)
	{
		return context.RemoveEmptyContext();
	}

	public static PredictionContext Join(PredictionContext context0, PredictionContext context1)
	{
		return Join(context0, context1, PredictionContextCache.Uncached);
	}

	internal static PredictionContext Join(PredictionContext context0, PredictionContext context1, PredictionContextCache contextCache)
	{
		if (context0 == context1)
		{
			return context0;
		}
		if (context0.IsEmpty)
		{
			if (!IsEmptyLocal(context0))
			{
				return AddEmptyContext(context1);
			}
			return context0;
		}
		if (context1.IsEmpty)
		{
			if (!IsEmptyLocal(context1))
			{
				return AddEmptyContext(context0);
			}
			return context1;
		}
		int size = context0.Size;
		int size2 = context1.Size;
		if (size == 1 && size2 == 1 && context0.GetReturnState(0) == context1.GetReturnState(0))
		{
			PredictionContext predictionContext = contextCache.Join(context0.GetParent(0), context1.GetParent(0));
			if (predictionContext == context0.GetParent(0))
			{
				return context0;
			}
			if (predictionContext == context1.GetParent(0))
			{
				return context1;
			}
			return predictionContext.GetChild(context0.GetReturnState(0));
		}
		int num = 0;
		PredictionContext[] array = new PredictionContext[size + size2];
		int[] array2 = new int[array.Length];
		int num2 = 0;
		int num3 = 0;
		bool flag = true;
		bool flag2 = true;
		while (num2 < size && num3 < size2)
		{
			if (context0.GetReturnState(num2) == context1.GetReturnState(num3))
			{
				array[num] = contextCache.Join(context0.GetParent(num2), context1.GetParent(num3));
				array2[num] = context0.GetReturnState(num2);
				flag = flag && array[num] == context0.GetParent(num2);
				flag2 = flag2 && array[num] == context1.GetParent(num3);
				num2++;
				num3++;
			}
			else if (context0.GetReturnState(num2) < context1.GetReturnState(num3))
			{
				array[num] = context0.GetParent(num2);
				array2[num] = context0.GetReturnState(num2);
				flag2 = false;
				num2++;
			}
			else
			{
				array[num] = context1.GetParent(num3);
				array2[num] = context1.GetReturnState(num3);
				flag = false;
				num3++;
			}
			num++;
		}
		while (num2 < size)
		{
			array[num] = context0.GetParent(num2);
			array2[num] = context0.GetReturnState(num2);
			num2++;
			flag2 = false;
			num++;
		}
		while (num3 < size2)
		{
			array[num] = context1.GetParent(num3);
			array2[num] = context1.GetReturnState(num3);
			num3++;
			flag = false;
			num++;
		}
		if (flag)
		{
			return context0;
		}
		if (flag2)
		{
			return context1;
		}
		if (num < array.Length)
		{
			array = Arrays.CopyOf(array, num);
			array2 = Arrays.CopyOf(array2, num);
		}
		if (array.Length == 0)
		{
			return EmptyFull;
		}
		if (array.Length == 1)
		{
			return new SingletonPredictionContext(array[0], array2[0]);
		}
		return new ArrayPredictionContext(array, array2);
	}

	public static bool IsEmptyLocal(PredictionContext context)
	{
		return context == EmptyLocal;
	}

	public static PredictionContext GetCachedContext(PredictionContext context, ConcurrentDictionary<PredictionContext, PredictionContext> contextCache, IdentityHashMap visited)
	{
		if (context.IsEmpty)
		{
			return context;
		}
		if (visited.TryGetValue(context, out var value))
		{
			return value;
		}
		if (contextCache.TryGetValue(context, out value))
		{
			visited[context] = value;
			return value;
		}
		bool flag = false;
		PredictionContext[] array = new PredictionContext[context.Size];
		for (int i = 0; i < array.Length; i++)
		{
			PredictionContext cachedContext = GetCachedContext(context.GetParent(i), contextCache, visited);
			if (!flag && cachedContext == context.GetParent(i))
			{
				continue;
			}
			if (!flag)
			{
				array = new PredictionContext[context.Size];
				for (int j = 0; j < context.Size; j++)
				{
					array[j] = context.GetParent(j);
				}
				flag = true;
			}
			array[i] = cachedContext;
		}
		if (!flag)
		{
			value = contextCache.GetOrAdd(context, context);
			visited[context] = value;
			return context;
		}
		PredictionContext predictionContext;
		if (array.Length == 1)
		{
			predictionContext = new SingletonPredictionContext(array[0], context.GetReturnState(0));
		}
		else
		{
			ArrayPredictionContext arrayPredictionContext = (ArrayPredictionContext)context;
			predictionContext = new ArrayPredictionContext(array, arrayPredictionContext.returnStates, context.cachedHashCode);
		}
		value = (visited[predictionContext] = contextCache.GetOrAdd(predictionContext, predictionContext));
		visited[context] = value;
		return predictionContext;
	}

	public virtual PredictionContext AppendContext(int returnContext, PredictionContextCache contextCache)
	{
		return AppendContext(EmptyFull.GetChild(returnContext), contextCache);
	}

	public abstract PredictionContext AppendContext(PredictionContext suffix, PredictionContextCache contextCache);

	public virtual PredictionContext GetChild(int returnState)
	{
		return new SingletonPredictionContext(this, returnState);
	}

	public sealed override int GetHashCode()
	{
		return cachedHashCode;
	}

	public abstract override bool Equals(object o);

	public virtual string[] ToStrings(IRecognizer recognizer, int currentState)
	{
		return ToStrings(recognizer, EmptyFull, currentState);
	}

	public virtual string[] ToStrings(IRecognizer recognizer, PredictionContext stop, int currentState)
	{
		List<string> list = new List<string>();
		int num = 0;
		while (true)
		{
			int num2 = 0;
			bool flag = true;
			PredictionContext predictionContext = this;
			int index = currentState;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			while (true)
			{
				if (!predictionContext.IsEmpty && predictionContext != stop)
				{
					int num3 = 0;
					if (predictionContext.Size > 0)
					{
						int i;
						for (i = 1; 1 << i < predictionContext.Size; i++)
						{
						}
						int num4 = (1 << i) - 1;
						num3 = (num >> num2) & num4;
						flag &= num3 >= predictionContext.Size - 1;
						if (num3 >= predictionContext.Size)
						{
							break;
						}
						num2 += i;
					}
					if (recognizer != null)
					{
						if (stringBuilder.Length > 1)
						{
							stringBuilder.Append(' ');
						}
						ATN atn = recognizer.Atn;
						ATNState aTNState = atn.states[index];
						string value = recognizer.RuleNames[aTNState.ruleIndex];
						stringBuilder.Append(value);
					}
					else if (predictionContext.GetReturnState(num3) != int.MaxValue && !predictionContext.IsEmpty)
					{
						if (stringBuilder.Length > 1)
						{
							stringBuilder.Append(' ');
						}
						stringBuilder.Append(predictionContext.GetReturnState(num3));
					}
					index = predictionContext.GetReturnState(num3);
					predictionContext = predictionContext.GetParent(num3);
					continue;
				}
				stringBuilder.Append("]");
				list.Add(stringBuilder.ToString());
				if (!flag)
				{
					break;
				}
				return list.ToArray();
			}
			num++;
		}
	}
}
