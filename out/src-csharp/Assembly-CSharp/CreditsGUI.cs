using DG.Tweening;
using UnityEngine;

public class CreditsGUI : BaseGUI
{
	public GameObject obj;

	public UIWidget first_line;

	public UIWidget last_line;

	public GameObject black;

	private float _y;

	private float _y0;

	private bool _scrolling;

	public float scroll_speed = 0.1f;

	protected override bool OnPressedBack()
	{
		GUIElements.me.main_menu.OnBackFromCredits();
		return true;
	}

	public override void Open()
	{
		Open(play_open_sound: false);
		UIPanel uipanel = base.gameObject.GetComponent<UIPanel>();
		uipanel.alpha = 0f;
		DOTween.To(delegate(float alpha)
		{
			uipanel.alpha = alpha;
		}, 0f, 1f, 1f);
		_y0 = 0f - first_line.transform.localPosition.y - (float)Screen.height / 4f - 10f;
		_y = 0f - (first_line.transform.localPosition.y - (float)Screen.height / 12f);
		obj.transform.localPosition = new Vector3(0f, _y);
		_scrolling = true;
		black.SetActive(value: false);
		obj.transform.localPosition = Vector3.zero;
	}

	public void OpenScrolling()
	{
		Open(play_open_sound: false);
		_y0 = (_y = Mathf.RoundToInt(0f - first_line.transform.localPosition.y - (float)Screen.height / 4f - 10f));
		obj.transform.localPosition = new Vector3(0f, _y);
		_scrolling = true;
		black.SetActive(value: true);
		GUIElements.me.ingame_menu.SetControllsActive(active: false);
	}

	public override void Update()
	{
		base.Update();
		if (_scrolling)
		{
			_y += Time.deltaTime * scroll_speed;
			obj.transform.localPosition = new Vector3(0f, _y);
			if (obj.transform.localPosition.y - Mathf.Abs(last_line.transform.localPosition.y) > (float)Screen.height / 4f + 10f)
			{
				_y = _y0;
			}
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		base.Hide(play_hide_sound);
		if (_scrolling)
		{
			GUIElements.me.ingame_menu.ReturnToMainMenu();
		}
	}
}
