using System.Threading.Tasks;
public abstract class CombatAction(Character actor, Combat combat, ICombatTarget actorObject, double preCastActionPointCost, double postCastActionPointCost)
{
	public readonly Character actor = actor;
	public readonly Combat combat = combat;
	public readonly ICombatTarget actorObject = actorObject;
	public readonly double preCastActionPointCost = preCastActionPointCost;
	public readonly double postCastActionPointCost = postCastActionPointCost;
	public abstract string Description { get; }
	/// <summary>
	///     是否在界面上显示该行动
	/// </summary>
	public virtual bool Visible => true;
	/// <summary>
	///     true: 界面上禁用该行动，false: 可用
	/// </summary>
	public virtual bool Disabled => false;
	/// <summary>
	///     由Buff导致的禁用
	/// </summary>
	public virtual bool DisabledByBuff =>
		actorObject is BodyPart bodyPart &&
		((bodyPart.HasBuff(BuffCode.Restrained, true) && this is not BreakFreeAction) ||
		(bodyPart.HasBuff(BuffCode.Grappling, true) && this is not ReleaseAction));
	public bool CanUse => Visible && !Disabled && !DisabledByBuff;
	public async Task StartTask()
	{
		actor.actionPoint.value -= preCastActionPointCost;
		await OnStartTask();
	}
	public async Task<bool> UpdateTask()
	{
		if (actor.actionPoint.value >= actor.actionPoint.maxValue)
		{
			actor.actionPoint.value -= postCastActionPointCost;
			await OnExecute();
			return false;
		}
		return true;
	}
	protected abstract Task OnStartTask();
	protected abstract Task OnExecute();
}
