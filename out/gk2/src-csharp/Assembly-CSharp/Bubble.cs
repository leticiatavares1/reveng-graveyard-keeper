using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour, ILazyGUIElement
{
	[SerializeField]
	private float boundsOffset;

	[SerializeField]
	private float boundsOffsetY;

	[SerializeField]
	private TextMeshProUGUI speechBubbleLabel;

	[SerializeField]
	protected SpeechBubbleSettings settings;

	[SerializeField]
	protected List<UIBubbleCorner> corners;

	[SerializeField]
	private UIDialogBubble speechBubble;

	[SerializeField]
	private UIMultiAnswer multiAnswer;

	[SerializeField]
	private UIInteractingItem interactingItem;

	[SerializeField]
	private List<Image> speechBubbleCorners;

	[SerializeField]
	private Sprite defaultCornerSprite;

	[Space]
	[SerializeField]
	private List<SpeechBubblePreset> speechBubblePresets;

	private static Bubble instance;

	public void Init()
	{
		instance = this;
		speechBubble.Init();
		multiAnswer.Init();
		interactingItem.Init();
		MainGame.OnGameStarted = (Action)Delegate.Combine(MainGame.OnGameStarted, new Action(OnGameStarted));
	}

	private void OnGameStarted()
	{
		UISpeechBubble.ForceRemoveAll();
		List<UIMultiAnswer> list = GetComponentsInChildren<UIMultiAnswer>().ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != multiAnswer)
			{
				UnityEngine.Object.Destroy(list[i]);
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public static void ShowMultiAnswer(List<AnswerVisualData> answers, Transform targetTransform, WgoData dialogParticipant, Action<string> onChosen, Action onDisappeared, bool isOverBlackout = false)
	{
		UIMultiAnswer.ShowAnswers(answers, targetTransform, dialogParticipant, onChosen, onDisappeared, UIBasicBubble.ForceCornerPosition.Auto, isMainMultianswer: false, isOverBlackout);
	}

	public static void Talk(PhraseData data)
	{
		if (data.isPlayer)
		{
			data.preset = ((data.speechType == SpeechBubbleType.Talk) ? instance.speechBubblePresets[2] : instance.speechBubblePresets[3]);
			Debug.Log("#talk# [Player]: " + data.text);
		}
		else
		{
			data.preset = ((data.speechType == SpeechBubbleType.Talk) ? instance.speechBubblePresets[0] : instance.speechBubblePresets[1]);
			if (data.npcWgoData == null)
			{
				Debug.LogError("Talk error: NPC WGO is null, text = " + data.text);
				data.onFinished();
				return;
			}
			Debug.Log("#talk# [" + data.npcWgoData.id + "]: " + data.text);
		}
		if (data.cornerPosition == UIBasicBubble.ForceCornerPosition.Auto)
		{
			Vector3 targetPos = data.GetTargetPos();
			Direction direction = data.GetDirection();
			if (direction == Direction.Left || direction == Direction.Right)
			{
				UIBasicBubble.ForceCornerPosition forceCornerPosition = GetForceCornerPosition(UIBasicBubble.ForceCornerPosition.Auto, targetPos, direction, data.text, data.preset);
				data.cornerPosition = forceCornerPosition;
			}
		}
		UIDialogBubble.ShowMessage(data).GetComponent<RectTransform>().RefreshContentFitter();
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.SpeechSay, data.text);
	}

	public static void SetVisibility(bool isVisible)
	{
		instance.gameObject.SetActive(isVisible);
	}

	private static UIBasicBubble.ForceCornerPosition GetForceCornerPosition(UIBasicBubble.ForceCornerPosition forceCornerPosition, Vector3 pos, Direction direction, string textLocale, SpeechBubblePreset preset)
	{
		Vector3 screenPos = default(Vector3);
		Bounds screenBounds = default(Bounds);
		Vector2 bounds = default(Vector2);
		if (forceCornerPosition == UIBasicBubble.ForceCornerPosition.Auto)
		{
			CalcScreenPos();
			CalcScreenBounds();
			CalcBounds();
			if (direction == Direction.Right && Left())
			{
				forceCornerPosition = (Up() ? UIBasicBubble.ForceCornerPosition.BottomRight : UIBasicBubble.ForceCornerPosition.TopRight);
			}
			else if (direction == Direction.Left && Right())
			{
				forceCornerPosition = (Up() ? UIBasicBubble.ForceCornerPosition.BottomLeft : UIBasicBubble.ForceCornerPosition.TopLeft);
			}
		}
		return forceCornerPosition;
		void CalcBounds()
		{
			preset.textStyle.ApplyStyle(instance.speechBubbleLabel);
			Vector2 vector = LabelSizeCalculator.CalculateFitVector(instance.speechBubbleLabel, preset.highlightedTextColorStyle.TranslateAndColorizeTags(textLocale), instance.settings.preferredWidth);
			bounds = new Vector2(vector.x, vector.y);
			bounds.Scale(Vector2.one * LazyUI.ScaleFactor);
			bounds += new Vector2(instance.boundsOffset, instance.boundsOffsetY + Mathf.Abs(instance.corners[1].rectTransform.localPosition.y)) * LazyUI.ScaleFactor;
		}
		void CalcScreenBounds()
		{
			screenBounds = LazyUI.GetScreenBounds();
		}
		void CalcScreenPos()
		{
			screenPos = CameraSystem.WorldToScreenPoint(pos);
		}
		bool Left()
		{
			return screenPos.x - bounds.x > 0f;
		}
		bool Right()
		{
			return screenPos.x + bounds.x < screenBounds.max.x;
		}
		bool Up()
		{
			return screenPos.y + bounds.y < screenBounds.max.y;
		}
	}
}
