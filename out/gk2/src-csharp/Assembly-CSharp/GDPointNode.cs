using Pathfinding;

public class GDPointNode : PointNode
{
	public GDPointData GdPointData { get; }

	public GDPointNode(AstarPath aStarPath, GDPointData gdPointData)
		: base(aStarPath)
	{
		GdPointData = gdPointData;
	}
}
