using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree.Pattern;

internal class TagChunk : Chunk
{
	private readonly string tag;

	private readonly string label;

	[NotNull]
	public string Tag => tag;

	[Nullable]
	public string Label => label;

	public TagChunk(string tag)
		: this(null, tag)
	{
	}

	public TagChunk(string label, string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			throw new ArgumentException("tag cannot be null or empty");
		}
		this.label = label;
		this.tag = tag;
	}

	public override string ToString()
	{
		if (label != null)
		{
			return label + ":" + tag;
		}
		return tag;
	}
}
