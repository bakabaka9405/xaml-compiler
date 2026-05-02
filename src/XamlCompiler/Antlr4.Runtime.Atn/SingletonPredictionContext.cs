using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

#pragma warning disable CS0659
public class SingletonPredictionContext : PredictionContext
{
	[NotNull]
	public readonly PredictionContext parent;

	public readonly int returnState;

	public override int Size => 1;

	public override bool IsEmpty => false;

	public override bool HasEmpty => false;

	internal SingletonPredictionContext(PredictionContext parent, int returnState)
		: base(PredictionContext.CalculateHashCode(parent, returnState))
	{
		this.parent = parent;
		this.returnState = returnState;
	}

	public override PredictionContext GetParent(int index)
	{
		return parent;
	}

	public override int GetReturnState(int index)
	{
		return returnState;
	}

	public override int FindReturnState(int returnState)
	{
		if (this.returnState != returnState)
		{
			return -1;
		}
		return 0;
	}

	public override PredictionContext AppendContext(PredictionContext suffix, PredictionContextCache contextCache)
	{
		return contextCache.GetChild(parent.AppendContext(suffix, contextCache), returnState);
	}

	protected internal override PredictionContext AddEmptyContext()
	{
		PredictionContext[] parents = new PredictionContext[2]
		{
			parent,
			PredictionContext.EmptyFull
		};
		int[] returnStates = new int[2] { returnState, 2147483647 };
		return new ArrayPredictionContext(parents, returnStates);
	}

	protected internal override PredictionContext RemoveEmptyContext()
	{
		return this;
	}

	public override bool Equals(object o)
	{
		if (o == this)
		{
			return true;
		}
		if (!(o is SingletonPredictionContext))
		{
			return false;
		}
		SingletonPredictionContext singletonPredictionContext = (SingletonPredictionContext)o;
		if (GetHashCode() != singletonPredictionContext.GetHashCode())
		{
			return false;
		}
		if (returnState == singletonPredictionContext.returnState)
		{
			return parent.Equals(singletonPredictionContext.parent);
		}
		return false;
	}
}
#pragma warning restore CS0659
