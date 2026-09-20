using UnityEngine;

public class GameLogo : MonoBehaviour
{
	public enum GameLogoType
	{
		Vanilla,
		DLC,
		DLC_Texted
	}

	[SerializeField]
	private GameObject logo_vanilla;

	[SerializeField]
	private GameObject logo_dlc;

	[SerializeField]
	private GameObject logo_dlc_texted;

	public void Show(GameLogoType game_logo_type)
	{
		logo_vanilla.SetActive(value: false);
		logo_dlc.SetActive(value: false);
		logo_dlc_texted.SetActive(value: false);
		switch (game_logo_type)
		{
		case GameLogoType.Vanilla:
			logo_vanilla.SetActive(value: true);
			break;
		case GameLogoType.DLC:
			logo_dlc.SetActive(value: true);
			break;
		case GameLogoType.DLC_Texted:
			logo_dlc_texted.SetActive(value: true);
			break;
		}
	}
}
