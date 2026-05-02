namespace Antlr4.Runtime;

public interface IWritableToken : IToken
{
	new string Text { set; }

	new int Type { set; }

	new int Line { set; }

	new int Column { set; }

	new int Channel { set; }

	new int TokenIndex { set; }
}
