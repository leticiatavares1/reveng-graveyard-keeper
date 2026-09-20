using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PlayerWorkComponent : MonoBehaviour
{
	public Action OnInteractionUpdate;

	private PlayerLocalAreaMovement playerLocalAreaMovement;

	private PlayerController playerController;

	private ToolComponent toolComponent;

	private Wgo currentWgo;

	private WgoData currentWgoData;

	private DockPoint currentDockPoint;

	private bool workInProgress;

	private bool isActive = true;

	private WaitForFixedUpdate onePhysicFrameTime = new WaitForFixedUpdate();

	private Collider[] dockPointResults = new Collider[100];

	private bool isMagnetismDelayed;

	private bool canUpdateMagnetismDelayTimer;

	private float magnetismDelayTimer;

	public bool WorkIsTookControl => toolComponent.IsControlTakenByAnimation;

	public bool WorkInProgress => workInProgress;

	public bool IsMoving => playerLocalAreaMovement.IsMovementStarted;

	public bool IsActive => isActive;

	public ToolComponent ToolComponent => toolComponent;

	public Wgo Wgo => currentWgo;

	public bool CanUpdateMagnetismDelayTimer => canUpdateMagnetismDelayTimer;

	public void Init(PlayerController playerController, ToolComponent toolComponent, PlayerLocalAreaMovement playerLocalAreaMovement)
	{
		this.playerLocalAreaMovement = playerLocalAreaMovement;
		this.playerController = playerController;
		this.toolComponent = toolComponent;
	}

	public bool TryStartInteraction()
	{
		if (currentWgo == null)
		{
			Wgo wgoUnderInteraction = playerController.PlayerInteractionComponent.WgoUnderInteraction;
			FindWgoToWork(wgoUnderInteraction ? new List<Wgo> { wgoUnderInteraction } : null);
		}
		if (currentWgo == null)
		{
			return false;
		}
		if (playerLocalAreaMovement.IsMovementStarted && !IsOnWorkingSpot(currentWgo))
		{
			return true;
		}
		currentWgoData = currentWgo.Data;
		IComponent componentForInteraction = GetComponentForInteraction(currentWgo);
		if (componentForInteraction == null)
		{
			return false;
		}
		if (componentForInteraction.GetType() == typeof(CraftComponent))
		{
			playerController.SetCraftActivity(currentWgo.Data);
		}
		else
		{
			playerController.SetHPActivity(currentWgo.Data);
		}
		playerController.WorkerActivity.OnStartActivity();
		Item appropriateToolForWork = GetAppropriateToolForWork();
		if (appropriateToolForWork == null || appropriateToolForWork.IsEmpty)
		{
			return false;
		}
		if (!IsOnWorkingSpot(currentWgo))
		{
			if (playerLocalAreaMovement.IsMovementStarted)
			{
				return true;
			}
			playerLocalAreaMovement.StartMovement(currentDockPoint.transform.position, currentDockPoint.Direction);
			return true;
		}
		if (playerLocalAreaMovement.IsMovementStarted)
		{
			playerLocalAreaMovement.StopMovement();
		}
		playerController.SetPosition(currentDockPoint.transform.position, updateWispWgoData: true, instantCameraUpdate: false);
		AlignPlayerToDockPoint();
		if (!toolComponent.TryStartInteraction(playerController.WorkerActivity, appropriateToolForWork, currentDockPoint.Direction))
		{
			return false;
		}
		return true;
	}

	public void UpdateInteraction(float deltaTime)
	{
		OnInteractionUpdate?.Invoke();
		if (currentWgo == null || !currentWgo.HasData)
		{
			if (workInProgress)
			{
				StopInteraction();
				StartCoroutine(PauseComponentForOneFrame());
				return;
			}
			if (!TryStartInteraction())
			{
				StopInteraction();
			}
		}
		workInProgress = false;
		if (currentWgo != null)
		{
			if (IsOnWorkingSpot(currentWgo))
			{
				if (playerLocalAreaMovement.IsMovementStarted)
				{
					playerLocalAreaMovement.StopMovement();
				}
				if (!playerLocalAreaMovement.IsMovementStarted && !WorkIsTookControl)
				{
					AlignPlayerToDockPoint();
				}
				if ((toolComponent.IsActionActive || TryStartInteraction()) && toolComponent.IsActionActive)
				{
					toolComponent.TryUpdateToolInUse(GetAppropriateToolForWork());
					toolComponent.UpdateInteraction();
					workInProgress = true;
				}
			}
			else if (playerLocalAreaMovement.Status != PlayerLocalAreaMovement.MovementStatus.CalcPath)
			{
				playerLocalAreaMovement.CustomUpdate(deltaTime);
			}
		}
		else
		{
			StopInteraction();
		}
	}

	public void StopInteraction()
	{
		playerLocalAreaMovement.StopMovement(force: true);
		playerController.ClearWorkActivity();
		toolComponent.StopInteraction();
		currentWgo = null;
		currentWgoData = null;
		currentDockPoint = null;
		workInProgress = false;
		OnInteractionUpdate?.Invoke();
	}

	public void FindWgoToWork(List<Wgo> onlyWgoInList)
	{
		if (isMagnetismDelayed)
		{
			canUpdateMagnetismDelayTimer = true;
			return;
		}
		if (onlyWgoInList != null && onlyWgoInList.Count > 0)
		{
			currentWgo = FindWgoToWorkWith(ignorePlayerDirection: true, onlyWgoInList);
		}
		if (currentWgo == null)
		{
			currentWgo = FindWgoToWorkWith();
		}
		if (currentWgo != null)
		{
			currentWgoData = currentWgo.Data;
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerFindWgoToWork, currentWgo.Id ?? "");
			SetMagnetismDelayState(isDelayed: true);
		}
	}

	public Wgo FindWgoToWorkNoAssign(List<Wgo> onlyWgoInList)
	{
		Wgo wgo = null;
		if (onlyWgoInList != null && onlyWgoInList.Count > 0)
		{
			wgo = FindWgoToWorkWith(ignorePlayerDirection: true, onlyWgoInList);
		}
		if (wgo == null)
		{
			wgo = FindWgoToWorkWith();
		}
		return wgo;
	}

	private Item GetAppropriateToolForWork()
	{
		ItemType requiredInteractionToolType = currentWgo.InteractionHandler.GetRequiredInteractionToolType();
		Item itemByType = playerController.PlayerData.toolBeltInventory.Data.GetItemByType(requiredInteractionToolType);
		if (!itemByType.IsEmpty)
		{
			return itemByType;
		}
		return Item.Empty;
	}

	private IComponent GetComponentForInteraction(Wgo wgo)
	{
		CraftComponent craftComponent = wgo.Data.CraftComponent;
		if (craftComponent.IsManualActualCraftable && craftComponent.HasCraftsInQueue)
		{
			return craftComponent;
		}
		if (wgo.Data.Definition.hp > 0 || wgo.Data.Definition.hasInfiniteHp)
		{
			return wgo.Data.HpComponent;
		}
		return null;
	}

	private Wgo FindWgoToWorkWith(bool ignorePlayerDirection = false, List<Wgo> onlyWgoInList = null)
	{
		PlayerData playerData = playerController.PlayerData;
		DockPoint dockPoint = (currentDockPoint = FindNearestDockPoint(playerData.position.Value, playerData.Direction, ignorePlayerDirection, onlyWgoInList));
		if (dockPoint != null && dockPoint.Owner.Data.IsInteractable)
		{
			return dockPoint.Owner;
		}
		return null;
	}

	public bool IsOnWorkingSpot(Wgo wgo)
	{
		if (currentDockPoint != null)
		{
			return (currentDockPoint.transform.position - playerController.transform.position).sqrMagnitude < 0.01f;
		}
		return false;
	}

	private void AlignPlayerToDockPoint()
	{
		if (!(currentDockPoint == null) && currentDockPoint.Direction != 0)
		{
			playerController.PhysicalBody.SetFacingDirection(currentDockPoint.Direction.ConvertToVector2XZ());
		}
	}

	private DockPoint FindNearestDockPoint(Vector3 position, Vector2 direction, bool ignorePlayerDirection = false, List<Wgo> onlyWgoInList = null)
	{
		List<DockPoint> list = new List<DockPoint>();
		List<DockPoint> list2 = new List<DockPoint>();
		playerLocalAreaMovement.RescanPlayerGraph();
		if (onlyWgoInList == null)
		{
			Array.Clear(dockPointResults, 0, dockPointResults.Length);
			Debug.DrawLine(position, position + Vector3.right * 1.5f);
			Debug.DrawLine(position, position + Vector3.left * 1.5f);
			Debug.DrawLine(position, position + new Vector3(0f, 0f, 1f) * 1.5f);
			Debug.DrawLine(position, position + new Vector3(0f, 0f, -1f) * 1.5f);
			int b = Physics.OverlapSphereNonAlloc(position, 1.5f, dockPointResults, 128);
			for (int i = 0; i < Mathf.Min(dockPointResults.Length, b); i++)
			{
				if (dockPointResults[i].TryGetComponent<DockPoint>(out var component))
				{
					list2.Add(component);
				}
			}
		}
		else
		{
			foreach (Wgo onlyWgoIn in onlyWgoInList)
			{
				list2.AddRange(onlyWgoIn.DockPoints);
			}
		}
		foreach (DockPoint item in list2)
		{
			if (item.gameObject.activeInHierarchy && (bool)item.Owner && item.Owner.MainWgoPart.InteractableColliders != null && item.Owner.MainWgoPart.InteractableColliders.Count != 0 && CanWorkOn(item.Owner.Data) && playerLocalAreaMovement.IsReachable(item.transform.position))
			{
				list.Add(item);
			}
		}
		float num = float.MaxValue;
		DockPoint result = null;
		foreach (DockPoint item2 in list)
		{
			bool flag = onlyWgoInList != null;
			Vector3 position2 = item2.transform.position;
			Path path = ABPath.Construct(position, position2);
			playerLocalAreaMovement.Seeker.StartPath(path);
			path.BlockUntilCalculated();
			float num2 = path.GetTotalLength();
			float num3 = Vector2.Angle((Vector2)(position2 - position), direction);
			if (!ignorePlayerDirection)
			{
				num2 += num3 * 0.0026666666f;
			}
			if (num2 < num && (flag || num2 <= 2.2f))
			{
				num = num2;
				result = item2;
			}
		}
		return result;
	}

	public bool CanWorkOn(WgoData wgoData)
	{
		if (wgoData.Worker != null)
		{
			return false;
		}
		HPComponent hpComponent = wgoData.HpComponent;
		CraftComponent craftComponent = wgoData.CraftComponent;
		bool flag = (hpComponent.Hp > 0 && wgoData.Definition.playerHpActivityMod > 0) || (hpComponent.Hp < hpComponent.MaxHpValue && wgoData.Definition.playerHpActivityMod < 0);
		if (craftComponent.CraftableObject.CraftableType == CraftableType.ConveyorWorkbench && !craftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if ((flag && !hpComponent.isDeathDelayed) || (craftComponent.IsManualActualCraftable && craftComponent.HasCraftsInQueue))
		{
			return true;
		}
		return false;
	}

	public void UpdateMagnetismDelay(float deltaTime)
	{
		if (isMagnetismDelayed && canUpdateMagnetismDelayTimer)
		{
			magnetismDelayTimer += deltaTime;
			if (magnetismDelayTimer >= 0.25f)
			{
				SetMagnetismDelayState(isDelayed: false);
			}
		}
	}

	public void SetMagnetismDelayState(bool isDelayed)
	{
		isMagnetismDelayed = isDelayed;
		magnetismDelayTimer = 0f;
		canUpdateMagnetismDelayTimer = false;
	}

	private IEnumerator PauseComponentForOneFrame()
	{
		isActive = false;
		yield return onePhysicFrameTime;
		isActive = true;
	}
}
