namespace RealismCombat.Commands;
public class CheckStatusCommand(ProgramRoot programRoot) : Command(programRoot)
{
	public const string name = "game_check_status";
	public override void Execute()
	{
		Log.Print(programRoot.State.Status);
		programRoot.McpCheckPoint();
	}
}
