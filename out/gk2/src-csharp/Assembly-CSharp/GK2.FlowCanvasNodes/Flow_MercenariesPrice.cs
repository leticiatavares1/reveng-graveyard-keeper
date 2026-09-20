using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Pay Mercenary Price", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_MercenariesPrice : GKCustomFlowNode
{
	private ValueOutput<AnswerData> answerData;

	protected override void RegisterPorts()
	{
		answerData = AddValueOutput("answerData".CapitalizeFirst(), delegate
		{
			AnswerData answerData = new AnswerData();
			MercenariesDef data = GameBalance.Me.GetData<MercenariesDef>(MainGame.Instance.GameSave.militaryBaseData.MercenariesPaymentId);
			if (data != null)
			{
				SmartRes smartRes = new SmartRes();
				if (data.needItems.Count > 0)
				{
					foreach (NeedItemData needItem in data.needItems)
					{
						smartRes.items.Add(new ItemCount(needItem.id, needItem.GetCount()));
					}
				}
				else
				{
					smartRes.gameRes.Add("money", data.money);
				}
				answerData.costRes = smartRes;
			}
			return answerData;
		});
	}
}
