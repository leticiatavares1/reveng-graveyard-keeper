using System;

[Serializable]
public class ParishionerData
{
	public SermonDef sermonDef;

	public readonly int faithToDrop;

	public ParishionerData(SermonDef sermonDef, int faith)
	{
		this.sermonDef = sermonDef;
		faithToDrop = faith;
	}
}
