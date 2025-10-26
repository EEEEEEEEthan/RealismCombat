using RealismCombat.StateMachine;
namespace RealismCombat.Commands;
class StartNewGameCommand(ProgramRoot root) : Command(root)
{
	public const string name = "program_start_new_game";
	public override void Execute() => _ = new GameState(root);
}
