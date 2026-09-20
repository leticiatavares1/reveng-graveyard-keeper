using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Unloop Particle And Delete WGO", 0)]
public class Flow_UnloopParticlAndDeleteWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<float> in_time = AddValueInput<float>("time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value != null)
			{
				ParticlesMinIntensityAndDelete componentInChildren = in_wgo.value.GetComponentInChildren<ParticlesMinIntensityAndDelete>();
				if (componentInChildren != null)
				{
					componentInChildren.DoDelete(in_time.value);
				}
			}
			flow_out.Call(f);
		});
	}
}
