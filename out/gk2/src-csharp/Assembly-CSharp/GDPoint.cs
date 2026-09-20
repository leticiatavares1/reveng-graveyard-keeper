using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GDPoint : MonoBehaviour
{
	[SerializeField]
	private string id;

	[SerializeField]
	private string customTag;

	[SerializeField]
	private Direction direction = Direction.Down;

	[SerializeReference]
	private List<GDPoint> nextGdPoints = new List<GDPoint>();

	[SerializeField]
	private bool isTransitPoint;

	[SerializeField]
	private string transitToGdPointId;

	[SerializeField]
	private string worldIdToTransit;

	private GDPointData gdPointData;

	public string Id => id;

	public string CustomTag => customTag;

	public Direction Direction => direction;

	public bool IsTransitPoint => isTransitPoint;

	public List<GDPoint> NextGdPoints => nextGdPoints;

	public string TransitToGdPointId => transitToGdPointId;

	public string WorldIdToTransit => worldIdToTransit;

	public void Init(GDPointData gdPointData)
	{
		this.gdPointData = gdPointData;
		this.gdPointData.OnActiveStateChanged += OnActiveStateChanged;
		OnActiveStateChanged(gdPointData.Enabled);
	}

	public void OnActiveStateChanged(bool state)
	{
		if (this == null)
		{
			Debug.LogWarning("GDPoint.OnActiveStateChanged called but 'this' is null");
			return;
		}
		base.gameObject.SetActive(state);
		if (state && !base.gameObject.activeInHierarchy)
		{
			Debug.LogWarning("GDPoint [" + id + "] SetActive(true) called but activeInHierarchy is still false. Parent object is inactive: " + base.transform.parent?.name);
		}
	}

	public void OnDestroy()
	{
		if (gdPointData != null && Application.isPlaying)
		{
			gdPointData.OnActiveStateChanged -= OnActiveStateChanged;
		}
	}
}
