using FlowCanvas;
using FlowCanvas.Nodes;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class CustomFlowNode : FlowControlNode
{
	public static GameObject GetGameObjectFromNode(FlowNode flowNode)
	{
		Component component = flowNode.graphAgent;
		if (component == null)
		{
			return null;
		}
		return component.gameObject;
	}

	public static T GetComponentFromNode<T>(FlowNode flowNode) where T : MonoBehaviour
	{
		GameObject gameObjectFromNode = GetGameObjectFromNode(flowNode);
		if (gameObjectFromNode != null)
		{
			return gameObjectFromNode.GetComponent<T>();
		}
		return null;
	}

	public ValueInput<T> GetValueInputPort<T>(string portId)
	{
		return GetInputPort(portId) as ValueInput<T>;
	}

	public ValueInput GetValueInputPort(string portId)
	{
		return GetInputPort(portId) as ValueInput;
	}

	protected abstract void TerminateScript();
}
