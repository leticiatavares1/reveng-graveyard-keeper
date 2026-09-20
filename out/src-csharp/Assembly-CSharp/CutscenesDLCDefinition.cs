using System;

[Serializable]
public class CutscenesDLCDefinition : BalanceBaseObject
{
	public string item;

	public string flow_script;

	public string GetIconName()
	{
		return GameBalance.me.GetData<ItemDefinition>(item).GetIcon();
	}
}
