using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	TaskCompletionSource? taskCompletionSource;
	public TaskAwaiter GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
}
