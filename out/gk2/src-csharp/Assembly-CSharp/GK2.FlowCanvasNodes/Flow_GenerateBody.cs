using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Generate Body", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_GenerateBody : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool fromCurrentPlayerBodiesTier;

	private ValueInput<string> itemId;

	private ValueInput<bool> neverAutoDestroy;

	private ValueOutput<Item> body;

	protected override void RegisterPorts()
	{
		if (!fromCurrentPlayerBodiesTier)
		{
			itemId = AddValueInput<string>("itemId");
		}
		neverAutoDestroy = AddValueInput<bool>("neverAutoDestroy");
		body = AddValueOutput("body", delegate
		{
			Item item;
			if (fromCurrentPlayerBodiesTier)
			{
				List<BodyDef> list = new List<BodyDef>();
				int resInt = MainGame.PlayerData.GetResInt("bodies_tier");
				foreach (BodyDef bodyDef in GameBalance.Me.bodyDefs)
				{
					if (bodyDef.tier == resInt)
					{
						list.Add(bodyDef);
					}
				}
				item = list.GetRandom().GenerateItem();
			}
			else
			{
				item = GameBalance.Me.GetData<BodyDef>(itemId.value).GenerateItem();
			}
			if (neverAutoDestroy.value)
			{
				item.AddProperty(new NeverAutoDestroyDropSerializedItemProperty());
			}
			return item;
		});
	}
}
