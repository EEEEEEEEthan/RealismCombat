using Godot;
using RealismCombat.Characters;
using RealismCombat.Combats;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class CharacterNode : Control
{
	const float MoveDuration = 0.2f;
	const float ResizeDuration = 0.2f;
	const float ShakeDistance = 8f;
	const float ShakeStepDuration = 0.02f;
	static readonly Vector2 minSize = new(50f, 39f);
	static readonly Vector2 maxSize = new(50f, 86f);
	static readonly StringName enemyThemeName = new("PanelContainer_Orange");
	static readonly StringName allyThemeName = new("PanelContainer_Blue");
	static readonly Vector2 shakeLeftOffset = new(-ShakeDistance, 0f);
	static readonly Vector2 shakeRightOffset = new(ShakeDistance, 0f);
	static void ConfigureTween(Tween tween, Tween.TransitionType transition, Tween.EaseType ease) => tween.SetTrans(transition).SetEase(ease);
	Character? character;
	Control? moveAnchor;
	Container? rootContainer;
	VBoxContainer? propertyContainer;
	Label? nameLabel;
	Tween? moveTween;
	Tween? resizeTween;
	Tween? shakeTween;
	Vector2 rootContainerBasePosition;
	bool rootContainerBasePositionInitialized;
	bool expanded;
	Combat combat = null!;
	PropertyNode actionPointNode = null!;
	PropertyNode hitPointNode = null!;
	PropertyNode headHitPointNode = null!;
	PropertyNode leftArmHitPointNode = null!;
	PropertyNode rightArmHitPointNode = null!;
	PropertyNode torsoHitPointNode = null!;
	PropertyNode leftLegHitPointNode = null!;
	PropertyNode rightLegHitPointNode = null!;
	/// <summary>
	///     获取或设置当前阵营对应的主题。
	/// </summary>
	public bool IsEnemyTheme
	{
		get => RootContainer.ThemeTypeVariation == enemyThemeName;
		set => RootContainer.ThemeTypeVariation = value ? enemyThemeName : allyThemeName;
	}
	/// <summary>
	///     获取或设置当前节点是否处于展开状态。
	/// </summary>
	[Export]
	public bool Expanded
	{
		get => expanded;
		set
		{
			if (expanded == value) return;
			expanded = value;
			if (IsNodeReady())
				ApplyExpandedSizeImmediate();
			else
				ApplyExpandedSizeAnimated();
			UpdateOverviewVisibility();
		}
	}
	Control MoveAnchor => moveAnchor ??= GetNode<Control>("MoveAnchor");
	Container RootContainer => rootContainer ??= GetNode<Container>("MoveAnchor/RootContainer");
	VBoxContainer PropertyContainer => propertyContainer ??= GetNode<VBoxContainer>("MoveAnchor/RootContainer/Mask/VBoxContainer");
	Label NameLabel => nameLabel ??= PropertyContainer.GetNode<Label>("Name");
	public void Initialize(Combat combat, Character value)
	{
		character = value;
		NameLabel.Text = value.name;
		this.combat = combat;
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
		leftLegHitPointNode = GetOrCreatePropertyNode("LeftLegHitPoint", "左腿");
		rightLegHitPointNode = GetOrCreatePropertyNode("RightLegHitPoint", "右腿");
		CallDeferred(nameof(ApplyExpandedSizeImmediate));
		UpdateRootContainerBasePosition();
		UpdateOverviewVisibility();
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
		RootContainer.Position = basePosition;
		shakeTween = RootContainer.CreateTween();
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", basePosition + shakeLeftOffset, ShakeStepDuration);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.InOut);
		shakeTween.TweenProperty(RootContainer, "position", basePosition + shakeRightOffset, ShakeStepDuration * 2f);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", basePosition, ShakeStepDuration);
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (character is null) return;
		var actionPoint = character.actionPoint;
		var hasCombatAction = character.combatAction != null;
		var actionPointValue = hasCombatAction ? actionPoint.maxValue : actionPoint.value;
		actionPointNode.Value = (actionPointValue, actionPoint.maxValue);
		actionPointNode.Jump = hasCombatAction || combat.Considering == character;
		var headHitPoint = character.head.HitPoint;
		var torsoHitPoint = character.torso.HitPoint;
		var headRatio = headHitPoint.maxValue > 0 ? headHitPoint.value / (double)headHitPoint.maxValue : 0d;
		var torsoRatio = torsoHitPoint.maxValue > 0 ? torsoHitPoint.value / (double)torsoHitPoint.maxValue : 0d;
		var targetHitPoint = headRatio <= torsoRatio ? headHitPoint : torsoHitPoint;
		hitPointNode.Value = (targetHitPoint.value, targetHitPoint.maxValue);
		// 更新各个身体部位的血量显示
		headHitPointNode.Value = (headHitPoint.value, headHitPoint.maxValue);
		leftArmHitPointNode.Value = (character.leftArm.HitPoint.value, character.leftArm.HitPoint.maxValue);
		rightArmHitPointNode.Value = (character.rightArm.HitPoint.value, character.rightArm.HitPoint.maxValue);
		torsoHitPointNode.Value = (torsoHitPoint.value, torsoHitPoint.maxValue);
		leftLegHitPointNode.Value = (character.leftLeg.HitPoint.value, character.leftLeg.HitPoint.maxValue);
		rightLegHitPointNode.Value = (character.rightLeg.HitPoint.value, character.rightLeg.HitPoint.maxValue);
		MoveAnchor.Size = RootContainer.Size;
	}
	void ApplyExpandedSizeAnimated()
	{
		var container = RootContainer;
		var targetSize = expanded ? maxSize : minSize;
		resizeTween?.Kill();
		resizeTween = container.CreateTween();
		ConfigureTween(resizeTween, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		resizeTween.TweenProperty(container, "size", targetSize, ResizeDuration);
		resizeTween.Finished += UpdateRootContainerBasePosition;
	}
	void ApplyExpandedSizeImmediate()
	{
		var container = RootContainer;
		var targetSize = expanded ? maxSize : minSize;
		resizeTween?.Kill();
		container.Size = targetSize;
		UpdateRootContainerBasePosition();
	}
	void UpdateRootContainerBasePosition()
	{
		rootContainerBasePosition = RootContainer.Position;
		rootContainerBasePositionInitialized = true;
	}
	PropertyNode GetOrCreatePropertyNode(string nodeName, string title)
	{
		var container = PropertyContainer;
		var node = container.GetNodeOrNull<PropertyNode>(nodeName);
		if (node != null) return node;
		node = ResourceTable.propertyNodeScene.Value.Instantiate<PropertyNode>();
		node.Name = nodeName;
		node.Title = title;
		container.AddChild(node);
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
	void UpdateOverviewVisibility() => hitPointNode?.Visible = !expanded;
}
