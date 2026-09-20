using System;
using UnityEngine;

public class PuffFX : MonoBehaviour
{
	[Serializable]
	public enum PuffSize
	{
		Small,
		Large
	}

	private static PuffFX _prefab;

	public GameObject small;

	public GameObject large;

	[NonSerialized]
	private WorldGameObject _linked_wgo;

	public static PuffFX Create(WorldGameObject wgo, Bounds? wgo_bounds = null)
	{
		try
		{
			if (wgo == null || wgo.gameObject == null)
			{
				Debug.LogError("PuffFX.Create error: WGO is null");
				return null;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("PuffFX.Create error: exception: " + ex);
			return null;
		}
		if (_prefab == null)
		{
			_prefab = Resources.Load<PuffFX>("puff fx");
		}
		PuffFX puffFX = _prefab.Copy();
		puffFX._linked_wgo = wgo;
		puffFX.transform.SetParent(wgo.gameObject.transform, worldPositionStays: false);
		bool flag = false;
		PuffPoint componentInChildren = wgo.GetComponentInChildren<PuffPoint>(includeInactive: true);
		if (componentInChildren != null)
		{
			puffFX.transform.SetParent(componentInChildren.transform, worldPositionStays: false);
			puffFX.transform.localPosition = Vector3.zero;
			flag = componentInChildren.size == PuffSize.Small;
		}
		else
		{
			if (!wgo_bounds.HasValue)
			{
				wgo_bounds = wgo.GetTotalBounds();
			}
			Vector3 center = wgo_bounds.Value.center;
			puffFX.transform.position = center;
			flag = (new Vector2(Mathf.Round(wgo_bounds.Value.size.x), Mathf.Round(wgo_bounds.Value.size.y)) / 96f).magnitude <= 1f;
		}
		puffFX.small.gameObject.SetActive(flag);
		puffFX.large.gameObject.SetActive(!flag);
		puffFX.transform.localPosition = new Vector3(puffFX.transform.localPosition.x, puffFX.transform.localPosition.y, 0f);
		puffFX.transform.SetParent(MainGame.me.world_root);
		return puffFX;
	}

	public void Start()
	{
		GJTimer.AddTimer(2f, delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}
}
