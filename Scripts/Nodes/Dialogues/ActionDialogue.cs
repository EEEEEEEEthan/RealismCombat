using System;
using Godot;
using RealismCombat.Commands.CombatCommands;
using RealismCombat.Data;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
partial class ActionDialogue : Node
{
	public static ActionDialogue Create(CombatData combatData, CharacterData actor)
	{
		var scene = GD.Load<PackedScene>(ResourceTable.dialoguesActiondialogue);
		var instance = scene.Instantiate<ActionDialogue>();
		instance.combatData = combatData;
		instance.actor = actor;
		return instance;
	}
	[Export] PrinterLabelNode preview = null!;
	CombatData combatData = null!;
	CharacterData actor = null!;
	MenuDialogue? currentMenu;
	string selectedAttackerPart = "";
	string selectedAction = "";
	string selectedTargetName = "";
	string selectedTargetPart = "";
	Action<string>? onComplete;
	public void Start(Action<string> onComplete)
	{
		this.onComplete = onComplete;
		ShowActorName();
	}
	public override void _Process(double delta)
	{
		if (currentMenu != null) PositionMenu(currentMenu);
	}
	void ShowActorName()
	{
		preview.Show(actor.name);
		ShowBodyPartSelection();
	}
	void ShowBodyPartSelection()
	{
		ClearMenu();
		currentMenu = MenuDialogue.Create();
		currentMenu.Title = "选择身体部位";
		AddChild(currentMenu);
		PositionMenu(currentMenu);
		foreach (var bodyPart in actor.BodyParts)
		{
			var part = bodyPart.id.ToString();
			currentMenu.AddOption(
				option: GetBodyPartDisplayName(bodyPart.id),
				description: $"使用{GetBodyPartDisplayName(bodyPart.id)}",
				callback: () =>
				{
					selectedAttackerPart = part;
					preview.Show($"{actor.name}{GetBodyPartDisplayName(bodyPart.id)}");
					ShowActionSelection();
				}
			);
		}
	}
	void ShowActionSelection()
	{
		ClearMenu();
		currentMenu = MenuDialogue.Create();
		currentMenu.Title = "选择行动";
		AddChild(currentMenu);
		PositionMenu(currentMenu);
		currentMenu.AddOption(
			option: "攻击",
			description: "发动攻击",
			callback: () =>
			{
				selectedAction = "攻击";
				preview.Show($"{actor.name}{GetBodyPartDisplayName(Enum.Parse<BodyPartCode>(selectedAttackerPart))}{selectedAction}");
				ShowTargetSelection();
			}
		);
		currentMenu.AddOption(
			option: "返回",
			description: "返回上一步",
			callback: ShowBodyPartSelection
		);
	}
	void ShowTargetSelection()
	{
		ClearMenu();
		currentMenu = MenuDialogue.Create();
		currentMenu.Title = "选择目标";
		AddChild(currentMenu);
		PositionMenu(currentMenu);
		foreach (var character in combatData.characters)
			if (character.team != actor.team && !character.Dead)
			{
				var targetName = character.name;
				currentMenu.AddOption(
					option: targetName,
					description: $"攻击{targetName}",
					callback: () =>
					{
						selectedTargetName = targetName;
						preview.Show(
							$"{actor.name}{GetBodyPartDisplayName(Enum.Parse<BodyPartCode>(selectedAttackerPart))}{selectedAction}{selectedTargetName}");
						ShowTargetBodyPartSelection();
					}
				);
			}
		currentMenu.AddOption(
			option: "返回",
			description: "返回上一步",
			callback: ShowActionSelection
		);
	}
	void ShowTargetBodyPartSelection()
	{
		ClearMenu();
		currentMenu = MenuDialogue.Create();
		currentMenu.Title = "选择目标部位";
		AddChild(currentMenu);
		PositionMenu(currentMenu);
		var target = GetCharacterByName(selectedTargetName);
		if (target != null)
			foreach (var bodyPart in target.BodyParts)
			{
				var part = bodyPart.id.ToString();
				currentMenu.AddOption(
					option: GetBodyPartDisplayName(bodyPart.id),
					description: $"攻击{GetBodyPartDisplayName(bodyPart.id)}",
					callback: () =>
					{
						selectedTargetPart = part;
						preview.Show(
							$"{actor.name}{GetBodyPartDisplayName(Enum.Parse<BodyPartCode>(selectedAttackerPart))}{selectedAction}{selectedTargetName}{GetBodyPartDisplayName(Enum.Parse<BodyPartCode>(selectedTargetPart))}");
						ShowConfirmation();
					}
				);
			}
		currentMenu.AddOption(
			option: "返回",
			description: "返回上一步",
			callback: ShowTargetSelection
		);
	}
	void ShowConfirmation()
	{
		ClearMenu();
		currentMenu = MenuDialogue.Create();
		currentMenu.Title = "确认行动";
		AddChild(currentMenu);
		PositionMenu(currentMenu);
		currentMenu.AddOption(
			option: "确认",
			description: "执行此行动",
			callback: () =>
			{
				var command = $"{AttackCommand.name} target {selectedTargetName} attackerPart {selectedAttackerPart} targetPart {selectedTargetPart}";
				onComplete?.Invoke(command);
			}
		);
		currentMenu.AddOption(
			option: "返回",
			description: "返回上一步",
			callback: ShowTargetBodyPartSelection
		);
	}
	void ClearMenu()
	{
		if (currentMenu != null && currentMenu.Valid())
		{
			currentMenu.QueueFree();
			currentMenu = null;
		}
	}
	void PositionMenu(MenuDialogue menuDialogue)
	{
		var font = preview.GetThemeFont("normal_font");
		var fontSize = preview.GetThemeFontSize("normal_font_size");
		var textWidth = font.GetStringSize(text: preview.Text, alignment: HorizontalAlignment.Left, width: -1, fontSize: fontSize).X;
		var height = menuDialogue.GetRect().Size.Y;
		menuDialogue.GlobalPosition = preview.GlobalPosition + new Vector2(x: textWidth, y: -height);
	}
	CharacterData? GetCharacterByName(string name)
	{
		foreach (var character in combatData.characters)
			if (character.name == name)
				return character;
		return null;
	}
	string GetBodyPartDisplayName(BodyPartCode code) =>
		code switch
		{
			BodyPartCode.Head => "头部",
			BodyPartCode.Chest => "胸部",
			BodyPartCode.LeftArm => "左手",
			BodyPartCode.RightArm => "右手",
			BodyPartCode.LeftLeg => "左腿",
			BodyPartCode.RightLeg => "右腿",
			_ => code.ToString(),
		};
}
