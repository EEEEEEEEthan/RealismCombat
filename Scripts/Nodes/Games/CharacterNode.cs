using Godot;
namespace RealismCombat.Nodes.Games;
public partial class CharacterNode : Control
{
	Container? rootContainer;
	Container RootContainer => rootContainer ??= GetNode<Container>("RootContainer");
}
