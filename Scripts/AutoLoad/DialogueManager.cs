using System;
using Godot;
public partial class DialogueManager : Node
{
	public interface IDialogue
	{
		void HandleInput(InputEvent @event);
	}
	static DialogueManager instance = null!;
	public static GenericDialogue CreateGenericDialogue(params string[] initialTexts)
	{
		if (instance.currentDialogue is not null) throw new InvalidOperationException("不允许创建多个对话框");
		var dialogue = new GenericDialogue(initialTexts);
		AddDialogue(dialogue);
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue(string title, bool allowEscapeReturn, params MenuOption[] options)
	{
		if (instance.currentDialogue is not null) throw new InvalidOperationException("不允许创建多个对话框");
		var dialogue = MenuDialogue.Create(title, options, allowEscapeReturn);
		AddDialogue(dialogue);
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue(string title, params MenuOption[] options) => CreateMenuDialogue(title, false, options);
	public static BaseDialogue? GetTopDialogue() => instance.currentDialogue;
	public static int GetDialogueCount() => instance.currentDialogue is null ? 0 : 1;
	static void AddDialogue(BaseDialogue dialogue)
	{
		instance.currentDialogue = dialogue;
		instance.AddChild(dialogue);
		dialogue.OnClosed += instance.DialogueClosed;
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
		if (topDialogue is IDialogue dialogue) dialogue.HandleInput(@event);
	}
	void DialogueClosed(BaseDialogue dialogue)
	{
		if (currentDialogue != dialogue) return;
		currentDialogue.OnClosed -= DialogueClosed;
		currentDialogue = null;
	}
}
