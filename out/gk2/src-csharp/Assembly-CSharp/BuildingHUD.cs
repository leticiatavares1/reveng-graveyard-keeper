using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingHUD : LazyWidget<BuildingHUDData>
{
	[SerializeField]
	private TextMeshProUGUI buildHint;

	[SerializeField]
	private TextMeshProUGUI rotationHint;

	[SerializeField]
	private TextMeshProUGUI exitHint;

	[SerializeField]
	private TextMeshProUGUI buildHintGamepad;

	[SerializeField]
	private TextMeshProUGUI rotationHintGamepad;

	[SerializeField]
	private TextMeshProUGUI exitHintGamepad;

	[SerializeField]
	private GameObject gamepadParent;

	[SerializeField]
	private GameObject mouseParent;

	public override void Init()
	{
		base.Init();
		LazyInput.OnInputChanged += UpdateHints;
	}

	public override void Redraw()
	{
		UpdateHints();
	}

	private void UpdateHints()
	{
		if (base.gameObject.activeSelf && data != null)
		{
			if (data.isTargetCanBeRotated)
			{
				rotationHint.transform.parent.gameObject.SetActive(value: true);
				rotationHintGamepad.gameObject.SetActive(value: true);
			}
			else
			{
				rotationHint.transform.parent.gameObject.SetActive(value: false);
				rotationHintGamepad.gameObject.SetActive(value: false);
			}
			if (LazyInput.IsGamepadActive)
			{
				gamepadParent.SetActive(value: true);
				mouseParent.SetActive(value: false);
			}
			else
			{
				gamepadParent.SetActive(value: false);
				mouseParent.SetActive(value: true);
			}
			buildHintGamepad.text = ControllerIconLibrary.GetIconId(GameKey.Build) + LLBase.L("ui_build");
			rotationHintGamepad.text = ControllerIconLibrary.GetIconId(GameKey.Rotate) + LLBase.L("ui_rotate");
			exitHintGamepad.text = ControllerIconLibrary.GetIconId(GameKey.Back) + LLBase.L("ui_build_mode_exit");
			buildHint.text = LLBase.L("ui_build");
			rotationHint.text = LLBase.L("ui_rotate");
			exitHint.text = LLBase.L("ui_build_mode_exit");
			((RectTransform)base.transform).RefreshContentFitterAndDisable();
		}
	}

	protected override void TestDraw()
	{
	}
}
