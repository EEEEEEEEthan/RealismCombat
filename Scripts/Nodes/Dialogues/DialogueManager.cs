using System.Collections.Generic;
using Godot;
namespace RealismCombat.Nodes;
public partial class DialogueManager : Node
{
	static DialogueManager? instance;
	public static bool IsInitialized => instance != null;
	public static GenericDialogue CreateGenericDialogue()
	{
		if (instance == null)
		{
			Log.PrintErr("[DialogueManager] DialogueManager未初始化");
			return null!;
		}
		var dialogue = new GenericDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 创建GenericDialogue, 当前堆栈大小: {instance.dialogueStack.Count}");
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue()
	{
		if (instance == null)
		{
			Log.PrintErr("[DialogueManager] DialogueManager未初始化");
			return null!;
		}
		var dialogue = new MenuDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 创建MenuDialogue, 当前堆栈大小: {instance.dialogueStack.Count}");
		return dialogue;
	}
	public static void RemoveDialogue(BaseDialogue dialogue)
	{
		if (instance == null)
		{
			Log.PrintErr("[DialogueManager] DialogueManager未初始化");
			return;
		}
		instance.dialogueStack.Remove(dialogue);
		Log.Print($"[DialogueManager] 移除Dialogue: {dialogue.GetType().Name}, 当前堆栈大小: {instance.dialogueStack.Count}");
	}
	public static bool IsTopDialogue(BaseDialogue dialogue)
	{
		if (instance == null || instance.dialogueStack.Count == 0) return false;
		return instance.dialogueStack[^1] == dialogue;
	}
	public static BaseDialogue? GetTopDialogue()
	{
		if (instance == null || instance.dialogueStack.Count == 0) return null;
		return instance.dialogueStack[^1];
	}
	public static int GetDialogueCount() => instance?.dialogueStack.Count ?? 0;
	static void AddDialogue(BaseDialogue dialogue)
	{
		if (instance == null)
		{
			Log.PrintErr("[DialogueManager] DialogueManager未初始化");
			return;
		}
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 添加Dialogue: {dialogue.GetType().Name}, 当前堆栈大小: {instance.dialogueStack.Count}");
	}
	readonly List<BaseDialogue> dialogueStack = [];
	public override void _Ready()
	{
		if (instance != null)
		{
			Log.PrintErr("[DialogueManager] 单例已存在，销毁重复实例");
			QueueFree();
			return;
		}
		instance = this;
		Log.Print("[DialogueManager] DialogueManager初始化完成");
	}
	public override void _ExitTree()
	{
		if (instance == this) instance = null;
	}
}
