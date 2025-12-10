using System;
using System.Threading.Tasks;
using Godot;
public partial class DialogueManager : Node
{
	public interface IDialogue
	{
		void HandleInput(InputEvent @event);
	}
	sealed class DialogueScope(GenericDialogue dialogue) : IDisposable
	{
		bool disposed;
		public void Dispose()
		{
			if (disposed) return;
			disposed = true;
			DestroyDialogue(dialogue);
		}
	}
	static DialogueManager instance = null!;
	public static BaseDialogue? TopDialogue => instance.currentDialogue;
	public static IDisposable CreateGenericDialogue(out GenericDialogue dialogue)
	{
		switch (instance.currentDialogue)
		{
			case { } existing when !existing.IsQueuedForDeletion():
				throw new InvalidOperationException("不允许创建多个对话框");
			case { } queued when queued.IsQueuedForDeletion():
				queued.OnClosed -= instance.DialogueClosed;
				instance.currentDialogue = null;
				break;
		}
		dialogue = new();
		AddDialogue(dialogue);
		return new DialogueScope(dialogue);
	}
	public static async Task<int> ShowGenericDialogue(string text, params string[] options)
	{
		using var scope = CreateGenericDialogue(out var dialogue);
		return await dialogue.ShowTextTask(text, options);
	}
	public static void DestroyDialogue(BaseDialogue dialogue)
	{
		if (instance.currentDialogue == dialogue) instance.currentDialogue = null;
		dialogue.OnClosed -= instance.DialogueClosed;
		dialogue.QueueFree();
	}
	public static MenuDialogue CreateMenuDialogue(string title, bool allowEscapeReturn, params MenuOption[] options)
	{
		if (instance.currentDialogue is not null && !instance.currentDialogue.IsQueuedForDeletion()) throw new InvalidOperationException("不允许创建多个对话框");
		if (instance.currentDialogue is { } queued && queued.IsQueuedForDeletion())
		{
			queued.OnClosed -= instance.DialogueClosed;
			instance.currentDialogue = null;
		}
		var dialogue = MenuDialogue.Create(title, options, allowEscapeReturn);
		AddDialogue(dialogue);
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue(string title, params MenuOption[] options) => CreateMenuDialogue(title, false, options);
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
		var topDialogue = TopDialogue;
		if (topDialogue is IDialogue dialogue) dialogue.HandleInput(@event);
	}
	void DialogueClosed(BaseDialogue dialogue)
	{
		if (currentDialogue != dialogue) return;
		currentDialogue.OnClosed -= DialogueClosed;
		currentDialogue = null;
	}
}
