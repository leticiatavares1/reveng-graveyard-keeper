using System;
using System.Collections;
using LazyBearTechnology;
using UnityEngine;

public class PlayerFishingComponent : MonoBehaviour
{
	[Header("Mini-game")]
	[SerializeField]
	private Transform fishingCatchFx;

	[SerializeField]
	private float fishingCatchFxVertOffset = -0.18f;

	[SerializeField]
	private FishingMiniGame miniGame;

	[SerializeField]
	private float blinkSpeed = 10f;

	private PlayerView playerView;

	private Wgo reservoir;

	private bool isBlinking;

	private float blinkTimer;

	private float moveProgress;

	private Vector3 bobStartPos;

	private bool needToMoveToStart = true;

	private FishingSettings fishingSettings;

	private bool isStarted;

	private bool hasActivatedAtPeak;

	[Header("Bob Movement")]
	[SerializeField]
	private float bobMovementDuration = 0.1f;

	[SerializeField]
	private float bobDivingAmount = 0.3f;

	[SerializeField]
	private AnimationCurve bobMovementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve bobDivingCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, -1f), new Keyframe(1f, 0f));

	[Header("Bite Animation")]
	[SerializeField]
	private AnimationCurve biteAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, -1f), new Keyframe(1f, 0f));

	[SerializeField]
	private float biteAnimationDuration = 0.4f;

	[SerializeField]
	private float biteDiveAmount = 0.2f;

	[SerializeField]
	private float bitePauseAfterDive = 0.1f;

	[SerializeField]
	private float bitePauseAfterReturn = 0.3f;

	[Header("Rope Blink")]
	[SerializeField]
	private float ropeBlinkTensionAmount = 85f;

	[SerializeField]
	private float ropeDefaultEmission = 0.3f;

	[SerializeField]
	private AnimationCurve ropeEmissionByTension = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool IsMiniGameActive => miniGame?.IsActive ?? false;

	public FishingMiniGame MiniGame
	{
		get
		{
			return miniGame;
		}
		set
		{
			miniGame = value;
		}
	}

	private FishingContainer FishingContainer => playerView?.FishingContainer;

	private ReservoirConfig ReservoirConfig
	{
		get
		{
			ReservoirView component = reservoir.MainWgoPart.GetComponent<ReservoirView>();
			if (!(component != null))
			{
				return LazySingletonSerializedSO<FishingSettings>.Instance.DefaultReservoirConfig;
			}
			return component.Config;
		}
	}

	public void StartActivity(Wgo wgo)
	{
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFishing, isEnabled: false);
		playerView = MainGame.PlayerController.View;
		fishingSettings = LazySingletonSerializedSO<FishingSettings>.Instance;
		reservoir = wgo;
		playerView.PlayerAnimation.SetDirection(reservoir.TryGetDockPointForWorker().Direction);
		playerView.PlayerAnimation.SetState(AnimationState.FishingStart);
		isStarted = true;
	}

	public void StopActivity(bool forceStop = false, Action onFinish = null)
	{
		if (forceStop)
		{
			miniGame?.CancelGame();
			StartCoroutine(OnFinishMiniGame(openChoicePanel: false, onFinish));
			needToMoveToStart = true;
			isStarted = false;
			LazyUI.GetWindow<UIFishingWindow>().Close();
			playerView.FishingContainer.gameObject.SetActive(value: false);
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFishing, isEnabled: true);
			playerView.PlayerAnimation.SetState(AnimationState.Idle);
			return;
		}
		if (miniGame != null && miniGame.IsActive)
		{
			miniGame.FinishGame();
			return;
		}
		if (miniGame != null)
		{
			miniGame.CancelGame();
		}
		needToMoveToStart = true;
		isStarted = false;
		LazyUI.GetWindow<UIFishingWindow>().Close();
		playerView.FishingContainer.gameObject.SetActive(value: false);
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFishing, isEnabled: true);
		playerView.PlayerAnimation.SetState(AnimationState.Idle);
	}

	protected void LateUpdate()
	{
		if (!isStarted)
		{
			return;
		}
		miniGame?.UpdateGame();
		if (IsMiniGameActive)
		{
			FishingMiniGame fishingMiniGame = miniGame;
			if (fishingMiniGame != null && fishingMiniGame.CurrentStage == FishingMiniGame.Stage.PlayingWithFish)
			{
				if (FishingContainer?.Bob != null && ReservoirConfig != null)
				{
					Vector3 vector = new Vector3(MainGame.PlayerData.Direction.x, 0f, 0f);
					Vector3 vector2 = bobStartPos - vector * (Mathf.Clamp(miniGame.progress / 100f, -1f, 1f) * ReservoirConfig.progressRange);
					float num = miniGame?.IdleDriftOffset ?? 0f;
					Vector3 vector3 = miniGame?.IdleDriftAxis ?? Vector3.forward;
					Vector3 position = vector2 + vector3 * num;
					FishingContainer.Bob.transform.position = position;
				}
				float tension = miniGame.tension;
				float ropeEmission = Mathf.Max(ropeDefaultEmission, ropeEmissionByTension.Evaluate(tension / 100f));
				FishingContainer?.SetRopeEmission(ropeEmission);
				if (tension >= ropeBlinkTensionAmount)
				{
					if (!isBlinking)
					{
						isBlinking = true;
						blinkTimer = 0f;
					}
					blinkTimer += Time.deltaTime * blinkSpeed;
					blinkTimer = MathUtilities.ClampCycle(blinkTimer, 0f, 1f);
					Color ropeColor = fishingSettings.ropeGradient.Evaluate(blinkTimer);
					FishingContainer?.SetRopeColor(ropeColor);
				}
				else
				{
					isBlinking = false;
					Color ropeColor2 = fishingSettings.ropeGradient.Evaluate(Mathf.Clamp01(tension / 100f) * 0.8f);
					FishingContainer?.SetRopeColor(ropeColor2);
				}
				return;
			}
		}
		if (!IsMiniGameActive && FishingContainer.gameObject.activeInHierarchy && needToMoveToStart)
		{
			BringBobToStart();
		}
	}

	private void BringBobToStart()
	{
		moveProgress += Time.deltaTime / bobMovementDuration;
		float time = Mathf.Clamp01(moveProgress);
		float num = bobMovementCurve.Evaluate(time);
		Vector3 vector = Vector3.Lerp(FishingContainer.CastStartPoint.position, bobStartPos, num);
		float num2 = bobDivingCurve.Evaluate(time) * bobDivingAmount;
		Vector3 position = vector;
		position.y += num2;
		FishingContainer.Bob.transform.position = position;
		if (num >= 0.2f && !hasActivatedAtPeak)
		{
			if (playerView.PlayerAnimation.Animator.TryGetComponent<FXContainerEventListener>(out var component))
			{
				fishingCatchFx.position = new Vector3(FishingContainer.Bob.GetWorldPosition().x, bobStartPos.y + fishingCatchFxVertOffset, FishingContainer.Bob.GetWorldPosition().z);
				component.PlayFx("fishing_catch");
				LazyAudio.PlayAtGameObject("fishing_blop", base.transform, SpatialType.sound3D);
			}
			hasActivatedAtPeak = true;
		}
		if (moveProgress >= 1f)
		{
			needToMoveToStart = false;
			moveProgress = 0f;
			hasActivatedAtPeak = false;
		}
	}

	public void RunMiniGame(FishingDef fishingDef, WgoData wgoData, ItemDef fishingRodDef)
	{
		StartCoroutine(StartMiniGame(fishingDef, wgoData, fishingRodDef));
	}

	private IEnumerator StartMiniGame(FishingDef fishingDef, WgoData wgoData, ItemDef fishingRodDef)
	{
		UIFishingWindow fishingWindow = LazyUI.GetWindow<UIFishingWindow>();
		fishingWindow.SetWindowVisible(isVisible: false);
		bobStartPos = GetBobStartPos();
		playerView.PlayerAnimation.SetState(AnimationState.FishingCast);
		playerView.FishingContainer.AnimationComponent.SetTrigger("fishCast");
		yield return new WaitUntil(() => !needToMoveToStart);
		LazyAudio.PlayAtGameObject("fishing_bite_success", base.transform, SpatialType.sound3D);
		playerView.FishingContainer.AnimationComponent.SetTrigger("fishStart");
		playerView.PlayerAnimation.SetState(AnimationState.FishingIdle);
		miniGame = new FishingMiniGame();
		miniGame.StartNewGame(fishingDef, wgoData, fishingRodDef, OnPhaseChange);
		fishingWindow.SetWindowVisible(isVisible: true);
	}

	private IEnumerator DoBiteAnimation()
	{
		Vector3 startPos = playerView.FishingContainer.Bob.transform.position;
		float timer = 0f;
		while (timer < biteAnimationDuration)
		{
			timer += Time.deltaTime;
			float time = timer / biteAnimationDuration;
			float num = biteAnimationCurve.Evaluate(time);
			Vector3 position = startPos + Vector3.up * (num * biteDiveAmount);
			playerView.FishingContainer.Bob.transform.position = position;
			yield return null;
		}
		playerView.FishingContainer.Bob.transform.position = startPos;
		yield return new WaitForSeconds(bitePauseAfterReturn);
	}

	private IEnumerator OnFinishMiniGame(bool openChoicePanel = false, Action onFinish = null)
	{
		UIFishingWindow fishingWindow = LazyUI.GetWindow<UIFishingWindow>();
		fishingCatchFx.position = new Vector3(FishingContainer.Bob.GetWorldPosition().x, bobStartPos.y + fishingCatchFxVertOffset, FishingContainer.Bob.GetWorldPosition().z);
		FishingContainer.Bob.transform.position = FishingContainer.CastStartPoint.position;
		if (openChoicePanel)
		{
			fishingWindow.SetWindowVisible(isVisible: false);
		}
		while (true)
		{
			playerView.PlayerAnimation.SetState(AnimationState.FishingCatch);
			if (playerView.PlayerAnimation.Animator.GetCurrentAnimatorStateInfo(0).IsName("FishingCatch"))
			{
				break;
			}
			yield return null;
		}
		yield return null;
		while (!Mathf.Clamp01(playerView.PlayerAnimation.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime).EqualsTo(1f, 0.01f))
		{
			yield return null;
		}
		needToMoveToStart = true;
		if (openChoicePanel)
		{
			fishingWindow.DisplayChoicePanel();
			fishingWindow.SetWindowVisible(isVisible: true);
		}
		else
		{
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByFishing, isEnabled: true);
			fishingWindow.Close();
			playerView.PlayerAnimation.SetState(AnimationState.Idle);
		}
		yield return null;
		onFinish?.Invoke();
	}

	private void OnPhaseChange(FishingMiniGame.Stage newStage)
	{
		switch (newStage)
		{
		case FishingMiniGame.Stage.Start:
			playerView.PlayerAnimation.SetState(AnimationState.FishingIdle);
			LazyUI.GetWindow<UIFishingWindow>().DisplayChoicePanel();
			break;
		case FishingMiniGame.Stage.Biting:
			StartCoroutine(DoBiteAnimation());
			break;
		case FishingMiniGame.Stage.Finish:
			FishingContainer.gameObject.SetActive(value: false);
			FishingContainer.ResetColor();
			FishingContainer.SetRopeEmission(ropeDefaultEmission);
			StartCoroutine(OnFinishMiniGame(openChoicePanel: true));
			break;
		case FishingMiniGame.Stage.WaitingForBite:
		case FishingMiniGame.Stage.PlayingWithFish:
			break;
		}
	}

	private Vector3 GetBobStartPos()
	{
		Vector3 vector = new Vector3(MainGame.PlayerData.Direction.x, 0f, 0f);
		return MainGame.PlayerController.transform.position + vector.normalized * ReservoirConfig.fishSpawnHorOffset + Vector3.down * ReservoirConfig.fishSpawnVertOffset;
	}
}
