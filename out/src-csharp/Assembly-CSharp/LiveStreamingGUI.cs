public class LiveStreamingGUI : BaseMenuGUI
{
	private bool _mixer;

	public SimpleUITable table;

	public MenuItemGUI btn_mixer;

	public MenuItemGUI btn_twitch;

	public override void Open()
	{
		base.Open();
		Redraw();
	}

	private void Redraw()
	{
		btn_mixer.additional_go.SetActive(_mixer);
	}

	public void OnBackBtnClicked()
	{
		OnClosePressed();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		base.OnClosePressed();
		GUIElements.me.main_menu.Open(switch_music: false);
	}

	public void OnPressedMixer()
	{
		if (MixerLightIntegration.IsAvailable())
		{
			_mixer = !_mixer;
			MixerLightIntegration.ApplyEnableMode(_mixer ? 1 : 0);
			Redraw();
		}
	}
}
