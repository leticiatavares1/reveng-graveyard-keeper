using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-20)]
[ExecuteAlways]
public class SystemsContainer : MonoBehaviour
{
	private void Awake()
	{
		TryDestroyManagerIfNotOnMainScene(this);
	}

	private static bool TryDestroyManagerIfNotOnMainScene(MonoBehaviour component)
	{
		if (Application.isPlaying)
		{
			for (int i = 0; i < SceneManager.sceneCount && !(SceneManager.GetSceneAt(i).name == "MainScene"); i++)
			{
			}
			if (component.gameObject.scene.name != "MainScene")
			{
				Object.Destroy(component.gameObject);
				return true;
			}
		}
		return false;
	}
}
