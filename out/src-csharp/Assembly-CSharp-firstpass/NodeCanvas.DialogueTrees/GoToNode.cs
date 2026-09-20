using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Obsolete("Use Jumpers instead")]
[Description("Jump to another Dialogue node. Usefull if that other node is far away to connect, but otherwise it's exactly the same.\n\nPlease enable 'Show Node IDs' in Editor Prefs for convenience")]
[Icon("Set", false, "")]
[Category("Control")]
[Name("GO TO", 0)]
[Color("00b9e8")]
public class GoToNode : DTNode
{
	[SerializeField]
	private DTNode _targetNode;

	public override int maxOutConnections => 0;

	public override bool requireActorSelection => false;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (_targetNode == null)
		{
			return Error("Target node of GOTO node is null");
		}
		base.DLGTree.EnterNode(_targetNode);
		return Status.Success;
	}
}
