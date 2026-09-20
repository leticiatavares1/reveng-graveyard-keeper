using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GOsList
{
	[SerializeField]
	public List<GameObject> list;

	public GOsList()
	{
		list = new List<GameObject>();
	}

	public GOsList(List<GameObject> t_list)
	{
		list = ((t_list == null) ? new List<GameObject>() : t_list);
	}
}
