using System;
using UnityEngine;

[Serializable]
public class FightingLevelData
{
	public string id;

	[SerializeField]
	private int curStageId;

	public int CurStageId
	{
		get
		{
			return curStageId;
		}
		set
		{
			curStageId = value;
			FightingLevelData.OnFightingLevelDataChanged?.Invoke(this);
		}
	}

	public static event Action<FightingLevelData> OnFightingLevelDataChanged;

	public FightingLevelData()
	{
	}

	public FightingLevelData(string id)
	{
		this.id = id;
	}
}
