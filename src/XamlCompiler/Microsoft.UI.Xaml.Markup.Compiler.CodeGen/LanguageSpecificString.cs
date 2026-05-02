namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

public class LanguageSpecificString : ICodeGenOutput
{
	public static LanguageSpecificString Null = new LanguageSpecificString(() => "nullptr", () => "nullptr", () => "null", () => "Nothing");

	public CodeGenDelegate CppCXName { get; }

	public CodeGenDelegate CppWinRTName { get; }

	public CodeGenDelegate CSharpName { get; }

	public CodeGenDelegate VBName { get; }

	public LanguageSpecificString(CodeGenDelegate cppCX, CodeGenDelegate cppWinRT, CodeGenDelegate cs, CodeGenDelegate vb)
	{
		CSharpName = cs;
		CppCXName = cppCX;
		CppWinRTName = cppWinRT;
		VBName = vb;
	}

	public LanguageSpecificString(CodeGenDelegate all)
		: this(all, all, all, all)
	{
	}

	public override int GetHashCode()
	{
		return (CppCXName() + CppWinRTName() + CSharpName() + VBName()).GetHashCode();
	}

	public override bool Equals(object other)
	{
		if (other is LanguageSpecificString && other != null)
		{
			return GetHashCode() == other.GetHashCode();
		}
		return false;
	}

	public static bool operator ==(LanguageSpecificString left, LanguageSpecificString right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(LanguageSpecificString left, LanguageSpecificString right)
	{
		return !(left == right);
	}
}
