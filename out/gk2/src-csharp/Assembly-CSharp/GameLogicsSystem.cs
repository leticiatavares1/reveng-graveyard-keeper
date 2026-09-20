public class GameLogicsSystem : ICustomUpdatable
{
	private GameLogicsSystemData Data => MainGame.Instance.GameSave.gameLogicSystemData;

	public void CustomUpdate(float deltaTime)
	{
		foreach (GameLogicData gameLogic in Data.gameLogics)
		{
			EnvironmentData environmentData = MainGame.Instance.GameSave.environmentData;
			if (!(gameLogic is CustomGameLogicData))
			{
				GameLogicDef definition = gameLogic.Definition;
				if (definition == null || definition.gameLogicStartType != GameLogicStartType.Period)
				{
					goto IL_007d;
				}
			}
			if (environmentData.Day > gameLogic.execDay || (environmentData.Day == gameLogic.execDay && environmentData.TimeOfDay >= gameLogic.execTime))
			{
				gameLogic.TryExecute();
			}
			goto IL_007d;
			IL_007d:
			GameLogicDef definition2 = gameLogic.Definition;
			if (definition2 != null && definition2.gameLogicStartType == GameLogicStartType.Day && environmentData.CurrentDayNumber == ConstDef.Get(gameLogic.Definition.dayNumber).IntValue && environmentData.TimeOfDay >= gameLogic.Definition.dayTime && environmentData.Day > gameLogic.lastExecDay)
			{
				gameLogic.lastExecDay = environmentData.Day;
				gameLogic.TryExecute();
			}
		}
	}
}
