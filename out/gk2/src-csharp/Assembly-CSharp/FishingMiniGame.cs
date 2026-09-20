using System;
using System.Collections;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class FishingMiniGame
{
	public enum Stage
	{
		Start,
		WaitingForBite,
		Biting,
		PlayingWithFish,
		Finish
	}

	private class FishingCoroutineRunner : MonoBehaviour
	{
		private static FishingCoroutineRunner instance;

		private static FishingCoroutineRunner Instance
		{
			get
			{
				if (instance == null)
				{
					GameObject obj = new GameObject("FishingMiniGameRunner");
					UnityEngine.Object.DontDestroyOnLoad(obj);
					instance = obj.AddComponent<FishingCoroutineRunner>();
				}
				return instance;
			}
		}

		public static Coroutine StartRoutine(IEnumerator routine)
		{
			if (routine == null)
			{
				return null;
			}
			return Instance.StartCoroutine(routine);
		}

		public static void StopRoutine(Coroutine routine)
		{
			if (routine != null && !(instance == null))
			{
				instance.StopCoroutine(routine);
			}
		}
	}

	private static readonly int FishBiting = Animator.StringToHash("fishBiting");

	private static readonly int FishPulling = Animator.StringToHash("fishPulling");

	private static readonly int PlayerPulling = Animator.StringToHash("playerPulling");

	public Action<Stage> OnStageChanged;

	[Header("Fishing Params")]
	private FishingDef fishingDef;

	private ItemDef fishingRod;

	[Header("Calculated time indicators")]
	private float fishWaitTime;

	private float fishIdleTime;

	private float fishResistTime;

	[Header("Debug")]
	[SerializeField]
	private Stage currentStage;

	private float gameTime;

	private bool isFishResisting;

	public bool isPlayerPulling;

	public float progress;

	public float tension;

	public float resistance;

	private WgoData reservoir;

	private PlayerAnimation playerAnim;

	private FishUnderwaterGfx fishUnderwaterGfx;

	private int playerTalentLevel;

	private int fishAnger;

	private bool isReelSoundStarted;

	private float targetResistance;

	private bool usePhaseDurations;

	private float splashTimer;

	private bool splashResistActive;

	private FishingSettings fishingSettings;

	private FishingContainer fishingContainer;

	private SoundHandler reelSound;

	private Coroutine gameFlowRoutine;

	private Coroutine biteTimeoutRoutine;

	private Coroutine resistanceRoutine;

	private float idleDriftTimer;

	private float idleDriftDuration;

	private float idleDriftPhase;

	private float idleDriftAmplitude;

	private float idleDriftAmplitudeTarget;

	private float idleDriftAmplitudeSpeed;

	private float idleDriftFrequency;

	private float idleDriftFrequencyTarget;

	private float idleDriftOffset;

	private float idleDriftOffsetFadeSpeed;

	private Vector3 idleDriftAxis = Vector3.forward;

	private Vector3 idleDriftAxisTarget = Vector3.forward;

	private FishingSettings.FishDriftStateSettings currentDriftPreset;

	private FishingSettings.FishDriftStateSettings pendingPreset;

	private bool presetInitialized;

	private bool isFadingOut;

	private bool lastOffsetWasPositive;

	public Stage CurrentStage => currentStage;

	public bool IsActive
	{
		get
		{
			Stage stage = currentStage;
			if (stage != 0)
			{
				return stage != Stage.Finish;
			}
			return false;
		}
	}

	public bool IsFishResisting => isFishResisting;

	public float IdleDriftOffset => idleDriftOffset;

	public Vector3 IdleDriftAxis => idleDriftAxis;

	public void StartNewGame(FishingDef fishingDef, WgoData reservoir, ItemDef fishingRod, Action<Stage> OnMiniGameStageChanged)
	{
		this.fishingDef = fishingDef;
		this.reservoir = reservoir;
		this.fishingRod = fishingRod;
		fishingContainer = MainGame.PlayerController.View.FishingContainer;
		fishingSettings = LazySingletonSerializedSO<FishingSettings>.Instance;
		fishUnderwaterGfx = fishingContainer.FishUnderwaterGfx;
		SetSignActive(active: false);
		OnStageChanged = (Action<Stage>)Delegate.Combine(OnStageChanged, OnMiniGameStageChanged);
		playerAnim = MainGame.PlayerController.View.PlayerAnimation;
		gameTime = 0f;
		splashTimer = 0f;
		splashResistActive = false;
		playerTalentLevel = MainGame.PlayerController.GetMasteryLevelForTalentBranch("talent_green");
		fishAnger = ((this.fishingDef != null) ? fishingDef.anger.EvaluateInt() : 0);
		fishWaitTime = ((this.fishingDef == null) ? (-1f) : GetRandomFloatInRange(fishingDef.WaitTimeLeft, fishingDef.WaitTimeRight));
		fishIdleTime = ((this.fishingDef == null) ? (-1f) : GetRandomFloatInRange(fishingDef.IntervalTimeLeft, fishingDef.IntervalTimeRight));
		fishResistTime = ((this.fishingDef == null) ? (-1f) : GetRandomFloatInRange(fishingDef.ResistTimeLeft, fishingDef.ResistTimeRight));
		usePhaseDurations = fishIdleTime > 0f && fishResistTime > 0f;
		fishUnderwaterGfx.UpdateGfx();
		ResetIdleDrift();
		StartGameFlow(OnMiniGameStageChanged);
	}

	private float GetRandomFloatInRange(float min, float max, int decimalPlaces = 2)
	{
		if (min >= max)
		{
			return min;
		}
		float num = Mathf.Clamp(UnityEngine.Random.Range(min, max), min, max);
		float num2 = Mathf.Pow(10f, decimalPlaces);
		return Mathf.Round(num * num2) / num2;
	}

	public void FinishGame()
	{
		StopAllRoutines();
		EnterStage(Stage.Finish);
		if (fishingContainer != null)
		{
			fishingContainer.AnimationComponent.SetLayerWeight(1, 0f);
			fishingContainer.AnimationComponent.Animator.SetBool(FishBiting, value: false);
			fishingContainer.AnimationComponent.Animator.SetBool(FishPulling, value: false);
			SetSignActive(active: false);
		}
		reelSound?.Stop();
		LazyAudio.Stop("fishing_floundering");
		fishUnderwaterGfx?.UpdateGfx();
		OnStageChanged = null;
	}

	public void CancelGame()
	{
		StopAllRoutines();
		OnStageChanged = null;
		EnterStage(Stage.Finish);
		if (fishingContainer != null)
		{
			fishingContainer.AnimationComponent.SetLayerWeight(1, 0f);
			fishingContainer.AnimationComponent.Animator.SetBool(FishBiting, value: false);
			fishingContainer.AnimationComponent.Animator.SetBool(FishPulling, value: false);
			SetSignActive(active: false);
		}
		reelSound?.Stop();
		LazyAudio.Stop("fishing_floundering");
		ResetIdleDrift();
	}

	public void UpdateGame()
	{
		HandleInput();
		UpdateStage(currentStage);
		UpdateIdleDrift();
	}

	private void UpdateStage(Stage stage)
	{
		switch (stage)
		{
		case Stage.WaitingForBite:
			if (isPlayerPulling)
			{
				HandleFailure();
			}
			break;
		case Stage.Biting:
			SetSignActive(active: true);
			if (isPlayerPulling)
			{
				EnterStage(Stage.PlayingWithFish);
			}
			fishUnderwaterGfx.UpdateGfx(fishingDef.underwaterGfxType);
			break;
		case Stage.PlayingWithFish:
		{
			isFishResisting = resistance > 0.05f;
			UpdateFishSplashes();
			playerAnim.SetState(isPlayerPulling ? AnimationState.FishingPull : AnimationState.FishingRelease);
			bool flag = isPlayerPulling || isFishResisting;
			if (flag && !isReelSoundStarted)
			{
				reelSound?.Stop();
				reelSound = LazyAudio.PlayAtGameObject("fishing_reel_long", playerAnim.transform, SpatialType.sound3D);
				isReelSoundStarted = true;
			}
			else if (!flag && isReelSoundStarted)
			{
				reelSound?.Stop();
				isReelSoundStarted = false;
			}
			fishingContainer.AnimationComponent.Animator.SetBool(PlayerPulling, isPlayerPulling);
			fishingContainer.AnimationComponent.Animator.SetBool(FishPulling, isFishResisting);
			SetSignActive(!isFishResisting);
			UpdateProgress();
			break;
		}
		case Stage.Finish:
			ResetIdleDrift();
			break;
		}
	}

	private void EnterStage(Stage newStage)
	{
		currentStage = newStage;
		switch (currentStage)
		{
		case Stage.Biting:
			LazyAudio.PlayAtGameObject("fishing_bite", playerAnim.transform, SpatialType.sound3D);
			fishingContainer.AnimationComponent.Animator.SetBool(FishBiting, value: true);
			SetSignActive(active: true);
			break;
		case Stage.PlayingWithFish:
			LazyAudio.PlayAtGameObject("fishing_bite_success", playerAnim.transform, SpatialType.sound3D);
			fishingContainer.AnimationComponent.SetLayerWeight(1, 1f);
			fishingContainer.AnimationComponent.Animator.SetBool(FishBiting, value: false);
			StartResistanceRoutine();
			StartIdleDriftRoutine();
			break;
		case Stage.Finish:
			StopResistanceRoutine();
			StopIdleDriftRoutine();
			break;
		}
		OnStageChanged?.Invoke(newStage);
	}

	private void SetSignActive(bool active)
	{
		if ((bool)fishingContainer?.SignGameObject && fishingContainer.SignGameObject.activeSelf != active)
		{
			fishingContainer.SignGameObject.SetActive(active);
		}
	}

	private void HandleInput()
	{
		bool flag = LazyInput.GetKey(GameKey.LeftClick) || LazyInput.GetKey(GameKey.Interaction);
		if (IsActive && isPlayerPulling != flag)
		{
			if (flag)
			{
				LazyAudio.PlayAtGameObject("fishing_reel_short", playerAnim.transform, SpatialType.sound3D);
			}
			isPlayerPulling = flag;
		}
		if (!IsActive)
		{
			isPlayerPulling = false;
		}
		if (LazyInput.GetKeyDown(GameKey.Back))
		{
			FinishGame();
		}
	}

	private void UpdateFishSplashes()
	{
		if (!isFishResisting)
		{
			splashTimer = 0f;
			splashResistActive = false;
			return;
		}
		float a = ((fishingDef != null) ? fishingDef.splashCoef : 1f);
		float num = GetCurrentDriftPreset()?.MinSplashPeriodTime ?? 0.8f;
		if (num <= 0f)
		{
			num = 0.8f;
		}
		float num2 = num / Mathf.Max(a, 0.01f);
		bool num3 = !splashResistActive;
		splashResistActive = true;
		splashTimer += Time.deltaTime;
		if (num3 || !(splashTimer < num2))
		{
			Transform obj = ((fishingContainer?.Bob != null) ? fishingContainer.Bob.transform : ((fishUnderwaterGfx != null) ? fishUnderwaterGfx.transform : playerAnim.transform));
			if (LazyAudio.PlayAtGameObject("fishing_floundering", obj, SpatialType.sound3D, checkDelay: false) != null)
			{
				splashTimer = 0f;
			}
		}
	}

	private void UpdateProgress()
	{
		gameTime += Time.deltaTime;
		if (usePhaseDurations)
		{
			if (!resistance.EqualsTo(targetResistance))
			{
				float num = Mathf.Abs(targetResistance - resistance) / fishingDef.speedChangeTime;
				resistance = Mathf.Clamp01(Mathf.MoveTowards(resistance, targetResistance, num * Time.deltaTime));
			}
		}
		else
		{
			resistance = fishingSettings.GetCurveById(fishingDef.curvePresetId).Evaluate(gameTime);
		}
		float num2;
		float num3;
		if (isPlayerPulling)
		{
			float f = (float)playerTalentLevel - (float)fishAnger * resistance * fishingSettings.CurveMultiplier;
			num2 = Mathf.Sign(f) * Mathf.Pow(Mathf.Abs(f), 0.5f) * fishingSettings.PullingProgressConst;
			num3 = (10f + resistance * 500f + (float)(fishAnger * 10)) / (5f + 0.1f * (float)(fishingRod.talentBonus + playerTalentLevel));
			fishUnderwaterGfx.SetFishState(FishUnderwaterGfx.FishGfxState.Losing);
		}
		else
		{
			float f2 = (float)(-fishAnger) * resistance * fishingSettings.CurveMultiplier;
			num2 = Mathf.Sign(f2) * Mathf.Pow(Mathf.Abs(f2), 0.5f) * fishingSettings.IdleProgressConst;
			num3 = -(80 + 3 * (fishingRod.talentBonus + playerTalentLevel));
			fishUnderwaterGfx.SetFishState(FishUnderwaterGfx.FishGfxState.Pulling);
		}
		progress = Mathf.Clamp(progress + num2 * Time.deltaTime, -100f, 100f);
		tension = Mathf.Clamp(tension + num3 * Time.deltaTime, 0f, 100f);
		if (tension >= 100f)
		{
			HandleFailure(isFishingLineBroke: true);
		}
		else if (progress <= -100f)
		{
			HandleFailure();
		}
		else if (progress >= 100f)
		{
			HandleSuccess();
		}
	}

	private void HandleSuccess()
	{
		reservoir.SubGameRes(fishingDef.fishId, 1);
		reservoir.SetGameRes(fishingDef.fishId + "_caught", 1);
		MainGame.Instance.GameSave.gameLogicSystemData.gameLogics.Find((GameLogicData x) => x.id == reservoir.id + "_restore")?.Init();
		Item item = new Item(fishingDef.fishId);
		reservoir.MakeDrop(item);
		fishingDef.expressionsOnEnd.ForEach(delegate(LazyExpression x)
		{
			x.Evaluate();
		});
		AchievementsSystem.Instance.TriggerCountable("fish_caught");
		LazyAudio.PlayAtGameObject("fishing_success", playerAnim.transform, SpatialType.sound3D);
		FinishGame();
	}

	private void HandleFailure(bool isFishingLineBroke = false)
	{
		LazyAudio.PlayAtGameObject(isFishingLineBroke ? "fishing_line_break" : "fishing_fail", playerAnim.transform, SpatialType.sound3D);
		FinishGame();
		Bubble.Talk(new PhraseData(isPlayer: true, null, LLBase.L("fishing_next_time"), null, null, SpeechBubbleType.Think));
	}

	private void StartGameFlow(Action<Stage> stageCallback)
	{
		StopAllRoutines();
		OnStageChanged = (Action<Stage>)Delegate.Combine(OnStageChanged, stageCallback);
		gameFlowRoutine = FishingCoroutineRunner.StartRoutine(GameFlow());
	}

	private IEnumerator GameFlow()
	{
		EnterStage(Stage.WaitingForBite);
		if (fishWaitTime > 0f)
		{
			yield return new WaitForSeconds(fishWaitTime);
		}
		if (!IsActive || currentStage != Stage.WaitingForBite)
		{
			yield break;
		}
		EnterStage(Stage.Biting);
		biteTimeoutRoutine = FishingCoroutineRunner.StartRoutine(BiteTimeout());
		while (IsActive && currentStage == Stage.Biting)
		{
			yield return null;
		}
		if (biteTimeoutRoutine != null)
		{
			FishingCoroutineRunner.StopRoutine(biteTimeoutRoutine);
		}
		if (IsActive && currentStage == Stage.PlayingWithFish)
		{
			while (IsActive && currentStage == Stage.PlayingWithFish)
			{
				yield return null;
			}
		}
	}

	private IEnumerator BiteTimeout()
	{
		yield return new WaitForSeconds(fishingDef.baitTime);
		if (IsActive && currentStage == Stage.Biting)
		{
			HandleFailure();
		}
	}

	private void StartResistanceRoutine()
	{
		StopResistanceRoutine();
		if (!usePhaseDurations)
		{
			targetResistance = 0f;
		}
		else
		{
			resistanceRoutine = FishingCoroutineRunner.StartRoutine(ResistanceLoop());
		}
	}

	private void StopResistanceRoutine()
	{
		if (resistanceRoutine != null)
		{
			FishingCoroutineRunner.StopRoutine(resistanceRoutine);
		}
		resistanceRoutine = null;
	}

	private IEnumerator ResistanceLoop()
	{
		targetResistance = 0f;
		while (IsActive && currentStage == Stage.PlayingWithFish)
		{
			float randomFloatInRange = GetRandomFloatInRange(fishingDef.IntervalTimeLeft, fishingDef.IntervalTimeRight);
			yield return new WaitForSeconds(randomFloatInRange);
			if (!IsActive || currentStage != Stage.PlayingWithFish)
			{
				break;
			}
			targetResistance = 1f;
			float randomFloatInRange2 = GetRandomFloatInRange(fishingDef.ResistTimeLeft, fishingDef.ResistTimeRight);
			yield return new WaitForSeconds(randomFloatInRange2);
			if (!IsActive || currentStage != Stage.PlayingWithFish)
			{
				break;
			}
			targetResistance = 0f;
		}
	}

	private void StopAllRoutines()
	{
		if (gameFlowRoutine != null)
		{
			FishingCoroutineRunner.StopRoutine(gameFlowRoutine);
		}
		if (biteTimeoutRoutine != null)
		{
			FishingCoroutineRunner.StopRoutine(biteTimeoutRoutine);
		}
		StopResistanceRoutine();
		StopIdleDriftRoutine();
		gameFlowRoutine = null;
		biteTimeoutRoutine = null;
	}

	private void StartIdleDriftRoutine()
	{
		ResetIdleDrift();
	}

	private void StopIdleDriftRoutine()
	{
		ResetIdleDrift();
	}

	private void ResetIdleDrift(bool smoothToZero = false)
	{
		if (smoothToZero && !isFadingOut && Math.Abs(idleDriftOffset) > Mathf.Epsilon)
		{
			float num = ((fishingSettings != null) ? fishingSettings.DriftFadeOutDuration : 0.35f);
			isFadingOut = true;
			idleDriftOffsetFadeSpeed = ((num > 0f) ? (Mathf.Abs(idleDriftOffset) / num) : 1000f);
			return;
		}
		idleDriftTimer = 0f;
		idleDriftDuration = 0f;
		idleDriftPhase = 0f;
		idleDriftAmplitude = 0f;
		idleDriftAmplitudeTarget = 0f;
		idleDriftAmplitudeSpeed = 0f;
		idleDriftFrequency = 0f;
		idleDriftFrequencyTarget = 0f;
		idleDriftOffset = 0f;
		idleDriftOffsetFadeSpeed = 0f;
		idleDriftAxis = Vector3.forward;
		idleDriftAxisTarget = Vector3.forward;
		currentDriftPreset = null;
		pendingPreset = null;
		presetInitialized = false;
		isFadingOut = false;
		lastOffsetWasPositive = false;
	}

	private void UpdateIdleDrift()
	{
		if (isFadingOut)
		{
			idleDriftOffset = Mathf.MoveTowards(idleDriftOffset, 0f, idleDriftOffsetFadeSpeed * Time.deltaTime);
			if (Mathf.Abs(idleDriftOffset) < Mathf.Epsilon)
			{
				isFadingOut = false;
				idleDriftOffset = 0f;
				idleDriftPhase = (lastOffsetWasPositive ? MathF.PI : 0f);
				idleDriftAmplitude = 0f;
				if (pendingPreset != null)
				{
					FishingSettings.FishDriftStateSettings preset = pendingPreset;
					pendingPreset = null;
					currentDriftPreset = preset;
					ApplyPresetTargets(preset);
				}
				else
				{
					ResetIdleDrift();
				}
			}
			return;
		}
		if (!IsActive || currentStage != Stage.PlayingWithFish)
		{
			ResetIdleDrift(smoothToZero: true);
			return;
		}
		FishingSettings.FishDriftStateSettings fishDriftStateSettings = GetCurrentDriftPreset();
		if (currentDriftPreset != fishDriftStateSettings && pendingPreset != fishDriftStateSettings)
		{
			ApplyPresetTargets(fishDriftStateSettings);
			if (!isFadingOut)
			{
				currentDriftPreset = fishDriftStateSettings;
			}
		}
		else if (currentDriftPreset != null && currentDriftPreset.EnableRandomization && (idleDriftDuration <= 0f || idleDriftTimer >= idleDriftDuration))
		{
			RandomizeCurrentPreset();
		}
		idleDriftTimer += Time.deltaTime;
		idleDriftAmplitude = Mathf.MoveTowards(idleDriftAmplitude, idleDriftAmplitudeTarget, idleDriftAmplitudeSpeed * Time.deltaTime);
		idleDriftFrequency = Mathf.MoveTowards(idleDriftFrequency, idleDriftFrequencyTarget, fishingSettings.FrequencyTransitionSpeed * Time.deltaTime);
		idleDriftAxis = Vector3.MoveTowards(idleDriftAxis, idleDriftAxisTarget, fishingSettings.AxisTransitionSpeed * Time.deltaTime);
		idleDriftPhase += idleDriftFrequency * Time.deltaTime;
		if (idleDriftPhase > MathF.PI * 2f)
		{
			idleDriftPhase -= MathF.PI * 2f;
		}
		idleDriftOffset = idleDriftAmplitude * Mathf.Sin(idleDriftPhase);
	}

	private FishingSettings.FishDriftStateSettings GetCurrentDriftPreset()
	{
		bool flag = isFishResisting;
		bool flag2 = isPlayerPulling;
		if (flag && flag2)
		{
			return fishingSettings.BothPullingDrift;
		}
		if (flag && !flag2)
		{
			return fishingSettings.FishPullingOnlyDrift;
		}
		return fishingSettings.IdleDrift;
	}

	private void ApplyPresetTargets(FishingSettings.FishDriftStateSettings preset)
	{
		if (pendingPreset == null && currentDriftPreset == null && Mathf.Abs(idleDriftOffset) > Mathf.Epsilon)
		{
			lastOffsetWasPositive = idleDriftOffset > 0f;
			pendingPreset = preset;
			float num = ((fishingSettings != null) ? fishingSettings.DriftFadeOutDuration : 0.35f);
			isFadingOut = true;
			idleDriftOffsetFadeSpeed = ((num > 0f) ? (Mathf.Abs(idleDriftOffset) / num) : 1000f);
			return;
		}
		idleDriftTimer = 0f;
		if (preset.EnableRandomization)
		{
			idleDriftDuration = UnityEngine.Random.Range(preset.MinDuration, preset.MaxDuration);
			idleDriftAmplitudeTarget = UnityEngine.Random.Range(preset.MinAmplitude, preset.MaxAmplitude);
		}
		else
		{
			idleDriftDuration = preset.MinDuration;
			idleDriftAmplitudeTarget = preset.MinAmplitude;
		}
		idleDriftFrequencyTarget = preset.Frequency;
		if (fishingDef != null)
		{
			idleDriftFrequencyTarget *= fishingDef.wriggleCoef;
		}
		idleDriftAxisTarget = preset.OscillationAxis;
		float num2 = Mathf.Abs(idleDriftAmplitudeTarget - idleDriftAmplitude);
		idleDriftAmplitudeSpeed = ((idleDriftDuration > 0f) ? (num2 / Mathf.Max(0.25f, idleDriftDuration * 0.5f)) : num2);
		if (!presetInitialized)
		{
			idleDriftFrequency = idleDriftFrequencyTarget;
			idleDriftAxis = idleDriftAxisTarget;
			presetInitialized = true;
		}
	}

	private void RandomizeCurrentPreset()
	{
		if (currentDriftPreset != null)
		{
			idleDriftTimer = 0f;
			idleDriftDuration = UnityEngine.Random.Range(currentDriftPreset.MinDuration, currentDriftPreset.MaxDuration);
			idleDriftAmplitudeTarget = UnityEngine.Random.Range(currentDriftPreset.MinAmplitude, currentDriftPreset.MaxAmplitude);
			float num = Mathf.Abs(idleDriftAmplitudeTarget - idleDriftAmplitude);
			idleDriftAmplitudeSpeed = ((idleDriftDuration > 0f) ? (num / Mathf.Max(0.25f, idleDriftDuration * 0.5f)) : num);
		}
	}
}
