using System.Collections.Generic;
using LazyBearTechnology;

public class BuildingHUDData : LazyWidgetDataBase
{
	private string buildHintText;

	private string rotationHintText;

	private string exitHintText;

	private IReadOnlyList<DockPoint> dockPoints = new List<DockPoint>();

	public bool isTargetCanBeRotated;

	public IReadOnlyList<DockPoint> DockPoints => dockPoints;

	public BuildingHUDData(IReadOnlyList<DockPoint> targetDockPoints = null, bool hasRotation = false)
	{
		isTargetCanBeRotated = hasRotation;
		if (targetDockPoints != null && targetDockPoints.Count > 0)
		{
			dockPoints = targetDockPoints;
		}
	}
}
