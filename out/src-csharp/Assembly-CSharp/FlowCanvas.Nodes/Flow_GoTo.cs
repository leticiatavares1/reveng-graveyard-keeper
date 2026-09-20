using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Icon("CubeArrowStraight", false, "")]
[Name("GoTo", 0)]
public class Flow_GoTo : MyFlowNode
{
	protected override void RegisterPorts()
	{
		string cur_gd_point = string.Empty;
		WorldGameObject _wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<GameObject> par_target = AddValueInput<GameObject>("target");
		ValueInput<string> par_event = AddValueInput<string>("finished_event");
		ValueInput<float> par_speed = AddValueInput<float>("speed");
		ValueInput<MovementComponent.GoToMethod> par_goto_method = AddValueInput<MovementComponent.GoToMethod>("method");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_came = AddFlowOutput("Came to dest");
		AddValueOutput("Current GDPoint", () => cur_gd_point);
		AddValueOutput("WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject o = WGOParamOrSelf(par_wgo);
			_wgo = o;
			if (o == null)
			{
				Debug.LogError("WGO GoTo error: WGO #1 is null");
				flow_out.Call(f);
			}
			else
			{
				GameObject value = par_target.value;
				if (value == null)
				{
					Debug.LogError("GoTo: target is null");
					flow_out.Call(f);
				}
				else
				{
					BaseCharacterComponent character = o.components.character;
					if (character == null)
					{
						Debug.LogError("Can't move a non-character WGO");
						flow_out.Call(f);
					}
					else
					{
						ChunkedGameObject chunk = o.gameObject.GetComponent<ChunkedGameObject>();
						bool do_chunk_lock = chunk != null && !chunk.always_active;
						if (do_chunk_lock)
						{
							chunk.active_now_because_of_movement = true;
						}
						uint? filter_astar_area = null;
						GDPoint component = value.GetComponent<GDPoint>();
						if (par_goto_method.value == MovementComponent.GoToMethod.GDGraph)
						{
							if (component != null)
							{
								try
								{
									filter_astar_area = component.node.Area;
								}
								catch (Exception)
								{
									Debug.LogError("Error with GDPoint " + component.gd_tag, component);
									throw;
								}
							}
							else
							{
								Debug.LogError("[" + o.obj_id + "] Trying to walk by GDGraph, but target is not GDPoint: " + value.name, value);
							}
						}
						o.cur_zone = string.Empty;
						o.cur_gd_point = string.Empty;
						character.GoTo(value, snap_to_node: false, delegate
						{
							if (do_chunk_lock && chunk != null)
							{
								chunk.active_now_because_of_movement = false;
							}
							_wgo = o;
							flow_came.Call(f);
						}, null, with_cinematic: false, par_goto_method.value, par_event.value, filter_astar_area, from_script: true, component);
						if (base.wgo == null || !base.wgo.is_player)
						{
							character.SetSpeed(par_speed.HasValue() ? par_speed.value : 1.2f);
						}
						_wgo = o;
						flow_out.Call(f);
					}
				}
			}
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("finished_event");
	}
}
