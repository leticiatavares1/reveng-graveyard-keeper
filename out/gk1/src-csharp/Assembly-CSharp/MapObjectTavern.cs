using UnityEngine;

public class MapObjectTavern : MonoBehaviour
{
	[SerializeField]
	private GameObject _content;

	private const string _PLAYERS_TAVERN_CUSTOM_TAG = "players_tavern";

	private void Awake()
	{
		base.gameObject.SetActive(value: true);
		_content.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (CheckIsTavernBuild())
		{
			_content.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		_content.SetActive(value: false);
	}

	private bool CheckIsTavernBuild()
	{
		if (!(WorldMap.GetWorldGameObjectByCustomTag("players_tavern", ignore_not_found_error: true) != null))
		{
			return false;
		}
		return true;
	}
}
