using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

#pragma warning disable CS0659
public class ArrayPredictionContext : PredictionContext
{
	[NotNull]
	public readonly PredictionContext[] parents;

	[NotNull]
	public readonly int[] returnStates;

	public override int Size => returnStates.Length;

	public override bool IsEmpty => false;

	public override bool HasEmpty => returnStates[returnStates.Length - 1] == int.MaxValue;

	internal ArrayPredictionContext(PredictionContext[] parents, int[] returnStates)
		: base(PredictionContext.CalculateHashCode(parents, returnStates))
	{
		this.parents = parents;
		this.returnStates = returnStates;
	}

	internal ArrayPredictionContext(PredictionContext[] parents, int[] returnStates, int hashCode)
		: base(hashCode)
	{
		this.parents = parents;
		this.returnStates = returnStates;
	}

	public override PredictionContext GetParent(int index)
	{
		return parents[index];
	}

	public override int GetReturnState(int index)
	{
		return returnStates[index];
	}

	public override int FindReturnState(int returnState)
	{
		return Array.BinarySearch(returnStates, returnState);
	}

	protected internal override PredictionContext AddEmptyContext()
	{
		if (HasEmpty)
		{
			return this;
		}
		PredictionContext[] array = Arrays.CopyOf(parents, parents.Length + 1);
		int[] array2 = Arrays.CopyOf(returnStates, returnStates.Length + 1);
		array[array.Length - 1] = PredictionContext.EmptyFull;
		array2[array2.Length - 1] = int.MaxValue;
		return new ArrayPredictionContext(array, array2);
	}

	protected internal override PredictionContext RemoveEmptyContext()
	{
		if (!HasEmpty)
		{
			return this;
		}
		if (returnStates.Length == 2)
		{
			return new SingletonPredictionContext(parents[0], returnStates[0]);
		}
		PredictionContext[] array = Arrays.CopyOf(parents, parents.Length - 1);
		int[] array2 = Arrays.CopyOf(returnStates, returnStates.Length - 1);
		return new ArrayPredictionContext(array, array2);
	}

	public override PredictionContext AppendContext(PredictionContext suffix, PredictionContextCache contextCache)
	{
		return AppendContext(this, suffix, new IdentityHashMap());
	}

	private static PredictionContext AppendContext(PredictionContext context, PredictionContext suffix, IdentityHashMap visited)
	{
		if (suffix.IsEmpty)
		{
			if (PredictionContext.IsEmptyLocal(suffix))
			{
				if (context.HasEmpty)
				{
					return PredictionContext.EmptyLocal;
				}
				throw new NotSupportedException("what to do here?");
			}
			return context;
		}
		if (suffix.Size != 1)
		{
			throw new NotSupportedException("Appending a tree suffix is not yet supported.");
		}
		if (!visited.TryGetValue(context, out var value))
		{
			if (context.IsEmpty)
			{
				value = suffix;
			}
			else
			{
				int num = context.Size;
				if (context.HasEmpty)
				{
					num--;
				}
				PredictionContext[] array = new PredictionContext[num];
				int[] array2 = new int[num];
				for (int i = 0; i < num; i++)
				{
					array2[i] = context.GetReturnState(i);
				}
				for (int j = 0; j < num; j++)
				{
					array[j] = AppendContext(context.GetParent(j), suffix, visited);
				}
				value = ((array.Length != 1) ? ((PredictionContext)new ArrayPredictionContext(array, array2)) : ((PredictionContext)new SingletonPredictionContext(array[0], array2[0])));
				if (context.HasEmpty)
				{
					value = PredictionContext.Join(value, suffix);
				}
			}
			visited[context] = value;
		}
		return value;
	}

	public override bool Equals(object o)
	{
		if (this == o)
		{
			return true;
		}
		if (!(o is ArrayPredictionContext))
		{
			return false;
		}
		if (GetHashCode() != o.GetHashCode())
		{
			return false;
		}
		ArrayPredictionContext other = (ArrayPredictionContext)o;
		return Equals(other, new HashSet<PredictionContextCache.IdentityCommutativePredictionContextOperands>());
	}

	private bool Equals(ArrayPredictionContext other, HashSet<PredictionContextCache.IdentityCommutativePredictionContextOperands> visited)
	{
		Stack<PredictionContext> stack = new Stack<PredictionContext>();
		Stack<PredictionContext> stack2 = new Stack<PredictionContext>();
		stack.Push(this);
		stack2.Push(other);
		while (stack.Count > 0)
		{
			PredictionContextCache.IdentityCommutativePredictionContextOperands identityCommutativePredictionContextOperands = new PredictionContextCache.IdentityCommutativePredictionContextOperands(stack.Pop(), stack2.Pop());
			if (!visited.Add(identityCommutativePredictionContextOperands))
			{
				continue;
			}
			int size = identityCommutativePredictionContextOperands.X.Size;
			if (size == 0)
			{
				if (!identityCommutativePredictionContextOperands.X.Equals(identityCommutativePredictionContextOperands.Y))
				{
					return false;
				}
				continue;
			}
			int size2 = identityCommutativePredictionContextOperands.Y.Size;
			if (size != size2)
			{
				return false;
			}
			for (int i = 0; i < size; i++)
			{
				if (identityCommutativePredictionContextOperands.X.GetReturnState(i) != identityCommutativePredictionContextOperands.Y.GetReturnState(i))
				{
					return false;
				}
				PredictionContext parent = identityCommutativePredictionContextOperands.X.GetParent(i);
				PredictionContext parent2 = identityCommutativePredictionContextOperands.Y.GetParent(i);
				if (parent.GetHashCode() != parent2.GetHashCode())
				{
					return false;
				}
				if (parent != parent2)
				{
					stack.Push(parent);
					stack2.Push(parent2);
				}
			}
		}
		return true;
	}
}
#pragma warning restore CS0659
