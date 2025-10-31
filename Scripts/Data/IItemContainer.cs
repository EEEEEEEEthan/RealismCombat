using System;
using System.Collections.Generic;
namespace RealismCombat.Data;
public interface IItemContainer
{
	IReadOnlyList<ItemData?> items { get; }
	event Action? ItemsChanged;
}
