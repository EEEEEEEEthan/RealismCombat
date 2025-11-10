using Godot;
namespace RealismCombat.Nodes.Games;
public partial class CharacterNode : Control
{
	const float MoveDuration = 0.2f;
	const float ShakeDistance = 8f;
	const float ShakeStepDuration = 0.02f;
	Control? moveAnchor;
	Container? rootContainer;
	Label? nameLabel;
	Tween? moveTween;
	Tween? shakeTween;
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
		moveTween.SetTrans(Tween.TransitionType.Cubic);
		moveTween.SetEase(Tween.EaseType.Out);
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
		shakeTween.SetTrans(Tween.TransitionType.Sine);
		shakeTween.SetEase(Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", new Vector2(-ShakeDistance, 0f), ShakeStepDuration);
		shakeTween.SetTrans(Tween.TransitionType.Sine);
		shakeTween.SetEase(Tween.EaseType.InOut);
		shakeTween.TweenProperty(RootContainer, "position", new Vector2(ShakeDistance, 0f), ShakeStepDuration * 2f);
		shakeTween.SetTrans(Tween.TransitionType.Sine);
		shakeTween.SetEase(Tween.EaseType.Out);
		shakeTween.TweenProperty(RootContainer, "position", Vector2.Zero, ShakeStepDuration);
	}
	/// <summary>
	///     根据阵营更新RootContainer的主题变体。
	/// </summary>
	public void SetTeamTheme(bool isEnemy) => RootContainer.ThemeTypeVariation = isEnemy ? new("PanelContainer_Orange") : new StringName("PanelContainer_Blue");
	/// <summary>
	///     设置角色名字。
	/// </summary>
	public void SetCharacterName(string characterName) => NameLabel.Text = characterName;
}
