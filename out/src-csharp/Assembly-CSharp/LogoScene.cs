using LazyBearGames.Preloader;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoScene : MonoBehaviour
{
	public LBPreloader preloader;

	private void Awake()
	{
		preloader.SetOnFinishedDelegate(OnFinished);
		preloader.StartAnimations();
	}

	private void OnFinished()
	{
		SceneManager.LoadScene("preloader");
	}
}
