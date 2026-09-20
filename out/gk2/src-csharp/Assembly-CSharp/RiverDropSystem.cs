using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class RiverDropSystem : ICustomUpdatable
{
	private class ThrowSession
	{
		public Vector3 startPos;

		public Vector3 endPos;

		public float elapsed;

		public float duration;

		public AnimationCurve heightCurve;
	}

	private readonly Dictionary<SGuid, DropView> views = new Dictionary<SGuid, DropView>();

	private readonly Dictionary<SGuid, ThrowSession> throws = new Dictionary<SGuid, ThrowSession>();

	private bool subscribedToWgoSpawn;

	private RiverDropSystemData Data
	{
		get
		{
			GameSave gameSave = MainGame.Instance?.GameSave;
			if (gameSave == null)
			{
				return null;
			}
			GameSave gameSave2 = gameSave;
			if (gameSave2.riverDropSystemData == null)
			{
				gameSave2.riverDropSystemData = new RiverDropSystemData();
			}
			RiverDropSystemData riverDropSystemData = gameSave.riverDropSystemData;
			if (riverDropSystemData.floatingDrops == null)
			{
				riverDropSystemData.floatingDrops = new List<RiverFloatingDropData>();
			}
			return gameSave.riverDropSystemData;
		}
	}

	public void ClearRuntimeState()
	{
		foreach (DropView value in views.Values)
		{
			if (value != null)
			{
				value.DespawnView();
			}
		}
		views.Clear();
		throws.Clear();
	}

	public bool BeginDump(Item item, RiverBodyReceiver receiver, Vector3 throwStartPos)
	{
		EnsureSubscribed();
		if (item == null || receiver == null || !receiver.HasFlowSpline)
		{
			Debug.LogError("[RiverDropSystem] Cannot dump body: missing item or flow spline.");
			return false;
		}
		if (!receiver.GetNearestFlowPoint(throwStartPos, out var worldPoint, out var t))
		{
			Debug.LogError("[RiverDropSystem] Failed to find nearest flow point.");
			return false;
		}
		Wgo wgo = receiver.Wgo;
		RiverFloatingDropData riverFloatingDropData = new RiverFloatingDropData
		{
			item = item,
			wgoUniqueId = ((wgo != null) ? wgo.Data.UniqueId : SGuid.Empty),
			worldId = ((wgo != null) ? wgo.Data.WorldId : MainGame.PlayerData.currentGameSceneId),
			splineIndex = 0,
			t = t,
			splineWorldLengths = receiver.CopySplineWorldLengths(),
			flowSpeed = receiver.FlowSpeed,
			fallSpeed = receiver.FallSpeed
		};
		Data.floatingDrops.Add(riverFloatingDropData);
		throws[riverFloatingDropData.UniqueId] = new ThrowSession
		{
			startPos = throwStartPos,
			endPos = worldPoint,
			elapsed = 0f,
			duration = Mathf.Max(0.01f, receiver.ThrowDuration),
			heightCurve = receiver.ThrowHeightCurve
		};
		SpawnView(riverFloatingDropData, throwStartPos, enableBob: false);
		return true;
	}

	public void HandleReceiverReady(RiverBodyReceiver receiver)
	{
		EnsureSubscribed();
		if (receiver == null || receiver.Wgo == null || Data == null)
		{
			return;
		}
		SGuid uniqueId = receiver.Wgo.Data.UniqueId;
		for (int i = 0; i < Data.floatingDrops.Count; i++)
		{
			RiverFloatingDropData riverFloatingDropData = Data.floatingDrops[i];
			if (!(riverFloatingDropData.wgoUniqueId != uniqueId) && (!views.ContainsKey(riverFloatingDropData.UniqueId) || !(views[riverFloatingDropData.UniqueId] != null)))
			{
				TrySpawnViewOnSpline(riverFloatingDropData, receiver);
			}
		}
	}

	public void HandleSceneReady(GameScene gameScene)
	{
		EnsureSubscribed();
		if (gameScene == null || Data == null)
		{
			return;
		}
		for (int i = 0; i < Data.floatingDrops.Count; i++)
		{
			RiverFloatingDropData riverFloatingDropData = Data.floatingDrops[i];
			if (!(riverFloatingDropData.worldId != gameScene.Id) && (!views.ContainsKey(riverFloatingDropData.UniqueId) || !(views[riverFloatingDropData.UniqueId] != null)))
			{
				Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(riverFloatingDropData.wgoUniqueId);
				RiverBodyReceiver riverBodyReceiver = ((wgoViewGlobal != null) ? wgoViewGlobal.RiverBodyReceiver : null);
				if (!(riverBodyReceiver == null))
				{
					TrySpawnViewOnSpline(riverFloatingDropData, riverBodyReceiver);
				}
			}
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		EnsureSubscribed();
		if (Data?.floatingDrops == null || Data.floatingDrops.Count == 0 || MainGame.IsGamePaused)
		{
			return;
		}
		for (int num = Data.floatingDrops.Count - 1; num >= 0; num--)
		{
			RiverFloatingDropData riverFloatingDropData = Data.floatingDrops[num];
			if (riverFloatingDropData?.item == null)
			{
				RemoveAt(num);
				continue;
			}
			SGuid uniqueId = riverFloatingDropData.UniqueId;
			if (throws.TryGetValue(uniqueId, out var value))
			{
				UpdateThrow(riverFloatingDropData, value, deltaTime);
				continue;
			}
			riverFloatingDropData.t += riverFloatingDropData.GetCurrentSpeed() * deltaTime / riverFloatingDropData.GetCurrentSplineLength();
			if (riverFloatingDropData.t >= 1f)
			{
				if (!riverFloatingDropData.HasMoreSplinesAfterCurrent())
				{
					RemoveAt(num);
					continue;
				}
				riverFloatingDropData.splineIndex++;
				riverFloatingDropData.t = 0f;
				SetViewBob(uniqueId, enabled: false);
			}
			ApplySplinePose(riverFloatingDropData);
		}
	}

	private void UpdateThrow(RiverFloatingDropData dropData, ThrowSession throwSession, float deltaTime)
	{
		throwSession.elapsed += deltaTime;
		float num = Mathf.Clamp01(throwSession.elapsed / throwSession.duration);
		Vector3 riverWorldPosition = Vector3.Lerp(throwSession.startPos, throwSession.endPos, num);
		float num2 = 0f;
		if (throwSession.heightCurve != null && throwSession.heightCurve.length > 0)
		{
			num2 = throwSession.heightCurve.Evaluate(num);
		}
		riverWorldPosition.y += num2;
		if (views.TryGetValue(dropData.UniqueId, out var value) && value != null)
		{
			value.SetRiverWorldPosition(riverWorldPosition);
		}
		if (!(num < 1f))
		{
			throws.Remove(dropData.UniqueId);
			SetViewBob(dropData.UniqueId, dropData.splineIndex == 0);
			ApplySplinePose(dropData);
			if (views.TryGetValue(dropData.UniqueId, out value) && value != null)
			{
				LazyAudio.PlayAtGameObject("fishing_blop", value.transform, SpatialType.sound3D);
			}
		}
	}

	private void ApplySplinePose(RiverFloatingDropData dropData)
	{
		if (views.TryGetValue(dropData.UniqueId, out var value) && !(value == null))
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(dropData.wgoUniqueId);
			RiverBodyReceiver riverBodyReceiver = ((wgoViewGlobal != null) ? wgoViewGlobal.RiverBodyReceiver : null);
			if (!(riverBodyReceiver == null) && riverBodyReceiver.TryEvaluatePose(dropData.splineIndex, dropData.t, out var worldPos))
			{
				value.SetRiverWorldPosition(worldPos);
			}
		}
	}

	private void TrySpawnViewOnSpline(RiverFloatingDropData dropData, RiverBodyReceiver receiver)
	{
		Vector3 worldPos = receiver.transform.position;
		if (receiver.TryEvaluatePose(dropData.splineIndex, dropData.t, out var worldPos2))
		{
			worldPos = worldPos2;
		}
		SpawnView(dropData, worldPos, dropData.splineIndex == 0);
	}

	private void SpawnView(RiverFloatingDropData dropData, Vector3 worldPos, bool enableBob)
	{
		if (dropData?.item == null)
		{
			return;
		}
		Transform transform = MainGame.PlayerController?.CurrentGameScene?.transform;
		if (!(transform == null) && !(MainGame.PlayerController.CurrentGameScene.Id != dropData.worldId))
		{
			DropView dropView = DropView.SpawnDrop(new DropData(dropData.item, worldPos, dropData.worldId), transform, skipWorldPlacement: true);
			if (!(dropView == null))
			{
				dropView.PrepareAsRiverDump();
				dropView.SetRiverWorldPosition(worldPos);
				dropView.SetWaterBobEnabled(enableBob);
				views[dropData.UniqueId] = dropView;
			}
		}
	}

	private void SetViewBob(SGuid id, bool enabled)
	{
		if (views.TryGetValue(id, out var value) && value != null)
		{
			value.SetWaterBobEnabled(enabled);
		}
	}

	private void RemoveAt(int index)
	{
		RiverFloatingDropData riverFloatingDropData = Data.floatingDrops[index];
		SGuid sGuid = ((riverFloatingDropData != null) ? riverFloatingDropData.UniqueId : SGuid.Empty);
		if (sGuid != SGuid.Empty)
		{
			throws.Remove(sGuid);
			if (views.TryGetValue(sGuid, out var value))
			{
				views.Remove(sGuid);
				if (value != null)
				{
					value.DespawnView();
				}
			}
		}
		Data.floatingDrops.RemoveAt(index);
	}

	private void HandleWgoSpawn(Wgo wgo)
	{
		if (!(wgo == null))
		{
			RiverBodyReceiver riverBodyReceiver = wgo.RiverBodyReceiver;
			if (riverBodyReceiver == null)
			{
				riverBodyReceiver = wgo.GetComponentInChildren<RiverBodyReceiver>(includeInactive: true);
			}
			if (riverBodyReceiver != null)
			{
				HandleReceiverReady(riverBodyReceiver);
			}
		}
	}

	private void EnsureSubscribed()
	{
		if (!subscribedToWgoSpawn)
		{
			subscribedToWgoSpawn = true;
			Wgo.OnWgoSpawn += HandleWgoSpawn;
		}
	}
}
