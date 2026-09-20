using Galaxy.Api;
using UnityEngine;

[DisallowMultipleComponent]
public class GogGalaxyManager : MonoBehaviour
{
	public string clientID;

	public string clientSecret;

	private static GogGalaxyManager singleton;

	private bool isInitialized;

	public static GogGalaxyManager Instance
	{
		get
		{
			if (singleton == null)
			{
				return new GameObject("GogGalaxyManager").AddComponent<GogGalaxyManager>();
			}
			return singleton;
		}
	}

	public static bool IsInitialized()
	{
		if (singleton != null)
		{
			return singleton.isInitialized;
		}
		return false;
	}

	private void Awake()
	{
		if (singleton != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		singleton = this;
		Object.DontDestroyOnLoad(base.gameObject);
		try
		{
			GalaxyInstance.Init(new InitParams(clientID, clientSecret));
		}
		catch (GalaxyInstance.Error error)
		{
			Debug.LogError("Failed to initialize GOG Galaxy: Error = " + error.ToString(), this);
			return;
		}
		Debug.Log("Galaxy SDK was initialized", this);
		isInitialized = true;
	}

	private void OnDestroy()
	{
		if (singleton != this)
		{
			return;
		}
		singleton = null;
		if (isInitialized)
		{
			if (Application.isEditor)
			{
				GalaxyInstance.ShutdownEx(new ShutdownParams(_preserveStaticObjects: true));
			}
			else
			{
				GalaxyInstance.Shutdown(unloadModule: true);
			}
		}
	}

	private void Update()
	{
		if (isInitialized)
		{
			GalaxyInstance.ProcessData();
		}
	}
}
