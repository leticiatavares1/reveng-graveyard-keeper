using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Idle Point", 0)]
[Category("Game Actions")]
[Description("Get Idle Point")]
public class Flow_GetIdlePoint : MyFlowNode
{
	public List<string> personal_points = new List<string>();

	public const bool LOG_STOCK_POINTS_MANAGEMENT = true;

	private string idle_point_name = string.Empty;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_stock_wgo = AddValueInput<WorldGameObject>("Stock WGO");
		ValueInput<string> in_points_prefix = AddValueInput<string>("Points Prefix");
		ValueInput<WorldGameObject> in_npc_wgo = AddValueInput<WorldGameObject>("NPC");
		ValueInput<int> in_free_point_name = AddValueInput<int>("Free Point Num");
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		WorldGameObject _wgo = null;
		AddValueOutput("Point Name", () => idle_point_name);
		FlowOutput flow_yes = AddFlowOutput("Found");
		FlowOutput flow_no = AddFlowOutput("Not Found");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			_wgo = worldGameObject;
			WorldGameObject worldGameObject2 = null;
			if (!in_stock_wgo.HasValue())
			{
				Debug.LogError("Stock WGO not connected!");
				flow_no.Call(f);
			}
			else if (string.IsNullOrEmpty(in_points_prefix.value))
			{
				Debug.LogError("Points prefix is null!");
				flow_no.Call(f);
			}
			else
			{
				worldGameObject2 = in_stock_wgo.value;
				int paramInt = worldGameObject2.GetParamInt("max_idle_points");
				if (paramInt < 1)
				{
					Debug.LogError("Max idle points with prefix \"" + in_points_prefix.value + "\", stored in [" + worldGameObject2.name + "::" + worldGameObject2.obj_id + "] is wrong: " + paramInt);
				}
				else
				{
					_ = string.Empty;
					if (in_npc_wgo.value != null && in_npc_wgo.value.cur_gd_point.StartsWith(in_points_prefix.value))
					{
						_ = in_npc_wgo.value.cur_gd_point;
					}
					else if (in_free_point_name.value > 0)
					{
						_ = in_points_prefix.value + in_free_point_name.value;
					}
					List<int> list = new List<int>();
					for (int i = 1; i <= paramInt; i++)
					{
						if (worldGameObject2.data.GetParamInt(in_points_prefix.value + i) == 0)
						{
							list.Add(i);
						}
					}
					bool flag = false;
					if (personal_points.Count > 0)
					{
						List<string> list2 = new List<string>();
						for (int j = 0; j < personal_points.Count; j++)
						{
							if (_wgo.data.GetParam(personal_points[j]) == 0f)
							{
								list2.Add(personal_points[j]);
							}
						}
						if (list2.Count > 0)
						{
							int maxExclusive = list2.Count / (list.Count + list2.Count);
							int num = UnityEngine.Random.Range(1, maxExclusive);
							if (list2.Count * 2 >= num)
							{
								flag = true;
								int index = UnityEngine.Random.Range(0, list2.Count - 1);
								idle_point_name = list2[index];
								_wgo.SetParam(idle_point_name, 1f);
								if (_wgo != null)
								{
									if (_wgo.cur_gd_point.StartsWith(in_points_prefix.value))
									{
										if (worldGameObject2.data.GetParamInt(_wgo.cur_gd_point) == 1)
										{
											worldGameObject2.data.SetParam(_wgo.cur_gd_point, 0f);
										}
									}
									else if (_wgo.cur_gd_point.StartsWith("personal_") && _wgo.data.GetParamInt(_wgo.cur_gd_point) == 1)
									{
										_wgo.data.SetParam(_wgo.cur_gd_point, 0f);
									}
								}
								flow_yes.Call(f);
							}
						}
					}
					if (!flag)
					{
						switch (list.Count)
						{
						case 0:
							flow_no.Call(f);
							break;
						case 1:
							idle_point_name = in_points_prefix.value + list[0];
							worldGameObject2.data.SetParam(idle_point_name, 1f);
							Debug.Log("#ipm# [" + worldGameObject2.obj_id + "]: lock idle point {" + idle_point_name + "}");
							if (_wgo != null)
							{
								if (_wgo.cur_gd_point.StartsWith(in_points_prefix.value))
								{
									if (worldGameObject2.data.GetParamInt(_wgo.cur_gd_point) == 1)
									{
										worldGameObject2.data.SetParam(_wgo.cur_gd_point, 0f);
									}
								}
								else if (_wgo.cur_gd_point.StartsWith("personal_") && _wgo.data.GetParamInt(_wgo.cur_gd_point) == 1)
								{
									_wgo.data.SetParam(_wgo.cur_gd_point, 0f);
								}
							}
							flow_yes.Call(f);
							break;
						default:
						{
							int index2 = UnityEngine.Random.Range(0, list.Count - 1);
							idle_point_name = in_points_prefix.value + list[index2];
							worldGameObject2.data.SetParam(idle_point_name, 1f);
							Debug.Log("#ipm# [" + worldGameObject2.obj_id + "]: lock idle point {" + idle_point_name + "}");
							if (_wgo != null)
							{
								if (_wgo.cur_gd_point.StartsWith(in_points_prefix.value))
								{
									if (worldGameObject2.data.GetParamInt(_wgo.cur_gd_point) == 1)
									{
										worldGameObject2.data.SetParam(_wgo.cur_gd_point, 0f);
									}
								}
								else if (_wgo.cur_gd_point.StartsWith("personal_") && _wgo.data.GetParamInt(_wgo.cur_gd_point) == 1)
								{
									_wgo.data.SetParam(_wgo.cur_gd_point, 0f);
								}
							}
							flow_yes.Call(f);
							break;
						}
						}
					}
				}
			}
		});
	}
}
