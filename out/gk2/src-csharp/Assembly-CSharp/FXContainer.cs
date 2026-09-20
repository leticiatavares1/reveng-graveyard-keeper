using System.Collections.Generic;
using UnityEngine;

public class FXContainer : MonoBehaviour
{
	[SerializeField]
	private List<WorldFX> fxObjects = new List<WorldFX>();

	private void Awake()
	{
		foreach (WorldFX fxObject in fxObjects)
		{
			fxObject.gameObject.SetActive(value: false);
		}
	}

	public void PlayFx(string fxName)
	{
		WorldFX worldFX = fxObjects.Find((WorldFX x) => x.gameObject.name == fxName);
		if (worldFX == null)
		{
			Debug.LogWarning("FX not found: " + fxName);
			return;
		}
		worldFX.gameObject.SetActive(value: true);
		worldFX.Play(null, delegate
		{
			worldFX.gameObject.SetActive(value: false);
		});
	}
}
