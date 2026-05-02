using Antlr4.Runtime.Atn;

namespace Antlr4.Runtime.Dfa;

public class AcceptStateInfo
{
	private readonly int prediction;

	private readonly LexerActionExecutor lexerActionExecutor;

	public virtual int Prediction => prediction;

	public virtual LexerActionExecutor LexerActionExecutor => lexerActionExecutor;

	public AcceptStateInfo(int prediction)
	{
		this.prediction = prediction;
		lexerActionExecutor = null;
	}

	public AcceptStateInfo(int prediction, LexerActionExecutor lexerActionExecutor)
	{
		this.prediction = prediction;
		this.lexerActionExecutor = lexerActionExecutor;
	}
}
