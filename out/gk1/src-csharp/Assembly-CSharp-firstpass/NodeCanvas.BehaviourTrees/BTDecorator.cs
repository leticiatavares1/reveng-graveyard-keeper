using NodeCanvas.Framework;
using ParadoxNotion;

namespace NodeCanvas.BehaviourTrees;

public abstract class BTDecorator : BTNode
{
	public sealed override int maxOutConnections => 1;

	public sealed override Alignment2x2 commentsAlignment => Alignment2x2.Right;

	protected Connection decoratedConnection
	{
		get
		{
			try
			{
				return base.outConnections[0];
			}
			catch
			{
				return null;
			}
		}
	}

	protected Node decoratedNode
	{
		get
		{
			try
			{
				return base.outConnections[0].targetNode;
			}
			catch
			{
				return null;
			}
		}
	}
}
