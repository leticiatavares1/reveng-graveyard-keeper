using System.Collections.Generic;
using UnityEngine;

public class GroundTilesLayout : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	[Space]
	[SerializeField]
	private int xTiles = 1;

	[SerializeField]
	private int nxTiles;

	[SerializeField]
	private int zTiles = 1;

	[SerializeField]
	private int nzTiles;

	[SerializeField]
	[HideInInspector]
	private List<GameObject> createdList = new List<GameObject>();

	private void Redraw()
	{
		meshFilter.gameObject.SetActive(value: false);
		for (int i = 0; i < createdList.Count; i++)
		{
			if (createdList[i] == null)
			{
				createdList.RemoveAt(i);
				i--;
			}
			else
			{
				createdList[i].SetActive(value: false);
			}
		}
		Bounds bounds = meshFilter.GetComponent<Renderer>().bounds;
		float x = bounds.size.x;
		float z = bounds.size.z;
		int num = 0;
		int count = createdList.Count;
		for (int j = nzTiles; j < zTiles; j++)
		{
			for (int k = nxTiles; k < xTiles; k++)
			{
				float x2 = (float)k * x;
				float z2 = (float)j * z;
				Vector3 position = base.transform.position + new Vector3(x2, 0f, z2);
				GameObject gameObject;
				if (num < count)
				{
					gameObject = createdList[num];
					gameObject.transform.position = position;
				}
				else
				{
					gameObject = Object.Instantiate(meshFilter.gameObject, position, base.transform.rotation);
					createdList.Add(gameObject);
				}
				gameObject.transform.parent = base.transform;
				gameObject.SetActive(value: true);
				num++;
			}
		}
	}
}
