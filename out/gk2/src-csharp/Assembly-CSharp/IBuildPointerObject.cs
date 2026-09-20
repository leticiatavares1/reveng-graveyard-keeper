using UnityEngine;

public interface IBuildPointerObject
{
	bool HasRotation();

	void Rotate();

	bool TryDoBuildAction();

	void SetupSelectionCells(BuildSelectionCell[] prefabCells, Transform parent);

	void UpdateSelectionCellsState();

	void UpdatePosition(Vector3 position);

	void ClearSelectionCells();
}
