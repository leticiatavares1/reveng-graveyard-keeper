using UnityEngine;

public class MainMenuLogoController : MonoBehaviour
{
	[SerializeField]
	private GameLogo game_logo;

	[SerializeField]
	private DLCLogoController dlc_logo_controller;

	[SerializeField]
	private SimpleUITable _ui_table;

	public void ShowLogos()
	{
		ShowGameLogo();
		dlc_logo_controller.Show();
		_ui_table.Reposition();
	}

	public void ShowGameLogo()
	{
		int num = DLCEngine.DLCAvailableCount();
		if (num == 0)
		{
			game_logo.Show(GameLogo.GameLogoType.Vanilla);
		}
		else if ((float)Screen.width / (float)Screen.height > 1.5f)
		{
			if (Screen.width <= 1280)
			{
				if (num > 2)
				{
					game_logo.Show(GameLogo.GameLogoType.DLC_Texted);
				}
				else
				{
					game_logo.Show(GameLogo.GameLogoType.DLC);
				}
			}
			else if (Screen.width <= 1440)
			{
				if (num == 4)
				{
					game_logo.Show(GameLogo.GameLogoType.DLC_Texted);
				}
				else
				{
					game_logo.Show(GameLogo.GameLogoType.DLC);
				}
			}
			else
			{
				game_logo.Show(GameLogo.GameLogoType.DLC);
			}
		}
		else
		{
			game_logo.Show(GameLogo.GameLogoType.DLC);
		}
	}
}
