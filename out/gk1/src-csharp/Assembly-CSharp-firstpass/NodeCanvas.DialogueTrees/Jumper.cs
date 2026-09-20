using LinqTools;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Color("00b9e8")]
[Icon("Set", false, "")]
[Description("Select a target node to jump to.\nFor your convenience in identifying nodes in the dropdown, please give a Tag name to the nodes you want to use in this way.")]
[Name("JUMP", 0)]
[Category("Control")]
public class Jumper : DTNode
{
	[SerializeField]
	private string _sourceNodeUID;

	private object _sourceNode;

	private string sourceNodeUID
	{
		get
		{
			return _sourceNodeUID;
		}
		set
		{
			_sourceNodeUID = value;
		}
	}

	private DTNode sourceNode
	{
		get
		{
			if (_sourceNode == null)
			{
				_sourceNode = base.graph.allNodes.OfType<DTNode>().FirstOrDefault((DTNode n) => n.UID == sourceNodeUID);
				if (_sourceNode == null)
				{
					_sourceNode = new object();
				}
			}
			return _sourceNode as DTNode;
		}
		set
		{
			_sourceNode = value;
		}
	}

	public override int maxOutConnections => 0;

	public override bool requireActorSelection => false;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (sourceNode == null)
		{
			return Error("Target Node of Jumper node is null");
		}
		base.DLGTree.EnterNode(sourceNode);
		return Status.Success;
	}
}
