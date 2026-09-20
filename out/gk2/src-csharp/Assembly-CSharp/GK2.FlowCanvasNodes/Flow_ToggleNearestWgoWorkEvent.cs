using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Toggle Nearest Wgo Work Event", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
[ParadoxNotion.Design.Icon("Dialogue", false, "")]
public class Flow_ToggleNearestWgoWorkEvent : GKCustomFlowNode
{
	private const string WorkEventId = "work";

	private FlowInput @in;

	private FlowOutput @out;

	private SGuid lastTargetUniqueId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Toggle);
		@out = AddFlowOutput("out".CapitalizeFirst());
		AddValueOutput("WgoData", GetLastTarget);
	}

	private void Toggle(Flow flow)
	{
		WgoData lastTarget = GetLastTarget();
		if (lastTarget != null && HasWorkEvent(lastTarget))
		{
			lastTarget.RemoveInteractionEvent("work");
			lastTargetUniqueId = null;
		}
		else
		{
			WgoData wgoData = FindNearestWgo();
			if (wgoData == null)
			{
				Debug.LogWarning("Flow_ToggleNearestWgoWorkEvent: no WGO found near the player");
			}
			else
			{
				wgoData.AddInteractionEvent("work", isFake: true);
				lastTargetUniqueId = wgoData.UniqueId;
			}
		}
		@out.Call(flow);
	}

	private WgoData GetLastTarget()
	{
		if (!(lastTargetUniqueId == null))
		{
			return base.WorldData.GetWgoData(lastTargetUniqueId);
		}
		return null;
	}

	private WgoData FindNearestWgo()
	{
		if (base.PlayerData == null || base.WorldData == null)
		{
			return null;
		}
		Vector3 value = base.PlayerData.position.Value;
		string currentGameSceneId = base.PlayerData.currentGameSceneId;
		WgoData result = null;
		float num = float.MaxValue;
		foreach (WgoData value2 in base.WorldData.Cache.wgoDataByUidCache.Values)
		{
			if (value2 != null && !value2.IsHidden && !value2.isTempObject && (string.IsNullOrEmpty(currentGameSceneId) || !(value2.WorldId != currentGameSceneId)))
			{
				float sqrMagnitude = (value2.Position - value).sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					num = sqrMagnitude;
					result = value2;
				}
			}
		}
		return result;
	}

	private static bool HasWorkEvent(WgoData wgoData)
	{
		foreach (InteractionEvent @event in wgoData.Events)
		{
			if (@event.str == "work")
			{
				return true;
			}
		}
		return false;
	}
}
