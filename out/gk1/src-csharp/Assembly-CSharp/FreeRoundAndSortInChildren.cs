using UnityEngine;

[ExecuteInEditMode]
public class FreeRoundAndSortInChildren : MonoBehaviour
{
	private void Awake()
	{
	}

	public void FreeRoundAndSort()
	{
		RoundAndSortComponent[] componentsInChildren = GetComponentsInChildren<RoundAndSortComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].never_disable = true;
		}
	}
}
