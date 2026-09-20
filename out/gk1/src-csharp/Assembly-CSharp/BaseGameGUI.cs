public class BaseGameGUI : BaseGUI
{
	public virtual void OpenFromGameGUI()
	{
		Open();
	}

	public virtual void CloseFromGameGUI()
	{
		OnClosePressed();
	}
}
