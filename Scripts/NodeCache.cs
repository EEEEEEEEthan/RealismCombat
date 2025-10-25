using System;
using Godot;
namespace RealismCombat;
public class NodeCache<T>(Func<T> nodeGetter) where T : Node
{
	Func<T>? nodeGetter = nodeGetter;
	T? node;
	public T Node
	{
		get
		{
			if (node is null)
			{
				node = nodeGetter?.Invoke();
				nodeGetter = null;
			}
			return node!;
		}
	}
}
