namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class CodeInfoAndTaskItemPair
{
	public XamlClassCodeInfo ClassCodeInfo { get; private set; }

	public TaskItemFilename TaskItem { get; private set; }

	public CodeInfoAndTaskItemPair(XamlClassCodeInfo classCodeInfo, TaskItemFilename taskItem)
	{
		ClassCodeInfo = classCodeInfo;
		TaskItem = taskItem;
	}
}
