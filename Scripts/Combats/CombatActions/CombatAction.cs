using System.Collections.Generic;
using System.Threading.Tasks;
public abstract class CombatAction(Character actor, Combat combat, ICombatTarget actorObject, double preCastActionPointCost, double postCastActionPointCost)
{
	protected readonly Character actor = actor;
	protected readonly Combat combat = combat;
	protected readonly ICombatTarget actorObject = actorObject;
	public Character Actor => actor;
	public Combat Combat => combat;
	public ICombatTarget ActorObject => actorObject;
public abstract string Description { get; }
	public virtual bool Available => true;
	public virtual bool Disabled => false;
	public bool CanUse => Available && !Disabled;
	public virtual IEnumerable<Character> AvailableTargets => [];
	public virtual Character? Target { get; set; }
	public virtual IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects => [];
	public virtual ICombatTarget? TargetObject { get; set; }
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