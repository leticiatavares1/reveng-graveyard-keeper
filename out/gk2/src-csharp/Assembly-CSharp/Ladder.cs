using UnityEngine;

public class Ladder : MonoBehaviour
{
	[SerializeField]
	private LadderEdgePart topPart;

	[SerializeField]
	private LadderEdgePart botPart;

	public LadderEdgePart TopPart => topPart;

	public LadderEdgePart BotPart => botPart;

	public LadderEdgePart GetNearestLadderPart(Vector3 position)
	{
		float magnitude = (position - TopPart.StartPoint.position).magnitude;
		if (!((position - BotPart.StartPoint.position).magnitude < magnitude))
		{
			return TopPart;
		}
		return BotPart;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		if ((bool)topPart?.StartPoint && (bool)botPart?.StartPoint)
		{
			Gizmos.DrawLine(topPart.StartPoint.position, botPart.StartPoint.position);
		}
	}
}
