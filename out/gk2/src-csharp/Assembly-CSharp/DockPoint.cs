using Pathfinding;
using UnityEngine;

[ExecuteAlways]
public class DockPoint : MonoBehaviour
{
	[SerializeField]
	public Direction direction;

	[SerializeField]
	private bool isForZombie;

	[SerializeField]
	private bool hideInFighting;

	[SerializeField]
	private DockPointTag dockPointTag;

	[SerializeField]
	private bool applyRotationToDirection;

	[SerializeField]
	private bool disableTargetingForCaretaker;

	[SerializeField]
	private bool dontUseForWorkerPlacement;

	[SerializeField]
	private WgoPart owner;

	public Wgo Owner => owner?.Wgo;

	public bool IsForZombie => isForZombie;

	public bool HideInFighting => hideInFighting;

	public DockPointTag DockPointTag => dockPointTag;

	public bool DisableTargetingForCaretaker => disableTargetingForCaretaker;

	public bool DontUseForWorkerPlacement => dontUseForWorkerPlacement;

	public Direction Direction
	{
		get
		{
			Direction direction = this.direction;
			if (base.transform.lossyScale.x < 0f && (direction == Direction.Left || direction == Direction.Right))
			{
				direction = direction.OppositeDir();
			}
			if (applyRotationToDirection)
			{
				direction = (Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f) * direction.ConvertToVector3()).ConvertFromVector3();
			}
			return direction;
		}
	}

	public void Init(WgoPart owner)
	{
		this.owner = owner;
	}

	public bool IsReachable(float playerRadius)
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, playerRadius, 0);
		foreach (Collider2D collider2D in array)
		{
			if (!collider2D.isTrigger)
			{
				Wgo wgo = collider2D.GetComponent<Wgo>() ?? collider2D.GetComponentInParent<Wgo>();
				if (wgo == null || wgo.Data.UniqueId != owner.Wgo.Data.UniqueId)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsReachable(RecastGraph recastGraph)
	{
		if (recastGraph.IsPointOnNavmesh(base.transform.position.XZ()))
		{
			return true;
		}
		return false;
	}

	public bool IsDockReached(Vector2 otherObjPosition)
	{
		return ((Vector2)base.gameObject.transform.position - otherObjPosition).magnitude <= 0.001f;
	}
}
