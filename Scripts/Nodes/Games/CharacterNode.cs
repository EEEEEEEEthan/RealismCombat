using Godot;
using RealismCombat.Characters;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class CharacterNode : Control
{
	const float MoveDuration = 0.2f;
	const float ResizeDuration = 0.2f;
	const float ShakeDistance = 8f;
	const float ShakeStepDuration = 0.02f;
	static readonly Vector2 minSize = new(50f, 40f);
	static readonly Vector2 maxSize = new(60f, 100f);
	static readonly StringName enemyThemeName = new("PanelContainer_Orange");
	static readonly StringName allyThemeName = new("PanelContainer_Blue");
	static readonly Vector2 shakeLeftOffset = new(-ShakeDistance, 0f);
	static readonly Vector2 shakeRightOffset = new(ShakeDistance, 0f);
	static void ConfigureTween(Tween tween, Tween.TransitionType transition, Tween.EaseType ease) => tween.SetTrans(transition).SetEase(ease);
	Character? character;
	Control? moveAnchor;
	Container? rootContainer;
	Label? nameLabel;
	PropertyNode? actionPointNode;
	PropertyNode? hitPointNode;
	Tween? moveTween;
	Tween? resizeTween;
	Tween? shakeTween;
	Vector2 rootContainerBasePosition;
	bool rootContainerBasePositionInitialized;
	bool expanded;
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
			ApplyExpandedSize(true);
		}
	}
	/// <summary>
	///     绑定角色。
	/// </summary>
	public Character? BindCharacter
	{
		get => character;
		set
		{
			character = value;
			if (value is null) return;
			NameLabel.Text = value.name;
		}
	}
	Control MoveAnchor => moveAnchor ??= GetNode<Control>("MoveAnchor");
	Container RootContainer => rootContainer ??= GetNode<Container>("MoveAnchor/RootContainer");
	Label NameLabel => nameLabel ??= GetNode<Label>("MoveAnchor/RootContainer/Mask/Name");
	PropertyNode ActionPointNode => actionPointNode ??= GetNode<PropertyNode>("MoveAnchor/RootContainer/Mask/ActionPoint");
	PropertyNode HitPointNode => hitPointNode ??= GetNode<PropertyNode>("MoveAnchor/RootContainer/Mask/HitPointOverview");
	public override void _Ready()
	{
		base._Ready();
		ApplyExpandedSize(false);
		UpdateRootContainerBasePosition();
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
		ActionPointNode.Value = (actionPointValue, actionPoint.maxValue);
		ActionPointNode.Jump = hasCombatAction;
		var headHitPoint = character.head.HitPoint;
		var torsoHitPoint = character.torso.HitPoint;
		var headRatio = headHitPoint.maxValue > 0 ? headHitPoint.value / (double)headHitPoint.maxValue : 0d;
		var torsoRatio = torsoHitPoint.maxValue > 0 ? torsoHitPoint.value / (double)torsoHitPoint.maxValue : 0d;
		var targetHitPoint = headRatio <= torsoRatio ? headHitPoint : torsoHitPoint;
		HitPointNode.Value = (targetHitPoint.value, targetHitPoint.maxValue);
	}
	void ApplyExpandedSize(bool animated)
	{
		var container = RootContainer;
		var targetSize = expanded ? maxSize : minSize;
		resizeTween?.Kill();
		if (!IsInsideTree())
		{
			container.Size = targetSize;
			UpdateRootContainerBasePosition();
			return;
		}
		if (!animated)
		{
			container.Size = targetSize;
			UpdateRootContainerBasePosition();
			return;
		}
		resizeTween = container.CreateTween();
		ConfigureTween(resizeTween, Tween.TransitionType.Cubic, Tween.EaseType.Out);
		resizeTween.TweenProperty(container, "size", targetSize, ResizeDuration);
		resizeTween.Finished += UpdateRootContainerBasePosition;
	}
	void UpdateRootContainerBasePosition()
	{
		rootContainerBasePosition = RootContainer.Position;
		rootContainerBasePositionInitialized = true;
	}
	Vector2 GetRootContainerBasePosition()
	{
		if (!rootContainerBasePositionInitialized) UpdateRootContainerBasePosition();
		return rootContainerBasePosition;
	}
}
