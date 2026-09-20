using UnityEngine;

public static class HudFX
{
	public static GameObject Spawn(RectTransform parent, string fxName)
	{
		if (parent == null || string.IsNullOrEmpty(fxName))
		{
			return null;
		}
		if (!WorldFXPool.TryLoadPrefab(fxName, out var prefab))
		{
			Debug.LogError("Error spawning HudFX \"" + fxName + "\": Effect not found.");
			return null;
		}
		GameObject gameObject = Object.Instantiate(prefab, parent, worldPositionStays: false);
		gameObject.name = fxName;
		gameObject.transform.SetAsLastSibling();
		if (gameObject.transform is RectTransform rectTransform)
		{
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localRotation = Quaternion.identity;
		}
		Object.Destroy(gameObject, GetLifetime(gameObject));
		return gameObject;
	}

	private static float GetLifetime(GameObject instance)
	{
		float num = 0.5f;
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystem.MainModule main = componentsInChildren[i].main;
			float num2 = Mathf.Max(main.startLifetime.constantMin, main.startLifetime.constantMax);
			float num3 = main.startDelay.constant + main.duration + num2;
			if (main.simulationSpeed > 0f)
			{
				num3 /= main.simulationSpeed;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		return num + 0.1f;
	}
}
