using UnityEngine;

public class LadderEdgePart : MonoBehaviour
{
	[SerializeField]
	private Part part;

	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private Transform tpPoint;

	[SerializeField]
	private Transform startPoint;

	[SerializeField]
	private Transform bubblePointToDisplay;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float readyToLeaveLadderRange = 0.1f;

	public Part Part => part;

	public BoxCollider BoxCollider => boxCollider;

	public Transform TpPoint => tpPoint;

	public Transform StartPoint => startPoint;

	public Transform BubblePointToDisplay => bubblePointToDisplay;

	public Ladder Ladder { get; private set; }

	public bool IsInLeaveRange(Vector3 position)
	{
		return Mathf.Abs(startPoint.position.y - position.y) < readyToLeaveLadderRange;
	}

	private void Awake()
	{
		TryGetComponent<BoxCollider>(out boxCollider);
		Ladder = GetComponentInParent<Ladder>();
	}

	private void OnLeaveLadderRangeChange()
	{
		if ((bool)boxCollider)
		{
			int num = ((part != Part.Top) ? 1 : (-1));
			boxCollider.center = new Vector3(boxCollider.center.x, (float)num * readyToLeaveLadderRange / 2f, boxCollider.center.z);
			boxCollider.size = new Vector3(boxCollider.size.x, readyToLeaveLadderRange, boxCollider.size.z);
		}
	}

	private void OnDrawGizmos()
	{
		Color yellow = Color.yellow;
		Gizmos.color = new Color(0.9f, 0.7f, 0.3f, 0.6f);
		Gizmos.DrawCube(boxCollider.bounds.center, boxCollider.size);
		Gizmos.color = yellow;
		Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.size);
		Gizmos.color = yellow;
		Gizmos.DrawSphere(TpPoint.position, 0.05f);
		Gizmos.color = yellow;
		Gizmos.DrawWireSphere(tpPoint.position, 0.2f);
	}
}
