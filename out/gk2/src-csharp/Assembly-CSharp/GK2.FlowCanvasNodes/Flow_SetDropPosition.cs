using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Set Drop Position", 0)]
[Category("Game/Item")]
[Description("Sets the position of a dropped item. Optionally disables its physics collision.")]
[Color("FFFFFF")]
public class Flow_SetDropPosition : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool useGdPoint;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> item;

	private new ValueInput<Vector3> position;

	private ValueInput<GDPointData> gdPoint;

	private ValueInput<bool> disableCollision;

	public override string name => "Set Drop Position" + (useGdPoint ? " (GDPoint)" : "");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetPosition);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueInput<Item>("item");
		if (useGdPoint)
		{
			gdPoint = AddValueInput<GDPointData>("gdPoint");
		}
		else
		{
			position = AddValueInput<Vector3>("position");
		}
		disableCollision = AddValueInput<bool>("disableCollision");
	}

	private void SetPosition(Flow flow)
	{
		Item value = item.value;
		if (value != null && !value.IsEmpty)
		{
			Vector3 vector = ((!useGdPoint) ? position.value : ((gdPoint.value != null) ? gdPoint.value.Position : Vector3.zero));
			DropData dropData = null;
			DropView dropView = null;
			foreach (GameSceneData gameSceneData in MainGame.Instance.GameSave.worldData.gameSceneDataList)
			{
				foreach (DropData droppedItem in gameSceneData.droppedItems)
				{
					if (droppedItem.UniqueId == value.UniqueId)
					{
						dropData = droppedItem;
						break;
					}
				}
				if (dropData != null)
				{
					break;
				}
			}
			if (dropData != null)
			{
				dropData.Position = vector;
				foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
				{
					dropView = loadedGameScene.GetDropView(value);
					if (dropView != null)
					{
						break;
					}
				}
				if (dropView != null)
				{
					if (disableCollision.value)
					{
						dropView.SetNonPhysicState(isPhysicDisabled: true);
					}
					dropView.transform.position = vector;
					if (dropView.TryGetComponent<Rigidbody>(out var component))
					{
						component.position = vector;
						component.linearVelocity = Vector3.zero;
						component.angularVelocity = Vector3.zero;
					}
				}
				else
				{
					Debug.LogWarning($"[Flow_SetDropPosition] DropView not found for item {value.UniqueId}. Drop might not be spawned yet.");
				}
			}
			else
			{
				Debug.LogWarning($"[Flow_SetDropPosition] DropData not found for item {value.UniqueId}");
			}
		}
		@out.Call(flow);
	}
}
