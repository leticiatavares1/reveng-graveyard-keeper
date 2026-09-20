using System;
using UnityEngine;

[Serializable]
[Command]
public class WorldDataCommand : CommandTargeted<WorldData>
{
	public enum Operation : byte
	{
		None,
		AddWgoToScene,
		RemoveWgoFromScene
	}

	[SerializeField]
	private Operation operation;

	[SerializeField]
	private string wgoId;

	[SerializeField]
	private string uniqueId;

	[SerializeField]
	private string gameSceneId;

	[SerializeField]
	private Vector3 position;

	public override void Execute(ulong senderClientId, GameSave gameSave)
	{
		Debug.Log(string.Format("Command: {0} [op:{1}, wgoId:{2}, uID:{3}, pos:{4}, locId:{5}]", "WorldDataCommand", operation, wgoId, uniqueId, position, gameSceneId));
		switch (operation)
		{
		case Operation.AddWgoToScene:
		{
			WgoData wgoData = new WgoData(wgoId, position, gameSceneId);
			if (!LazyNetwork.NetworkManager.IsHost)
			{
				wgoData.UniqueId.Id = uniqueId;
			}
			else
			{
				uniqueId = wgoData.UniqueId.Id;
			}
			gameSave.worldData.AddWgoData(wgoData);
			break;
		}
		case Operation.RemoveWgoFromScene:
			gameSave.worldData.RemoveWgoDataFromGameScene(SGuid.Parse(uniqueId));
			break;
		}
	}

	public void AddWgoDataToGameScene(WgoData data)
	{
		operation = Operation.AddWgoToScene;
		wgoId = data.id;
		uniqueId = data.UniqueId.Id;
		gameSceneId = gameSceneId;
		position = data.Position;
		HandleCommand(this);
	}

	public void RemoveWgoDataFromGameScene(WgoData data)
	{
		operation = Operation.RemoveWgoFromScene;
		wgoId = data.id;
		uniqueId = data.UniqueId.Id;
		gameSceneId = gameSceneId;
		HandleCommand(this);
	}

	public void MoveWgoDataToAnotherGameScene(string wgoId, string locationIdFrom, string locationIdTo)
	{
		throw new NotImplementedException();
	}

	public void ReplaceWgoDataGameScene(WgoData wgoData, string locationIdFrom, string locationIdTo)
	{
		throw new NotImplementedException();
	}

	public WorldDataCommand(WorldData target)
		: base(target)
	{
	}

	public WorldDataCommand()
	{
	}
}
