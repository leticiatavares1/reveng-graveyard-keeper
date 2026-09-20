using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class FlyingTechPoint : MonoBehaviour
{
	private static Pool pool;

	private static Dictionary<string, int> flyingTechPointsCountByName = new Dictionary<string, int>();

	[SerializeField]
	private TextMeshProUGUI label;

	private string techPointName;

	private Tweener moveTween;

	private float delay;

	private Action onReachedDestination;

	private bool useUnscaledTime;

	private bool inFlight;

	private bool rewardApplied;

	private bool releasedToPool;

	private Action pendingShow;

	public static Dictionary<string, int> FlyingTechPointsCountByName => flyingTechPointsCountByName;

	public static FlyingTechPoint DropFromUI(Vector3 uiWorldPos, string techPointName, Action onReachedDestination = null, int overrodeSorting = -1)
	{
		return Drop(uiWorldPos, techPointName, onReachedDestination, overrodeSorting, posIsUiWorldPosition: true);
	}

	public static FlyingTechPoint Drop(Vector3 pos, string techPointName, Action onReachedDestination = null, int overrodeSorting = -1)
	{
		return Drop(pos, techPointName, onReachedDestination, overrodeSorting, posIsUiWorldPosition: false);
	}

	private static FlyingTechPoint Drop(Vector3 pos, string techPointName, Action onReachedDestination, int overrodeSorting, bool posIsUiWorldPosition)
	{
		if (pool == null)
		{
			pool = LazyPooler.CreatePool(Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Drops/FlyingTechPoint.prefab").WaitForCompletion().GetComponent<FlyingTechPoint>(), 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		}
		HUD hud = LazyUI.Get<HUD>();
		FlyingTechPoint drop = pool.GetOrCreateObject<FlyingTechPoint>();
		drop.ResetFlightState();
		drop.onReachedDestination = onReachedDestination;
		drop.techPointName = techPointName;
		drop.useUnscaledTime = posIsUiWorldPosition;
		drop.inFlight = true;
		if (!string.IsNullOrEmpty(techPointName) && !flyingTechPointsCountByName.TryAdd(techPointName, 1))
		{
			flyingTechPointsCountByName[techPointName]++;
		}
		if (techPointName == "happiness" || hud.TryTurnOnTechPointsPanel())
		{
			Show();
		}
		else
		{
			drop.pendingShow = Show;
			hud.onTechPointsPanelShown += Show;
		}
		return drop;
		void Show()
		{
			if (drop.inFlight && !drop.rewardApplied)
			{
				TextMeshProUGUI hudLabel = hud.GetHudLabel(techPointName);
				Transform transform = ((hudLabel != null) ? hudLabel.transform.parent : null);
				if (transform == null || !transform.gameObject.activeInHierarchy)
				{
					drop.CompleteImmediately();
				}
				else
				{
					drop.transform.SetParent(transform);
					drop.gameObject.SetActive(value: true);
					Vector2 to = hudLabel.transform.position;
					drop.transform.position = (posIsUiWorldPosition ? pos : CameraSystem.WorldToScreenPoint(pos));
					drop.label.text = techPointName.FontIcon();
					drop.delay = UnityEngine.Random.Range(0f, 0.2f);
					drop.transform.localScale = Vector3.one;
					drop.StartFly(to);
					if (drop.pendingShow != null)
					{
						hud.onTechPointsPanelShown -= drop.pendingShow;
						drop.pendingShow = null;
					}
					if (!drop.TryGetComponent<Canvas>(out var component))
					{
						component = drop.gameObject.AddComponent<Canvas>();
					}
					if (overrodeSorting == -1)
					{
						component.overrideSorting = false;
					}
					else
					{
						component.overrideSorting = true;
						component.sortingOrder = overrodeSorting;
					}
				}
			}
		}
	}

	public void CompleteImmediately()
	{
		if (!rewardApplied)
		{
			CancelPendingShow();
			ApplyRewardAndFinish(releaseToPool: true);
		}
	}

	public static void Clear()
	{
		flyingTechPointsCountByName.Clear();
	}

	private void StartFly(Vector2 to)
	{
		EnsureTweenStopped();
		moveTween = base.transform.DOMove(to, 1f + delay).OnComplete(OnReachedDestination).SetEase(Ease.OutCubic)
			.SetUpdate(useUnscaledTime);
	}

	private void OnReachedDestination()
	{
		ApplyRewardAndFinish(releaseToPool: true);
	}

	private void OnDisable()
	{
		if (inFlight && !rewardApplied)
		{
			ApplyRewardAndFinish(releaseToPool: false);
			ReleaseToPoolNextFrame();
		}
	}

	private void ApplyRewardAndFinish(bool releaseToPool)
	{
		if (rewardApplied)
		{
			return;
		}
		rewardApplied = true;
		inFlight = false;
		EnsureTweenStopped();
		if (!string.IsNullOrEmpty(techPointName))
		{
			if (flyingTechPointsCountByName.ContainsKey(techPointName))
			{
				flyingTechPointsCountByName[techPointName]--;
			}
			GK2GameResSystem.GetSystem(techPointName).Add(1f, silent: true);
			LazyUI.Get<HUD>().UpdateTechPointsInstant();
		}
		Action action = onReachedDestination;
		onReachedDestination = null;
		action?.Invoke();
		if (releaseToPool)
		{
			ReleaseToPoolIfNeeded();
		}
	}

	private void ReleaseToPoolIfNeeded()
	{
		if (!releasedToPool && pool != null)
		{
			releasedToPool = true;
			pool.ReleaseObject(this);
		}
	}

	private async void ReleaseToPoolNextFrame()
	{
		await Awaitable.NextFrameAsync(base.destroyCancellationToken);
		if (!(this == null))
		{
			ReleaseToPoolIfNeeded();
		}
	}

	private void ResetFlightState()
	{
		CancelPendingShow();
		EnsureTweenStopped();
		inFlight = false;
		rewardApplied = false;
		releasedToPool = false;
		useUnscaledTime = false;
		onReachedDestination = null;
		techPointName = null;
		pendingShow = null;
	}

	private void CancelPendingShow()
	{
		if (pendingShow != null)
		{
			HUD hUD = LazyUI.Get<HUD>();
			if (hUD != null)
			{
				hUD.onTechPointsPanelShown -= pendingShow;
			}
			pendingShow = null;
		}
	}

	private void EnsureTweenStopped()
	{
		Tweener tweener = moveTween;
		moveTween = null;
		if (tweener != null && tweener.IsActive())
		{
			tweener.Kill();
		}
	}
}
