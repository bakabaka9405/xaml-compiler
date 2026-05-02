using System;
using System.Collections.Generic;

namespace Antlr4.Runtime;

public class ProxyErrorListener<Symbol> : IAntlrErrorListener<Symbol>
{
	private readonly IEnumerable<IAntlrErrorListener<Symbol>> delegates;

	protected internal virtual IEnumerable<IAntlrErrorListener<Symbol>> Delegates => delegates;

	public ProxyErrorListener(IEnumerable<IAntlrErrorListener<Symbol>> delegates)
	{
		if (delegates == null)
		{
			throw new ArgumentNullException("delegates");
		}
		this.delegates = delegates;
	}

	public virtual void SyntaxError(IRecognizer recognizer, Symbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		foreach (IAntlrErrorListener<Symbol> @delegate in delegates)
		{
			@delegate.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
		}
	}
}
