using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Open Organ Enhancer GUI", 0)]
public class Flow_OpenOrganEnhancer : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		WorldGameObject _wgo = null;
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			_wgo = worldGameObject;
			GUIElements.me.organ_enhancer_gui.Open(_wgo);
			flow_out.Call(f);
		});
	}
}
