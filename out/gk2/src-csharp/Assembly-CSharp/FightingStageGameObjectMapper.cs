using System;
using System.Collections.Generic;
using UnityEngine;

public class FightingStageGameObjectMapper : MonoBehaviour
{
	[Serializable]
	public class FightingStageGameObjectMapperData
	{
		public string id;

		public GameObject go;
	}

	[SerializeField]
	private List<FightingStageGameObjectMapperData> data = new List<FightingStageGameObjectMapperData>();

	public bool TryGetGameObject(string id, out GameObject go)
	{
		go = null;
		if (string.IsNullOrEmpty(id) || data == null)
		{
			return false;
		}
		for (int i = 0; i < data.Count; i++)
		{
			FightingStageGameObjectMapperData fightingStageGameObjectMapperData = data[i];
			if (fightingStageGameObjectMapperData != null && fightingStageGameObjectMapperData.id == id)
			{
				go = fightingStageGameObjectMapperData.go;
				return go != null;
			}
		}
		return false;
	}
}
