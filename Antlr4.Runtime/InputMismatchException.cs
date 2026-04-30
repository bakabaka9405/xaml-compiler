using System;

namespace Antlr4.Runtime;

[Serializable]
public class InputMismatchException : RecognitionException
{
	private const long serialVersionUID = 1532568338707443067L;

	public InputMismatchException(Parser recognizer)
		: base(recognizer, (ITokenStream)recognizer.InputStream, recognizer.RuleContext)
	{
		base.OffendingToken = recognizer.CurrentToken;
	}
}
