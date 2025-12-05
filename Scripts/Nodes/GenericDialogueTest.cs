using System.Threading.Tasks;
using Godot;
/// <summary>
///     用于演示和验证 GenericDialogue 的基本交互
/// </summary>
public partial class GenericDialogueTest : Node
{
	public override async void _Ready()
	{
		await DialogueManager.ShowGenericDialogue("无选项示例，按任意键继续");
		var selection = await DialogueManager.ShowGenericDialogue("请选择一个选项", "选项A", "选项B", "选项C");
		Log.Print($"选中了选项索引: {selection}");
		using (DialogueManager.CreateGenericDialogue(out var dialogue))
		{
			await dialogue.ShowTextTask("同一个对话框的多段文本（1）");
			await dialogue.ShowTextTask("同一个对话框的多段文本（2）");
			await dialogue.ShowTextTask("同一个对话框的选项展示", "继续", "结束");
		}
	}
}

