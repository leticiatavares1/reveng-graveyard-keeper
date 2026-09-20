using UnityEngine;

public class TitleScreen : MonoBehaviour
{
	public TitleScreenCamera camera;

	public static TitleScreen me;

	public void Awake()
	{
		Debug.Log("TitleScreen.Awake", this);
		me = this;
	}

	public static void Show()
	{
		Debug.Log("TitleScreen.Show", me);
		if (me != null)
		{
			me.gameObject.SetActive(value: true);
		}
		RenderSettings.ambientLight = Color.white;
		RenderSettings.ambientIntensity = 1f;
	}

	public static void Hide()
	{
		Debug.Log("TitleScreen.Hide", me);
		if (me != null)
		{
			me.gameObject.SetActive(value: false);
		}
	}
}
