using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Transformed Worker", 0)]
[Category("Game Actions")]
public class Flow_TransformedGetWorker : MyFlowNode
{
	private Item out_item;

	private WorldGameObject out_wgo;

	protected override void RegisterPorts()
	{
		ValueInput<Worker.WorkerTransformationType> in_transform_type = AddValueInput<Worker.WorkerTransformationType>("transform_type");
		ValueInput<WorldGameObject> in_zombie_wgo = AddValueInput<WorldGameObject>("worker_wgo");
		ValueInput<Item> in_overhead_worker_item = AddValueInput<Item>("overhead_item");
		ValueInput<Item> in_on_ground_worker_item = AddValueInput<Item>("on_ground_item");
		AddValueOutput("worker_item", () => out_item);
		AddValueOutput("worker_wgo", () => out_wgo);
		FlowOutput flow_success = AddFlowOutput("Success");
		FlowOutput flow_fail = AddFlowOutput("Fail");
		AddFlowInput("In", delegate(Flow f)
		{
			out_item = null;
			out_wgo = null;
			Worker.WorkerState from_state;
			Worker.WorkerState to_state;
			Item in_item;
			switch (in_transform_type.value)
			{
			case Worker.WorkerTransformationType.FromOverheadToWGO:
				from_state = Worker.WorkerState.ItemOverhead;
				to_state = Worker.WorkerState.WGO;
				in_item = in_overhead_worker_item.value;
				break;
			case Worker.WorkerTransformationType.FromOverheadToOnGround:
				from_state = Worker.WorkerState.ItemOverhead;
				to_state = Worker.WorkerState.ItemOnGround;
				in_item = in_overhead_worker_item.value;
				break;
			case Worker.WorkerTransformationType.FromOnGroundToOverhead:
				from_state = Worker.WorkerState.ItemOnGround;
				to_state = Worker.WorkerState.ItemOverhead;
				in_item = in_on_ground_worker_item.value;
				break;
			case Worker.WorkerTransformationType.FromOnGroundToWGO:
				from_state = Worker.WorkerState.ItemOnGround;
				to_state = Worker.WorkerState.WGO;
				in_item = in_on_ground_worker_item.value;
				break;
			case Worker.WorkerTransformationType.FromWGOToOverhead:
				from_state = Worker.WorkerState.WGO;
				to_state = Worker.WorkerState.ItemOverhead;
				in_item = null;
				break;
			case Worker.WorkerTransformationType.FromWGOToOnGround:
				from_state = Worker.WorkerState.WGO;
				to_state = Worker.WorkerState.ItemOnGround;
				in_item = null;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text = Worker.TransformWorker(from_state, in_item, in_zombie_wgo.value, to_state, out out_item, out out_wgo);
			if (!string.IsNullOrEmpty(text))
			{
				Debug.LogError(text);
				flow_fail.Call(f);
			}
			else
			{
				flow_success.Call(f);
			}
		});
	}
}
