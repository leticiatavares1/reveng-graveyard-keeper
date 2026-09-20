using System;

[Serializable]
public sealed class BodyZombieStartItemsSerializedItemProperty : SerializedItemProperty
{
	public string armorId;

	public string handsId;

	public override SerializedItemProperty Clone()
	{
		return new BodyZombieStartItemsSerializedItemProperty
		{
			armorId = armorId,
			handsId = handsId
		};
	}
}
