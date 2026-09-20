using UnityEngine;

public class SceneWaypointContent : MonoBehaviour, IGameSceneContent
{
	[SerializeField]
	private string worldId;

	public void SetId(string id)
	{
		worldId = id;
	}

	public string GetId()
	{
		return worldId;
	}

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
