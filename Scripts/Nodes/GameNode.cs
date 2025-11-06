using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	TaskCompletionSource<object>? taskCompletionSource;
	public TaskAwaiter<object> GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	public void SetResult(object result)
	{
		taskCompletionSource ??= new();
		taskCompletionSource.TrySetResult(result);
	}
	public void SetResult() => SetResult(null!);
	public void SetException(System.Exception exception)
	{
		taskCompletionSource ??= new();
		taskCompletionSource.TrySetException(exception);
	}
	public void SetCanceled()
	{
		taskCompletionSource ??= new();
		taskCompletionSource.TrySetCanceled();
	}
	public void Reset() => taskCompletionSource = new();
}
