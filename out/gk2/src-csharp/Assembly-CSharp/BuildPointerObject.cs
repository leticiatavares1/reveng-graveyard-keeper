using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public abstract class BuildPointerObject : MonoBehaviour, IBuildPointerObject
{
	protected const float CELL_Y_LOCAL_OFFSET = 0.01f;

	protected BuildData buildData;

	protected List<BuildSelectionCell> cells = new List<BuildSelectionCell>();

	protected BuildSelectionCell[,] cellsGrid;

	protected byte[,] cellsGridMask;

	protected bool shownAsActive;

	protected string worldZoneId;

	protected Bounds roundedBounds;

	private HashSet<BuildSelectionCell> previouslyActiveCells = new HashSet<BuildSelectionCell>();

	protected HashSet<PreSetModuleBuildView> PreSetModuleBuildViews => LazySingleton<BuildManager>.Instance.BuildController.BuildLayout.BuildGrid3D.PreSetModuleBuildViews;

	protected Bounds WorldRoundedBounds => new Bounds(roundedBounds.center + base.transform.position, roundedBounds.size);

	public BuildData BuildData => buildData;

	public Bounds GetWorldRoundedBounds()
	{
		return WorldRoundedBounds;
	}

	public void Init(BuildData buildData, string worldZoneId)
	{
		this.buildData = buildData;
		this.worldZoneId = worldZoneId;
	}

	public virtual bool HasRotation()
	{
		return false;
	}

	public virtual void Rotate()
	{
	}

	public abstract bool TryDoBuildAction();

	public abstract void SetupSelectionCells(BuildSelectionCell[] prefabCells, Transform parent);

	public virtual void ApplySelectionCellsVisuals()
	{
	}

	public virtual void UpdateSelectionCellsState()
	{
		UpdateCellStatus();
	}

	public virtual void ShowHints()
	{
	}

	public virtual void UpdatePosition(Vector3 position)
	{
	}

	public virtual void ClearSelectionCells()
	{
		for (int i = 0; i < cells.Count; i++)
		{
			Object.Destroy(cells[i].gameObject);
		}
		cells.Clear();
	}

	public Vector3 GetObjectCenterLocal()
	{
		Bounds bounds = default(Bounds);
		foreach (BuildSelectionCell cell in cells)
		{
			bounds.Encapsulate(cell.SpriteBounds);
		}
		return base.transform.InverseTransformDirection(bounds.center);
	}

	public virtual Vector3 GetCellsCenterLocal()
	{
		if (cells == null || cells.Count == 0)
		{
			return Vector3.zero;
		}
		Bounds bounds = new Bounds(cells[0].transform.localPosition, Vector3.zero);
		for (int i = 1; i < cells.Count; i++)
		{
			BuildSelectionCell buildSelectionCell = cells[i];
			if (!(buildSelectionCell == null))
			{
				bounds.Encapsulate(buildSelectionCell.transform.localPosition);
			}
		}
		return bounds.center;
	}

	public virtual void OnPointerDisable()
	{
	}

	public virtual void SetVisibleSelectionCells(bool isVisible)
	{
		if (!isVisible)
		{
			previouslyActiveCells.Clear();
		}
		foreach (BuildSelectionCell cell in cells)
		{
			if (!(cell is BuffCell))
			{
				if (!isVisible && cell.gameObject.activeSelf)
				{
					previouslyActiveCells.Add(cell);
				}
				if (!isVisible || previouslyActiveCells.Contains(cell))
				{
					cell.gameObject.SetActive(isVisible);
				}
			}
		}
		if (isVisible)
		{
			previouslyActiveCells.Clear();
		}
	}

	protected void UpdateCellStatus()
	{
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].IsAvailableForBuild = shownAsActive;
		}
	}

	public virtual void UpdateCollider()
	{
	}
}
