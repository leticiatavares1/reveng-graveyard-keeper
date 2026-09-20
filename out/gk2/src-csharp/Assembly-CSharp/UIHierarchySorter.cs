using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class UIHierarchySorter : MonoBehaviour
{
	[SerializeField]
	private bool editorOnly;

	[SerializeField]
	private RectTransform hierarchyTarget;

	private List<UISortComponent> sortComponents;

	public RectTransform HierarchyTarget => hierarchyTarget;

	public void ReinitChildrenAndSortComponents()
	{
		sortComponents = HierarchyTarget.GetComponentsInChildren<UISortComponent>(includeInactive: true).ToList();
		Sort();
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			Sort();
		}
	}

	public void Sort()
	{
		if (sortComponents == null || sortComponents.Count == 0)
		{
			return;
		}
		if (hierarchyTarget == null)
		{
			hierarchyTarget = base.transform as RectTransform;
		}
		sortComponents.Sort(delegate(UISortComponent a, UISortComponent b)
		{
			float value = a.transform.position.y + a.FloorLine;
			return (b.transform.position.y + b.FloorLine).CompareTo(value);
		});
		for (int i = 0; i < sortComponents.Count; i++)
		{
			if (hierarchyTarget != null)
			{
				sortComponents[i].transform.SetParent(hierarchyTarget.transform);
			}
			sortComponents[i].transform.SetSiblingIndex(i);
		}
	}
}
