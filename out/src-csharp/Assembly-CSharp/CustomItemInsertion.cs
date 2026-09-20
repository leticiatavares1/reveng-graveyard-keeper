using System;

[Serializable]
public class CustomItemInsertion
{
	public enum InsertionType
	{
		None,
		OnUse
	}

	public string item_id;

	public InsertionType insertion_type;
}
