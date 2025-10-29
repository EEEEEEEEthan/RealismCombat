using Godot;
using RealismCombat;
using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.Nodes.Dialogues;
public partial class GameNode : Node
{
	GameData gameData = null!;
	ProgramRootNode root = null!;

	public static GameNode FromLoad(GameData gameData)
	{
		var gameNode = Create();
		gameNode.gameData = gameData;
		return gameNode;
	}
	public static GameNode FromNew()
	{
		var gameNode = Create();
		gameNode.gameData = new();
		return gameNode;
	}
	static GameNode Create()
	{
		return GD.Load<PackedScene>(ResourceTable.game).Instantiate<GameNode>();
	}

	public override void _Ready()
	{
		root = GetParent<ProgramRootNode>();
		GetTree().CreateTimer(1.0).Timeout += ShowMainMenu;
	}

	void ShowMainMenu()
	{
		var dialogue = root.CreateDialogue();
		dialogue.Initialize(new()
		{
			title = "游戏菜单",
			options =
			[
				new()
				{
					option = "进入战斗",
					description = "开始战斗",
					onPreview = () => { },
					onConfirm = () =>
					{
						dialogue.QueueFree();
						EnterCombat();
						root.McpRespond();
					},
					available = true,
				},
			],
		});
	}

	void EnterCombat()
	{
		var combatData = new CombatData();
		var combatNode = CombatNode.Create(this, combatData);
		AddChild(combatNode);
	}
}
