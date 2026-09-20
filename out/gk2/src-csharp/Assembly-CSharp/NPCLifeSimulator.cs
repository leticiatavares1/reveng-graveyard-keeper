using System;
using LazyBearTechnology;
using UnityEngine;

public class NPCLifeSimulator : ICustomUpdatable
{
	private int curRandomOffset;

	private NPCLifeSimulatorData Data => MainGame.Instance.GameSave.npcLifeSimulatorData;

	public void CustomUpdate(float deltaTime)
	{
		CheckActionsForWgos(deltaTime);
		CheckAnimationsRollForWgos(deltaTime);
	}

	public void RollActivityForWgo(SGuid wgoUniqueId, NPCGroupPointOfInterestData groupData, float delay)
	{
		WgoData wgoData = MainGame.WorldData.GetWgoData(wgoUniqueId);
		if (wgoData == null)
		{
			Debug.LogError($"RollActivityForWgo WGO data with sguid:[{wgoUniqueId}] not found");
			return;
		}
		Debug.Log("#npc_sim# RollActivityForWgo wgoData:[" + wgoData.id + "]");
		if (wgoData.GetGameRes("npc_life_sim_disabled_flag") > 0f)
		{
			Debug.Log("RollActivityForWgo WGO:[" + wgoData.id + "] ignore disabled npc");
			return;
		}
		if (groupData == null)
		{
			groupData = Data.GetGroupByWGOId(wgoUniqueId);
		}
		if (groupData == null)
		{
			Debug.LogError("RollActivityForWgo WGO:[" + wgoData.id + "] group not found");
			return;
		}
		if (!string.IsNullOrEmpty(wgoData.occupiedPointOfInterest))
		{
			NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
			if (pointById != null)
			{
				ExecuteAnimationWhenLeavedPointForWgo(wgoData);
				pointById.Deoccupy();
			}
		}
		NPCPointOfInterestData nPCPointOfInterestData = Data.RollPoint(groupData.Id);
		if (nPCPointOfInterestData == null)
		{
			Debug.LogError("RollActivityForWgo WGO:[" + wgoData.id + "] no free points");
			return;
		}
		wgoData.occupiedPointOfInterest = nPCPointOfInterestData.Id;
		nPCPointOfInterestData.Occupy(wgoData.UniqueId);
		Debug.Log("#npc_sim# RollActivityForWgo wgoData:[" + wgoData.id + "] AddActionData GoToPointOfInterest:[" + wgoData.occupiedPointOfInterest + "]");
		Data.AddActionData(new NPCLifeSimulatorActionData(wgoUniqueId, delay, NPCLifeSimulatorActionType.GoToPointOfInterest));
	}

	public void RollActivityForGroup(string groupId)
	{
		Debug.Log("#npc_sim# RollActivityForGroup:[" + groupId + "]");
		NPCGroupPointOfInterestData groupById = Data.GetGroupById(groupId);
		if (groupById == null)
		{
			Debug.LogError("RollActivityForGroup no such group:[" + groupId + "]");
			return;
		}
		int num = 0;
		float minActionDelay = LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.MinActionDelay;
		float num2 = (LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.MaxActionDelay - minActionDelay) / (float)groupById.Wgos.Count;
		foreach (SGuid wgo in groupById.Wgos)
		{
			float num3 = minActionDelay + num2 * (float)num;
			float num4 = UnityEngine.Random.Range(0f, num2);
			RollActivityForWgo(wgo, groupById, num3 + num4);
			num++;
		}
	}

	public void RollActivityForAllGroups()
	{
		Debug.Log("#npc_sim# RollActivityForAllGroups");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			RollActivityForGroup(allGroup.Id);
		}
	}

	public void SendWgoHome(SGuid wgoUniqueId, NPCGroupPointOfInterestData groupData, float delay)
	{
		WgoData wgoData = MainGame.WorldData.GetWgoData(wgoUniqueId);
		if (wgoData == null)
		{
			Debug.LogError($"SendGroupHome WGO data with sguid:[{wgoUniqueId}] not found");
			return;
		}
		if (wgoData.GetGameRes("npc_life_sim_disabled_flag") > 0f)
		{
			Debug.Log("SendGroupHome WGO:[" + wgoData.id + "] ignore disabled npc");
			return;
		}
		Debug.Log("#npc_sim# SendWgoHome wgoData:[" + wgoData.id + "]");
		if (groupData == null)
		{
			groupData = Data.GetGroupByWGOId(wgoUniqueId);
		}
		if (groupData == null)
		{
			Debug.LogError("SendWgoHome WGO:[" + wgoData.id + "] group not found");
			return;
		}
		ReleaseOccupiedPointForWgo(wgoData);
		Data.AddActionData(new NPCLifeSimulatorActionData(wgoUniqueId, delay, NPCLifeSimulatorActionType.GoHome));
	}

	public void SendGroupHome(string groupId)
	{
		Debug.Log("#npc_sim# SendGroupHome:[" + groupId + "]");
		NPCGroupPointOfInterestData groupById = Data.GetGroupById(groupId);
		if (groupById == null)
		{
			Debug.LogError("SendGroupHome no such group:[" + groupId + "]");
			return;
		}
		int num = 0;
		float minActionDelay = LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.MinActionDelay;
		float num2 = (LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.MaxActionDelay - minActionDelay) / (float)groupById.Wgos.Count;
		foreach (SGuid wgo in groupById.Wgos)
		{
			float num3 = minActionDelay + num2 * (float)num;
			float num4 = UnityEngine.Random.Range(0f, num2);
			SendWgoHome(wgo, groupById, num3 + num4);
			num++;
		}
	}

	public void SendAllGroupsHome()
	{
		Debug.Log("#npc_sim# SendAllGroupsHome");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			SendGroupHome(allGroup.Id);
		}
	}

	public void ForceAllGroupsTeleportHome()
	{
		Debug.Log("#npc_sim# ForceAllGroupsTeleportHome");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			foreach (SGuid wgo in allGroup.Wgos)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(wgo);
				if (wgoData == null)
				{
					Debug.LogError($"ForceAllGroupsTeleportHome WGO data with sguid:[{wgo}] not found");
					continue;
				}
				string text = wgoData.GameResStr.Get("npc_life_sim_home");
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogError("ForceAllGroupsTeleportHome Can't send wgo:[" + wgoData.id + "] home because home point is empty!!!");
					continue;
				}
				GDPointData gDPointDataById = MainGame.WorldData.gdPointsData.GetGDPointDataById(text);
				if (gDPointDataById == null)
				{
					Debug.LogError("ForceAllGroupsTeleportHome Can't send wgo:[" + wgoData.id + "] home because home gd point is null!!!");
					continue;
				}
				wgoData.MovementComponent.ForceStop();
				wgoData.Position = gDPointDataById.Position;
				ReleaseOccupiedPointForWgo(wgoData);
			}
		}
	}

	public void ForceGroupTeleportHome(string groupId)
	{
		Debug.Log("#npc_sim# ForceGroupTeleportHome for " + groupId + " group");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			if (!(allGroup.Id == groupId))
			{
				continue;
			}
			foreach (SGuid wgo in allGroup.Wgos)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(wgo);
				if (wgoData == null)
				{
					Debug.LogError($"ForceGroupTeleportHome WGO data with sguid:[{wgo}] not found");
					continue;
				}
				string text = wgoData.GameResStr.Get("npc_life_sim_home");
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogError("ForceGroupTeleportHome Can't send wgo:[" + wgoData.id + "] home because home point is empty!!!");
					continue;
				}
				GDPointData gDPointDataById = MainGame.WorldData.gdPointsData.GetGDPointDataById(text);
				if (gDPointDataById == null)
				{
					Debug.LogError("ForceGroupTeleportHome Can't send wgo:[" + wgoData.id + "] home because home gd point is null!!!");
					continue;
				}
				wgoData.MovementComponent.ForceStop();
				wgoData.Position = gDPointDataById.Position;
				ReleaseOccupiedPointForWgo(wgoData);
				break;
			}
		}
	}

	public void OnWgoReachedPointOfInterest(WgoData wgoData)
	{
		Debug.Log("#npc_sim# OnWgoReachedPointOfInterest wgoData:[" + wgoData.id + "]");
		if (Data.GetGroupByWGOId(wgoData.UniqueId) == null)
		{
			return;
		}
		NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
		if (pointById != null)
		{
			GDPointData gDPointData = pointById.GDPointData;
			if (gDPointData != null)
			{
				wgoData.direction.Value = gDPointData.Direction.ConvertToVector2XZ();
			}
			ExecuteAnimationWhenReachedPointForWgo(wgoData);
		}
	}

	public void AddWgoToGroupFromBalance(string wgoId)
	{
		WGODef data = GameBalance.Me.GetData<WGODef>(wgoId);
		if (data == null)
		{
			Debug.LogError("AddWgoToGroupFromBalance WGO def with id:[" + wgoId + "] not found");
			return;
		}
		Debug.Log("#npc_sim# AddWgoToGroup wgoData:[" + wgoId + "]");
		AddWgoToGroup(wgoId, data.npcLifeSimGroup);
	}

	public void AddWgoToGroupFromBalance(WgoData wgoData)
	{
		if (wgoData?.Definition == null)
		{
			Debug.LogError("AddWgoToGroupFromBalance WGO data not found");
			return;
		}
		Debug.Log("#npc_sim# AddWgoToGroup wgoData:[" + wgoData.id + "]");
		AddWgoToGroup(wgoData.id, wgoData.Definition.npcLifeSimGroup);
	}

	public void AddWgoToGroup(string wgoId, string groupId)
	{
		RemoveWgoFromGroup(wgoId);
		NPCGroupPointOfInterestData groupById = Data.GetGroupById(groupId);
		if (groupById == null)
		{
			Debug.LogError("AddWgoToGroup WGO:[" + wgoId + "] group:[" + groupId + "] not found");
			return;
		}
		Debug.Log("#npc_sim# AddWgoToGroup wgoData:[" + wgoId + "] groupId:[" + groupId + "]");
		WgoData wgoData = MainGame.WorldData.GetWgoData(wgoId);
		if (wgoData == null)
		{
			Debug.LogError("AddWgoToGroup WGO data with id:[" + wgoId + "] not found");
		}
		else
		{
			groupById.AddWgoToGroup(wgoData);
		}
	}

	public void AddWgoToGroup(WgoData wgoData, string groupId)
	{
		RemoveWgoFromGroup(wgoData);
		NPCGroupPointOfInterestData groupById = Data.GetGroupById(groupId);
		if (groupById == null)
		{
			Debug.LogError("AddWgoToGroup WGO: group:[" + groupId + "] not found");
			return;
		}
		if (wgoData == null)
		{
			Debug.LogError("AddWgoToGroup WGO data not found");
			return;
		}
		Debug.Log("#npc_sim# AddWgoToGroup wgoData:[" + wgoData.id + "] groupId:[" + groupId + "]");
		groupById.AddWgoToGroup(wgoData);
	}

	public void RemoveWgoFromGroup(string wgoId)
	{
		WgoData wgoData = MainGame.WorldData.GetWgoData(wgoId);
		if (wgoData == null)
		{
			Debug.LogError("RemoveWgoFromGroup WGO data with id:[" + wgoId + "] not found");
			return;
		}
		Debug.Log("#npc_sim# RemoveWgoFromGroup wgoId:[" + wgoId + "]");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			foreach (SGuid wgo in allGroup.Wgos)
			{
				if (wgo.Guid == wgoData.UniqueId.Guid)
				{
					allGroup.RemoveWgoFromGroup(wgoData);
					return;
				}
			}
		}
	}

	public void RemoveWgoFromGroup(WgoData wgoData)
	{
		if (wgoData == null)
		{
			Debug.LogError("RemoveWgoFromGroup WGO data not found");
			return;
		}
		Debug.Log("#npc_sim# RemoveWgoFromGroup wgoId:[" + wgoData.id + "]");
		foreach (NPCGroupPointOfInterestData allGroup in Data.AllGroups)
		{
			foreach (SGuid wgo in allGroup.Wgos)
			{
				if (wgo.Guid == wgoData.UniqueId.Guid)
				{
					allGroup.RemoveWgoFromGroup(wgoData);
					return;
				}
			}
		}
	}

	public void UnlockPointOfInterest(string pointId)
	{
		NPCPointOfInterestData pointById = MainGame.Instance.GameSave.npcLifeSimulatorData.GetPointById(pointId);
		if (pointById == null)
		{
			Debug.LogError("UnlockPointOfInterest : Point " + pointId + " not found");
			return;
		}
		Debug.Log("#npc_sim# UnlockPointOfInterest pointId:[" + pointId + "]");
		pointById.Enabled = true;
	}

	public void LockPointOfInterest(string pointId)
	{
		NPCPointOfInterestData pointById = MainGame.Instance.GameSave.npcLifeSimulatorData.GetPointById(pointId);
		if (pointById == null)
		{
			Debug.LogError("LockPointOfInterest : Point " + pointId + " not found");
			return;
		}
		Debug.Log("#npc_sim# LockPointOfInterest pointId:[" + pointId + "]");
		pointById.Enabled = false;
	}

	private void ReleaseOccupiedPointForWgo(WgoData wgoData)
	{
		if (!string.IsNullOrEmpty(wgoData.occupiedPointOfInterest))
		{
			NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
			if (pointById != null)
			{
				ExecuteAnimationWhenLeavedPointForWgo(wgoData);
				pointById.Deoccupy();
			}
			wgoData.occupiedPointOfInterest = string.Empty;
		}
	}

	private void CheckActionsForWgos(float deltaTime)
	{
		for (int num = Data.ActionsData.Count - 1; num >= 0; num--)
		{
			NPCLifeSimulatorActionData nPCLifeSimulatorActionData = Data.ActionsData[num];
			nPCLifeSimulatorActionData.RemainingTimeToAction -= deltaTime;
			if (!(nPCLifeSimulatorActionData.RemainingTimeToAction <= 0f))
			{
				continue;
			}
			WgoData wgoData = MainGame.WorldData.GetWgoData(nPCLifeSimulatorActionData.WgoId);
			if (wgoData == null)
			{
				Debug.LogError($"CheckActionsForWgos WGO data with sguid:[{nPCLifeSimulatorActionData.WgoId}] not found");
				Data.ActionsData.RemoveAt(num);
				continue;
			}
			switch (nPCLifeSimulatorActionData.ActionType)
			{
			case NPCLifeSimulatorActionType.GoToPointOfInterest:
			{
				NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
				if (pointById == null)
				{
					Debug.LogError("CheckActionsForWgos WGO:[" + wgoData.id + "] can't find point");
					Data.ActionsData.RemoveAt(num);
					continue;
				}
				Debug.Log("#npc_sim# CheckActionsForWgos WGO:[" + wgoData.id + "] GoToPointOfInterest:[" + pointById.Id + "]");
				if (wgoData.MovementComponent.IsMoving)
				{
					wgoData.MovementComponent.ForceStop();
				}
				wgoData.MovementComponent.StartPath(pointById, 1.125f);
				break;
			}
			case NPCLifeSimulatorActionType.GoHome:
			{
				ReleaseOccupiedPointForWgo(wgoData);
				string text = wgoData.GameResStr.Get("npc_life_sim_home");
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogError("CheckActionsForWgos Can't send wgo:[" + wgoData.id + "] home because home point is empty!!!");
					Data.ActionsData.RemoveAt(num);
					continue;
				}
				GDPointData gDPointDataById = MainGame.WorldData.gdPointsData.GetGDPointDataById(text);
				if (gDPointDataById == null)
				{
					Debug.LogError("CheckActionsForWgos Can't send wgo:[" + wgoData.id + "] home because home gd point is null!!!");
					Data.ActionsData.RemoveAt(num);
					continue;
				}
				Debug.Log("#npc_sim# CheckActionsForWgos WGO:[" + wgoData.id + "] GoHome:[" + gDPointDataById.Id + "]");
				if (wgoData.MovementComponent.IsMoving)
				{
					wgoData.MovementComponent.ForceStop();
				}
				wgoData.MovementComponent.StartPath(gDPointDataById.Position, wgoData.WorldId, gDPointDataById.GameSceneDataId, MovementType.GDGraph);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			Data.ActionsData.RemoveAt(num);
		}
	}

	private void CheckAnimationsRollForWgos(float deltaTime)
	{
		for (int num = Data.AnimationDatas.Count - 1; num >= 0; num--)
		{
			NPCPointOfInterestAnimationData nPCPointOfInterestAnimationData = Data.AnimationDatas[num];
			nPCPointOfInterestAnimationData.RemainingTimeToRoll -= deltaTime;
			if (nPCPointOfInterestAnimationData.RemainingTimeToRoll <= 0f)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(nPCPointOfInterestAnimationData.WgoId);
				if (wgoData == null)
				{
					Debug.LogError($"Update rolling animations WGO data with sguid:[{nPCPointOfInterestAnimationData.WgoId}] not found");
					Data.AnimationDatas.RemoveAt(num);
				}
				else
				{
					ExecuteAnimationWhenReachedPointForWgo(wgoData);
				}
			}
		}
	}

	private void ExecuteAnimationWhenReachedPointForWgo(WgoData wgoData)
	{
		if (Data.GetGroupByWGOId(wgoData.UniqueId) == null)
		{
			Debug.LogError("ExecuteAnimationWhenReachedPointForWgo WGO:[" + wgoData.id + "] group not found");
			return;
		}
		NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
		if (pointById == null)
		{
			Debug.LogError("ExecuteAnimationWhenReachedPointForWgo WGO:[" + wgoData.id + "] no current point");
			return;
		}
		NPCPointOfInterestConfiguration configuration = pointById.Configuration;
		if (configuration == null)
		{
			Debug.LogError("ExecuteAnimationWhenReachedPointForWgo WGO:[" + wgoData.id + "] no config for point");
			return;
		}
		switch (configuration.AnimationType)
		{
		case NPCPointOfInterestAnimationType.Roll:
			if (configuration.AnimationsForRoll.Count > 0)
			{
				NPCPointOfInterestAnimationConfiguration random = configuration.AnimationsForRoll.GetRandom();
				Data.AddAnimationData(new NPCPointOfInterestAnimationData(wgoData, random));
				Debug.Log("#npc_sim# ExecuteAnimationWhenReachedPointForWgo wgoData:[" + wgoData.id + "] animationConfiguration.TriggerId:[" + random.TriggerId + "]");
				wgoData.SetTriggerToAnimator(random.TriggerId);
				wgoData.SetCustomAnimationTrigger(random.TriggerId);
			}
			break;
		case NPCPointOfInterestAnimationType.TriggerCustomIdle:
			if (!string.IsNullOrEmpty(configuration.IdleTriggerId))
			{
				Debug.Log("#npc_sim# ExecuteAnimationWhenReachedPointForWgo wgoData:[" + wgoData.id + "] configuration.IdleTriggerId:[" + configuration.IdleTriggerId + "]");
				wgoData.SetTriggerToAnimator(configuration.IdleTriggerId);
				wgoData.SetCustomAnimationTrigger(configuration.IdleTriggerId);
			}
			else
			{
				Debug.LogError("#npc_sim# ExecuteAnimationWhenReachedPointForWgo wgoData:[" + wgoData.id + "] configuration.IdleTriggerId is empty!!!");
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case NPCPointOfInterestAnimationType.None:
			break;
		}
	}

	private void ExecuteAnimationWhenLeavedPointForWgo(WgoData wgoData)
	{
		if (Data.GetGroupByWGOId(wgoData.UniqueId) == null)
		{
			Debug.LogError("ExecuteAnimationWhenLeavedPointForWgo WGO:[" + wgoData.id + "] group not found");
			return;
		}
		NPCPointOfInterestData pointById = Data.GetPointById(wgoData.occupiedPointOfInterest);
		if (pointById == null)
		{
			Debug.LogError("ExecuteAnimationWhenLeavedPointForWgo WGO:[" + wgoData.id + "] no current point");
			return;
		}
		NPCPointOfInterestConfiguration configuration = pointById.Configuration;
		if (configuration == null)
		{
			Debug.LogError("ExecuteAnimationWhenLeavedPointForWgo WGO:[" + wgoData.id + "] no config for point");
			return;
		}
		switch (configuration.AnimationType)
		{
		case NPCPointOfInterestAnimationType.Roll:
			wgoData.SetCustomAnimationTrigger();
			Data.TryRemoveAnimationData(wgoData.UniqueId);
			break;
		case NPCPointOfInterestAnimationType.TriggerCustomIdle:
			wgoData.SetCustomAnimationTrigger();
			wgoData.SetTriggerToAnimator(AnimationComponentBase.RESET_TO_IDLE_TRIGGER);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case NPCPointOfInterestAnimationType.None:
			break;
		}
	}
}
