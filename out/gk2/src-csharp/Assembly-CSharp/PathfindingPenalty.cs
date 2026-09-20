using System;
using Pathfinding;

[Serializable]
public class PathfindingPenalty
{
	public PathfindingTag tag;

	public bool isTraversable = true;

	public uint penalty;
}
