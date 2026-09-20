using UnityEngine;

public class ColliderSizeFixer : MonoBehaviour
{
	private BoxCollider2D _collider;

	private Transform _tf;

	private bool _fixed;

	private void Start()
	{
		_fixed = CheckIfFixed();
	}

	private void OnEnable()
	{
		_fixed = CheckIfFixed();
	}

	private void Update()
	{
		if (_fixed)
		{
			return;
		}
		Init();
		if (_collider.enabled)
		{
			_fixed = CheckIfFixed();
			if (!_fixed)
			{
				_collider.size = new Vector2(_collider.size.x + 2f, _collider.size.y + 2f);
			}
		}
	}

	private bool CheckIfFixed()
	{
		Init();
		Collider2D[] array = Physics2D.OverlapPointAll(_tf.position, 8192);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetInstanceID() == _collider.GetInstanceID())
			{
				return true;
			}
		}
		return false;
	}

	private void Init()
	{
		if (!(_collider != null))
		{
			_collider = GetComponent<BoxCollider2D>();
			_tf = base.transform;
		}
	}
}
