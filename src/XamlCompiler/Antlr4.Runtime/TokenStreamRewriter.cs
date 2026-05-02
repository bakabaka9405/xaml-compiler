using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class TokenStreamRewriter
{
	public class RewriteOperation
	{
		protected internal readonly ITokenStream tokens;

		protected internal int instructionIndex;

		protected internal int index;

		protected internal object text;

		protected internal RewriteOperation(ITokenStream tokens, int index)
		{
			this.tokens = tokens;
			this.index = index;
		}

		protected internal RewriteOperation(ITokenStream tokens, int index, object text)
		{
			this.tokens = tokens;
			this.index = index;
			this.text = text;
		}

		public virtual int Execute(StringBuilder buf)
		{
			return index;
		}

		public override string ToString()
		{
			string fullName = GetType().FullName;
			int num = fullName.IndexOf('$');
			fullName = Antlr4.Runtime.Sharpen.Runtime.Substring(fullName, num + 1, fullName.Length);
			return "<" + fullName + "@" + tokens.Get(index)?.ToString() + ":\"" + text?.ToString() + "\">";
		}
	}

	internal class InsertBeforeOp : RewriteOperation
	{
		public InsertBeforeOp(ITokenStream tokens, int index, object text)
			: base(tokens, index, text)
		{
		}

		public override int Execute(StringBuilder buf)
		{
			buf.Append(text);
			if (tokens.Get(index).Type != -1)
			{
				buf.Append(tokens.Get(index).Text);
			}
			return index + 1;
		}
	}

	internal class ReplaceOp : RewriteOperation
	{
		protected internal int lastIndex;

		public ReplaceOp(ITokenStream tokens, int from, int to, object text)
			: base(tokens, from, text)
		{
			lastIndex = to;
		}

		public override int Execute(StringBuilder buf)
		{
			if (text != null)
			{
				buf.Append(text);
			}
			return lastIndex + 1;
		}

		public override string ToString()
		{
			if (text == null)
			{
				return "<DeleteOp@" + tokens.Get(index)?.ToString() + ".." + tokens.Get(lastIndex)?.ToString() + ">";
			}
			return "<ReplaceOp@" + tokens.Get(index)?.ToString() + ".." + tokens.Get(lastIndex)?.ToString() + ":\"" + text?.ToString() + "\">";
		}
	}

	public const string DefaultProgramName = "default";

	public const int ProgramInitSize = 100;

	public const int MinTokenIndex = 0;

	protected internal readonly ITokenStream tokens;

	protected internal readonly IDictionary<string, IList<RewriteOperation>> programs;

	protected internal readonly IDictionary<string, int> lastRewriteTokenIndexes;

	public ITokenStream TokenStream => tokens;

	public virtual int LastRewriteTokenIndex => GetLastRewriteTokenIndex("default");

	public TokenStreamRewriter(ITokenStream tokens)
	{
		this.tokens = tokens;
		programs = new Dictionary<string, IList<RewriteOperation>>();
		programs["default"] = new List<RewriteOperation>(100);
		lastRewriteTokenIndexes = new Dictionary<string, int>();
	}

	public virtual void Rollback(int instructionIndex)
	{
		Rollback("default", instructionIndex);
	}

	public virtual void Rollback(string programName, int instructionIndex)
	{
		if (programs.TryGetValue(programName, out var value))
		{
			programs[programName] = new List<RewriteOperation>(value.Skip(0).Take(instructionIndex));
		}
	}

	public virtual void DeleteProgram()
	{
		DeleteProgram("default");
	}

	public virtual void DeleteProgram(string programName)
	{
		Rollback(programName, 0);
	}

	public virtual void InsertAfter(IToken t, object text)
	{
		InsertAfter("default", t, text);
	}

	public virtual void InsertAfter(int index, object text)
	{
		InsertAfter("default", index, text);
	}

	public virtual void InsertAfter(string programName, IToken t, object text)
	{
		InsertAfter(programName, t.TokenIndex, text);
	}

	public virtual void InsertAfter(string programName, int index, object text)
	{
		InsertBefore(programName, index + 1, text);
	}

	public virtual void InsertBefore(IToken t, object text)
	{
		InsertBefore("default", t, text);
	}

	public virtual void InsertBefore(int index, object text)
	{
		InsertBefore("default", index, text);
	}

	public virtual void InsertBefore(string programName, IToken t, object text)
	{
		InsertBefore(programName, t.TokenIndex, text);
	}

	public virtual void InsertBefore(string programName, int index, object text)
	{
		RewriteOperation rewriteOperation = new InsertBeforeOp(tokens, index, text);
		IList<RewriteOperation> program = GetProgram(programName);
		rewriteOperation.instructionIndex = program.Count;
		program.Add(rewriteOperation);
	}

	public virtual void Replace(int index, object text)
	{
		Replace("default", index, index, text);
	}

	public virtual void Replace(int from, int to, object text)
	{
		Replace("default", from, to, text);
	}

	public virtual void Replace(IToken indexT, object text)
	{
		Replace("default", indexT, indexT, text);
	}

	public virtual void Replace(IToken from, IToken to, object text)
	{
		Replace("default", from, to, text);
	}

	public virtual void Replace(string programName, int from, int to, object text)
	{
		if (from > to || from < 0 || to < 0 || to >= tokens.Size)
		{
			throw new ArgumentException("replace: range invalid: " + from + ".." + to + "(size=" + tokens.Size + ")");
		}
		RewriteOperation rewriteOperation = new ReplaceOp(tokens, from, to, text);
		IList<RewriteOperation> program = GetProgram(programName);
		rewriteOperation.instructionIndex = program.Count;
		program.Add(rewriteOperation);
	}

	public virtual void Replace(string programName, IToken from, IToken to, object text)
	{
		Replace(programName, from.TokenIndex, to.TokenIndex, text);
	}

	public virtual void Delete(int index)
	{
		Delete("default", index, index);
	}

	public virtual void Delete(int from, int to)
	{
		Delete("default", from, to);
	}

	public virtual void Delete(IToken indexT)
	{
		Delete("default", indexT, indexT);
	}

	public virtual void Delete(IToken from, IToken to)
	{
		Delete("default", from, to);
	}

	public virtual void Delete(string programName, int from, int to)
	{
		Replace(programName, from, to, null);
	}

	public virtual void Delete(string programName, IToken from, IToken to)
	{
		Replace(programName, from, to, null);
	}

	protected internal virtual int GetLastRewriteTokenIndex(string programName)
	{
		if (!lastRewriteTokenIndexes.TryGetValue(programName, out var value))
		{
			return -1;
		}
		return value;
	}

	protected internal virtual void SetLastRewriteTokenIndex(string programName, int i)
	{
		lastRewriteTokenIndexes[programName] = i;
	}

	protected internal virtual IList<RewriteOperation> GetProgram(string name)
	{
		if (!programs.TryGetValue(name, out var value))
		{
			return InitializeProgram(name);
		}
		return value;
	}

	private IList<RewriteOperation> InitializeProgram(string name)
	{
		IList<RewriteOperation> list = new List<RewriteOperation>(100);
		programs[name] = list;
		return list;
	}

	public virtual string GetText()
	{
		return GetText("default", Interval.Of(0, tokens.Size - 1));
	}

	public virtual string GetText(Interval interval)
	{
		return GetText("default", interval);
	}

	public virtual string GetText(string programName, Interval interval)
	{
		if (!programs.TryGetValue(programName, out var value))
		{
			value = null;
		}
		int num = interval.a;
		int num2 = interval.b;
		if (num2 > tokens.Size - 1)
		{
			num2 = tokens.Size - 1;
		}
		if (num < 0)
		{
			num = 0;
		}
		if (value == null || value.Count == 0)
		{
			return tokens.GetText(interval);
		}
		StringBuilder stringBuilder = new StringBuilder();
		IDictionary<int, RewriteOperation> dictionary = ReduceToSingleOperationPerIndex(value);
		int num3 = num;
		while (num3 <= num2 && num3 < tokens.Size)
		{
			if (dictionary.TryGetValue(num3, out var value2))
			{
				dictionary.Remove(num3);
			}
			IToken token = tokens.Get(num3);
			if (value2 == null)
			{
				if (token.Type != -1)
				{
					stringBuilder.Append(token.Text);
				}
				num3++;
			}
			else
			{
				num3 = value2.Execute(stringBuilder);
			}
		}
		if (num2 == tokens.Size - 1)
		{
			foreach (RewriteOperation value3 in dictionary.Values)
			{
				if (value3.index >= tokens.Size - 1)
				{
					stringBuilder.Append(value3.text);
				}
			}
		}
		return stringBuilder.ToString();
	}

	protected internal virtual IDictionary<int, RewriteOperation> ReduceToSingleOperationPerIndex(IList<RewriteOperation> rewrites)
	{
		for (int i = 0; i < rewrites.Count; i++)
		{
			RewriteOperation rewriteOperation = rewrites[i];
			if (rewriteOperation == null || !(rewriteOperation is ReplaceOp))
			{
				continue;
			}
			ReplaceOp replaceOp = (ReplaceOp)rewrites[i];
			IList<InsertBeforeOp> kindOfOps = GetKindOfOps<InsertBeforeOp>(rewrites, i);
			foreach (InsertBeforeOp item in kindOfOps)
			{
				if (item.index == replaceOp.index)
				{
					rewrites[item.instructionIndex] = null;
					replaceOp.text = item.text.ToString() + ((replaceOp.text != null) ? replaceOp.text.ToString() : string.Empty);
				}
				else if (item.index > replaceOp.index && item.index <= replaceOp.lastIndex)
				{
					rewrites[item.instructionIndex] = null;
				}
			}
			IList<ReplaceOp> kindOfOps2 = GetKindOfOps<ReplaceOp>(rewrites, i);
			foreach (ReplaceOp item2 in kindOfOps2)
			{
				if (item2.index >= replaceOp.index && item2.lastIndex <= replaceOp.lastIndex)
				{
					rewrites[item2.instructionIndex] = null;
					continue;
				}
				bool flag = item2.lastIndex < replaceOp.index || item2.index > replaceOp.lastIndex;
				bool flag2 = item2.index == replaceOp.index && item2.lastIndex == replaceOp.lastIndex;
				if (item2.text == null && replaceOp.text == null && !flag)
				{
					rewrites[item2.instructionIndex] = null;
					replaceOp.index = Math.Min(item2.index, replaceOp.index);
					replaceOp.lastIndex = Math.Max(item2.lastIndex, replaceOp.lastIndex);
					Console.Out.WriteLine("new rop " + replaceOp);
				}
				else if (!flag && !flag2)
				{
					throw new ArgumentException("replace op boundaries of " + replaceOp?.ToString() + " overlap with previous " + item2);
				}
			}
		}
		for (int j = 0; j < rewrites.Count; j++)
		{
			RewriteOperation rewriteOperation2 = rewrites[j];
			if (rewriteOperation2 == null || !(rewriteOperation2 is InsertBeforeOp))
			{
				continue;
			}
			InsertBeforeOp insertBeforeOp = (InsertBeforeOp)rewrites[j];
			IList<InsertBeforeOp> kindOfOps3 = GetKindOfOps<InsertBeforeOp>(rewrites, j);
			foreach (InsertBeforeOp item3 in kindOfOps3)
			{
				if (item3.index == insertBeforeOp.index)
				{
					insertBeforeOp.text = CatOpText(insertBeforeOp.text, item3.text);
					rewrites[item3.instructionIndex] = null;
				}
			}
			IList<ReplaceOp> kindOfOps4 = GetKindOfOps<ReplaceOp>(rewrites, j);
			foreach (ReplaceOp item4 in kindOfOps4)
			{
				if (insertBeforeOp.index == item4.index)
				{
					item4.text = CatOpText(insertBeforeOp.text, item4.text);
					rewrites[j] = null;
				}
				else if (insertBeforeOp.index >= item4.index && insertBeforeOp.index <= item4.lastIndex)
				{
					throw new ArgumentException("insert op " + insertBeforeOp?.ToString() + " within boundaries of previous " + item4);
				}
			}
		}
		IDictionary<int, RewriteOperation> dictionary = new Dictionary<int, RewriteOperation>();
		for (int k = 0; k < rewrites.Count; k++)
		{
			RewriteOperation rewriteOperation3 = rewrites[k];
			if (rewriteOperation3 != null)
			{
				if (dictionary.ContainsKey(rewriteOperation3.index))
				{
					throw new InvalidOperationException("should only be one op per index");
				}
				dictionary[rewriteOperation3.index] = rewriteOperation3;
			}
		}
		return dictionary;
	}

	protected internal virtual string CatOpText(object a, object b)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		if (a != null)
		{
			text = a.ToString();
		}
		if (b != null)
		{
			text2 = b.ToString();
		}
		return text + text2;
	}

	protected internal virtual IList<T> GetKindOfOps<T>(IList<RewriteOperation> rewrites, int before)
	{
		return rewrites.Take(before).OfType<T>().ToList();
	}
}
