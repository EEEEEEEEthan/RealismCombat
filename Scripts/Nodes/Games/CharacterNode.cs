using System;
using Godot;
using RealismCombat.Characters;
using RealismCombat.Combats;
namespace RealismCombat.Nodes.Games;
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
	static readonly StringName enemyThemeName = new("PanelContainer_Orange");
	static readonly StringName allyThemeName = new("PanelContainer_Blue");
	static readonly Vector2 shakeLeftOffset = new(-ShakeDistance, 0f);
	static readonly Vector2 shakeRightOffset = new(ShakeDistance, 0f);
	static readonly Color[] playerBackgroundColors = [new("#5182ff"), new("#4141ff"), new("#2800ba"),];
	static readonly Color[] enemyBackgroundColors = [new("#ff7930"), new("#e35100"), new("#e23000"),];
	static readonly Color deadBackgroundColor = new("#797979");
	static void ConfigureTween(Tween tween, Tween.TransitionType transition, Tween.EaseType ease) => tween.SetTrans(transition).SetEase(ease);
	[Export] Vector2 minSize = new(50f, 39f);
	[Export] Vector2 maxSize = new(50f, 86f);
	Character? character;
	Container rootContainer = null!;
	VBoxContainer propertyContainer = null!;
	Label nameLabel = null!;
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
	NinePatchRect background = null!;
	Control moveAnchor = null!;
	bool isEnemyTheme;
	/// <summary>
	///     获取或设置当前阵营对应的主题。
	/// </summary>
	public bool IsEnemyTheme
	{
		get => isEnemyTheme;
		set
		{
			isEnemyTheme = value;
			UpdateBackground();
		}
	}
	/// <summary>
	///     获取或设置当前节点是否处于展开状态。
	/// </summary>
	[Export]
	bool Expanded
	{
		get => expanded;
		set
		{
			if (expanded == value) return;
			expanded = value;
			if (IsNodeReady())
				ApplyExpandedSizeAnimated();
			else
				ApplyExpandedSizeImmediate();
		}
	}
	public IDisposable ExpandScope() => new ExpandDisposable(this);
	public IDisposable MoveScope(Vector2 globalPosition) => new MoveDisposable(this, globalPosition);
	public void Initialize(Combat combat, Character value)
	{
		if (!IsNodeReady()) throw new InvalidOperationException("节点尚未准备好，无法初始化");
		character = value;
		nameLabel.Text = value.name;
		this.combat = combat;
		UpdateBackground();
	}
	public override void _Ready()
	{
		base._Ready();
		rootContainer = GetNode<Container>("MoveAnchor/RootContainer");
		background = rootContainer.GetNode<NinePatchRect>("Background/NinePatchRect");
		propertyContainer = GetNode<VBoxContainer>("MoveAnchor/RootContainer/Mask/VBoxContainer");
		nameLabel = propertyContainer.GetNode<Label>("Name");
		moveAnchor = GetNode<Control>("MoveAnchor");
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
		moveAnchor.Position = default;
		UpdateBackground();
	}
	/// <summary>
	///     将MoveAnchor平滑移动到指定的全局坐标。
	/// </summary>
	public void MoveTo(Vector2 globalPosition)
	{
		moveTween?.Kill();
		if (moveAnchor.GlobalPosition == globalPosition) return;
		moveTween = moveAnchor.CreateTween();
		ConfigureTween(moveTween, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		moveTween.TweenProperty(moveAnchor, "global_position", globalPosition, MoveDuration);
	}
	/// <summary>
	///     让RootContainer产生一次横向晃动并回到原位。
	/// </summary>
	public void Shake()
	{
		shakeTween?.Kill();
		var basePosition = GetRootContainerBasePosition();
		rootContainer.Position = basePosition;
		shakeTween = rootContainer.CreateTween();
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(rootContainer, "position", basePosition + shakeLeftOffset, ShakeStepDuration);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.InOut);
		shakeTween.TweenProperty(rootContainer, "position", basePosition + shakeRightOffset, ShakeStepDuration * 2f);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(rootContainer, "position", basePosition, ShakeStepDuration);
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
		moveAnchor.Size = rootContainer.Size;
		UpdateBackground();
	}
	void UpdateBackground()
	{
		if (!IsNodeReady()) return;
		if (character?.IsAlive != true)
		{
			background.SelfModulate = deadBackgroundColor;
			return;
		}
		var colors = isEnemyTheme ? enemyBackgroundColors : playerBackgroundColors;
		background.SelfModulate = hitPointNode.Progress switch
		{
			> 0.3 => colors[0],
			> 0.25 => colors[1],
			_ => colors[2],
		};
	}
	void ApplyExpandedSizeAnimated()
	{
		UpdateOverviewVisibility();
		var container = rootContainer;
		var targetSize = expanded ? maxSize : minSize;
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
		var container = rootContainer;
		var targetSize = expanded ? maxSize : minSize;
		resizeTween?.Kill();
		container.Size = targetSize;
		UpdateRootContainerBasePosition();
	}
	void UpdateRootContainerBasePosition()
	{
		rootContainerBasePosition = rootContainer.Position;
		rootContainerBasePositionInitialized = true;
	}
	PropertyNode GetOrCreatePropertyNode(string nodeName, string title)
	{
		var node = propertyContainer.GetNodeOrNull<PropertyNode>(nodeName);
		if (node != null) return node;
		node = ResourceTable.propertyNodeScene.Value.Instantiate<PropertyNode>();
		node.Name = nodeName;
		node.Title = title;
		propertyContainer.AddChild(node);
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
