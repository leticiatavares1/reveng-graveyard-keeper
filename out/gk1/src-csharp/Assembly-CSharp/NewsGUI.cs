using UnityEngine;

public class NewsGUI : MonoBehaviour
{
	public static string update_url;

	public static float last_ver;

	public GameObject loading_news_go;

	public GameObject update_button;

	public const bool NEWS_AVAILABLE = false;

	public void Open()
	{
		Hide();
	}

	public void Init()
	{
		Hide();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
