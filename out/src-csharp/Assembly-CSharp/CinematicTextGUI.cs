public class CinematicTextGUI : BaseGUI
{
	public UILabel text;

	private GJCommons.VoidDelegate _on_closed;

	public void Open(string txt_id, GJCommons.VoidDelegate on_closed = null)
	{
		text.text = GJL.L(txt_id);
		_on_closed = on_closed;
		base.Open();
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select("ok"));
		}
		else
		{
			base.button_tips.Clear();
		}
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	protected override bool OnPressedSelect()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		Hide();
		if (_on_closed != null)
		{
			GJCommons.VoidDelegate on_closed = _on_closed;
			_on_closed = null;
			on_closed?.Invoke();
		}
	}
}
