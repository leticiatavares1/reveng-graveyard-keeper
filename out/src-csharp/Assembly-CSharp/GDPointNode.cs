using Pathfinding;

public class GDPointNode : PointNode
{
	public GDPoint gd_point;

	public GDPointNode(AstarPath astar, GDPoint gd_point)
		: base(astar)
	{
		this.gd_point = gd_point;
	}

	public void LinkAdjacentNode(GDPointNode n, uint cost = 1u)
	{
		AddConnection(n, cost);
	}
}
