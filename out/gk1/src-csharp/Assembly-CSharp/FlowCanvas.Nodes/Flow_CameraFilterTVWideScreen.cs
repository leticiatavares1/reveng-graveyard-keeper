using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Add Camera Filter TV Wide Screen", 0)]
[Category("Game Actions")]
[Color("0bb0b0")]
public class Flow_CameraFilterTVWideScreen : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("Remove filter?").value)
			{
				return "Remove Camera Filter TV Wide Screen";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> par_remove = AddValueInput<bool>("Remove filter?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (par_remove.value)
			{
				MainGame.me.gameObject.DestroyComponentIfExists<CameraFilterPack_TV_WideScreenHV>();
			}
			else
			{
				CameraFilterPack_TV_WideScreenHV cameraFilterPack_TV_WideScreenHV = MainGame.me.gameObject.AddComponent<CameraFilterPack_TV_WideScreenHV>();
				cameraFilterPack_TV_WideScreenHV.Size = 0.8f;
				cameraFilterPack_TV_WideScreenHV.Smooth = 0.19f;
			}
			flow_out.Call(f);
		});
	}
}
