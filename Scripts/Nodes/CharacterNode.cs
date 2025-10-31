using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
partial class CharacterNode : Control
{
	const float maxVerticalOffset = 8.0f;
	const float moveLerpSpeed = 10.0f;
	const float shakeDuration = 0.3f;
	const float shakeStrength = 4.0f;
	public static CharacterNode Create()
	{
		var instance = GD.Load<PackedScene>(ResourceTable.character).Instantiate<CharacterNode>();
		return instance;
	}
	PanelContainer panelContainer = null!;
	HBoxContainer reactionContainer = null!;
	Label nameLabel = null!;
	PropertyDrawerNode actionPoint = null!;
	PropertyDrawerNode speed = null!;
	PropertyDrawerNode head = null!;
	PropertyDrawerNode chest = null!;
	PropertyDrawerNode rightArm = null!;
	PropertyDrawerNode leftArm = null!;
	PropertyDrawerNode rightLeg = null!;
	PropertyDrawerNode leftLeg = null!;
	Vector2 originalPanelPosition;
	float currentVerticalOffset;
	bool isActing;
	double shakeTime;
	int currentReactionCount;
	public CharacterData? CharacterData { get; set; }
	public bool IsActing
	{
		get => isActing;
		set
		{
			if (isActing == value) return;
			isActing = value;
		}
	}
	public void Shake() => shakeTime = shakeDuration;
	public PropertyDrawerNode? GetBodyPartDrawer(BodyPartCode bodyPart) =>
		bodyPart switch
		{
			BodyPartCode.Head => head,
			BodyPartCode.Chest => chest,
			BodyPartCode.LeftArm => leftArm,
			BodyPartCode.RightArm => rightArm,
			BodyPartCode.LeftLeg => leftLeg,
			BodyPartCode.RightLeg => rightLeg,
			_ => null,
		};
	public override void _Ready()
	{
		panelContainer = GetNode<PanelContainer>("PanelContainer");
		reactionContainer = GetNode<HBoxContainer>("PanelContainer/VBoxContainer/NameBar/ReactionContainer");
		nameLabel = GetNode<Label>("PanelContainer/VBoxContainer/NameBar/Title");
		actionPoint = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/ActionPoint");
		speed = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Speed");
		head = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Head");
		chest = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Chest");
		rightArm = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/RightArm");
		leftArm = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/LeftArm");
		rightLeg = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/RightLeg");
		leftLeg = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/LeftLeg");
		originalPanelPosition = panelContainer.Position;
	}
	public override void _Process(double delta)
	{
		if (CharacterData == null) return;
		panelContainer.ThemeTypeVariation = CharacterData.team == 0 ? "PanelContainer_Blue" : "PanelContainer_Orange";
		nameLabel.Text = CharacterData.name;
		actionPoint.Value = ((float)CharacterData.ActionPoint + 10) / 10;
		speed.Value = (float)CharacterData.speed / 10;
		head.Value = (float)CharacterData.head.hp / CharacterData.head.maxHp;
		chest.Value = (float)CharacterData.chest.hp / CharacterData.chest.maxHp;
		rightArm.Value = (float)CharacterData.rightArm.hp / CharacterData.rightArm.maxHp;
		leftArm.Value = (float)CharacterData.leftArm.hp / CharacterData.leftArm.maxHp;
		rightLeg.Value = (float)CharacterData.rightLeg.hp / CharacterData.rightLeg.maxHp;
		leftLeg.Value = (float)CharacterData.leftLeg.hp / CharacterData.leftLeg.maxHp;
		actionPoint.Title = "行动";
		speed.Title = "速度";
		head.Title = BodyPartCode.Head.GetName();
		chest.Title = BodyPartCode.Chest.GetName();
		rightArm.Title = BodyPartCode.RightArm.GetName();
		leftArm.Title = BodyPartCode.LeftArm.GetName();
		rightLeg.Title = BodyPartCode.RightLeg.GetName();
		leftLeg.Title = BodyPartCode.LeftLeg.GetName();
		var targetReactionCount = CharacterData.reaction;
		var actualChildCount = reactionContainer.GetChildCount();
		while (actualChildCount < targetReactionCount)
		{
			var reactionInstance = GD.Load<PackedScene>(ResourceTable.componentsReaction).Instantiate();
			reactionContainer.AddChild(reactionInstance);
			actualChildCount++;
		}
		while (actualChildCount > targetReactionCount)
		{
			var child = reactionContainer.GetChild(actualChildCount - 1);
			child.QueueFree();
			actualChildCount--;
		}
		currentReactionCount = actualChildCount;
		var targetVerticalOffset = 0.0f;
		if (isActing)
		{
			var direction = CharacterData.team == 0 ? -1.0f : 1.0f;
			targetVerticalOffset = maxVerticalOffset * direction;
		}
		currentVerticalOffset = Mathf.Lerp(from: currentVerticalOffset, to: targetVerticalOffset, weight: (float)delta * moveLerpSpeed);
		var horizontalOffset = 0.0f;
		if (shakeTime > 0)
		{
			shakeTime -= delta;
			if (shakeTime < 0) shakeTime = 0;
			var progress = 1.0f - (float)(shakeTime / shakeDuration);
			var currentShakeStrength = shakeStrength * (1.0f - progress);
			horizontalOffset = (GD.Randf() * 2.0f - 1.0f) * currentShakeStrength;
		}
		panelContainer.Position = originalPanelPosition + new Vector2(x: horizontalOffset, y: currentVerticalOffset);
	}
}
