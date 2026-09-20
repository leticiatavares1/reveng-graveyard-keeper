using System;
using FlowCanvas;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GDZone : MonoBehaviour
{
	public enum DistanceType
	{
		None,
		Point,
		Line
	}

	[Serializable]
	public class GDZoneEvent
	{
		public FlowScript flowScript;
	}

	public const float COLLIDER_MIN_SIZE = 100f;

	public string customTag;

	public GDZoneEvent onEnter;

	public GDZoneEvent onExit;

	public GDZoneEvent onCrossedToRight;

	public GDZoneEvent onCrossedToLeft;

	public GDZoneEvent onCrossedToUp;

	public GDZoneEvent onCrossedToDown;

	private Vector3 enterPoint;

	private bool isPlayerInside;

	public DistanceType distanceCounter;

	public Vector2 distP1;

	public Vector2 distP2;

	public float distSize = 1f;

	public float distFadeSize = 2f;

	[SerializeField]
	private Vector3 p1;

	[SerializeField]
	private Vector3 p2;

	[SerializeField]
	private Vector3[] innerPoly;

	[SerializeField]
	private Vector3 perpendicular;

	[SerializeField]
	private Vector3 lineDirection;

	private void OnTriggerEnter(Collider collision)
	{
		IPhysicallyMutable componentInChildren = collision.gameObject.GetComponentInChildren<IPhysicallyMutable>();
		if (componentInChildren != null && !componentInChildren.IsMuted && collision.transform.gameObject.layer == 10 && !isPlayerInside)
		{
			isPlayerInside = true;
			enterPoint = collision.transform.position;
			Debug.Log("GDZone.OnTriggerEnter " + base.name, this);
			ExecuteEvent(onEnter);
			if (!string.IsNullOrEmpty(customTag))
			{
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerEnterGDZone, customTag);
			}
		}
	}

	private void OnTriggerExit(Collider collision)
	{
		if (collision.transform.gameObject.layer != 10 || !isPlayerInside)
		{
			return;
		}
		IPhysicallyMutable componentInChildren = collision.gameObject.GetComponentInChildren<IPhysicallyMutable>();
		if (componentInChildren == null || componentInChildren.IsMuted)
		{
			return;
		}
		RaycastHit[] array = new RaycastHit[5];
		int num = Physics.RaycastNonAlloc(new Ray(collision.transform.position + Vector3.down * 100f, Vector3.up), array, 200f, 8388608);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.collider.TryGetComponent<GDZone>(out var component) && component == this)
			{
				return;
			}
		}
		isPlayerInside = false;
		Vector3 vector = collision.transform.position - enterPoint;
		Debug.Log("GDZone.OnTriggerExit " + base.name + ", diff_vector = " + vector, this);
		ExecuteEvent(onExit);
		if (vector.x * 2f > 100f)
		{
			ExecuteEvent(onCrossedToRight);
		}
		if (vector.x * 2f < -100f)
		{
			ExecuteEvent(onCrossedToLeft);
		}
		if (vector.z * 2f > 100f)
		{
			ExecuteEvent(onCrossedToDown);
		}
		if (vector.z * 2f < -100f)
		{
			ExecuteEvent(onCrossedToUp);
		}
		if (!string.IsNullOrEmpty(customTag))
		{
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerExitGDZone, customTag);
		}
	}

	private void ExecuteEvent(GDZoneEvent e)
	{
		if (e != null && !(e.flowScript == null) && !GlobalScriptsManager.HasFlowScript(e.flowScript.name) && e.flowScript != null)
		{
			GlobalScriptsManager.RunFlowScript(e.flowScript, null);
		}
	}

	public void RecalculateBounds()
	{
		p1 = base.transform.position + (Vector3)distP1 * 48f;
		p2 = base.transform.position + (Vector3)distP2 * 48f;
		lineDirection = (p2 - p1).normalized;
		perpendicular = Quaternion.AngleAxis(90f, lineDirection) * Vector3.forward;
		innerPoly = new Vector3[4]
		{
			p1 + perpendicular * distSize * 48f,
			p2 + perpendicular * distSize * 48f,
			p2 - perpendicular * distSize * 48f,
			p1 - perpendicular * distSize * 48f
		};
	}
}
