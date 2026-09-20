using UnityEngine;

public class DynamicLight : MonoBehaviour
{
	public GameObject light_zero_point;

	private bool _tf_set;

	private Transform _tf;

	public Vector3 pos = Vector3.zero;

	public bool active_in_hierarchy = true;

	[Range(0f, 2f)]
	public float intensity_k = 1f;

	private int _parent_go = -1;

	private bool _parent_go_set;

	public DynamicSpritePreset intensity_preset;

	public void OnEnable()
	{
		DynamicLights.dyn_lights.Add(this);
		CustomUpdate();
	}

	public void OnDisable()
	{
		DynamicLights.dyn_lights.Remove(this);
		active_in_hierarchy = false;
	}

	public void CustomUpdate()
	{
		if (!_tf_set)
		{
			_tf = ((light_zero_point == null) ? base.transform : light_zero_point.transform);
			_tf_set = true;
		}
		pos = _tf.position;
		active_in_hierarchy = base.gameObject.activeInHierarchy;
	}

	public bool DoesLightBelongsToTheSameObjectAsShadow(ObjectDynamicShadow shadow)
	{
		if (!_parent_go_set)
		{
			_parent_go_set = true;
			WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
			if (componentInParent != null)
			{
				_parent_go = componentInParent.gameObject.GetInstanceID();
			}
			else
			{
				WorldSimpleObject componentInParent2 = GetComponentInParent<WorldSimpleObject>();
				if (componentInParent2 != null)
				{
					_parent_go = componentInParent2.gameObject.GetInstanceID();
				}
			}
		}
		return _parent_go == shadow.ParentGoInstanceIDInstanceID;
	}
}
