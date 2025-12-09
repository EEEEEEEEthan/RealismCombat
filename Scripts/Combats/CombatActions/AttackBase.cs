using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     攻击基类
/// </summary>
public abstract class AttackBase(Character actor, BodyPart actorBodyPart, Combat combat, double preCastActionPointCost, double postCastActionPointCost)
	: CombatAction(actor, combat, actorBodyPart, preCastActionPointCost, postCastActionPointCost)
{
	public readonly BodyPart actorBodyPart = actorBodyPart;
	public Character? target;
	public ICombatTarget? targetObject;
	public override string Description
	{
		get
		{
			var builder = new StringBuilder();
			builder.AppendLine($"类型:{AttackType switch
			{
				AttackTypeCode.Swing => "挥砍",
				AttackTypeCode.Thrust => "刺击",
				AttackTypeCode.Special => "特殊",
				_ => "未知",
			}}");
			builder.AppendLine(DodgeImpact switch
			{
				>= 0.6 => "容易被闪避",
				>= 0.4 => "中等闪避难度",
				_ => "不易被闪避",
			});
			builder.AppendLine(BlockImpact switch
			{
				>= 0.6 => "容易被格挡",
				>= 0.4 => "中等格挡难度",
				_ => "不易被格挡",
			});
			builder.Append(Narrative);
			return builder.ToString();
		}
	}
	public override bool Visible => actorBodyPart.Available && IsBodyPartUsable(actorBodyPart);
	public abstract CombatActionCode Id { get; }
	public virtual IEnumerable<Character> AvailableTargets => GetOpponents().Where(c => c.IsAlive);
	public virtual IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects
	{
		get
		{
			var target = this.target;
			if (target == null) return [];
			return target.AvailableCombatTargets.Select(t => (t, !t.Available));
		}
	}
	public abstract string Narrative { get; }
	public BodyPart ActorBodyPart => actorBodyPart;
	public Damage Damage
	{
		get
		{
			var attackType = AttackType;
			if (UsesWeapon)
				foreach (var slot in ActorBodyPart.Slots)
				{
					var weapon = slot.Item;
					if (weapon != null && (weapon.flag & ItemFlagCode.Arm) != 0) return weapon.DamageProfile.Get(attackType).Scale(DamageMultiplier);
				}
			var baseDamage = attackType switch
			{
				AttackTypeCode.Special => new(0f, 0f, 1f),
				_ => Damage.Zero,
			};
			return baseDamage.Scale(DamageMultiplier);
		}
	}
	public abstract double DodgeImpact { get; }
	public abstract double BlockImpact { get; }
	public abstract AttackTypeCode AttackType { get; }
	public virtual double DamageMultiplier => 1.0;
	public virtual bool UsesWeapon => false;
	public abstract string? PreCastText { get; }
	public abstract string CastText { get; }
	protected abstract bool IsBodyPartUsable(BodyPart bodyPart);
	protected virtual Task OnAttackLanded(Character targetCharacter, ICombatTarget targetObject, GenericDialogue dialogue) =>
		Task.CompletedTask;
	protected override async Task OnStartTask()
	{
		var text = PreCastText;
		if (string.IsNullOrEmpty(text)) return;
		await DialogueManager.ShowGenericDialogue(text);
	}
	protected override async Task OnExecute()
	{
		var target = this.target!;
		var targetObject = this.targetObject!;
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetPKPosition(actor);
		var targetPosition = combat.combatNode.GetPKPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		await DialogueManager.ShowGenericDialogue(CastText);
		var reaction = await combat.HandleIncomingAttack(this);
		var reactionOutcome = ReactionSuccessCalculator.Resolve(reaction, this);
		var hitPosition = combat.combatNode.GetHitPosition(actor);
		actorNode.MoveTo(hitPosition);
		using var _____ = DialogueManager.CreateGenericDialogue(out var dialogue);
		switch (reactionOutcome.Type)
		{
			case ReactionType.Dodge when reactionOutcome.Succeeded:
			{
				await Task.Delay(10);
				targetNode.MoveTo(combat.combatNode.GetDogePosition(target));
				await dialogue.ShowTextTask($"{target.name}闪避成功");
				actorNode.MoveTo(actorPosition);
				goto END;
			}
			case ReactionType.Dodge:
			{
				await dialogue.ShowTextTask($"{target.name}尝试闪避但失败");
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				actorNode.MoveTo(actorPosition);
				goto FALLBACK;
			}
			case ReactionType.Block when reactionOutcome is { Succeeded: true, }:
			{
				await Task.Delay(50);
				targetNode.MoveTo(targetPosition + Vector2.Up * 12);
				targetNode.FlashFrame();
				await Task.Delay(100);
				targetNode.MoveTo(targetPosition);
				AudioManager.PlaySfx(ResourceTable.blockSound, 6f);
				await dialogue.ShowTextTask($"{target.name}使用{reaction.BlockTarget!.Name}格挡成功");
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
				actorNode.MoveTo(actorPosition);
				// todo: 结算格挡部位伤害
				goto END;
			}
			case ReactionType.Block:
			{
				await dialogue.ShowTextTask($"{target.name}尝试格挡但失败");
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				actorNode.MoveTo(actorPosition);
				goto FALLBACK;
			}
			case ReactionType.None:
			{
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				actorNode.MoveTo(actorPosition);
				goto FALLBACK;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}
	FALLBACK:
		// todo: 结算命中部位伤害
		await performHit(targetObject, dialogue);
		await OnAttackLanded(target, targetObject, dialogue);
	END:
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - postCastActionPointCost);
		return;
		async Task performHit(ICombatTarget target, GenericDialogue dialogue)
		{
			if (target is BodyPart bodyPart)
			{
				if (bodyPart.TryGetItem(ItemFlagCode.Armor, out var armor))
				{
					if (GD.Randf() < armor.Coverage)
					{
						// 砍伤能对护甲造成伤害
						var damageToArmor = (int)Mathf.Max(Damage.Slash - armor.Protection.Slash, 0);
						if (damageToArmor > 0)
						{
							await dialogue.ShowTextTask($"攻击打在{armor.Name}上，{armor.Name}受到了{damageToArmor}点伤害");
							armor.HitPoint.value -= damageToArmor;
							if (armor.HitPoint.value <= 0)
							{
								await dialogue.ShowTextTask($"{target.Name}上的{armor.Name}坏了！");
								bodyPart.RemoveItem(armor);
							}
						}
						// 计算实际对身体部位的伤害
						var damageToBody = Damage - armor.Protection;
						var totalDamage = (int)damageToBody.Total;
						if (totalDamage > 0)
						{
							await dialogue.ShowTextTask($"{target.Name}受到了{totalDamage}点伤害");
							bodyPart.HitPoint.value -= totalDamage;
						}
						// todo: 砍伤能造成流血
					}
					else
					{
						var damage = (int)Damage.Total;
						if (damage > 0)
						{
							await dialogue.ShowTextTask("击中了护甲的缝隙!");
							await dialogue.ShowTextTask($"{target.Name}受到了{damage}点伤害");
							bodyPart.HitPoint.value -= damage;
						}
					}
				}
				else
				{
					var damage = (int)Damage.Total;
					if (damage > 0)
					{
						await dialogue.ShowTextTask($"{target.Name}受到了{damage}点伤害");
						bodyPart.HitPoint.value -= damage;
					}
				}
			}
			else
			{
				await dialogue.ShowTextTask($"{target.Name}受到了攻击但是这种情况还没做");
			}
		}
	}
	IEnumerable<Character> GetOpponents() => combat.Allies.Contains(actor) ? combat.Enemies : combat.Allies;
}
