using Godot;
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
	Control? moveAnchor;
	Container? rootContainer;
	Label? nameLabel;
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
	///     获取或设置角色名字。
	/// </summary>
	public string CharacterName
	{
		get => NameLabel.Text;
		set => NameLabel.Text = value;
	}
	Control MoveAnchor => moveAnchor ??= GetNode<Control>("MoveAnchor");
	Container RootContainer => rootContainer ??= GetNode<Container>("MoveAnchor/RootContainer");
	Label NameLabel => nameLabel ??= GetNode<Label>("MoveAnchor/RootContainer/Mask/Name");
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
}
