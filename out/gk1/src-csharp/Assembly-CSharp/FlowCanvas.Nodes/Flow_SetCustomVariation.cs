using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Set Custom Variation", 0)]
public class Flow_SetCustomVariation : MyFlowNode
{
	private class VariationsSet
	{
		public string nearest_gd_point_tag = string.Empty;

		public int variation = 1;
	}

	public enum CustomVariationSetType
	{
		None,
		PlayersTavern
	}

	private VariationsSet[] tavern_sets = new VariationsSet[6]
	{
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_1",
			variation = 1
		},
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_2",
			variation = 2
		},
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_3",
			variation = 4
		},
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_4",
			variation = 8
		},
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_5",
			variation = 1
		},
		new VariationsSet
		{
			nearest_gd_point_tag = "tavern_custom_variation_6",
			variation = 2
		}
	};

	private int _variation = -1;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<CustomVariationSetType> in_type = AddValueInput<CustomVariationSetType>("Type");
		AddValueOutput("Variation", () => _variation);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			_variation = -1;
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_SetCustomVariation error: WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				switch (in_type.value)
				{
				case CustomVariationSetType.None:
					Debug.LogError("Flow_SetCustomVariation error: type is None!");
					flow_out.Call(f);
					break;
				case CustomVariationSetType.PlayersTavern:
				{
					VariationsSet[] array = tavern_sets;
					Vector2 vector = worldGameObject.transform.position;
					VariationsSet variationsSet = array[0];
					GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(variationsSet.nearest_gd_point_tag);
					if (gDPointByGDTag == null)
					{
						Debug.LogError("FATAL ERROR! Flow_SetCustomVariation error: not found GDPoint with tag \"" + variationsSet.nearest_gd_point_tag + "\"!");
						flow_out.Call(f);
					}
					else
					{
						float num = ((Vector2)gDPointByGDTag.pos - vector).sqrMagnitude;
						if (array.Length > 1)
						{
							for (int i = 1; i < array.Length; i++)
							{
								GDPoint gDPointByGDTag2 = WorldMap.GetGDPointByGDTag(array[i].nearest_gd_point_tag, log_if_null: false);
								if (!(gDPointByGDTag2 == null))
								{
									float sqrMagnitude = ((Vector2)gDPointByGDTag2.pos - vector).sqrMagnitude;
									if (sqrMagnitude < num)
									{
										variationsSet = array[i];
										gDPointByGDTag = gDPointByGDTag2;
										num = sqrMagnitude;
									}
								}
							}
						}
						if (variationsSet != null)
						{
							Debug.Log($"Set custom variation to WGO={worldGameObject.obj_id}, variation={variationsSet.variation}");
							worldGameObject.variation = variationsSet.variation;
							_variation = variationsSet.variation;
							worldGameObject.Redraw();
						}
						flow_out.Call(f);
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		});
	}
}
