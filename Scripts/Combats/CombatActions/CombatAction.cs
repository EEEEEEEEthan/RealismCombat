using System.Threading.Tasks;
using RealismCombat.Characters;
namespace RealismCombat.Combats.CombatActions;
public abstract class CombatAction(Character actor, Combat combat, double preCastActionPointCost, double postCastActionPointCost)
{
	protected readonly Character actor = actor;
	protected readonly Combat combat = combat;
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