using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIHUDWheel : MonoBehaviour, ILazyGUIElement
{
	[SerializeField]
	private RectTransform underParent;

	[SerializeField]
	private RectTransform overParent;

	[SerializeField]
	private UIHUDWheelElement[] elements;

	[SerializeField]
	private List<Sprite> dayIcons = new List<Sprite>();

	[SerializeField]
	private List<Sprite> dayCustomWheelCenterIconDay = new List<Sprite>();

	[SerializeField]
	private List<Sprite> dayCustomWheelCenterIconNight = new List<Sprite>();

	[SerializeField]
	private Image centerWheelIconDay;

	[SerializeField]
	private Image centerWheelIconNight;

	[SerializeField]
	private Sprite centerWheelIconDaySpriteDefault;

	[SerializeField]
	private Sprite centerWheelIconNightSpriteDefault;

	[SerializeField]
	private RectTransform sun;

	[SerializeField]
	private RectTransform moon;

	[SerializeField]
	private float radius;

	[SerializeField]
	private RectTransform center;

	[SerializeField]
	private Gradient gradientHorizon;

	[SerializeField]
	private Gradient gradientSky;

	[SerializeField]
	private Image sky;

	[SerializeField]
	private Image horizon;

	[SerializeField]
	private Image housesNight;

	[SerializeField]
	private Image[] starsNight;

	[SerializeField]
	private float nightObjectsTweenFade;

	[SerializeField]
	[Header("Day wheel rotation")]
	private float dayWheelAngularSpeedDeg = 120f;

	[SerializeField]
	private float fadeOutCurrentArrowDuration = 0.5f;

	[SerializeField]
	private float fadeOutCurrentIconDuration = 0.25f;

	[SerializeField]
	private float fadeInNewCurrentIconDuration = 0.25f;

	[SerializeField]
	private float fadeInNewCurrentArrowDuration = 0.5f;

	[SerializeField]
	private Image[] arrows;

	private Dictionary<int, Sprite> iconsByDayNumber;

	private Dictionary<int, string> dayNamesByDayNumber;

	private Dictionary<int, Sprite> customCenterIconDayByDayNumber;

	private Dictionary<int, Sprite> customCenterIconNightByDayNumber;

	private readonly Dictionary<int, string> newDaySoundsByDayNumber = new Dictionary<int, string>();

	private bool isDayTime;

	public bool forceTimeDependentChange;

	private float[] starsNightRadiuses;

	private float[] angleOffset;

	private Vector2 wheelCenterPos;

	private float[] slotAngles;

	private float[] slotRadii;

	private int[] elementSlotIndices;

	private float[] elementAnimStartAngles;

	private float[] elementAnimTargetAngles;

	private bool isDayWheelAnimating;

	private float dayWheelRotationProgress;

	private float dayWheelRotationDuration;

	private int pendingDay;

	private int pendingDayWheelSlotDelta;

	private int oldCurrentElementIndex;

	private int newCurrentElementIndex;

	private int lastAppliedWheelDay = -1;

	private float dayWheelOneStepRad;

	private bool wheelLayoutInitialized;

	private readonly List<Tween> dayWheelTweens = new List<Tween>();

	private const float STARS_LAYOUT_TIME = 1f;

	private const string ZOMBIE_BELL_SOUND_ID = "zombie_bell";

	public void Init()
	{
		EnvironmentEngine.OnNewDayStarted += OnNewDayStartedAnimated;
		EnvironmentEngine.OnTimeOfDayChangedEvent += OnTimeOfDayChanged;
		iconsByDayNumber = new Dictionary<int, Sprite>();
		dayNamesByDayNumber = new Dictionary<int, string>();
		customCenterIconDayByDayNumber = new Dictionary<int, Sprite>();
		customCenterIconNightByDayNumber = new Dictionary<int, Sprite>();
		newDaySoundsByDayNumber.Clear();
		for (int i = 0; i < LazyConsts.ConstDefs.AllDays.Length; i++)
		{
			AddDayVisuals(LazyConsts.ConstDefs.AllDays[i]);
		}
		RegisterNewDaySound("day_sloth", "zombie_bell");
		Vector2 anchoredPosition = center.anchoredPosition;
		float num = 4.712389f + MathF.PI;
		starsNightRadiuses = new float[starsNight.Length];
		angleOffset = new float[starsNight.Length];
		for (int j = 0; j < starsNight.Length; j++)
		{
			Vector2 vector = starsNight[j].rectTransform.anchoredPosition - anchoredPosition;
			starsNightRadiuses[j] = vector.magnitude;
			float num2 = Mathf.Atan2(vector.y, vector.x);
			angleOffset[j] = num2 - num;
		}
		InitWheelLayout();
		void AddDayVisuals(string dayName)
		{
			int intValue = ConstDef.Get(dayName).IntValue;
			dayNamesByDayNumber.Add(intValue, dayName);
			iconsByDayNumber.Add(intValue, dayIcons.Find((Sprite s) => (bool)s && s.name == dayName));
			customCenterIconDayByDayNumber.Add(intValue, dayCustomWheelCenterIconDay.Find((Sprite s) => (bool)s && s.name == dayName));
			customCenterIconNightByDayNumber.Add(intValue, dayCustomWheelCenterIconNight.Find((Sprite s) => (bool)s && s.name == dayName));
		}
	}

	public void OnTimeOfDayChanged(float time, bool isFake = false)
	{
		float num = (time * 360f - 90f) * (MathF.PI / 180f);
		float num2 = num + MathF.PI;
		Vector2 anchoredPosition = center.anchoredPosition;
		sun.anchoredPosition = anchoredPosition + new Vector2(Mathf.Cos(num) * radius, Mathf.Sin(num) * radius);
		moon.anchoredPosition = anchoredPosition + new Vector2(Mathf.Cos(num2) * radius, Mathf.Sin(num2) * radius);
		for (int i = 0; i < starsNight.Length; i++)
		{
			float f = num2 + angleOffset[i];
			float num3 = starsNightRadiuses[i];
			starsNight[i].rectTransform.anchoredPosition = anchoredPosition + new Vector2(Mathf.Cos(f) * num3, Mathf.Sin(f) * num3);
		}
		sky.color = gradientSky.Evaluate(time);
		horizon.color = gradientHorizon.Evaluate(time);
		if (time >= 0.25f && time < 0.8f)
		{
			if (!isDayTime || forceTimeDependentChange)
			{
				isDayTime = true;
				housesNight.DOFade(0f, nightObjectsTweenFade);
				Image[] array = starsNight;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].DOFade(0f, nightObjectsTweenFade);
				}
				forceTimeDependentChange = false;
			}
		}
		else if (isDayTime || forceTimeDependentChange)
		{
			isDayTime = false;
			housesNight.DOFade(1f, nightObjectsTweenFade);
			Image[] array = starsNight;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].DOFade(1f, nightObjectsTweenFade);
			}
			forceTimeDependentChange = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(center.position, radius);
	}

	private void Update()
	{
		UpdateDayWheelRotation();
	}

	private void OnDestroy()
	{
		EnvironmentEngine.OnNewDayStarted -= OnNewDayStartedAnimated;
		EnvironmentEngine.OnTimeOfDayChangedEvent -= OnTimeOfDayChanged;
		StopDayWheelAnimation();
	}

	private void OnNewDayStartedAnimated(int day)
	{
		OnNewDayStarted(day);
	}

	public void OnNewDayStarted(int day, bool animate = true)
	{
		if (!wheelLayoutInitialized)
		{
			InitWheelLayout();
		}
		if (!animate)
		{
			ApplyCenterWheelIcons(day);
			StopDayWheelAnimation();
			ResetWheelSlotsToBaseline();
			ApplyWheelVisualState(day);
			lastAppliedWheelDay = day;
			return;
		}
		int num = ((lastAppliedWheelDay < 0) ? 1 : (day - lastAppliedWheelDay));
		if (num <= 0)
		{
			ApplyCenterWheelIcons(day);
			lastAppliedWheelDay = day;
			return;
		}
		ApplyCenterWheelIcons(day);
		PlayNewDaySound(day);
		int num2 = Mod(num, 6);
		if (!base.gameObject.activeInHierarchy || num2 == 0)
		{
			SnapWheelToDay(day, num2);
			return;
		}
		if (isDayWheelAnimating)
		{
			StopDayWheelAnimation();
		}
		StartDayWheelAnimation(day, num2);
	}

	private void ApplyCenterWheelIcons(int day)
	{
		int dayNumberFromDay = EnvironmentEngine.Instance.Data.GetDayNumberFromDay(day);
		Sprite sprite = centerWheelIconDaySpriteDefault;
		Sprite sprite2 = centerWheelIconNightSpriteDefault;
		if (IsCustomCenterIconUnlocked(dayNumberFromDay))
		{
			if (customCenterIconDayByDayNumber.TryGetValue(dayNumberFromDay, out var value) && (bool)value)
			{
				sprite = value;
			}
			if (customCenterIconNightByDayNumber.TryGetValue(dayNumberFromDay, out var value2) && (bool)value2)
			{
				sprite2 = value2;
			}
		}
		centerWheelIconDay.sprite = sprite;
		centerWheelIconNight.sprite = sprite2;
	}

	private bool IsCustomCenterIconUnlocked(int dayNumber)
	{
		if (dayNamesByDayNumber.TryGetValue(dayNumber, out var value))
		{
			return MainGame.Instance.GameSave.knowledgeSystem.unlockedCustomHudDaySprites.Contains(value);
		}
		return false;
	}

	private void RegisterNewDaySound(string dayName, string soundId)
	{
		newDaySoundsByDayNumber[ConstDef.Get(dayName).IntValue] = soundId;
	}

	private void PlayNewDaySound(int day)
	{
		int dayNumberFromDay = EnvironmentEngine.Instance.Data.GetDayNumberFromDay(day);
		if (newDaySoundsByDayNumber.TryGetValue(dayNumberFromDay, out var value) && !string.IsNullOrEmpty(value))
		{
			LazyAudio.Play(value);
		}
	}

	private void SnapWheelToDay(int day, int slotDelta)
	{
		StopDayWheelAnimation();
		if (slotDelta > 0)
		{
			for (int i = 0; i < 6; i++)
			{
				elementSlotIndices[i] = Mod(elementSlotIndices[i] - slotDelta, 6);
				SetElementAngle(i, slotAngles[elementSlotIndices[i]]);
			}
		}
		ApplyWheelVisualState(day);
		lastAppliedWheelDay = day;
	}

	private void InitWheelLayout()
	{
		wheelCenterPos = Vector2.zero;
		slotAngles = new float[6];
		slotRadii = new float[6];
		elementSlotIndices = new int[6];
		elementAnimStartAngles = new float[6];
		elementAnimTargetAngles = new float[6];
		for (int i = 0; i < 6; i++)
		{
			Vector2 vector = elements[i].rectTransform.anchoredPosition - wheelCenterPos;
			slotAngles[i] = Mathf.Atan2(vector.y, vector.x);
			slotRadii[i] = vector.magnitude;
			elementSlotIndices[i] = i;
			SetElementAngle(i, slotAngles[i]);
		}
		InitArrows();
		dayWheelOneStepRad = GetSignedAngleDelta(slotAngles[1], slotAngles[0]);
		wheelLayoutInitialized = true;
	}

	private void InitArrows()
	{
		if (arrows == null)
		{
			return;
		}
		for (int i = 0; i < arrows.Length; i++)
		{
			if ((bool)arrows[i])
			{
				arrows[i].gameObject.SetActive(value: true);
				SetGraphicAlpha(arrows[i], 0f);
			}
		}
	}

	private void ResetWheelSlotsToBaseline()
	{
		for (int i = 0; i < 6; i++)
		{
			elementSlotIndices[i] = i;
			SetElementAngle(i, slotAngles[i]);
		}
	}

	private void StartDayWheelAnimation(int day, int dayDelta)
	{
		pendingDay = day;
		pendingDayWheelSlotDelta = dayDelta;
		oldCurrentElementIndex = FindElementIndexWithSlot(0);
		newCurrentElementIndex = FindElementIndexWithSlot(dayDelta);
		int num = FindElementIndexWithTargetSlot(GetNextDayArrowIndex(), dayDelta);
		float num2 = Mathf.Abs(dayWheelOneStepRad * (float)dayDelta);
		dayWheelRotationDuration = num2 / (dayWheelAngularSpeedDeg * (MathF.PI / 180f));
		dayWheelRotationProgress = 0f;
		isDayWheelAnimating = true;
		for (int i = 0; i < 6; i++)
		{
			int num3 = Mod(elementSlotIndices[i] - dayDelta, 6);
			elementAnimStartAngles[i] = slotAngles[elementSlotIndices[i]];
			elementAnimTargetAngles[i] = slotAngles[num3];
			int dayNumberFromDay = EnvironmentEngine.Instance.Data.GetDayNumberFromDay(day + num3);
			elements[i].icon.sprite = iconsByDayNumber[dayNumberFromDay];
		}
		KillDayWheelTweens();
		SetAllElementsUnderParentWithInactiveIcons();
		SetAllArrowsAlpha(0f);
		Image arrow = GetArrow(GetNextDayArrowIndex());
		if ((bool)arrow)
		{
			PrepareElementForFade(arrow, 1f);
			dayWheelTweens.Add(arrow.DOFade(0f, fadeOutCurrentArrowDuration));
		}
		if (oldCurrentElementIndex != newCurrentElementIndex && oldCurrentElementIndex != num)
		{
			SetElementOverParent(elements[oldCurrentElementIndex]);
			SetIconVisual(elements[oldCurrentElementIndex], active: true);
			TweenIconToActive(elements[oldCurrentElementIndex], active: false, fadeOutCurrentIconDuration);
		}
		SetElementOverParent(elements[newCurrentElementIndex]);
		TweenIconToActive(elements[newCurrentElementIndex], active: true, fadeInNewCurrentIconDuration);
		if (num >= 0 && num != newCurrentElementIndex)
		{
			SetElementOverParent(elements[num]);
			TweenIconToActive(elements[num], active: true, fadeInNewCurrentIconDuration);
		}
	}

	private int GetNextDayArrowIndex()
	{
		return EnvironmentEngine.Instance.Data.GetDiffInDaysBetweenCurrentAndNext();
	}

	private void UpdateDayWheelRotation()
	{
		if (isDayWheelAnimating)
		{
			dayWheelRotationProgress += Time.deltaTime / dayWheelRotationDuration;
			float num = Mathf.Clamp01(dayWheelRotationProgress);
			for (int i = 0; i < 6; i++)
			{
				float angle = elementAnimStartAngles[i] + dayWheelOneStepRad * (float)pendingDayWheelSlotDelta * num;
				SetElementAngleAnimated(i, angle, num);
			}
			if (!(num < 1f))
			{
				CompleteDayWheelAnimation();
			}
		}
	}

	private void CompleteDayWheelAnimation()
	{
		isDayWheelAnimating = false;
		for (int i = 0; i < 6; i++)
		{
			elementSlotIndices[i] = Mod(elementSlotIndices[i] - pendingDayWheelSlotDelta, 6);
			SetElementAngle(i, slotAngles[elementSlotIndices[i]]);
		}
		lastAppliedWheelDay = pendingDay;
		ApplyWheelVisualState(pendingDay);
		Image arrow = GetArrow(GetNextDayArrowIndex());
		if ((bool)arrow)
		{
			PrepareElementForFade(arrow, 0f);
			dayWheelTweens.Add(arrow.DOFade(1f, fadeInNewCurrentArrowDuration));
		}
	}

	private void ApplyWheelVisualState(int day)
	{
		KillDayWheelTweens();
		for (int i = 0; i < 6; i++)
		{
			ApplyElementVisualState(i, day);
		}
		ApplyArrowVisualState();
	}

	private void SetAllElementsUnderParentWithInactiveIcons()
	{
		for (int i = 0; i < 6; i++)
		{
			UIHUDWheelElement obj = elements[i];
			obj.rectTransform.SetParent(underParent);
			SetIconVisual(obj, active: false);
		}
	}

	private void ApplyElementVisualState(int elementIndex, int day)
	{
		UIHUDWheelElement uIHUDWheelElement = elements[elementIndex];
		int num = elementSlotIndices[elementIndex];
		int dayNumberFromDay = EnvironmentEngine.Instance.Data.GetDayNumberFromDay(day + num);
		bool num2 = dayNumberFromDay == EnvironmentEngine.Instance.Data.CurrentDayNumber;
		bool flag = dayNumberFromDay == EnvironmentEngine.Instance.Data.NextDayNumber;
		bool num3 = num2 || flag;
		uIHUDWheelElement.icon.sprite = iconsByDayNumber[dayNumberFromDay];
		if (num3)
		{
			SetElementOverParent(uIHUDWheelElement);
			SetIconVisual(uIHUDWheelElement, active: true);
		}
		else
		{
			uIHUDWheelElement.rectTransform.SetParent(underParent);
			SetIconVisual(uIHUDWheelElement, active: false);
		}
	}

	private void ApplyArrowVisualState()
	{
		SetAllArrowsAlpha(0f);
		Image arrow = GetArrow(GetNextDayArrowIndex());
		if ((bool)arrow)
		{
			SetGraphicAlpha(arrow, 1f);
		}
	}

	private int FindElementIndexWithSlot(int slot)
	{
		for (int i = 0; i < 6; i++)
		{
			if (elementSlotIndices[i] == slot)
			{
				return i;
			}
		}
		return 0;
	}

	private int FindElementIndexWithTargetSlot(int targetSlot, int dayDelta)
	{
		if (targetSlot < 0 || targetSlot >= 6)
		{
			return -1;
		}
		int slot = Mod(targetSlot + dayDelta, 6);
		return FindElementIndexWithSlot(slot);
	}

	private Image GetArrow(int slotIndex)
	{
		if (arrows == null || slotIndex < 0 || slotIndex >= arrows.Length)
		{
			return null;
		}
		return arrows[slotIndex];
	}

	private void SetAllArrowsAlpha(float alpha)
	{
		if (arrows == null)
		{
			return;
		}
		for (int i = 0; i < arrows.Length; i++)
		{
			if ((bool)arrows[i])
			{
				SetGraphicAlpha(arrows[i], alpha);
			}
		}
	}

	private static void SetIconVisual(UIHUDWheelElement element, bool active)
	{
		element.icon.color = (active ? element.iconColorDefault : element.iconColorInactive);
	}

	private void SetElementOverParent(UIHUDWheelElement element)
	{
		element.rectTransform.SetParent(overParent);
	}

	private void TweenIconToActive(UIHUDWheelElement element, bool active, float duration)
	{
		float endValue = (active ? element.iconColorDefault.a : element.iconColorInactive.a);
		Color color = (active ? element.iconColorDefault : element.iconColorInactive);
		Color color2 = element.icon.color;
		element.icon.color = new Color(color.r, color.g, color.b, color2.a);
		dayWheelTweens.Add(element.icon.DOFade(endValue, duration));
	}

	private void SetElementAngle(int elementIndex, float angle)
	{
		int num = elementSlotIndices[elementIndex];
		SetElementPosition(elementIndex, angle, slotRadii[num]);
	}

	private void SetElementAngleAnimated(int elementIndex, float angle, float t)
	{
		int num = elementSlotIndices[elementIndex];
		float num2 = (float)pendingDayWheelSlotDelta * t;
		int num3 = Mathf.FloorToInt(num2);
		float t2 = num2 - (float)num3;
		int num4 = Mod(num - num3, 6);
		int num5 = Mod(num - num3 - 1, 6);
		float r = Mathf.Lerp(slotRadii[num4], slotRadii[num5], t2);
		SetElementPosition(elementIndex, angle, r);
	}

	private void SetElementPosition(int elementIndex, float angle, float r)
	{
		elements[elementIndex].rectTransform.anchoredPosition = wheelCenterPos + new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
	}

	private static int Mod(int value, int modulus)
	{
		return (value % modulus + modulus) % modulus;
	}

	private static float GetSignedAngleDelta(float fromRad, float toRad)
	{
		return Mathf.DeltaAngle(fromRad * 57.29578f, toRad * 57.29578f) * (MathF.PI / 180f);
	}

	private static void SetGraphicAlpha(Graphic graphic, float alpha)
	{
		Color color = graphic.color;
		color.a = alpha;
		graphic.color = color;
	}

	private static void PrepareElementForFade(Graphic graphic, float alpha)
	{
		graphic.DOKill();
		SetGraphicAlpha(graphic, alpha);
	}

	private void StopDayWheelAnimation()
	{
		isDayWheelAnimating = false;
		KillDayWheelTweens();
	}

	private void KillDayWheelTweens()
	{
		for (int i = 0; i < dayWheelTweens.Count; i++)
		{
			dayWheelTweens[i]?.Kill();
		}
		dayWheelTweens.Clear();
		for (int j = 0; j < 6; j++)
		{
			elements[j].icon.DOKill();
		}
		if (arrows != null)
		{
			for (int k = 0; k < arrows.Length; k++)
			{
				arrows[k]?.DOKill();
			}
		}
	}
}
