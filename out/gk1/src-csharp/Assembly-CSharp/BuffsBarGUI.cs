public class BuffsBarGUI : BaseGUI
{
	public UILabel label;

	public override void Init()
	{
		base.Init();
	}

	public void Redraw()
	{
		string text = "";
		if (MainGame.me.player.sanity <= 50f)
		{
			text = "* You lose part of your tech points";
		}
		else
		{
			_ = MainGame.me.player.sanity;
			_ = 75f;
		}
		_ = MainGame.me.player.sanity;
		_ = 25f;
		label.text = text;
	}

	public new void Update()
	{
		if (MainGame.game_started)
		{
			Redraw();
		}
	}
}
