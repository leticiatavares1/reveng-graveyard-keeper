using FlowCanvas;
using LazyBearTechnology;
using UnityEngine;

public abstract class GKCustomFlowNode : CustomFlowNode
{
	public const string CUSTOM_ICONS = "Assets/GFX/ParadoxNotionCustomIcons/";

	public PlayerData PlayerData => MainGame.PlayerData;

	public WorldData WorldData => MainGame.Instance.GameSave.worldData;

	public virtual int MinWidth => -1;

	protected WgoData SelfWgoData
	{
		get
		{
			WgoDataScript componentFromNode = CustomFlowNode.GetComponentFromNode<WgoDataScript>(this);
			if (componentFromNode != null)
			{
				return componentFromNode.wgoData;
			}
			Debug.LogError("GKCustomFlowNode: Not found WgoData");
			return null;
		}
	}

	protected WgoData WgoDataParamOrSelf(ValueInput<WgoData> param)
	{
		WgoData wgoData = null;
		if (param.value != null)
		{
			return param.value;
		}
		return SelfWgoData;
	}

	protected T ParamValueOrSelf<T>(ValueInput<T> param) where T : MonoBehaviour
	{
		T val = null;
		if (param.value != null)
		{
			val = param.value;
		}
		else
		{
			val = CustomFlowNode.GetComponentFromNode<T>(this);
			if (val == null)
			{
				Debug.LogError("GKCustomFlowNode: Not Found Object T");
			}
		}
		return val;
	}

	protected bool TryGetParamValue<T>(ValueInput<T> valueInput, out T value) where T : class
	{
		value = null;
		if (valueInput == null)
		{
			Debug.LogError(string.Format("{0}: null ValueInput for type {1}", "GKCustomFlowNode", typeof(T)));
			return false;
		}
		if (valueInput.value == null)
		{
			Debug.LogError(string.Format("{0}: Not Found Object {1}", "GKCustomFlowNode", typeof(T)));
			return false;
		}
		value = valueInput.value;
		return true;
	}

	protected GameObject ParamValueOrSelf(ValueInput<GameObject> param)
	{
		GameObject gameObject = null;
		if (param.value != null)
		{
			gameObject = param.value;
		}
		else
		{
			gameObject = CustomFlowNode.GetGameObjectFromNode(this);
			if (gameObject == null)
			{
				Debug.LogError("GKCustomFlowNode: Not Found GameObject");
			}
		}
		return gameObject;
	}

	protected override void TerminateScript()
	{
		if (base.graph == null)
		{
			Debug.LogError("Node belongs to no graph", base.graph);
		}
		else
		{
			GameScriptUtility.TerminateScript(base.graphAgent.gameObject);
		}
	}
}
