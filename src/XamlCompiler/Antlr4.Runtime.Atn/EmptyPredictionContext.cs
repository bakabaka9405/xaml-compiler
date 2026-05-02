using System;

namespace Antlr4.Runtime.Atn;

#pragma warning disable CS0659
public sealed class EmptyPredictionContext : PredictionContext
{
	public static readonly EmptyPredictionContext LocalContext = new EmptyPredictionContext(fullContext: false);

	public static readonly EmptyPredictionContext FullContext = new EmptyPredictionContext(fullContext: true);

	private readonly bool fullContext;

	public bool IsFullContext => fullContext;

	public override int Size => 0;

	public override bool IsEmpty => true;

	public override bool HasEmpty => true;

	private EmptyPredictionContext(bool fullContext)
		: base(PredictionContext.CalculateEmptyHashCode())
	{
		this.fullContext = fullContext;
	}

	protected internal override PredictionContext AddEmptyContext()
	{
		return this;
	}

	protected internal override PredictionContext RemoveEmptyContext()
	{
		throw new NotSupportedException("Cannot remove the empty context from itself.");
	}

	public override PredictionContext GetParent(int index)
	{
		throw new ArgumentOutOfRangeException();
	}

	public override int GetReturnState(int index)
	{
		throw new ArgumentOutOfRangeException();
	}

	public override int FindReturnState(int returnState)
	{
		return -1;
	}

	public override PredictionContext AppendContext(int returnContext, PredictionContextCache contextCache)
	{
		return contextCache.GetChild(this, returnContext);
	}

	public override PredictionContext AppendContext(PredictionContext suffix, PredictionContextCache contextCache)
	{
		return suffix;
	}

	public override bool Equals(object o)
	{
		return this == o;
	}

	public override string[] ToStrings(IRecognizer recognizer, int currentState)
	{
		return new string[1] { "[]" };
	}

	public override string[] ToStrings(IRecognizer recognizer, PredictionContext stop, int currentState)
	{
		return new string[1] { "[]" };
	}
}
#pragma warning restore CS0659
