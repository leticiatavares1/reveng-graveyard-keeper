using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGraveWindow : LazyWindow<UIGraveWindowData>
{
	private const int MAX_AMOUNT_OF_SKULLS = 11;

	[SerializeField]
	private TextMeshProUGUI graveStatusIcon;

	[SerializeField]
	private TextMeshProUGUI graveQualityValue;

	[SerializeField]
	private TextMeshProUGUI sliderValue1;

	[SerializeField]
	private TextMeshProUGUI sliderValue2;

	[SerializeField]
	private TextStyle redStyle;

	[SerializeField]
	private TextStyle whiteStyle;

	[SerializeField]
	private Transform skullsCorpseParent;

	[SerializeField]
	private Transform skullsElementsParent;

	[SerializeField]
	private Transform skullsCorpseParent2;

	[SerializeField]
	private Transform skullsElementsParent2;

	[SerializeField]
	private TextMeshProUGUI skullPrefab;

	[SerializeField]
	private Slider skullsSliderRed;

	[SerializeField]
	private Slider skullsSliderWhite;

	[SerializeField]
	private float skullsSliderRedTopOffset;

	[SerializeField]
	private float skullsSliderWhiteBottomOffset;

	[SerializeField]
	private float skullsSliderWhiteTopOffset;

	[SerializeField]
	private Image topBg;

	[SerializeField]
	private Sprite topBgRed;

	[SerializeField]
	private Sprite topBgWhite;

	[SerializeField]
	private float editorPreviewQuality;

	[SerializeField]
	private int editorPreviewRedSkulls;

	[SerializeField]
	private int editorPreviewWhiteSkulls;

	[SerializeField]
	private UIGraveElementWidget tombstoneWidget;

	[SerializeField]
	private UIGraveElementWidget fenceWidget;

	[SerializeField]
	private UICorpseWidget corpseWIdget;

	private List<TextMeshProUGUI> skullPrefabsPool = new List<TextMeshProUGUI>();

	private List<TextMeshProUGUI> skullElements = new List<TextMeshProUGUI>();

	public override void Init()
	{
		base.Init();
		skullPrefab.gameObject.SetActive(value: false);
		UIMouseTooltip.Attach(graveQualityValue.transform.parent.gameObject, "tt_grave_1", null, addRaycastTarget: true, disableChildRaycasts: true, new UIMouseTooltipEdges(10f, 10f, 9f));
	}

	public override void Open(UIGraveWindowData data)
	{
		base.Open(data);
		data.WgoData.TrySetWorker(MainGame.PlayerController);
		TryStartTombstoneItemCellSelectionBlinking();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		tombstoneWidget.Draw(data.TombstoneWidgetData);
		fenceWidget.Draw(data.FenceWidgetData);
		if (!data.CorpseWidgetData.IsEmpty)
		{
			corpseWIdget.Draw(data.CorpseWidgetData);
		}
		else
		{
			corpseWIdget.Hide();
		}
		int num = Math.Min(data.CorpseWidgetData.RedSkulls, 11);
		DrawQuality(num, QualityType.RedSkull, skullsCorpseParent, skullsCorpseParent2);
		int count = Math.Min(data.CorpseWidgetData.WhiteSkulls, 11 - num);
		DrawQuality(count, QualityType.WhiteSkull, skullsCorpseParent, skullsCorpseParent2);
		int num2 = data.TombstoneWidgetData.Quality + data.FenceWidgetData.Quality;
		DrawQuality(Math.Min(num2, 11), QualityType.Wreath, skullsElementsParent, skullsElementsParent2);
		DrawCorpseSkullsOverflow(data.CorpseWidgetData.RedSkulls, data.CorpseWidgetData.WhiteSkulls);
		sliderValue2.transform.parent.gameObject.SetActive(value: false);
		DrawSkullsSliders(data.Quality);
		if (num2 > 11)
		{
			sliderValue2.transform.parent.gameObject.SetActive(value: true);
			sliderValue2.text = $"+{num2 - 11 - 1}";
		}
		DrawQuality();
	}

	private void TryStartTombstoneItemCellSelectionBlinking()
	{
		if (!MainGame.PlayerData.openedGraveWindowOnce)
		{
			MainGame.PlayerData.openedGraveWindowOnce = true;
			tombstoneWidget.StartItemCellSelectionBlinking();
		}
	}

	public override void Hide()
	{
		base.Hide();
		foreach (TextMeshProUGUI skullElement in skullElements)
		{
			skullElement.gameObject.SetActive(value: false);
			skullPrefabsPool.Add(skullElement);
		}
		skullElements.Clear();
		corpseWIdget.Hide();
	}

	public override void Close()
	{
		if (data.WgoData.Worker is PlayerController playerController && playerController == MainGame.PlayerController)
		{
			data.WgoData.ClearWorker();
		}
		base.Close();
	}

	private void DrawQuality()
	{
		DrawQuality(data.Quality);
	}

	private void DrawQuality(float quality)
	{
		graveQualityValue.text = quality.ToInvariantCultureString();
		DrawTopBg(quality);
		if (quality >= 0f)
		{
			graveStatusIcon.text = "wskull".FontIcon();
			whiteStyle.ApplyStyle(graveQualityValue);
		}
		else
		{
			graveStatusIcon.text = "wrskull".FontIcon();
			redStyle.ApplyStyle(graveQualityValue);
		}
	}

	private void DrawTopBg(float quality)
	{
		if (!(topBg == null))
		{
			if (quality > 0f)
			{
				topBg.sprite = topBgWhite;
				topBg.gameObject.SetActive(value: true);
			}
			else if (quality < 0f)
			{
				topBg.sprite = topBgRed;
				topBg.gameObject.SetActive(value: true);
			}
			else
			{
				topBg.gameObject.SetActive(value: false);
			}
		}
	}

	private void DrawSkullsSliders(float quality)
	{
		int redSkulls = Mathf.Clamp(data.CorpseWidgetData.RedSkulls, 0, 11);
		DrawSkullsSliders(quality, redSkulls);
	}

	private void DrawSkullsSliders(float quality, int redSkulls)
	{
		DrawRedSkullsSlider(quality, redSkulls);
		DrawWhiteSkullsSlider(quality, redSkulls);
	}

	private void DrawRedSkullsSlider(float quality, int redSkulls)
	{
		if (!(skullsSliderRed == null))
		{
			skullsSliderRed.value = ApplySliderValueTopOffset(skullsSliderRed, (float)redSkulls / 11f, skullsSliderRedTopOffset);
			skullsSliderRed.gameObject.SetActive(quality < 0f && skullsSliderRed.value > 0f);
		}
	}

	private void DrawWhiteSkullsSlider(float quality, int redSkulls)
	{
		if (!(skullsSliderWhite == null))
		{
			float num = (float)redSkulls / 11f;
			float num2 = 1f - num;
			if (quality <= 0f || num2 <= 0f)
			{
				skullsSliderWhite.gameObject.SetActive(value: false);
				return;
			}
			SetSliderRectZone(skullsSliderWhite, num, 1f, skullsSliderWhiteBottomOffset, 0f);
			skullsSliderWhite.value = ApplySliderValueTopOffset(skullsSliderWhite, Mathf.Clamp01(quality / (float)(11 - redSkulls)), skullsSliderWhiteTopOffset);
			skullsSliderWhite.gameObject.SetActive(skullsSliderWhite.value > 0f);
		}
	}

	private float ApplySliderValueTopOffset(Slider slider, float value, float topOffset)
	{
		if (Mathf.Approximately(topOffset, 0f))
		{
			return value;
		}
		RectTransform rectTransform = ((slider.fillRect != null) ? (slider.fillRect.parent as RectTransform) : null);
		if (rectTransform == null || rectTransform.rect.height <= 0f)
		{
			return value;
		}
		return Mathf.Clamp01(value - topOffset / rectTransform.rect.height);
	}

	private void SetSliderRectZone(Slider slider, float anchorMinY, float anchorMaxY, float bottomOffset, float topOffset)
	{
		RectTransform rectTransform = slider.transform as RectTransform;
		if (!(rectTransform == null))
		{
			Vector2 anchorMin = rectTransform.anchorMin;
			Vector2 anchorMax = rectTransform.anchorMax;
			anchorMin.y = anchorMinY;
			anchorMax.y = anchorMaxY;
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchorMax = anchorMax;
			Vector2 offsetMin = rectTransform.offsetMin;
			Vector2 offsetMax = rectTransform.offsetMax;
			offsetMin.y = bottomOffset;
			offsetMax.y = 0f - topOffset;
			rectTransform.offsetMin = offsetMin;
			rectTransform.offsetMax = offsetMax;
		}
	}

	private void DrawCorpseSkullsOverflow(int redSkulls, int whiteSkulls)
	{
		if (!(sliderValue1 == null) && !(sliderValue1.transform.parent == null))
		{
			int num = redSkulls + whiteSkulls;
			sliderValue1.transform.parent.gameObject.SetActive(value: false);
			if (num > 11)
			{
				sliderValue1.transform.parent.gameObject.SetActive(value: true);
				sliderValue1.text = $"+{num - 11 - 1}";
			}
		}
	}

	private void Editor_RedrawQualityPreview()
	{
		if (!Application.isPlaying)
		{
			int redSkulls = Mathf.Clamp(editorPreviewRedSkulls, 0, 11);
			int whiteSkulls = Mathf.Max(editorPreviewWhiteSkulls, 0);
			DrawQuality(editorPreviewQuality);
			DrawCorpseSkullsOverflow(redSkulls, whiteSkulls);
			DrawSkullsSliders(editorPreviewQuality, redSkulls);
		}
	}

	private void DrawQuality(int count, QualityType qualityType, Transform parent1, Transform parent2)
	{
		for (int i = 0; i < count; i++)
		{
			TextMeshProUGUI textMeshProUGUI;
			TextMeshProUGUI textMeshProUGUI2;
			if (i > skullPrefabsPool.Count - 1)
			{
				textMeshProUGUI = skullPrefab.Copy(null, activate: false);
				textMeshProUGUI2 = skullPrefab.Copy(null, activate: false);
			}
			else
			{
				textMeshProUGUI = skullPrefabsPool.PopLast();
				textMeshProUGUI2 = skullPrefabsPool.PopLast();
			}
			string text = string.Empty;
			switch (qualityType)
			{
			case QualityType.Wreath:
				if (data.CorpseWidgetData.RedSkulls > i)
				{
					text = "wr_red".FontIcon();
					textMeshProUGUI.transform.SetParent(parent2);
					textMeshProUGUI2.transform.SetParent(parent1);
				}
				else
				{
					text = "wr".FontIcon();
					textMeshProUGUI.transform.SetParent(parent1);
					textMeshProUGUI2.transform.SetParent(parent2);
				}
				break;
			case QualityType.RedSkull:
				text = "rskull".FontIcon();
				textMeshProUGUI.transform.SetParent(parent2);
				textMeshProUGUI2.transform.SetParent(parent1);
				break;
			case QualityType.WhiteSkull:
				text = "skull".FontIcon();
				textMeshProUGUI.transform.SetParent(parent1);
				textMeshProUGUI2.transform.SetParent(parent2);
				break;
			}
			textMeshProUGUI.text = text;
			textMeshProUGUI.gameObject.SetActive(value: true);
			textMeshProUGUI.transform.SetAsLastSibling();
			skullElements.Add(textMeshProUGUI);
			UIMouseTooltip.Attach(textMeshProUGUI.gameObject, "tt_grave_4", null, addRaycastTarget: true);
			textMeshProUGUI2.text = string.Empty;
			textMeshProUGUI2.gameObject.SetActive(value: true);
			textMeshProUGUI2.transform.SetAsLastSibling();
			skullElements.Add(textMeshProUGUI2);
			UIMouseTooltip.Attach(textMeshProUGUI2.gameObject, "tt_grave_4", null, addRaycastTarget: true);
		}
	}

	private bool OnExhumeButtonPressed()
	{
		corpseWIdget.OnButtonPressed();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.ExtractBody, OnExhumeButtonPressed);
		return gameKeyDelegates;
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		list.Add(LazyGameKeyTip.Select());
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		WgoData wgoData = new WgoData("grave_body", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		wgoData.Inventory.AddItemToInventory(GameBalance.Me.GetData<BodyDef>("body_0_1").GenerateItem());
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		LazyUI.GetWindow<UIGraveWindow>().Open(new UIGraveWindowData(wgo));
	}
}
