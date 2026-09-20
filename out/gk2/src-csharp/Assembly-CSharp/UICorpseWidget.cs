using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICorpseWidget : LazyWidget<UICorpseWidgetData>
{
	[SerializeField]
	private Image bodyImage;

	[SerializeField]
	private Image zombieImage;

	[SerializeField]
	private TextStyle activeStyle;

	[SerializeField]
	private TextStyle inactiveStyle;

	[SerializeField]
	private LazyButton exhumeButton;

	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[SerializeField]
	private GameObject noBodyObj;

	[SerializeField]
	private TextMeshProUGUI buttonLabel;

	[SerializeField]
	private TextMeshProUGUI gameKeyTipLabel;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

	[SerializeField]
	private TextMeshProUGUI redSkullsValue;

	[SerializeField]
	private TextMeshProUGUI whiteSkullsValue;

	[SerializeField]
	private UIFixedTypeItemCell collarCell;

	public override void Init()
	{
		base.Init();
		exhumeButton.onClick.AddListener(OnButtonPressed);
		exhumeButton.onNotInteractableEnter.AddListener(OnNonInteractableOver);
		exhumeButton.onNotInteractableExit.AddListener(OnOut);
		UIMouseTooltip.Attach(redSkullsValue.transform.parent.gameObject, "tt_grave_2", null, addRaycastTarget: false, disableChildRaycasts: false, new UIMouseTooltipEdges(0f, 0f, -20f, -20f), new Vector2(0f, -62f));
	}

	private void OnEnable()
	{
		LazyInput.OnInputChanged += OnInputChanged;
		LazyInput.OnActiveGamepadChangedEvent += OnInputChanged;
	}

	private void OnDisable()
	{
		LazyInput.OnInputChanged -= OnInputChanged;
		LazyInput.OnActiveGamepadChangedEvent += OnInputChanged;
	}

	public override void Redraw()
	{
		base.Redraw();
		redSkullsValue.text = ((data.CollarRedSkullsLimit >= 0) ? string.Format("{0}{1}/{2}", "rskull".FontIcon(), data.RedSkulls, data.CollarRedSkullsLimit) : string.Format("{0}{1}", "rskull".FontIcon(), data.RedSkulls));
		whiteSkullsValue.text = string.Format("{0}{1}", "skull".FontIcon(), data.WhiteSkulls);
		exhumeButton.interactable = data.ButtonInteractable && !data.IsEmpty;
		bodyImage.gameObject.SetActive(value: false);
		zombieImage.gameObject.SetActive(value: false);
		if (LazyInput.IsGamepadActive)
		{
			gameKeyTipLabel.text = new LazyGameKeyTip(data.GameKeyToExhume, data.ButtonText, exhumeButton.interactable, gamepadOnly: true, translate: false).ToString();
		}
		headerLabel.text = data.HeaderText;
		descriptionLabel.text = data.DescriptionText;
		buttonLabel.text = data.ButtonText;
		if (data.IsEmpty)
		{
			inactiveStyle.ApplyStyle(headerLabel);
			redSkullsValue.gameObject.SetActive(value: false);
			whiteSkullsValue.gameObject.SetActive(value: false);
			noBodyObj.SetActive(value: true);
			collarCell.gameObject.SetActive(value: false);
			return;
		}
		activeStyle.ApplyStyle(headerLabel);
		redSkullsValue.gameObject.SetActive(value: true);
		whiteSkullsValue.gameObject.SetActive(value: true);
		noBodyObj.SetActive(value: false);
		if (data.IsZombie)
		{
			if (data.ZombieWgoData.Collar.IsEmpty)
			{
				collarCell.DrawEmpty();
				Debug.LogError($"No collar on the zombie:[{data.ZombieWgoData}]");
			}
			else
			{
				collarCell.Draw(data.ZombieWgoData.Collar);
			}
			collarCell.UIItemCell.ClearCallbacks();
			collarCell.gameObject.SetActive(value: true);
			zombieImage.gameObject.SetActive(value: true);
			SkinPresetGK2 presetForWgoData = ZombieSkinHelper.GetPresetForWgoData(data.ZombieWgoData, "zombie_worker");
			zombieImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite($"i_zombie_{presetForWgoData.head.id}", "i_body");
			presetForWgoData.TryToApply(zombieImage, zombieImage.name, presetForWgoData.head);
		}
		else
		{
			bodyImage.gameObject.SetActive(value: true);
			bodyImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.IconId, "i_body");
			collarCell.gameObject.SetActive(value: false);
		}
	}

	public void OnButtonPressed()
	{
		data.OnButtonPressed?.Invoke();
	}

	private void OnInputChanged()
	{
		if (LazyInput.IsGamepadActive)
		{
			gameKeyTipLabel.text = new LazyGameKeyTip(data.GameKeyToExhume, data.ButtonText, exhumeButton.interactable, gamepadOnly: true, translate: false).ToString();
		}
	}

	private void OnNonInteractableOver()
	{
		if (!data.IsEmpty && !data.ButtonInteractable)
		{
			data.OnNonInteractableButtonOver?.Invoke(exhumeButton);
		}
	}

	private void OnOut()
	{
		UITooltip.Hide();
	}

	protected override void TestDraw()
	{
	}
}
