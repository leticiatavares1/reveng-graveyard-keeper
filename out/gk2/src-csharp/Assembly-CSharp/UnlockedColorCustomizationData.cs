using System;
using System.Collections.Generic;

[Serializable]
public class UnlockedColorCustomizationData
{
	public PlayerColorCustomizationType type;

	public int partSkinId;

	public List<string> unlockedNames = new List<string>();

	public List<int> unlockedIndices = new List<int>();

	public UnlockedColorCustomizationData()
	{
	}

	public UnlockedColorCustomizationData(PlayerColorCustomizationType type, int partSkinId, List<string> unlockedNames)
	{
		this.type = type;
		this.partSkinId = partSkinId;
		this.unlockedNames = unlockedNames;
	}
}
