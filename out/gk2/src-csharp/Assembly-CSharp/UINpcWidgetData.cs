using LazyBearTechnology;

public class UINpcWidgetData : LazyWidgetDataBase
{
	public string NpcId { get; set; }

	public WGODef WgoDef => GameBalance.Me.GetData<WGODef>(NpcId);
}
