using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Wait Obj", 0)]
public class Flow_WaitObj<T> : MyFlowNode
{
	protected override void RegisterPorts()
	{
		T o = default(T);
		ValueInput<float> par_time = AddValueInput<float>("time");
		ValueInput<T> par_o = AddValueInput<T>("obj");
		FlowOutput flow_out = AddFlowOutput("Immediate");
		FlowOutput flow_done = AddFlowOutput("Finished");
		AddValueOutput("obj", () => o);
		AddFlowInput("In", delegate(Flow f)
		{
			T v = par_o.value;
			o = v;
			GJTimer.AddTimer(par_time.value, delegate
			{
				o = v;
				flow_done.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
