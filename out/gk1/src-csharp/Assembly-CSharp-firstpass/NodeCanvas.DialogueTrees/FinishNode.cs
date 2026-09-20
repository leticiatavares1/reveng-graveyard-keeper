using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Category("Control")]
[Description("End the dialogue in Success or Failure.\nNote: A Dialogue will anyway End in Succcess if it has reached a node without child connections. Thus this node is mostly useful if you want to end a Dialogue in Failure.")]
[Icon("Halt", false, "")]
[Color("00b9e8")]
[Name("FINISH", 0)]
public class FinishNode : DTNode
{
	public CompactStatus finishState = CompactStatus.Success;

	public override int maxOutConnections => 0;

	public override bool requireActorSelection => false;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		base.status = (Status)finishState;
		base.DLGTree.Stop(finishState == CompactStatus.Success);
		return base.status;
	}
}
