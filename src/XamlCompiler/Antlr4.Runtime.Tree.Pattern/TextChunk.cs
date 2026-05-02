using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree.Pattern;

internal class TextChunk : Chunk
{
	[NotNull]
	private readonly string text;

	[NotNull]
	public string Text => text;

	public TextChunk(string text)
	{
		if (text == null)
		{
			throw new ArgumentException("text cannot be null");
		}
		this.text = text;
	}

	public override string ToString()
	{
		return "'" + text + "'";
	}
}
