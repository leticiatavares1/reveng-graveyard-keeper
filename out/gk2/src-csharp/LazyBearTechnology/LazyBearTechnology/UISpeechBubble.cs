using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

public class UISpeechBubble : UIBasicBubble
{
	[Serializable]
	private enum FlipHorizontallySetting
	{
		None,
		ForLeft,
		ForRight
	}

	private const float VOICE_OVER_ADDITIONAL_SHOW_TIME_IN_SECONDS = 1f;

	public static bool canReuseBubbles = true;

	[SerializeField]
	protected SpeechBubbleSettings settings;

	[Space]
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI textField;

	[SerializeField]
	protected LayoutElement textLayout;

	[SerializeField]
	protected Image background;

	[SerializeField]
	private List<Image> cornerImages;

	[SerializeField]
	private List<GameKey> keysToSkip = new List<GameKey>();

	protected TextAnimator textAnimator = new TextAnimator();

	private Action onTextAnimationEnd;

	private Action onDisappeared;

	private Action onDisappearedForReusingBubbles;

	private Func<Vector2> onUpdatePosition;

	private Func<bool> onForceHideCondition;

	private Vector2 uiTargetPosition;

	private int disableBubbleCounter = -1;

	private bool useDisappearAnimation = true;

	private bool canSkipBubble = true;

	private float bubblePauseCounter;

	[SerializeField]
	private FlipHorizontallySetting flipBackgroundHorizontally;

	private static Dictionary<int, UISpeechBubble> activeBubbles = new Dictionary<int, UISpeechBubble>();

	protected static UISpeechBubble instance;

	private float bubbleShowTime;

	private bool disappearing;

	private bool textHasLetters;

	private bool playSound = true;

	private string text;

	private int lastTextIndex;

	private int speakerId;

	private SpeechBubblePreset usingPreset;

	private TMP_FontAsset fontDefault;

	private Material materialPresetDefault;

	private static bool isPaused;

	private static Func<bool> customDialogSkipCondition = null;

	private static Vector2[] cachedCornerAnchoredPositions;

	private static Vector2[] cachedCornerImagesAnchoredPositions;

	private VoiceID voiceId;

	private string voiceOverLocalKey;

	private LazySpeechEngine.VoiceData voiceData;

	public string TextToDisplay => text;

	protected VoiceID VoiceId => voiceId;

	protected string VoiceOverLocalKey => voiceOverLocalKey;

	public LazySpeechEngine.VoiceData VoiceData
	{
		get
		{
			if (voiceData == null)
			{
				voiceData = LazySpeechEngine.Instance.GetVoiceData(voiceId);
			}
			return voiceData;
		}
	}

	private void Awake()
	{
		if (fontDefault == null)
		{
			fontDefault = textField.font;
			materialPresetDefault = textField.fontSharedMaterial;
		}
	}

	public virtual void Init()
	{
		if (!(instance != null))
		{
			cachedCornerAnchoredPositions = new Vector2[corners.Count];
			for (int i = 0; i < corners.Count; i++)
			{
				cachedCornerAnchoredPositions[i] = corners[i].rectTransform.anchoredPosition;
			}
			cachedCornerImagesAnchoredPositions = new Vector2[cornerImages.Count];
			for (int j = 0; j < cornerImages.Count; j++)
			{
				cachedCornerImagesAnchoredPositions[j] = cornerImages[j].rectTransform.anchoredPosition;
			}
			base.gameObject.SetActive(value: false);
			instance = this;
		}
	}

	private void ApplyPreset(SpeechBubblePreset preset)
	{
		if (preset == null)
		{
			Debug.LogError("preset for SpeechBubbleType is null.");
			return;
		}
		SetColor(preset.backgroundColor);
		SetSprites(preset);
		preset.textStyle.ApplyStyle(textField);
		usingPreset = preset;
	}

	private void SetColor(Color color)
	{
		background.color = color;
		foreach (Image cornerImage in cornerImages)
		{
			cornerImage.color = color;
		}
	}

	private void SetSprites(SpeechBubblePreset preset)
	{
		if (preset.backgroundSprite != null)
		{
			background.sprite = preset.backgroundSprite;
		}
		for (int i = 0; i < cornerImages.Count; i++)
		{
			Image image = cornerImages[i];
			UIBubbleCorner uIBubbleCorner = corners[i];
			if (preset.cornerSprite != null)
			{
				image.sprite = preset.cornerSprite;
				image.SetNativeSize();
				image.enabled = true;
			}
			else
			{
				image.enabled = false;
			}
			Vector2 anchoredPosition = cachedCornerImagesAnchoredPositions[i];
			Vector2 anchoredPosition2 = cachedCornerAnchoredPositions[i];
			if (i < preset.spriteOffset.Length)
			{
				anchoredPosition += preset.spriteOffset[i];
			}
			if (i < preset.cornerOffset.Length)
			{
				anchoredPosition2 += preset.cornerOffset[i];
			}
			image.rectTransform.anchoredPosition = anchoredPosition;
			uIBubbleCorner.rectTransform.anchoredPosition = anchoredPosition2;
		}
	}

	protected virtual void Update()
	{
		if (disableBubbleCounter > 0)
		{
			if (--disableBubbleCounter <= 0)
			{
				if (useDisappearAnimation)
				{
					canvasGroup.DOFade(0f, settings.fadeTime).OnComplete(DoDisableBubble);
					TryRemoveBubbleFromActiveBubbles();
				}
				else
				{
					DoDisableBubble();
					TryRemoveBubbleFromActiveBubbles();
				}
			}
		}
		else if (bubblePauseCounter > 0f)
		{
			bubblePauseCounter -= Time.deltaTime;
			if (bubblePauseCounter <= 0f)
			{
				StartAppearAnimation();
			}
		}
		else if (!isPaused)
		{
			if (canSkipBubble)
			{
				CheckSkip();
			}
			textAnimator.CustomUpdate();
			CheckBubbleTime();
			if (textAnimator.IsAnimating && textHasLetters && playSound && (!LazyAudio.VoiceOverPlayer.HasVoiceOver || !VoiceOverSettings.IsEnabled) && voiceId != VoiceID.None)
			{
				float remainingPlayingTime = textAnimator.GetRemainingTime() + settings.voiceAdditionalAverageTime;
				LazySpeechEngine.Instance.Play(voiceId, remainingPlayingTime);
			}
			if (playSound && VoiceData != null && VoiceData.useOneSampleOncePerCue)
			{
				playSound = false;
			}
		}
	}

	private void WaitBeforeBubbleAppear(float time)
	{
		bubblePauseCounter = time;
		canvasGroup.alpha = 0f;
		textAnimator.StartTime += time;
	}

	private void CheckSkip()
	{
		bool flag = Input.GetMouseButtonDown(0);
		if (customDialogSkipCondition != null)
		{
			flag = flag || customDialogSkipCondition();
		}
		for (int i = 0; i < keysToSkip.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (LazyInput.GetKeyDown(keysToSkip[i]))
			{
				flag = true;
			}
		}
		if (flag)
		{
			LazyInput.ClearAllKeysDown();
			if (textAnimator.IsAnimating)
			{
				textAnimator.Complete();
			}
			else
			{
				bubbleShowTime = -1f;
			}
		}
	}

	private void CheckBubbleTime()
	{
		if (!textAnimator.IsAnimating)
		{
			if (bubbleShowTime > 0f)
			{
				bubbleShowTime -= Time.deltaTime;
			}
			onTextAnimationEnd?.Invoke();
			onTextAnimationEnd = null;
			if (!disappearing && !(bubbleShowTime > 0f))
			{
				disappearing = true;
				DisableBubble(animated: true);
			}
		}
	}

	protected virtual void LateUpdate()
	{
		if (onForceHideCondition != null && onForceHideCondition())
		{
			ForceHide();
		}
		else if (onUpdatePosition != null)
		{
			UpdatePositionAndCorner(onUpdatePosition(), forcedCornerPosition);
		}
	}

	private void StartAppearAnimation()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, settings.fadeTime);
	}

	protected virtual void DisableBubble(bool animated = false)
	{
		useDisappearAnimation = animated;
		playSound = false;
		if (canReuseBubbles)
		{
			if (disableBubbleCounter == -1)
			{
				disableBubbleCounter = 2;
			}
		}
		else
		{
			DoDisableBubble();
			TryRemoveBubbleFromActiveBubbles();
		}
		LazyAudio.VoiceOverPlayer.Stop();
		onDisappearedForReusingBubbles = null;
		Action action = onDisappeared;
		onDisappeared = null;
		action?.Invoke();
	}

	private void TryRemoveBubbleFromActiveBubbles()
	{
		if (activeBubbles.ContainsKey(speakerId) && activeBubbles[speakerId] == this)
		{
			activeBubbles.Remove(speakerId);
		}
	}

	private void DoDisableBubble()
	{
		canvasGroup.DOKill();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ForceHide()
	{
		DisableBubble();
	}

	private void ShowMessage(string localKey, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, bool useAppearAnimation = true, float fixedShowTime = 0f)
	{
		textField.text = string.Empty;
		forcedCornerPosition = forceCornerPosition;
		voiceOverLocalKey = localKey;
		text = usingPreset.highlightedTextColorStyle.TranslateAndColorizeTags(localKey);
		textHasLetters = text.Any(char.IsLetter);
		textAnimator.ShowMessage(textField, text, settings.letterAnimAppearTime);
		if (VoiceOverPlayer.IsMuted(localKey))
		{
			playSound = false;
		}
		else
		{
			LazyAudio.VoiceOverPlayer.Play(localKey, voiceId);
		}
		if (LazyAudio.VoiceOverPlayer.HasVoiceOver)
		{
			bubbleShowTime = CalculateVoiceOverBubbleShowTime() + 1f;
		}
		else if (fixedShowTime > 0f)
		{
			bubbleShowTime = fixedShowTime;
		}
		else
		{
			bubbleShowTime = settings.CalculateBubbleShowingTime(text);
		}
		textField.RecalculateClipping();
		UpdatePositionAndCorner(uiTargetPosition, forcedCornerPosition);
		base.gameObject.SetActive(value: true);
		EnsureTextFits();
		if (useAppearAnimation)
		{
			if (activeBubbles.Count > 1)
			{
				WaitBeforeBubbleAppear(settings.fadeTime);
			}
			else
			{
				StartAppearAnimation();
			}
		}
	}

	private float CalculateVoiceOverBubbleShowTime()
	{
		float clipLength = LazyAudio.VoiceOverPlayer.ClipLength;
		if (!IsCurrentLanguageMatchingVoiceOver())
		{
			return clipLength;
		}
		float num = Mathf.Max(0f, textAnimator.GetRemainingTime());
		return Mathf.Max(0f, clipLength - VoiceOverSettings.AdditionalClipLength - num);
	}

	private static bool IsCurrentLanguageMatchingVoiceOver()
	{
		string b = (string.IsNullOrEmpty(VoiceOverSettings.LanguageId) ? "en" : VoiceOverSettings.LanguageId);
		return string.Equals(LLBase.CurrentLang, b, StringComparison.OrdinalIgnoreCase);
	}

	protected virtual void EnsureTextFits()
	{
		textLayout.preferredWidth = LabelSizeCalculator.CalculateFitWidth(textField, text, settings.preferredWidth);
	}

	public static UISpeechBubble ShowMessage(int speakerId, string localKey, Vector2 uiPosition, SpeechBubblePreset bubblePreset, VoiceID voiceId = null, Action onDisappeared = null, Action onTextAnimationEnd = null, Func<Vector2> onUpdatePosition = null, Func<bool> onForceHideCondition = null, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, float fixedShowTime = 0f, bool canSkipBubble = true, Vector3 localScale = default(Vector3))
	{
		return ShowMessage(speakerId, localKey, uiPosition, bubblePreset, onDisappeared, onTextAnimationEnd, onUpdatePosition, onForceHideCondition, forceCornerPosition, fixedShowTime, canSkipBubble, delegate(UISpeechBubble bubble)
		{
			bubble.voiceId = voiceId ?? VoiceID.None;
		}, forceCreateNewBubbleInsteadReuse: false, localScale);
	}

	private static UISpeechBubble ShowMessage(int speakerId, string localKey, Vector2 uiPosition, SpeechBubblePreset bubblePreset, Action onDisappeared = null, Action onTextAnimationEnd = null, Func<Vector2> onUpdatePosition = null, Func<bool> onForceHideCondition = null, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, float fixedShowTime = 0f, bool canSkipBubble = true, Action<UISpeechBubble> setVoiceParameters = null, bool forceCreateNewBubbleInsteadReuse = false, Vector3 localScale = default(Vector3))
	{
		if (instance == null)
		{
			Debug.LogError("SpeechBubble called but not initialized.");
			return null;
		}
		bool flag = activeBubbles.ContainsKey(speakerId);
		if (flag && (forceCreateNewBubbleInsteadReuse || !canReuseBubbles))
		{
			activeBubbles[speakerId].ForceHide();
			activeBubbles.Remove(speakerId);
			flag = false;
		}
		UISpeechBubble uISpeechBubble;
		if (flag)
		{
			uISpeechBubble = activeBubbles[speakerId];
			uISpeechBubble.onDisappearedForReusingBubbles?.Invoke();
			uISpeechBubble.disableBubbleCounter = -1;
			uISpeechBubble.disappearing = false;
			uISpeechBubble.playSound = true;
			uISpeechBubble.voiceData = null;
		}
		else
		{
			uISpeechBubble = instance.Copy();
			activeBubbles.Add(speakerId, uISpeechBubble);
		}
		if (localScale != default(Vector3))
		{
			uISpeechBubble.RootTransform.localScale = localScale;
		}
		uISpeechBubble.uiTargetPosition = uiPosition;
		uISpeechBubble.onDisappeared = onDisappeared;
		uISpeechBubble.onDisappearedForReusingBubbles = onDisappeared;
		uISpeechBubble.onTextAnimationEnd = onTextAnimationEnd;
		uISpeechBubble.onUpdatePosition = onUpdatePosition;
		uISpeechBubble.onForceHideCondition = onForceHideCondition;
		uISpeechBubble.speakerId = speakerId;
		setVoiceParameters?.Invoke(uISpeechBubble);
		uISpeechBubble.canSkipBubble = canSkipBubble;
		uISpeechBubble.ApplyPreset(bubblePreset);
		uISpeechBubble.ShowMessage(localKey, forceCornerPosition, !flag, fixedShowTime);
		return uISpeechBubble;
	}

	public static void ForceHideAll()
	{
		for (int num = activeBubbles.Count - 1; num >= 0; num--)
		{
			activeBubbles.ElementAt(num).Value.ForceHide();
		}
	}

	public static void ForceRemoveAll()
	{
		for (int num = activeBubbles.Count - 1; num >= 0; num--)
		{
			UISpeechBubble value = activeBubbles.ElementAt(num).Value;
			value.DoDisableBubble();
			value.TryRemoveBubbleFromActiveBubbles();
		}
	}

	protected override void OnCornerChanged()
	{
		base.OnCornerChanged();
		CheckBackgroundFlip();
	}

	protected virtual void CheckBackgroundFlip()
	{
		if (flipBackgroundHorizontally != 0)
		{
			int num = ((pickedCorner == BubbleCornerDirection.RightDown || pickedCorner == BubbleCornerDirection.RightDown) ? GetRightCornerSign() : (-GetRightCornerSign()));
			if (flipBackgroundHorizontally == FlipHorizontallySetting.ForLeft)
			{
				num = -num;
			}
			background.transform.localScale = new Vector3(num, 1f, 1f);
		}
	}

	protected virtual int GetRightCornerSign()
	{
		return -1;
	}

	public static void SetCustomDialogSkipCondition(Func<bool> customDialogSkipCondition)
	{
		UISpeechBubble.customDialogSkipCondition = customDialogSkipCondition;
	}

	public static void SetPause(bool isPaused)
	{
		UISpeechBubble.isPaused = isPaused;
	}
}
