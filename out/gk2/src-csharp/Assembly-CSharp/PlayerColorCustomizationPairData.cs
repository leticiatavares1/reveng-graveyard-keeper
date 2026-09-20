using System;

[Serializable]
public class PlayerColorCustomizationPairData
{
	public int index;

	public PlayerColorCustomizationType type;

	public PlayerColorCustomizationPairData()
	{
	}

	public PlayerColorCustomizationPairData(int index, PlayerColorCustomizationType type)
	{
		this.index = index;
		this.type = type;
	}
}
