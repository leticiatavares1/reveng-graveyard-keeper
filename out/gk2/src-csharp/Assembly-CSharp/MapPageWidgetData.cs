using LazyBearTechnology;

public class MapPageWidgetData : LazyWidgetDataBase
{
	public PlayerData PlayerData { get; set; }

	public bool MilestonesInteractable { get; private set; }

	public string CurrentMilestone { get; private set; }

	public MapPageWidgetData(GameSave gameSave, bool milestonesInteractable, string currentMilestone)
	{
		PlayerData = gameSave.playerData;
		MilestonesInteractable = milestonesInteractable;
		CurrentMilestone = currentMilestone;
	}
}
