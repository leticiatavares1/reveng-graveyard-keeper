using UnityEngine;

namespace FlowCanvas.Nodes;

public class MyFlowNode : FlowControlNode
{
	public WorldGameObject wgo => GetWGOFromNode(this);

	protected CustomFlowScript cfs
	{
		get
		{
			if (base.graph == null)
			{
				Debug.LogError("Can't get CFS because graph is null", base.graph);
				return null;
			}
			if (!base.graphAgent)
			{
				return null;
			}
			return base.graphAgent.gameObject.GetComponent<CustomFlowScript>();
		}
	}

	public static WorldGameObject GetWGOFromNode(FlowNode node)
	{
		Component component = node.graphAgent;
		if (component == null)
		{
			return null;
		}
		GameObject gameObject = component.gameObject;
		WorldGameObject component2 = gameObject.GetComponent<WorldGameObject>();
		if (component2 != null)
		{
			return component2;
		}
		if (gameObject.transform.parent == null)
		{
			return null;
		}
		return gameObject.transform.parent.GetComponent<WorldGameObject>();
	}

	public WorldGameObject WGOParamOrSelf(ValueInput<WorldGameObject> param)
	{
		if (param.value != null)
		{
			return param.value;
		}
		WorldGameObject worldGameObject = wgo;
		if (worldGameObject == null)
		{
			Debug.LogError("WGO is null");
		}
		return worldGameObject;
	}

	protected bool IsEmptyStringInputPort(string port_id)
	{
		ValueInput<string> inputValuePort = GetInputValuePort<string>(port_id);
		if (!inputValuePort.isConnected)
		{
			return string.IsNullOrEmpty(inputValuePort.value);
		}
		return false;
	}

	protected ValueInput<T> GetInputValuePort<T>(string port_id)
	{
		return GetInputPort(port_id) as ValueInput<T>;
	}

	protected ValueInput GetInputValuePort(string port_id)
	{
		return GetInputPort(port_id) as ValueInput;
	}

	protected void MakeStringNullIfEmpty(string port_id)
	{
		if (IsEmptyStringInputPort(port_id))
		{
			GetInputValuePort<string>(port_id).serializedValue = null;
		}
	}

	protected T GetVariable<T>(string name)
	{
		return cfs.storage.Get<T>(name);
	}

	protected void SetVariable<T>(string name, T val)
	{
		cfs.storage.Set(name, val);
	}

	protected virtual void OnNodeInspectorGUI()
	{
	}
}
