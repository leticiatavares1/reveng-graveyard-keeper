using System;
using System.Collections.Generic;

[Serializable]
public class UnlockedCustomizationPartData
{
	public CustomizablePartType type;

	public List<string> unlockedIds = new List<string>();

	public UnlockedCustomizationPartData()
	{
	}

	public UnlockedCustomizationPartData(CustomizablePartType type, List<string> unlockedIds)
	{
		this.type = type;
		this.unlockedIds = unlockedIds;
	}
}
