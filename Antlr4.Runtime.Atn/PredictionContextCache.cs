using System.Collections.Generic;

namespace Antlr4.Runtime.Atn;

public class PredictionContextCache
{
	protected internal sealed class PredictionContextAndInt
	{
		private readonly PredictionContext obj;

		private readonly int value;

		public PredictionContextAndInt(PredictionContext obj, int value)
		{
			this.obj = obj;
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PredictionContextAndInt))
			{
				return false;
			}
			if (obj == this)
			{
				return true;
			}
			PredictionContextAndInt predictionContextAndInt = (PredictionContextAndInt)obj;
			if (value == predictionContextAndInt.value)
			{
				if (this.obj != predictionContextAndInt.obj)
				{
					if (this.obj != null)
					{
						return this.obj.Equals(predictionContextAndInt.obj);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = 5;
			num = 7 * num + ((obj != null) ? obj.GetHashCode() : 0);
			return 7 * num + value;
		}
	}

	protected internal sealed class IdentityCommutativePredictionContextOperands
	{
		private readonly PredictionContext x;

		private readonly PredictionContext y;

		public PredictionContext X => x;

		public PredictionContext Y => y;

		public IdentityCommutativePredictionContextOperands(PredictionContext x, PredictionContext y)
		{
			this.x = x;
			this.y = y;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IdentityCommutativePredictionContextOperands))
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			IdentityCommutativePredictionContextOperands identityCommutativePredictionContextOperands = (IdentityCommutativePredictionContextOperands)obj;
			if (x != identityCommutativePredictionContextOperands.x || y != identityCommutativePredictionContextOperands.y)
			{
				if (x == identityCommutativePredictionContextOperands.y)
				{
					return y == identityCommutativePredictionContextOperands.x;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}
	}

	public static readonly PredictionContextCache Uncached = new PredictionContextCache(enableCache: false);

	private readonly IDictionary<PredictionContext, PredictionContext> contexts = new Dictionary<PredictionContext, PredictionContext>();

	private readonly IDictionary<PredictionContextAndInt, PredictionContext> childContexts = new Dictionary<PredictionContextAndInt, PredictionContext>();

	private readonly IDictionary<IdentityCommutativePredictionContextOperands, PredictionContext> joinContexts = new Dictionary<IdentityCommutativePredictionContextOperands, PredictionContext>();

	private readonly bool enableCache;

	public PredictionContextCache()
		: this(enableCache: true)
	{
	}

	private PredictionContextCache(bool enableCache)
	{
		this.enableCache = enableCache;
	}

	public virtual PredictionContext GetAsCached(PredictionContext context)
	{
		if (!enableCache)
		{
			return context;
		}
		if (!contexts.TryGetValue(context, out var value))
		{
			value = context;
			contexts[context] = context;
		}
		return value;
	}

	public virtual PredictionContext GetChild(PredictionContext context, int invokingState)
	{
		if (!enableCache)
		{
			return context.GetChild(invokingState);
		}
		PredictionContextAndInt key = new PredictionContextAndInt(context, invokingState);
		if (!childContexts.TryGetValue(key, out var value))
		{
			value = context.GetChild(invokingState);
			value = GetAsCached(value);
			childContexts[key] = value;
		}
		return value;
	}

	public virtual PredictionContext Join(PredictionContext x, PredictionContext y)
	{
		if (!enableCache)
		{
			return PredictionContext.Join(x, y, this);
		}
		IdentityCommutativePredictionContextOperands key = new IdentityCommutativePredictionContextOperands(x, y);
		if (joinContexts.TryGetValue(key, out var value))
		{
			return value;
		}
		value = PredictionContext.Join(x, y, this);
		value = GetAsCached(value);
		joinContexts[key] = value;
		return value;
	}
}
