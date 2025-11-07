using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	TaskCompletionSource? taskCompletionSource;
	public TaskAwaiter GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	public override void _Ready() => StartGameLoop();
	async void StartGameLoop()
	{
		try
		{
			while (true)
			{
				var menu = DialogueManager.CreateMenuDialogue();
				menu.AddOption(new() { title = "开始战斗", description = "进入战斗场景", });
				menu.AddOption(new() { title = "查看状态", description = "查看角色状态", });
				menu.AddOption(new() { title = "退出游戏", description = "返回主菜单", });
				menu.Start();
				var choice = await menu;
				if (choice == 0)
				{
					var dialogue = DialogueManager.CreateGenericDialogue();
					dialogue.SetText("战斗系统尚未实现");
					dialogue.Start();
					await dialogue;
				}
				else if (choice == 1)
				{
					var dialogue = DialogueManager.CreateGenericDialogue();
					dialogue.SetText("状态系统尚未实现");
					dialogue.Start();
					await dialogue;
				}
				else
				{
					var dialogue = DialogueManager.CreateGenericDialogue();
					dialogue.SetText("返回主菜单");
					dialogue.Start();
					await dialogue;
					taskCompletionSource?.SetResult();
					QueueFree();
					return;
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			taskCompletionSource?.SetResult();
			QueueFree();
		}
	}
}
