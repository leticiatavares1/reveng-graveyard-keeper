using UnityEngine;

public abstract class CustomNavMeshCutBakeSourceBase : MonoBehaviour, ICustomNavMeshCut
{
	[SerializeField]
	private bool bakable;

	public bool Bakable => bakable;

	public Transform BakeTransform => base.transform;

	protected virtual void Awake()
	{
		if (bakable && Application.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public virtual void OnCustomNavMeshCutSpawn(WgoData wgoData, WgoPartData wgoPartData)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
