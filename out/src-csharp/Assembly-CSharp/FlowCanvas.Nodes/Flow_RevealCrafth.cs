using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Reveal Craft", 0)]
[Category("Game Actions")]
public class Flow_RevealCrafth : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> craft_id = AddValueInput<string>("craft_id");
		ValueInput<bool> silent = AddValueInput<bool>("silent?");
		ValueInput<bool> not_add_to_one_time = AddValueInput<bool>("not_add_to_one_time?");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(craft_id.value))
			{
				Debug.LogError("Can not reveal craft: craft_id is null!");
				flow_out.Call(f);
			}
			else
			{
				CraftDefinition craftDefinition = GameBalance.me.GetData<CraftDefinition>(craft_id.value);
				ObjectCraftDefinition data = GameBalance.me.GetData<ObjectCraftDefinition>(craft_id.value);
				if (craftDefinition == null)
				{
					craftDefinition = data;
				}
				if (craftDefinition == null)
				{
					Debug.LogError("Can not reveal craft: craft with id \"" + craft_id.value + "\" not found!");
					flow_out.Call(f);
				}
				else if (silent.value)
				{
					if (!not_add_to_one_time.value)
					{
						MainGame.me.save.completed_one_time_crafts.Add(craft_id.value);
					}
					flow_out.Call(f);
				}
				else
				{
					if (!not_add_to_one_time.value)
					{
						MainGame.me.save.completed_one_time_crafts.Add(craft_id.value);
					}
					string text = string.Empty;
					if (craftDefinition.output == null || craftDefinition.output.Count == 0)
					{
						text = craftDefinition.id;
					}
					else
					{
						foreach (Item item in craftDefinition.output)
						{
							if (!(item.id == "r") && !(item.id == "g") && !(item.id == "b"))
							{
								text = item.id;
							}
						}
						if (string.IsNullOrEmpty(text))
						{
							text = craftDefinition.id;
						}
					}
					TechDefinition tech = new TechDefinition
					{
						id = text,
						crafts = { craft_id.value },
						price = new GameRes()
					};
					GUIElements.me.tech_dialog.Open(tech, delegate
					{
						flow_out.Call(f);
					}, forced_unlock: true, reveal_tech: false, show_tech_tree_after: false, pseudotech: true);
				}
			}
		});
	}
}
