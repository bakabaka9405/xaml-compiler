using System;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal abstract class XamlDomNode : IXamlDomNode
{
	private bool isSealed;

	private int startLineNumber;

	private int startLinePosition;

	private int endLineNumber;

	private int endLinePosition;

	public string SourceFilePath { get; }

	public int StartLineNumber
	{
		get
		{
			return startLineNumber;
		}
		set
		{
			CheckSealed();
			startLineNumber = value;
		}
	}

	public int StartLinePosition
	{
		get
		{
			return startLinePosition;
		}
		set
		{
			CheckSealed();
			startLinePosition = value;
		}
	}

	public int EndLineNumber
	{
		get
		{
			return endLineNumber;
		}
		set
		{
			CheckSealed();
			endLineNumber = value;
		}
	}

	public int EndLinePosition
	{
		get
		{
			return endLinePosition;
		}
		set
		{
			CheckSealed();
			endLinePosition = value;
		}
	}

	public bool IsSealed => isSealed;

	public XamlDomNode(string sourceFilePath)
	{
		SourceFilePath = sourceFilePath;
	}

	public virtual void Seal()
	{
		isSealed = true;
	}

	protected void CheckSealed()
	{
		if (IsSealed)
		{
			throw new InvalidOperationException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_SealedXamlDomNode));
		}
	}
}
