using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Multiple Answer", 0)]
[Category("Game Functions")]
[Color("00ff00")]
public class Flow_MultipleAnswer : PureFunctionNode<MultipleAnswerData, List<AnswerData>, SmartRes>
{
	public override MultipleAnswerData Invoke(List<AnswerData> datas, SmartRes reward)
	{
		return new MultipleAnswerData(reward, datas);
	}
}
