using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Answer", 0)]
[Category("Game Functions")]
[Color("00ff00")]
public class Flow_Answer : PureFunctionNode<AnswerData, SmartRes, SmartRes, SmartRes>
{
	public override AnswerData Invoke(SmartRes price, SmartRes @lock, SmartRes reward)
	{
		return new AnswerData
		{
			d_lock = @lock,
			d_price = price,
			d_reward = reward
		};
	}
}
