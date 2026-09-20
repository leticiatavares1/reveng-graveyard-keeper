using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBounds : MonoBehaviour
{
	private bool _initialized;

	[Range(0f, 2f)]
	public float offset = 1f;

	[Range(0f, 2f)]
	public float transition_time = 1f;

	public EaseType ease_type = EaseType.EaseOut;

	private void Awake()
	{
		BoxCollider2D component = GetComponent<BoxCollider2D>();
		component.offset = component.offset.Round(0.1f);
		component.size = component.size.Round(0.1f);
	}

	private void Update()
	{
		if (!_initialized && GameLoader.camera_initialized)
		{
			BoxCollider2D component = GetComponent<BoxCollider2D>();
			Transform obj = new GameObject(base.name).transform;
			obj.SetParent(base.transform, worldPositionStays: false);
			Vector3 center = component.bounds.center;
			center.z = 0f;
			obj.position = center;
			obj.localScale = component.bounds.size;
			ProCamera2DTriggerBoundaries proCamera2DTriggerBoundaries = obj.gameObject.AddComponent<ProCamera2DTriggerBoundaries>();
			proCamera2DTriggerBoundaries.UseTopBoundary = (proCamera2DTriggerBoundaries.UseBottomBoundary = (proCamera2DTriggerBoundaries.UseLeftBoundary = (proCamera2DTriggerBoundaries.UseRightBoundary = true)));
			float num = offset * 96f;
			proCamera2DTriggerBoundaries.TopBoundary = component.bounds.max.y + num;
			proCamera2DTriggerBoundaries.BottomBoundary = component.bounds.min.y - num;
			proCamera2DTriggerBoundaries.LeftBoundary = component.bounds.min.x - num;
			proCamera2DTriggerBoundaries.RightBoundary = component.bounds.max.x + num;
			proCamera2DTriggerBoundaries.TriggerShape = TriggerShape.RECTANGLE;
			proCamera2DTriggerBoundaries.AreBoundariesRelative = false;
			proCamera2DTriggerBoundaries.TransitionEaseType = ease_type;
			proCamera2DTriggerBoundaries.TransitionDuration = transition_time;
			_initialized = true;
		}
	}
}
