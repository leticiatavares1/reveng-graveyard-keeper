using UnityEngine;

public class RandomSiblingOrder : MonoBehaviour
{
	[SerializeField]
	private Transform parent;

	public void RandomizeSiblingOrder()
	{
		if (parent == null)
		{
			Debug.LogWarning("parent was not specified. Using self.");
			parent = base.transform;
		}
		foreach (Transform item in parent)
		{
			int siblingIndex = Random.Range(0, 1000);
			item.SetSiblingIndex(siblingIndex);
		}
	}
}
