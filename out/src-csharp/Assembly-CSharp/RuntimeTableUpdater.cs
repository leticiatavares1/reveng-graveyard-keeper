using UnityEngine;

public class RuntimeTableUpdater : MonoBehaviour
{
	private void OnDisable()
	{
		UpdateTable();
	}

	private void UpdateTable()
	{
		UITable[] componentsInParent = GetComponentsInParent<UITable>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].Reposition();
		}
		SimpleUITable[] componentsInParent2 = GetComponentsInParent<SimpleUITable>();
		for (int i = 0; i < componentsInParent2.Length; i++)
		{
			componentsInParent2[i].Reposition();
		}
		UIGrid[] componentsInParent3 = GetComponentsInParent<UIGrid>();
		for (int i = 0; i < componentsInParent3.Length; i++)
		{
			componentsInParent3[i].Reposition();
		}
	}
}
