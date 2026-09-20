using FlowCanvas;
using UnityEngine;

public abstract class GKCustomFlowNodeWithWgoData : GKCustomFlowNode
{
	[GatherPortsCallback]
	[InspectorName("Use ID for WGO link")]
	public bool findById;

	protected ValueInput<string> wgoId;

	protected ValueInput<WgoData> wgoDataInput;

	private ValueOutput<WgoData> wgoOutput;

	protected override void RegisterPorts()
	{
		if (findById)
		{
			wgoId = AddValueInput<string>("WgoData");
		}
		else
		{
			wgoDataInput = AddValueInput<WgoData>("WgoData");
		}
		wgoOutput = AddValueOutput("WgoData", () => GetWgoData());
	}

	protected WgoData GetWgoData()
	{
		if (!findById)
		{
			return WgoDataParamOrSelf(wgoDataInput);
		}
		return MainGame.Instance.GameSave.worldData.GetWgoData(wgoId.value);
	}
}
