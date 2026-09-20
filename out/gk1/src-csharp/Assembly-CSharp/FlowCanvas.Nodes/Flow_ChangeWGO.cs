using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Change WGO", 0)]
[Category("Game Actions")]
[Icon("CubeArrowCube", false, "")]
[Description("If WGO is null, then self")]
public class Flow_ChangeWGO : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (IsEmptyStringInputPort("obj_id"))
			{
				return text + "\n<color=red>obj_id is empty</color>";
			}
			return text;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_obj_id = AddValueInput<string>("obj_id");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (!(worldGameObject == null))
			{
				worldGameObject.ReplaceWithObject(par_obj_id.value);
				worldGameObject.Redraw();
				flow_out.Call(f);
			}
		});
	}
}
