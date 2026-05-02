using System.CodeDom.Compiler;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[GeneratedCode("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
internal class MoComCppTypeInfoPass1b : CppCX_CodeGenerator<TypeInfoDefinition>
{
	public override string TransformText()
	{
		if (!string.IsNullOrEmpty(base.ProjectInfo.PrecompiledHeaderFile))
		{
			Write("#include \"");
			Write(base.ToStringHelper.ToStringWithCulture(base.ProjectInfo.PrecompiledHeaderFile));
			Write("\"\r\n");
		}
		return base.GenerationEnvironment.ToString();
	}
}
