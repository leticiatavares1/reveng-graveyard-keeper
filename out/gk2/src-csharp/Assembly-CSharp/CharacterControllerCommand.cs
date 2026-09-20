using System;
using UnityEngine;

[Serializable]
[Command]
public class CharacterControllerCommand : Command
{
	public enum Operation : byte
	{
		None,
		UseTool
	}

	[SerializeField]
	public Operation operation;

	[SerializeField]
	public string itemGuid;

	[SerializeField]
	public float tickRate;

	public override void Execute(ulong senderClientId, GameSave gameSave)
	{
		Debug.Log(string.Format("Command: {0} [op:{1}, itemGUID:{2}]", "CharacterControllerCommand", operation, itemGuid));
		if (operation == Operation.UseTool)
		{
			MainGame.PlayerController.PlayerData.toolBeltInventory.GetItemByUniqueId(itemGuid);
		}
	}

	public void UseTool(float tickRate, Item tool)
	{
		operation = Operation.UseTool;
		itemGuid = tool.UniqueId.ToString();
		this.tickRate = tickRate;
	}
}
