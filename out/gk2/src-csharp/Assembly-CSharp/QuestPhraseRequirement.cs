using System;
using LazyBearTechnology;

[Serializable]
public class QuestPhraseRequirement
{
	public enum Requirement
	{
		Price,
		Lock,
		Day,
		Order
	}

	public enum Entity
	{
		Item,
		GameResAtom,
		Day,
		Order
	}

	public Requirement requirement;

	public Entity entity;

	public ItemCount itemCount;

	public GameResAtom gameResAtom;

	public string dayNumber;

	public string order;
}
