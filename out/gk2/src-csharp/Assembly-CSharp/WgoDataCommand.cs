using System;
using UnityEngine;

[Serializable]
[Command]
public class WgoDataCommand : CommandTargeted<WgoData>
{
	public enum Operation
	{
		None,
		ApplyTool
	}

	[SerializeField]
	private Operation operation;

	[SerializeField]
	private ulong clientTriggerId;

	[SerializeField]
	private string wgoUniqueId;

	[SerializeField]
	private float tickRate;

	[SerializeField]
	private string toolInUseGuid;

	public override void Execute(ulong senderClientId, GameSave gameSave)
	{
		Debug.Log(string.Format("Command: {0} [op:{1}, who:{2}, wgoId:{3}, tickRate:{4}, toolInUseGUID:{5}]", "WgoDataCommand", operation, clientTriggerId, wgoUniqueId, tickRate, toolInUseGuid));
		if (operation == Operation.ApplyTool)
		{
			MainGame.Instance.GameSave.GetClient((int)clientTriggerId, out var clientPlayer, considerHost: true);
			clientPlayer.playerData.toolBeltInventory.GetItemByUniqueId(toolInUseGuid);
		}
	}

	public void ApplyTool(float tickRate, Item tool)
	{
		operation = Operation.ApplyTool;
		wgoUniqueId = target.UniqueId.Id;
		this.tickRate = tickRate;
		toolInUseGuid = tool.UniqueId.ToString();
		HandleCommand(this);
	}

	public WgoDataCommand()
	{
	}

	public WgoDataCommand(WgoData target)
		: base(target)
	{
	}
}
