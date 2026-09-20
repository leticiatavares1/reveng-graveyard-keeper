using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Answer Data", 0)]
[Category("Game/Dialogue")]
[Color("40addb")]
public class Flow_AnswerData : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool notAvailable;

	private ValueInput<SmartRes> locks;

	private ValueInput<SmartRes> costs;

	private ValueInput<SmartRes> rewards;

	private ValueInput<SmartRes> fakeRewards;

	private ValueInput<string> dayNumber;

	private ValueInput<string> order;

	private ValueInput<bool> customHideCondition;

	private ValueOutput<AnswerData> answerData;

	protected override void RegisterPorts()
	{
		locks = AddValueInput<SmartRes>("locks".CapitalizeFirst());
		costs = AddValueInput<SmartRes>("costs".CapitalizeFirst());
		rewards = AddValueInput<SmartRes>("rewards".CapitalizeFirst());
		fakeRewards = AddValueInput<SmartRes>("fakeRewards".CapitalizeFirst());
		dayNumber = AddValueInput<string>("dayNumber".CapitalizeFirst());
		order = AddValueInput<string>("order".CapitalizeFirst());
		customHideCondition = AddValueInput<bool>("customHideCondition".CapitalizeFirst());
		answerData = AddValueOutput("answerData".CapitalizeFirst(), delegate
		{
			AnswerData answerData = new AnswerData();
			if (locks?.value != null)
			{
				answerData.lockRes = locks.value;
			}
			if (costs?.value != null)
			{
				answerData.costRes = costs.value;
			}
			if (rewards?.value != null)
			{
				answerData.rewardRes = rewards.value;
			}
			if (fakeRewards?.value != null)
			{
				answerData.fakeRewardRes = fakeRewards.value;
			}
			if (dayNumber?.value != null)
			{
				answerData.dayNumber = dayNumber.value;
			}
			if (order?.value != null)
			{
				answerData.order = order.value;
			}
			ValueInput<bool> valueInput = customHideCondition;
			if (valueInput != null)
			{
				_ = valueInput.value;
				if (true)
				{
					answerData.customHideCondition = customHideCondition.value;
				}
			}
			answerData.notAvailable = notAvailable;
			return answerData;
		});
	}
}
