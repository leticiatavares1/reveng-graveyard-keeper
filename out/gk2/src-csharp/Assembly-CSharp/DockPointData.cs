using System;
using Pathfinding;
using UnityEngine;

[Serializable]
public class DockPointData
{
	public enum Availability
	{
		All,
		OnlyOccupied,
		OnlyNotOccupied
	}

	public enum Filter
	{
		All,
		OnlyZombie,
		OnlyNotZombie
	}

	[Serializable]
	public class Baked
	{
		[SerializeField]
		private Vector3 position;

		[SerializeField]
		private Direction direction;

		[SerializeField]
		private bool isForZombie;

		[SerializeField]
		private bool hideInFighting;

		[SerializeField]
		private DockPointTag dockPointTag;

		[SerializeField]
		private bool disableTargetingForCaretaker;

		[SerializeField]
		private bool dontUseForWorkerPlacement;

		public Vector3 Position => position;

		public Direction Direction => direction;

		public bool IsForZombie => isForZombie;

		public bool HideInFighting => hideInFighting;

		public DockPointTag DockPointTag => dockPointTag;

		public bool DisableTargetingForCaretaker => disableTargetingForCaretaker;

		public bool DontUseForWorkerPlacement => dontUseForWorkerPlacement;

		public Baked()
		{
		}

		public Baked(DockPoint dockPoint)
		{
			WgoPart componentInParent = dockPoint.GetComponentInParent<WgoPart>(includeInactive: true);
			position = componentInParent.transform.InverseTransformPoint(dockPoint.transform.position);
			direction = dockPoint.Direction;
			isForZombie = dockPoint.IsForZombie;
			hideInFighting = dockPoint.HideInFighting;
			dockPointTag = dockPoint.DockPointTag;
			disableTargetingForCaretaker = dockPoint.DisableTargetingForCaretaker;
			dontUseForWorkerPlacement = dockPoint.DontUseForWorkerPlacement;
		}
	}

	[SerializeField]
	private SGuid occupiedBy = SGuid.Empty;

	public Baked BakedData { get; set; }

	public Direction Direction => BakedData.Direction;

	public SGuid OccupiedBy
	{
		get
		{
			return occupiedBy;
		}
		set
		{
			occupiedBy = value;
		}
	}

	public bool IsOccupied => occupiedBy != SGuid.Empty;

	public event Action OnOccupiedStatusChanged;

	public Vector3 GetPosFrom(Vector3 pos)
	{
		return pos + BakedData.Position;
	}

	public void Occupy(SGuid occupantId)
	{
		occupiedBy = occupantId;
		this.OnOccupiedStatusChanged?.Invoke();
	}

	public void UnOccupy()
	{
		occupiedBy = SGuid.Empty;
		this.OnOccupiedStatusChanged?.Invoke();
	}

	public bool IsOccupiedBy(SGuid sGuid)
	{
		if (occupiedBy == sGuid)
		{
			return occupiedBy != SGuid.Empty;
		}
		return false;
	}

	public Vector3 GetDropPos(ItemSize itemSize, float dropOffsetRight, float dropOffsetForward, float playerBackOffset)
	{
		Direction direction = BakedData.Direction;
		Vector3 position = BakedData.Position;
		if (direction == Direction.Up && itemSize == ItemSize.Big)
		{
			position += new Vector3(0f, 0f, 0f - playerBackOffset);
		}
		else
		{
			Vector2 vector = direction switch
			{
				Direction.Right => new Vector2(dropOffsetRight, dropOffsetForward), 
				Direction.Left => new Vector2(0f - dropOffsetRight, playerBackOffset), 
				Direction.Up => new Vector2(playerBackOffset, dropOffsetRight), 
				Direction.Down => new Vector2(dropOffsetForward, dropOffsetRight), 
				_ => Vector2.zero, 
			};
			Direction direction2 = direction.OppositeDir();
			position += direction2.ConvertToVector3() * vector.x + direction2.ClockwiseDir().ConvertToVector3() * vector.y;
		}
		position.y = BakedData.Position.y;
		return position;
	}

	public bool IsOnRecast(Vector3 parentPos, RecastGraph recastGraph)
	{
		if (recastGraph.IsPointOnNavmesh(GetPosFrom(parentPos.XZ())))
		{
			return true;
		}
		return false;
	}
}
