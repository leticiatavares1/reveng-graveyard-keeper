using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Find WgoData", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_FindWgoData : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool getList;

	[GatherPortsCallback]
	public bool displayFoundStateOuput;

	private ValueInput<string> wgoId;

	private ValueInput<string> customTag;

	private ValueOutput<WgoData> wgoData;

	private ValueOutput<bool> wasFound;

	private ValueOutput<List<WgoData>> wgoDataList;

	public override string name => "Find WgoData" + (getList ? " List By Tag" : "");

	protected override void RegisterPorts()
	{
		if (!getList)
		{
			wgoId = AddValueInput<string>("wgoId");
			wgoData = AddValueOutput("wgoData", TryGetWgoData);
		}
		else
		{
			wgoDataList = AddValueOutput("wgoDataList", TryGetWgoDataList);
		}
		customTag = AddValueInput<string>("customTag");
		if (displayFoundStateOuput)
		{
			wasFound = AddValueOutput("wasFound", WasFound);
		}
	}

	private bool WasFound()
	{
		if (!getList)
		{
			WgoData wgoDataByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoData(wgoId.value);
			if (wgoDataByCustomTag == null)
			{
				wgoDataByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(customTag.value);
			}
			return wgoDataByCustomTag != null;
		}
		List<WgoData> wgoDataListByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoDataListByCustomTag(customTag.value);
		if (wgoDataListByCustomTag != null)
		{
			return wgoDataListByCustomTag.Count > 0;
		}
		return false;
	}

	private WgoData TryGetWgoData()
	{
		WgoData wgoDataByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoData(wgoId.value);
		if (wgoDataByCustomTag == null)
		{
			wgoDataByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(customTag.value);
			if (wgoDataByCustomTag == null)
			{
				Debug.LogError("Flow_FindWgoData: WGO [" + wgoId.value + "] is null");
			}
		}
		return wgoDataByCustomTag;
	}

	private List<WgoData> TryGetWgoDataList()
	{
		List<WgoData> wgoDataListByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoDataListByCustomTag(customTag.value);
		if (wgoDataListByCustomTag == null)
		{
			Debug.LogError("Flow_FindWgoData: WGO [" + wgoId.value + "] is null");
		}
		return wgoDataListByCustomTag;
	}
}
