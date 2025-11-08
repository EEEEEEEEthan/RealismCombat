using System;
using Godot;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.AutoLoad;
public partial class DialogueManager : Node
{
	static DialogueManager instance = null!;
	public static GenericDialogue CreateGenericDialogue()
	{
		var dialogue = new GenericDialogue();
		AddDialogue(dialogue);
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue()
	{
		var dialogue = new MenuDialogue();
		AddDialogue(dialogue);
		return dialogue;
	}
	public static BaseDialogue? GetTopDialogue() => instance.currentDialogue;
	public static int GetDialogueCount() => instance.currentDialogue is null ? 0 : 1;
	static void AddDialogue(BaseDialogue dialogue)
	{
		if (instance.currentDialogue is not null) throw new InvalidOperationException("不允许创建多个对话框");
		instance.currentDialogue = dialogue;
		instance.AddChild(dialogue);
		dialogue.OnDisposing += instance.OnDialogueDisposing;
		Log.Print($"[DialogueManager] 添加Dialogue: {dialogue.GetType().Name}, 当前堆栈大小: {GetDialogueCount()}");
	}
	BaseDialogue? currentDialogue;
	public override void _Ready()
	{
		instance = this;
		Log.Print("[DialogueManager] DialogueManager初始化完成");
	}
	public override void _Input(InputEvent @event)
	{
		var topDialogue = GetTopDialogue();
		topDialogue?.HandleInput(@event);
	}
	void OnDialogueDisposing(BaseDialogue dialogue)
	{
		if (currentDialogue != dialogue) return;
		currentDialogue.OnDisposing -= OnDialogueDisposing;
		currentDialogue = null;
	}
}
