using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get WGO Id From Data", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_GetWgoIdFromData : GKCustomFlowNodeWithWgoData
{
	private new ValueOutput<string> wgoId;

	private ValueOutput<string> wgoCustomTag;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		wgoId = AddValueOutput("wgoId", () => GetWgoData().id);
		wgoCustomTag = AddValueOutput("wgoCustomTag", () => GetWgoData().CustomTag);
	}
}
