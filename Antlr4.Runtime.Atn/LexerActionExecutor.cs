using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class LexerActionExecutor
{
	[NotNull]
	private readonly ILexerAction[] lexerActions;

	private readonly int hashCode;

	[NotNull]
	public virtual ILexerAction[] LexerActions => lexerActions;

	public LexerActionExecutor(ILexerAction[] lexerActions)
	{
		this.lexerActions = lexerActions;
		int hash = MurmurHash.Initialize();
		foreach (ILexerAction value in lexerActions)
		{
			hash = MurmurHash.Update(hash, value);
		}
		hashCode = MurmurHash.Finish(hash, lexerActions.Length);
	}

	[return: NotNull]
	public static LexerActionExecutor Append(LexerActionExecutor lexerActionExecutor, ILexerAction lexerAction)
	{
		if (lexerActionExecutor == null)
		{
			return new LexerActionExecutor(new ILexerAction[1] { lexerAction });
		}
		ILexerAction[] array = Arrays.CopyOf(lexerActionExecutor.lexerActions, lexerActionExecutor.lexerActions.Length + 1);
		array[array.Length - 1] = lexerAction;
		return new LexerActionExecutor(array);
	}

	public virtual LexerActionExecutor FixOffsetBeforeMatch(int offset)
	{
		ILexerAction[] array = null;
		for (int i = 0; i < lexerActions.Length; i++)
		{
			if (lexerActions[i].IsPositionDependent && !(lexerActions[i] is LexerIndexedCustomAction))
			{
				if (array == null)
				{
					array = (ILexerAction[])lexerActions.Clone();
				}
				array[i] = new LexerIndexedCustomAction(offset, lexerActions[i]);
			}
		}
		if (array == null)
		{
			return this;
		}
		return new LexerActionExecutor(array);
	}

	public virtual void Execute(Lexer lexer, ICharStream input, int startIndex)
	{
		bool flag = false;
		int index = input.Index;
		try
		{
			ILexerAction[] array = lexerActions;
			foreach (ILexerAction lexerAction in array)
			{
				ILexerAction lexerAction2 = lexerAction;
				if (lexerAction2 is LexerIndexedCustomAction)
				{
					int offset = ((LexerIndexedCustomAction)lexerAction2).Offset;
					input.Seek(startIndex + offset);
					lexerAction2 = ((LexerIndexedCustomAction)lexerAction2).Action;
					flag = startIndex + offset != index;
				}
				else if (lexerAction2.IsPositionDependent)
				{
					input.Seek(index);
					flag = false;
				}
				lexerAction2.Execute(lexer);
			}
		}
		finally
		{
			if (flag)
			{
				input.Seek(index);
			}
		}
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is LexerActionExecutor))
		{
			return false;
		}
		LexerActionExecutor lexerActionExecutor = (LexerActionExecutor)obj;
		if (hashCode == lexerActionExecutor.hashCode)
		{
			return Arrays.Equals(lexerActions, lexerActionExecutor.lexerActions);
		}
		return false;
	}
}
