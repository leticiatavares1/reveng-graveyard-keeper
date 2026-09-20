using System.Collections.Generic;
using UnityEngine;

public class IndoorArea : MonoBehaviour
{
	[SerializeField]
	private string id;

	[SerializeField]
	private List<BoxCollider> colliders = new List<BoxCollider>();

	public string Id
	{
		get
		{
			if (!string.IsNullOrEmpty(id))
			{
				return id;
			}
			return base.name;
		}
	}

	public IReadOnlyList<BoxCollider> Colliders => colliders;

	public IndoorAreaData ToData()
	{
		IndoorAreaData indoorAreaData = new IndoorAreaData
		{
			id = Id,
			bounds = new List<IndoorAreaBoundData>()
		};
		if (colliders == null)
		{
			return indoorAreaData;
		}
		for (int i = 0; i < colliders.Count; i++)
		{
			BoxCollider boxCollider = colliders[i];
			if (!(boxCollider == null))
			{
				indoorAreaData.bounds.Add(new IndoorAreaBoundData(boxCollider));
			}
		}
		return indoorAreaData;
	}

	public bool ContainsXZ(Vector3 worldPos)
	{
		if (colliders == null)
		{
			return false;
		}
		for (int i = 0; i < colliders.Count; i++)
		{
			BoxCollider boxCollider = colliders[i];
			if (!(boxCollider == null) && new IndoorAreaBoundData(boxCollider).ContainsXZ(worldPos))
			{
				return true;
			}
		}
		return false;
	}
}
