public class InteractionInfo
{
	public string text;

	public ItemType equippedItemType;

	public bool isItemEquipped = true;

	public TalentDef assignedTalent;

	public int masteryLock;

	public bool isEnoughMastery = true;

	public string customIconId;

	public InteractionInfo()
	{
	}

	public InteractionInfo(string text)
	{
		this.text = text;
	}

	public InteractionInfo(string text, string customIconId)
		: this(text)
	{
		this.customIconId = customIconId;
	}

	public InteractionInfo(string text, ItemType equippedItemType, bool isItemEquipped, TalentDef assignedTalent, int masteryLock, bool isEnoughMastery)
	{
		this.text = text;
		this.equippedItemType = equippedItemType;
		this.isItemEquipped = isItemEquipped;
		this.assignedTalent = assignedTalent;
		this.masteryLock = masteryLock;
		this.isEnoughMastery = isEnoughMastery;
	}

	public InteractionInfo(string text, string customIconId, ItemType equippedItemType, bool isItemEquipped, TalentDef assignedTalent, int masteryLock, bool isEnoughMastery)
	{
		this.text = text;
		this.equippedItemType = equippedItemType;
		this.isItemEquipped = isItemEquipped;
		this.assignedTalent = assignedTalent;
		this.masteryLock = masteryLock;
		this.isEnoughMastery = isEnoughMastery;
		this.customIconId = customIconId;
	}
}
