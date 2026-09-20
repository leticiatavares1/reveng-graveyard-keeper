using DarkTonic.MasterAudio;
using UnityEngine;

[AudioScriptOrder(-10)]
public class ListenerFollower : MonoBehaviour
{
	private Transform _transToFollow;

	private GameObject _goToFollow;

	private Transform _trans;

	private GameObject _go;

	private SphereCollider _collider;

	public SphereCollider Trigger
	{
		get
		{
			if (_collider != null)
			{
				return _collider;
			}
			_collider = GameObj.AddComponent<SphereCollider>();
			_collider.isTrigger = true;
			return _collider;
		}
	}

	public GameObject GameObj
	{
		get
		{
			if (_go != null)
			{
				return _go;
			}
			_go = base.gameObject;
			return _go;
		}
	}

	public Transform Trans
	{
		get
		{
			if (_trans == null)
			{
				_trans = base.transform;
			}
			return _trans;
		}
	}

	private void Awake()
	{
		_ = Trigger == null;
	}

	public void StartFollowing(Transform transToFollow, string soundType, float trigRadius)
	{
		_transToFollow = transToFollow;
		_goToFollow = transToFollow.gameObject;
		Trigger.radius = trigRadius;
	}

	private void LateUpdate()
	{
		BatchOcclusionRaycasts();
		if (!(_transToFollow == null) && DTMonoHelper.IsActive(_goToFollow))
		{
			Trans.position = _transToFollow.position;
		}
	}

	private void BatchOcclusionRaycasts()
	{
		if (!MasterAudio.Instance.useOcclusion)
		{
			return;
		}
		int num = 0;
		while (num < MasterAudio.Instance.occlusionMaxRayCastsPerFrame && MasterAudio.HasQueuedOcclusionRays())
		{
			SoundGroupVariationUpdater soundGroupVariationUpdater = MasterAudio.OldestQueuedOcclusionRay();
			if (!(soundGroupVariationUpdater == null) && soundGroupVariationUpdater.enabled && soundGroupVariationUpdater.RayCastForOcclusion())
			{
				num++;
			}
		}
	}
}
