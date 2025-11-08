using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	TaskCompletionSource? taskCompletionSource;
	public GameNode() => StartGameLoop();
	public TaskAwaiter GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	void Quit()
	{
		taskCompletionSource?.SetResult();
		QueueFree();
	}
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
				var choice = await menu.Start();
				switch (choice)
				{
					case 0:
					{
						var dialogue = DialogueManager.CreateGenericDialogue();
						dialogue.SetText("战斗系统尚未实现");
						dialogue.Start();
						await dialogue;
						break;
					}
					case 1:
					{
						var dialogue = DialogueManager.CreateGenericDialogue();
						dialogue.SetText("状态系统尚未实现");
						dialogue.Start();
						await dialogue;
						break;
					}
					case 3:
					{
						Quit();
						return;
					}
					default:
					{
						throw new InvalidOperationException($"未知的菜单选项: {choice}");
					}
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			Quit();
		}
	}
}
