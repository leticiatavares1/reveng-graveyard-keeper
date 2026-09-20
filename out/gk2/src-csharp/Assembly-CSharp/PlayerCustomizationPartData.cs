using System;

[Serializable]
public class PlayerCustomizationPartData
{
	public string id;

	public CustomizablePartType type;

	public PlayerCustomizationPartData()
	{
	}

	public PlayerCustomizationPartData(string id, CustomizablePartType type)
	{
		this.id = id;
		this.type = type;
	}
}
