using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Add Camera Filter Scene In The Past", 0)]
[Category("Game Actions")]
[Color("0bb0b0")]
public class Flow_AddCameraFilterSceneInThePast : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("Remove filter?").value)
			{
				return "Remove Camera Filter Scene In The Past";
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
				MainGame.me.gameObject.DestroyComponentIfExists<CameraFilterPack_Blur_Tilt_Shift_Hole>();
				MainGame.me.gameObject.DestroyComponentIfExists<CameraFilterPack_TV_WideScreenCircle>();
			}
			else
			{
				CameraFilterPack_Blur_Tilt_Shift_Hole cameraFilterPack_Blur_Tilt_Shift_Hole = MainGame.me.gameObject.AddComponent<CameraFilterPack_Blur_Tilt_Shift_Hole>();
				cameraFilterPack_Blur_Tilt_Shift_Hole.Amount = 3f;
				cameraFilterPack_Blur_Tilt_Shift_Hole.FastFilter = 2;
				cameraFilterPack_Blur_Tilt_Shift_Hole.Smooth = 0.191f;
				cameraFilterPack_Blur_Tilt_Shift_Hole.Size = 0.661f;
				CameraFilterPack_TV_WideScreenCircle cameraFilterPack_TV_WideScreenCircle = MainGame.me.gameObject.AddComponent<CameraFilterPack_TV_WideScreenCircle>();
				cameraFilterPack_TV_WideScreenCircle.Size = 0.8f;
				cameraFilterPack_TV_WideScreenCircle.Smooth = 0.4f;
			}
			flow_out.Call(f);
		});
	}
}
