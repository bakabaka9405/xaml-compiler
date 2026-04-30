namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ApiInformationMethod
{
	public bool Condition { get; }

	public string MethodName { get; }

	public string UniqueName => string.Format("{0}{1}", MethodName, Condition ? "" : "Not");

	public ApiInformationMethod(string methodName, bool condition)
	{
		Condition = condition;
		MethodName = methodName;
	}
}
