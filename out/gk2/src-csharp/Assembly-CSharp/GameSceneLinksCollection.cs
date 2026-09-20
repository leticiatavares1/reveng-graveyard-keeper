using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GameSceneLinksCollection : MonoBehaviour
{
	[SerializeField]
	private List<NodeLink2> links = new List<NodeLink2>();

	public void RefreshLinks()
	{
		foreach (NodeLink2 link in links)
		{
			if (!((Object)(object)link == null))
			{
				link.Apply();
			}
		}
	}
}
