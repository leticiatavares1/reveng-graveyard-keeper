using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("0bb0b0")]
[Category("Game Actions")]
[Name("Add Camera Filter VCR and Glitch", 0)]
public class Flow_CameraFilterVCRGlitch : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("Remove filter?").value)
			{
				return "Remove Camera Filter VCR and Glitch";
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
				MainGame.me.gameObject.DestroyComponentIfExists<CameraFilterPack_FX_Glitch1>();
				MainGame.me.gameObject.DestroyComponentIfExists<CameraFilterPack_TV_Vcr>();
			}
			else
			{
				MainGame.me.gameObject.AddComponent<CameraFilterPack_FX_Glitch1>();
				MainGame.me.gameObject.AddComponent<CameraFilterPack_TV_Vcr>();
			}
			flow_out.Call(f);
		});
	}
}
