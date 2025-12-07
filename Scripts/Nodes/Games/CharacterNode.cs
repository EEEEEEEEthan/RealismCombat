using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
[Tool]
public partial class CharacterNode : Control
{
	readonly struct ExpandDisposable : IDisposable
	{
		readonly CharacterNode node;
		public ExpandDisposable(CharacterNode node)
		{
			this.node = node;
			node.Expanded = true;
		}
		public void Dispose() => node.Expanded = false;
	}
	readonly struct MoveDisposable : IDisposable
	{
		readonly CharacterNode node;
		public MoveDisposable(CharacterNode node, Vector2 globalPosition)
		{
			this.node = node;
			node.MoveTo(globalPosition);
		}
		public void Dispose() => node.MoveTo(node.GlobalPosition);
	}
	const float MoveDuration = 0.2f;
	const float ResizeDuration = 0.2f;
	const float ShakeDistance = 8f;
	const float ShakeStepDuration = 0.02f;
	static readonly Vector2 shakeLeftOffset = new(-ShakeDistance, 0f);
	static readonly Vector2 shakeRightOffset = new(ShakeDistance, 0f);
	static readonly Color deadBackgroundColor = GameColors.grayGradient[^2];
	static void ConfigureTween(Tween tween, Tween.TransitionType transition, Tween.EaseType ease) => tween.SetTrans(transition).SetEase(ease);
	[Export] Vector2 minSize = new(55f, 39f);
	[Export] Vector2 maxSize = new(55f, 98f);
	Character? character;
	Tween? moveTween;
	Tween? resizeTween;
	Tween? shakeTween;
	Vector2 rootContainerBasePosition;
	bool rootContainerBasePositionInitialized;
	Combat combat = null!;
	PropertyNode actionPointNode = null!;
	PropertyNode hitPointNode = null!;
	PropertyNode headHitPointNode = null!;
	PropertyNode leftArmHitPointNode = null!;
	PropertyNode rightArmHitPointNode = null!;
	PropertyNode torsoHitPointNode = null!;
	PropertyNode groinHitPointNode = null!;
	PropertyNode leftLegHitPointNode = null!;
	PropertyNode rightLegHitPointNode = null!;
	/// <summary>
	///     获取或设置当前阵营对应的主题。
	/// </summary>
	public bool IsEnemyTheme
	{
		get;
		set
		{
			field = value;
			UpdateBackground();
		}
	}
	[field: AllowNull, MaybeNull,] CardFrame CardFrame => field ??= GetNode<CardFrame>("%CardFrame");
	[field: AllowNull, MaybeNull,] VBoxContainer PropertyContainer => field ??= GetNode<VBoxContainer>("%PropertyContainer");
	[field: AllowNull, MaybeNull,] Label NameLabel => field ??= PropertyContainer.GetNode<Label>("%NameLabel");
	[field: AllowNull, MaybeNull,] Control MoveAnchor => field ??= GetNode<Control>("%MoveAnchor");
	[field: AllowNull, MaybeNull,] Container ReactionContainer => field ??= PropertyContainer.GetNode<Container>("%ReactionContainer");
	/// <summary>
	///     获取或设置当前节点是否处于展开状态。
	/// </summary>
	[Export]
	bool Expanded
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			ZIndex = value ? 1 : 0;
			if (IsNodeReady())
				ApplyExpandedSizeAnimated();
			else
				ApplyExpandedSizeImmediate();
		}
	}
	/// <summary>
	///     获取或设置当前反应点数。
	/// </summary>
	[Export]
	int ReactionCount
	{
		get;
		set
		{
			if (field == value) return;
			field = value;
			if (IsNodeReady()) UpdateReactionDisplay();
		}
	}
	public IDisposable ExpandScope() => new ExpandDisposable(this);
	public IDisposable MoveScope(Vector2 globalPosition) => new MoveDisposable(this, globalPosition);
	public void Initialize(Combat combat, Character value)
	{
		if (!IsNodeReady()) throw new InvalidOperationException("节点尚未准备好，无法初始化");
		character = value;
		NameLabel.Text = value.name;
		this.combat = combat;
		UpdateBackground();
		ReactionCount = value.reaction;
	}
	public override void _Ready()
	{
		base._Ready();
		actionPointNode = GetOrCreatePropertyNode("ActionPoint", "行动");
		hitPointNode = GetOrCreatePropertyNode("HitPointOverview", "生命");
		headHitPointNode = GetOrCreatePropertyNode("HeadHitPoint", "头部");
		leftArmHitPointNode = GetOrCreatePropertyNode("LeftArmHitPoint", "左臂");
		rightArmHitPointNode = GetOrCreatePropertyNode("RightArmHitPoint", "右臂");
		torsoHitPointNode = GetOrCreatePropertyNode("TorsoHitPoint", "躯干");
		groinHitPointNode = GetOrCreatePropertyNode("GroinHitPoint", "裆部");
		leftLegHitPointNode = GetOrCreatePropertyNode("LeftLegHitPoint", "左腿");
		rightLegHitPointNode = GetOrCreatePropertyNode("RightLegHitPoint", "右腿");
		CallDeferred(nameof(ApplyExpandedSizeImmediate));
		UpdateRootContainerBasePosition();
		UpdateOverviewVisibility();
		MoveAnchor.Position = default;
		UpdateBackground();
	}
	/// <summary>
	///     将MoveAnchor平滑移动到指定的全局坐标。
	/// </summary>
	public void MoveTo(Vector2 globalPosition)
	{
		moveTween?.Kill();
		if (MoveAnchor.GlobalPosition == globalPosition) return;
		moveTween = MoveAnchor.CreateTween();
		ConfigureTween(moveTween, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		moveTween.TweenProperty(MoveAnchor, "global_position", globalPosition, MoveDuration);
	}
	/// <summary>
	///     让RootContainer产生一次横向晃动并回到原位。
	/// </summary>
	public void Shake()
	{
		shakeTween?.Kill();
		var basePosition = GetRootContainerBasePosition();
		CardFrame.Position = basePosition;
		shakeTween = CardFrame.CreateTween();
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(CardFrame, "position", basePosition + shakeLeftOffset, ShakeStepDuration);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.InOut);
		shakeTween.TweenProperty(CardFrame, "position", basePosition + shakeRightOffset, ShakeStepDuration * 2f);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(CardFrame, "position", basePosition, ShakeStepDuration);
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (character is null) return;
		var actionPoint = character.actionPoint;
		var hasCombatAction = character.combatAction != null;
		var actionPointValue = hasCombatAction ? actionPoint.maxValue : actionPoint.value;
		actionPointNode.Value = (actionPointValue, actionPoint.maxValue);
		// 角色死亡后,行动力条不再抖动
		actionPointNode.Jump = character.IsAlive && (hasCombatAction || combat.Considering == character);
		var headHitPoint = character.head.HitPoint;
		var torsoHitPoint = character.torso.HitPoint;
		var headRatio = headHitPoint.maxValue > 0 ? headHitPoint.value / (double)headHitPoint.maxValue : 0d;
		var torsoRatio = torsoHitPoint.maxValue > 0 ? torsoHitPoint.value / (double)torsoHitPoint.maxValue : 0d;
		var targetHitPoint = headRatio <= torsoRatio ? headHitPoint : torsoHitPoint;
		hitPointNode.Value = (targetHitPoint.value, targetHitPoint.maxValue);
		// 更新各个身体部位的血量显示
		headHitPointNode.Value = (headHitPoint.value, headHitPoint.maxValue);
		headHitPointNode.BarWidth = headHitPoint.maxValue * 2 - 1;
		leftArmHitPointNode.Value = (character.leftArm.HitPoint.value, character.leftArm.HitPoint.maxValue);
		leftArmHitPointNode.BarWidth = character.leftArm.HitPoint.maxValue * 2 - 1;
		rightArmHitPointNode.Value = (character.rightArm.HitPoint.value, character.rightArm.HitPoint.maxValue);
		rightArmHitPointNode.BarWidth = character.rightArm.HitPoint.maxValue * 2 - 1;
		torsoHitPointNode.Value = (torsoHitPoint.value, torsoHitPoint.maxValue);
		torsoHitPointNode.BarWidth = torsoHitPoint.maxValue * 2 - 1;
		groinHitPointNode.Value = (character.groin.HitPoint.value, character.groin.HitPoint.maxValue);
		groinHitPointNode.BarWidth = character.groin.HitPoint.maxValue * 2 - 1;
		leftLegHitPointNode.Value = (character.leftLeg.HitPoint.value, character.leftLeg.HitPoint.maxValue);
		leftLegHitPointNode.BarWidth = character.leftLeg.HitPoint.maxValue * 2 - 1;
		rightLegHitPointNode.Value = (character.rightLeg.HitPoint.value, character.rightLeg.HitPoint.maxValue);
		rightLegHitPointNode.BarWidth = character.rightLeg.HitPoint.maxValue * 2 - 1;
		MoveAnchor.Size = CardFrame.Size;
		UpdateBackground();
		ReactionCount = character.reaction;
	}
	/// <summary>
	///     根据战斗目标找到对应的PropertyNode并闪烁
	/// </summary>
	public void FlashPropertyNode(ICombatTarget combatTarget)
	{
		if (character is null) return;
		PropertyNode? targetNode = null;
		if (combatTarget is BodyPart bodyPart)
			targetNode = bodyPart.id switch
			{
				BodyPartCode.Head => headHitPointNode,
				BodyPartCode.LeftArm => leftArmHitPointNode,
				BodyPartCode.RightArm => rightArmHitPointNode,
				BodyPartCode.Torso => torsoHitPointNode,
				BodyPartCode.Groin => groinHitPointNode,
				BodyPartCode.LeftLeg => leftLegHitPointNode,
				BodyPartCode.RightLeg => rightLegHitPointNode,
				_ => null,
			};
		targetNode?.FlashRed();
	}
	public void FlashFrame() => CardFrame.Flash();
	void UpdateBackground()
	{
		if (!IsNodeReady()) return;
		if (character?.IsAlive != true)
		{
			CardFrame.Color = deadBackgroundColor;
			return;
		}
		var colors = IsEnemyTheme ? GameColors.sunFlareOrangeGradient : GameColors.skyBlueGradient;
		CardFrame.Color = hitPointNode.Progress switch
		{
			> 0.3 => colors[1],
			> 0.25 => colors[2],
			_ => colors[3],
		};
	}
	void ApplyExpandedSizeAnimated()
	{
		UpdateOverviewVisibility();
		var container = CardFrame;
		var targetSize = GetTargetSize();
		resizeTween?.Kill();
		resizeTween = container.CreateTween();
		ConfigureTween(resizeTween, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		resizeTween.TweenProperty(container, "size", targetSize, ResizeDuration);
		resizeTween.Finished += UpdateRootContainerBasePosition;
	}
	void ApplyExpandedSizeImmediate()
	{
		if (!IsNodeReady()) return;
		UpdateOverviewVisibility();
		var container = CardFrame;
		var targetSize = GetTargetSize();
		resizeTween?.Kill();
		container.Size = targetSize;
		UpdateRootContainerBasePosition();
	}
	void UpdateRootContainerBasePosition()
	{
		rootContainerBasePosition = CardFrame.Position;
		rootContainerBasePositionInitialized = true;
	}
	/// <summary>
	///     计算展开状态下的目标尺寸，避免由于子节点显隐导致留白。
	/// </summary>
	Vector2 GetExpandedSize()
	{
		if (!IsNodeReady()) return maxSize;
		PropertyContainer.QueueSort();
		var containerSize = PropertyContainer.GetCombinedMinimumSize();
		return maxSize with { Y = Mathf.Max(minSize.Y, containerSize.Y), };
	}
	Vector2 GetTargetSize() => Expanded ? GetExpandedSize() : minSize;
	PropertyNode GetOrCreatePropertyNode(string nodeName, string title)
	{
		var node = PropertyContainer.GetNodeOrNull<PropertyNode>(nodeName);
		if (node != null) return node;
		node = ResourceTable.propertyNodeScene.Value.Instantiate<PropertyNode>();
		node.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
		node.Name = nodeName;
		node.Title = title;
		PropertyContainer.AddChild(node);
		return node;
	}
	Vector2 GetRootContainerBasePosition()
	{
		if (!rootContainerBasePositionInitialized) UpdateRootContainerBasePosition();
		return rootContainerBasePosition;
	}
	/// <summary>
	///     根据展开状态更新Overview（总生命值）的可见性。
	///     展开时隐藏Overview，折叠时显示Overview。
	/// </summary>
	void UpdateOverviewVisibility() => hitPointNode?.Visible = !Expanded;
	/// <summary>
	///     更新反应点数显示，确保TextureRect数量与ReactionCount同步。
	/// </summary>
	void UpdateReactionDisplay()
	{
		if (!IsNodeReady()) return;
		var children = ReactionContainer.GetChildren();
		foreach (var child in children) child.QueueFree();
		for (var i = 0; i < ReactionCount; i++)
		{
			var textureRect = new TextureRect();
			textureRect.Texture = SpriteTable.Star;
			ReactionContainer.AddChild(textureRect);
		}
	}
}
