using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Operate Cargo Lift", 0)]
[Category("Game")]
public class Flow_OperateCargoLift : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueOutput<bool> result;

	private bool success;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SendItemsToCargo);
		@out = AddFlowOutput("out".CapitalizeFirst());
		result = AddValueOutput("result".CapitalizeFirst(), () => success);
	}

	private void SendItemsToCargo(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData == null)
		{
			success = false;
			@out.Call(flow);
			return;
		}
		if (!string.IsNullOrEmpty(wgoData.GameResStr.Get("target_storage_wgo")))
		{
			success = false;
			@out.Call(flow);
			return;
		}
		PorterStationDef data = GameBalance.Me.GetData<PorterStationDef>(wgoData.id);
		if (data == null)
		{
			Debug.LogError($"CargoLift {wgoData.UniqueId} has no PorterStationDef configured!");
			success = false;
			@out.Call(flow);
			return;
		}
		WgoData wgoData2 = null;
		if (data.customTargets != null && data.customTargets.Count > 0)
		{
			foreach (string customTarget in data.customTargets)
			{
				if (!string.IsNullOrEmpty(customTarget))
				{
					List<WgoData> wgoDataList = MainGame.WorldData.GetWgoDataList(customTarget);
					if (wgoDataList != null && wgoDataList.Count > 0)
					{
						wgoData2 = wgoDataList[0];
						break;
					}
				}
			}
		}
		if (wgoData2 == null)
		{
			Debug.LogError("[Flow_OperateCargoLift]: Target storage wgo was not found!");
			success = false;
			@out.Call(flow);
			return;
		}
		int inventorySize = wgoData.Inventory.Data.InventorySize;
		if (inventorySize <= 0)
		{
			Debug.LogError(string.Format("[{0}]: CargoLift {1} has invalid inventory size!", "Flow_OperateCargoLift", wgoData.UniqueId));
			success = false;
			@out.Call(flow);
			return;
		}
		wgoData.Inventory.Clear();
		bool flag = false;
		int num = 0;
		foreach (NeedItemData item in data.items)
		{
			if (wgoData.GetGameResInt(item.id) != 1)
			{
				continue;
			}
			WorldZoneData worldZoneData = wgoData.WorldZoneData;
			if (worldZoneData == null)
			{
				Debug.LogWarning(string.Format("[{0}]: CargoLift {1} is not in any world zone!", "Flow_OperateCargoLift", wgoData.UniqueId));
				continue;
			}
			MultiInventory multiInventory = new MultiInventory(worldZoneData);
			int totalCount = multiInventory.GetTotalCount(item.id);
			if (totalCount <= 0)
			{
				Debug.LogWarning("[Flow_OperateCargoLift]: Item " + item.id + " not found in zone " + worldZoneData.id + "!");
				continue;
			}
			int num2 = inventorySize - num;
			if (num2 <= 0)
			{
				break;
			}
			int count = Mathf.Min(totalCount, num2);
			List<Item> list = multiInventory.RemoveItemByCount(item.id, count);
			if (list.Count > 0)
			{
				foreach (Item item2 in list)
				{
					wgoData.Inventory.AddItemToInventory(item2);
					num += item2.Count;
				}
				flag = true;
			}
			else
			{
				Debug.LogWarning("[Flow_OperateCargoLift]: Failed to remove item " + item.id + " from zone MultiInventory!");
			}
		}
		if (flag)
		{
			wgoData.SetCustomAnimationTrigger();
			wgoData.GameResStr.Set("target_storage_wgo", wgoData2.id);
		}
		success = flag;
		@out.Call(flow);
	}
}
