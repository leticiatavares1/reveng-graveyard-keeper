using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("ArrowDown", false, "")]
[Category("Game Actions")]
[Name("Drop Story", 0)]
[Color("857fff")]
[Description("If WGO is null, then self")]
public class Flow_DropStory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		ValueInput<float> par_story_br = AddValueInput<float>("Story brnz");
		ValueInput<float> par_story_silv = AddValueInput<float>("Story silv");
		ValueInput<float> par_story_gold = AddValueInput<float>("Story gold");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				worldGameObject.DropStory(par_story_br.value, par_story_silv.value, par_story_gold.value);
				flow_out.Call(f);
			}
		});
	}
}
