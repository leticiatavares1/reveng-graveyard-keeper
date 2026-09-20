using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINotesWindow : LazyWindow<UINotesWindowData>
{
	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI textLabel;

	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private float maxHeightPercent = 0.8f;

	[SerializeField]
	private float aspectRatio = 1.7777778f;

	[SerializeField]
	private RectTransform root;

	[SerializeField]
	private float minHeight = 120f;

	[SerializeField]
	private RectTransform commonParent;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private float gamepadScrollSpeed = 0.03f;

	private float ySpeed;

	private float hSpeed;

	private float hPos;

	private float vPos;

	private bool scrollEnabled;

	private UIDialogWindowData.ButtonData btnData;

	private Vector2 Direction => LazyInput.GetDirection2();

	public override void Init()
	{
		base.Init();
		scrollRect.vertical = true;
		scrollRect.horizontal = false;
	}

	public override void Draw(UINotesWindowData data)
	{
		base.Draw(data);
		nameLabel.text = LLBase.L(data.NoteItem.id);
		textLabel.text = LLBase.L(data.NoteItem.id + "_note");
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		lazyButton.Draw(btnData);
		Fit();
	}

	private void Fit()
	{
		float num = root.rect.height * maxHeightPercent;
		textLabel.FitToAspectOrOverflowAndGetScrollHeight(aspectRatio, minHeight, num, out var heightForScroll);
		if (num - textLabel.rectTransform.sizeDelta.y <= 10f)
		{
			scrollEnabled = true;
			scrollRect.verticalNormalizedPosition = 0f;
			scrollRect.gameObject.SetActive(value: true);
			Vector2 sizeDelta = new Vector2(textLabel.rectTransform.sizeDelta.x, heightForScroll);
			scrollRect.GetComponent<RectTransform>().sizeDelta = sizeDelta;
			textLabel.transform.SetParent(scrollRect.content);
			textLabel.rectTransform.anchoredPosition = new Vector2(textLabel.rectTransform.anchoredPosition.x, 0f);
		}
		else
		{
			scrollEnabled = false;
			scrollRect.gameObject.SetActive(value: false);
			textLabel.transform.SetParent(commonParent);
		}
		root.RefreshContentFitter();
	}

	protected override void Update()
	{
		base.Update();
		if (scrollEnabled && LazyInput.IsGamepadActive)
		{
			hSpeed = Direction.x * (Mathf.Abs(hSpeed) + 0.1f);
			ySpeed = Direction.y * (Mathf.Abs(ySpeed) + 0.1f);
			vPos = scrollRect.verticalNormalizedPosition + ySpeed * gamepadScrollSpeed;
			hPos = scrollRect.horizontalNormalizedPosition + hSpeed * gamepadScrollSpeed;
			ySpeed = Mathf.Lerp(ySpeed, 0f, 0.1f);
			hSpeed = Mathf.Lerp(hSpeed, 0f, 0.1f);
			if (scrollRect.movementType == ScrollRect.MovementType.Clamped)
			{
				vPos = Mathf.Clamp01(vPos);
				hPos = Mathf.Clamp01(hPos);
			}
			scrollRect.verticalNormalizedPosition = vPos;
			scrollRect.horizontalNormalizedPosition = hPos;
		}
	}

	protected override void PrintTips()
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (scrollEnabled)
		{
			list.Add(new LazyGameKeyTip(GameKey.RightStickAsGameKey, "tip_scroll"));
		}
		lazyButtonTips.Print(list);
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Open(new UINotesWindowData(GameBalance.Me.GetData<ItemDef>("test_note")));
	}
}
