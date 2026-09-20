using LazyBearTechnology;
using UnityEngine;

public static class LazyUITester
{
	private const string KEY_TEST_MODE = "LazyUITester.TestMode";

	public static bool isTesting => !string.IsNullOrEmpty(PlayerPrefs.GetString("LazyUITester.TestMode"));

	public static void OnGameStart()
	{
		Debug.Log("LazyUITester.OnGameStart");
		string @string = PlayerPrefs.GetString("LazyUITester.TestMode");
		SetTestMode();
		if (!string.IsNullOrEmpty(@string))
		{
			new LazyUITesterMethodData(@string).Invoke();
		}
	}

	public static void SetTestMode(LazyUITesterMethodData methodToRun = null)
	{
		PlayerPrefs.SetString("LazyUITester.TestMode", (methodToRun == null) ? "" : methodToRun.ToJson());
	}
}
