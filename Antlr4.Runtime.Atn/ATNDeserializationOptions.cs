using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class ATNDeserializationOptions
{
	private static readonly ATNDeserializationOptions defaultOptions;

	private bool readOnly;

	private bool verifyATN;

	private bool generateRuleBypassTransitions;

	private bool optimize;

	[NotNull]
	public static ATNDeserializationOptions Default => defaultOptions;

	public bool IsReadOnly => readOnly;

	public bool VerifyAtn
	{
		get
		{
			return verifyATN;
		}
		set
		{
			bool flag = value;
			ThrowIfReadOnly();
			verifyATN = flag;
		}
	}

	public bool GenerateRuleBypassTransitions
	{
		get
		{
			return generateRuleBypassTransitions;
		}
		set
		{
			bool flag = value;
			ThrowIfReadOnly();
			generateRuleBypassTransitions = flag;
		}
	}

	public bool Optimize
	{
		get
		{
			return optimize;
		}
		set
		{
			bool flag = value;
			ThrowIfReadOnly();
			optimize = flag;
		}
	}

	static ATNDeserializationOptions()
	{
		defaultOptions = new ATNDeserializationOptions();
		defaultOptions.MakeReadOnly();
	}

	public ATNDeserializationOptions()
	{
		verifyATN = true;
		generateRuleBypassTransitions = false;
		optimize = true;
	}

	public ATNDeserializationOptions(ATNDeserializationOptions options)
	{
		verifyATN = options.verifyATN;
		generateRuleBypassTransitions = options.generateRuleBypassTransitions;
		optimize = options.optimize;
	}

	public void MakeReadOnly()
	{
		readOnly = true;
	}

	protected internal virtual void ThrowIfReadOnly()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("The object is read only.");
		}
	}
}
