using System.Threading.Tasks;
public abstract class CombatAction(Character actor, Combat combat, ICombatTarget actorObject, double preCastActionPointCost, double postCastActionPointCost)
{
	protected readonly Character actor = actor;
	protected readonly Combat combat = combat;
	public Character Actor => actor;
	public abstract string Description { get; }
	/// <summary>
	/// 是否在界面上显示该行动
	/// </summary>
	public virtual bool Visible => true;
	/// <summary>
	/// true: 界面上禁用该行动，false: 可用
	/// </summary>
	public virtual bool Disabled => false;
	public bool CanUse => Visible && !Disabled;
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
