using Godot;
using RealismCombat.Characters;
namespace RealismCombat.Nodes.Games;
public partial class CharacterNode : Control
{
	const float MoveDuration = 0.2f;
	const float ShakeDistance = 8f;
	const float ShakeStepDuration = 0.02f;
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
	Tween? shakeTween;
	/// <summary>
	///     获取或设置当前阵营对应的主题。
	/// </summary>
	public bool IsEnemyTheme
	{
		get => RootContainer.ThemeTypeVariation == enemyThemeName;
		set => RootContainer.ThemeTypeVariation = value ? enemyThemeName : allyThemeName;
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
		RootContainer.Position = Vector2.Zero;
		shakeTween = RootContainer.CreateTween();
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", shakeLeftOffset, ShakeStepDuration);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.InOut);
		shakeTween.TweenProperty(RootContainer, "position", shakeRightOffset, ShakeStepDuration * 2f);
		ConfigureTween(shakeTween, Tween.TransitionType.Sine, Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", Vector2.Zero, ShakeStepDuration);
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
		var headHitPoint = character.head.hp;
		var torsoHitPoint = character.torso.hp;
		var headRatio = headHitPoint.maxValue > 0 ? headHitPoint.value / (double)headHitPoint.maxValue : 0d;
		var torsoRatio = torsoHitPoint.maxValue > 0 ? torsoHitPoint.value / (double)torsoHitPoint.maxValue : 0d;
		var targetHitPoint = headRatio <= torsoRatio ? headHitPoint : torsoHitPoint;
		HitPointNode.Value = (targetHitPoint.value, targetHitPoint.maxValue);
	}
}
