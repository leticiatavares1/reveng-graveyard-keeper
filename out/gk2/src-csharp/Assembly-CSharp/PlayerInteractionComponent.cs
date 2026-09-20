using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PlayerInteractionComponent : MonoBehaviour
{
	private const int COLLIDERS_LIMIT_COUNT = 8;

	public Action<Wgo> OnInteractionTargetEnter;

	public Action<Wgo> OnInteractionTargetChanged;

	public Action OnInteractionTargetExit;

	public Action<DropView> OnInteractionBigDropTargetEnter;

	public Action<DropView> OnInteractionBigDropTargetChanged;

	public Action OnInteractionBigDropTargetExit;

	[SerializeField]
	private BoxCollider interactionCollider;

	private Vector3 initialLocalPosition;

	private Collider[] interactionTargets = new Collider[8];

	private List<Wgo> wgoTargets = new List<Wgo>();

	private Wgo wgoUnderInteraction;

	private bool hasWgoUnderInteraction;

	private PlayerData playerData;

	private DropView bigDropUnderInteraction;

	private bool hasBigDropUnderInteraction;

	private MultiFlagAND<PlayerInteractionPauseType> pauseMultiFlag = new MultiFlagAND<PlayerInteractionPauseType>();

	public bool HasWgoUnderInteraction => hasWgoUnderInteraction;

	public DropView BigDropUnderInteraction => bigDropUnderInteraction;

	public Wgo WgoUnderInteraction => wgoUnderInteraction;

	public bool IsPaused => !pauseMultiFlag.ResultFlag;

	public void Init()
	{
		if (interactionCollider == null)
		{
			Debug.Log("Incorrect InteractionComponent initialization: cannot find interaction collider");
		}
		pauseMultiFlag.Init(null, initialFlag: true);
		initialLocalPosition = base.transform.localPosition;
		wgoTargets.Capacity = 8;
		GameSceneData.OnWgoDataOnScenePreRemove += HandleWgoDataRemoved;
		GameSceneData.OnDropOnScenePreRemoved += HandeDropRemove;
		MainGame.PlayerController.OnControlStateChanged += OnPlayerControlStateChanged;
		HandleWgoInteractionExit(wgoUnderInteraction);
	}

	public void SetPlayerData(PlayerData playerData)
	{
		this.playerData = playerData;
	}

	public void ResetInteractionState()
	{
		DoLeaveWgoInteraction();
		Update();
	}

	public void SetPauseState(PlayerInteractionPauseType type, bool isPaused)
	{
		Debug.Log($"Player SetPauseState:[{type}] isPaused:[{isPaused}]");
		pauseMultiFlag.UpdateFlag(type, !isPaused);
		interactionCollider.enabled = !IsPaused;
		if (IsPaused)
		{
			DoLeaveWgoInteraction();
			DoLeaveDropViewInteraction();
		}
	}

	public DropView TryGetClosestInteractionTarget()
	{
		if (playerData == null)
		{
			return null;
		}
		Array.Clear(interactionTargets, 0, 8);
		base.transform.localPosition = Vector3.Scale(initialLocalPosition, new Vector3(playerData.Direction.x, 1f, playerData.Direction.y));
		Physics.OverlapBoxNonAlloc(base.transform.position, interactionCollider.bounds.extents, interactionTargets, Quaternion.identity, 65600, QueryTriggerInteraction.Collide);
		TryGetClosestDropViewFromColliders(interactionTargets, out var dropView);
		return dropView;
	}

	private void Update()
	{
		if (IsPaused || playerData == null)
		{
			return;
		}
		DropView dropView = TryGetClosestInteractionTarget();
		if (dropView != null && dropView.Data.Size == ItemSize.Big && TryInteractWithDropView(dropView))
		{
			DoLeaveWgoInteraction();
			return;
		}
		DoLeaveDropViewInteraction();
		wgoTargets.Clear();
		GetWgoTargetsFromColliders(interactionTargets, wgoTargets);
		if (TryFindClosestWgo(wgoTargets, out var closestWgo))
		{
			if (!hasWgoUnderInteraction)
			{
				DoEnterInteraction(closestWgo);
			}
			else if (closestWgo.Data.UniqueId != wgoUnderInteraction.Data.UniqueId)
			{
				Debug.Log("Changing interaction wgo");
				DoLeaveWgoInteraction();
				DoEnterInteraction(closestWgo);
			}
		}
		else
		{
			DoLeaveWgoInteraction();
		}
	}

	private void HandleWgoDataRemoved(string gameSceneId, WgoData removedWgo)
	{
		if (!(wgoUnderInteraction == null) && wgoUnderInteraction.Data.UniqueId == removedWgo.UniqueId)
		{
			DoLeaveWgoInteraction();
		}
	}

	private void HandeDropRemove(string gameSceneId, DropData removedDrop)
	{
		if (!(bigDropUnderInteraction == null) && bigDropUnderInteraction.Data.UniqueId == removedDrop.UniqueId)
		{
			DoLeaveDropViewInteraction();
		}
	}

	private void DoEnterInteraction(Wgo wgo)
	{
		if (MainGame.PlayerController.IsControlsEnabled && (!MainGame.PlayerData.isInTutorialMode || MainGame.PlayerData.tutorialModeExcludedList.Contains(wgo.Data.UniqueId.Id)))
		{
			wgoUnderInteraction = wgo;
			hasWgoUnderInteraction = true;
			HandleWgoInteractionEnter(wgoUnderInteraction);
			OnInteractionTargetEnter?.Invoke(wgoUnderInteraction);
			Debug.Log("Interaction enter with [" + wgoUnderInteraction.Data.id + "]");
		}
	}

	private void DoEnterInteraction(DropView dropView)
	{
		bigDropUnderInteraction = dropView;
		hasBigDropUnderInteraction = true;
		OnInteractionBigDropTargetEnter?.Invoke(dropView);
		HandleDropInteractionEnter(dropView);
		Debug.Log("Drop interaction enter with [" + bigDropUnderInteraction.Data.Id + "]");
	}

	private void DoLeaveWgoInteraction()
	{
		if (hasWgoUnderInteraction)
		{
			Debug.Log("Interaction exit from [" + wgoUnderInteraction.Data.id + "]");
			Wgo interactedWgo = wgoUnderInteraction;
			hasWgoUnderInteraction = false;
			wgoUnderInteraction = null;
			HandleWgoInteractionExit(interactedWgo);
			OnInteractionTargetExit?.Invoke();
		}
	}

	private void DoLeaveDropViewInteraction()
	{
		if (hasBigDropUnderInteraction)
		{
			Debug.Log("Drop interaction exit from [" + bigDropUnderInteraction.Data.Id + "]");
			HandleDropInteractionExit();
			bigDropUnderInteraction = null;
			hasBigDropUnderInteraction = false;
			OnInteractionBigDropTargetExit?.Invoke();
		}
	}

	private bool TryFindClosestWgo(List<Wgo> wgos, out Wgo closestWgo)
	{
		closestWgo = null;
		if (wgos.Count == 0)
		{
			return false;
		}
		closestWgo = wgos[0];
		float num = float.PositiveInfinity;
		for (int i = 0; i < wgos.Count; i++)
		{
			Wgo wgo = wgos[i];
			Debug.DrawLine(base.transform.position + Vector3.up, wgo.Data.Position + Vector3.up, Color.magenta);
			float magnitude = (wgo.Data.Position - base.transform.position).magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				closestWgo = wgo;
			}
		}
		return true;
	}

	private bool TryInteractWithDropView(DropView dropView)
	{
		if (dropView == null || dropView.Data.Size != ItemSize.Big)
		{
			return false;
		}
		if (!hasBigDropUnderInteraction)
		{
			DoEnterInteraction(dropView);
			return true;
		}
		if (dropView.Data.UniqueId != bigDropUnderInteraction.Data.UniqueId)
		{
			Debug.Log("Changing interaction drop");
			DoLeaveDropViewInteraction();
			DoEnterInteraction(dropView);
		}
		return true;
	}

	private bool TryGetClosestDropViewFromColliders(Collider[] colliders, out DropView dropView)
	{
		dropView = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] == null)
			{
				continue;
			}
			DropView componentInParent = colliders[i].gameObject.GetComponentInParent<DropView>();
			if (componentInParent != null && !componentInParent.IsDespawning && !componentInParent.IsRiverDump)
			{
				float num2 = Vector3.Distance(componentInParent.Data.Position, base.transform.position);
				if (componentInParent != dropView && num2 < num)
				{
					dropView = componentInParent;
					num = num2;
				}
			}
		}
		return dropView != null;
	}

	private void GetWgoTargetsFromColliders(Collider[] colliders, List<Wgo> wgoTargets)
	{
		foreach (Collider collider in colliders)
		{
			if (!(collider == null))
			{
				Wgo componentInParent = collider.GetComponentInParent<Wgo>();
				if (!(componentInParent == null) && !componentInParent.IsDespawning && !wgoTargets.Contains(componentInParent) && componentInParent.Data.IsInteractable && (!(componentInParent.InteractionHandler is ConveyorCellInteractionHandler conveyorCellInteractionHandler) || conveyorCellInteractionHandler.HasInteraction(MainGame.PlayerController)))
				{
					wgoTargets.Add(componentInParent);
				}
			}
		}
	}

	private void OnDestroy()
	{
		GameSceneData.OnWgoDataOnScenePreRemove -= HandleWgoDataRemoved;
	}

	private void HandleWgoInteractionEnter(Wgo wgo)
	{
		wgo.InteractionHandler.OnInteractionTargetEnter(MainGame.PlayerController);
	}

	private void HandleWgoInteractionExit(Wgo interactedWgo)
	{
		if (!(interactedWgo == null))
		{
			interactedWgo.InteractionHandler.OnInteractionTargetExit();
		}
	}

	private void HandleDropInteractionEnter(DropView dropView)
	{
		dropView.InteractionHandler.OnInteractionTargetEnter();
		dropView.SetInteractionVisualState(isUnderInteraction: true);
	}

	private void HandleDropInteractionExit()
	{
		if (!(bigDropUnderInteraction == null))
		{
			bigDropUnderInteraction.SetInteractionVisualState(isUnderInteraction: false);
			bigDropUnderInteraction.InteractionHandler.OnInteractionTargetExit();
		}
	}

	private void OnPlayerControlStateChanged()
	{
		if (!MainGame.PlayerController.IsControlsEnabled)
		{
			SetPauseState(PlayerInteractionPauseType.ByControl, isPaused: true);
			ResetInteractionState();
		}
		else
		{
			SetPauseState(PlayerInteractionPauseType.ByControl, isPaused: false);
			Update();
		}
		GameScene.RedrawWgosWithInteractionEvents();
	}
}
