using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIDialogInputWindow : LazyWindow<DialogInputWindowData>
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TMP_InputField idInputField;

	[SerializeField]
	private TMP_InputField amountInputField;

	[SerializeField]
	private LazyButton actionButton;

	[SerializeField]
	private TextMeshProUGUI actionButtonText;

	private Action<string, float> onActionButtonClicked;

	public override void Init()
	{
		base.Init();
		actionButton.onClick.AddListener(OnActionButtonClicked);
		amountInputField.onValidateInput = (string text, int index, char addedChar) => "0123456789".Contains(addedChar) ? addedChar : '\0';
	}

	protected override void SetData(DialogInputWindowData data)
	{
		base.SetData(data);
		onActionButtonClicked = data.OnButtonPressed;
	}

	public override void Redraw()
	{
		base.Redraw();
		label.text = LLBase.L(data.HeaderText ?? "");
		actionButtonText.text = data.ButtonText;
	}

	public override void Hide()
	{
		base.Hide();
		onActionButtonClicked = null;
	}

	private void OnActionButtonClicked()
	{
		onActionButtonClicked?.Invoke(idInputField.text, float.TryParse(amountInputField.text, out var result) ? result : 0f);
		Close();
	}

	protected override void TestDraw()
	{
		Draw(new DialogInputWindowData("Test", "OK", null));
	}
}
