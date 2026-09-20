using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class TransformFollower : MonoBehaviour
{
	[Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
	public Transform RuntimeFollowingTransform;

	private GameObject _goToFollow;

	private Transform _trans;

	private GameObject _go;

	private SphereCollider _collider;

	private string _soundType;

	private bool _willFollowSource;

	private bool _isInsideTrigger;

	private bool _hasPlayedSound;

	private bool _groupLoadFailed;

	private MasterAudioGroup _groupToPlay;

	private bool _positionAtClosestColliderPoint;

	private readonly List<Collider> _actorColliders = new List<Collider>();

	private readonly List<Collider2D> _actorColliders2D = new List<Collider2D>();

	private Vector3 _lastListenerPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	private readonly Dictionary<Collider, Vector3> _lastPositionByCollider = new Dictionary<Collider, Vector3>();

	private readonly Dictionary<Collider2D, Vector3> _lastPositionByCollider2D = new Dictionary<Collider2D, Vector3>();

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
		if (!(Trigger == null) && _actorColliders.Count != 0 && _actorColliders2D.Count != 0 && !_positionAtClosestColliderPoint)
		{
			_ = _lastListenerPos == Vector3.zero;
		}
	}

	private void Start()
	{
		_groupToPlay = MasterAudio.GrabGroup(_soundType, logIfMissing: false);
	}

	public void StartFollowing(Transform transToFollow, string soundType, float trigRadius, bool willFollowSource, bool positionAtClosestColliderPoint, bool useTopCollider, bool useChildColliders)
	{
		RuntimeFollowingTransform = transToFollow;
		_goToFollow = transToFollow.gameObject;
		Trigger.radius = trigRadius;
		_soundType = soundType;
		_willFollowSource = willFollowSource;
		_lastPositionByCollider.Clear();
		_lastPositionByCollider2D.Clear();
		if (useTopCollider)
		{
			Collider component = transToFollow.GetComponent<Collider>();
			if (component != null)
			{
				_actorColliders.Add(component);
				_lastPositionByCollider.Add(component, transToFollow.position);
			}
			else
			{
				Collider2D component2 = transToFollow.GetComponent<Collider2D>();
				if (component2 != null)
				{
					_actorColliders2D.Add(component2);
					_lastPositionByCollider2D.Add(component2, transToFollow.position);
				}
			}
		}
		if (useChildColliders)
		{
			for (int i = 0; i < Trans.childCount; i++)
			{
				Transform child = Trans.GetChild(i);
				Collider component3 = child.GetComponent<Collider>();
				if (component3 != null)
				{
					_actorColliders.Add(component3);
					_lastPositionByCollider.Add(component3, transToFollow.position);
					continue;
				}
				Collider2D component4 = child.GetComponent<Collider2D>();
				if (component4 != null)
				{
					_actorColliders2D.Add(component4);
					_lastPositionByCollider2D.Add(component4, transToFollow.position);
				}
			}
		}
		_lastListenerPos = MasterAudio.ListenerTrans.position;
		if (_actorColliders.Count == 0 && _actorColliders2D.Count == 0 && positionAtClosestColliderPoint)
		{
			Debug.Log("Can't follow collider of '" + transToFollow.name + "' because it doesn't have any colliders.");
			return;
		}
		_positionAtClosestColliderPoint = positionAtClosestColliderPoint;
		if (_positionAtClosestColliderPoint)
		{
			MasterAudio.QueueTransformFollowerForColliderPositionRecalc(this);
		}
	}

	private void StopFollowing()
	{
		RuntimeFollowingTransform = null;
		Object.Destroy(GameObj);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (RuntimeFollowingTransform == null || other == null || base.name == "~ListenerFollower~" || other.name != "~ListenerFollower~")
		{
			return;
		}
		_isInsideTrigger = true;
		if (_groupToPlay != null)
		{
			switch (_groupToPlay.GroupLoadStatus)
			{
			case MasterAudio.InternetFileLoadStatus.Failed:
				if (MasterAudio.LogSoundsEnabled)
				{
					MasterAudio.LogWarning("TransformFollower: '" + base.name + "' not attempting to play Sound Group '" + _soundType + "' because the Sound Group failed to load.");
				}
				_groupLoadFailed = true;
				return;
			case MasterAudio.InternetFileLoadStatus.Loading:
				return;
			}
		}
		PlaySound();
	}

	private void PlaySound()
	{
		if (_willFollowSource)
		{
			MasterAudio.PlaySound3DFollowTransformAndForget(_soundType, RuntimeFollowingTransform);
		}
		else
		{
			MasterAudio.PlaySound3DAtTransformAndForget(_soundType, RuntimeFollowingTransform);
		}
		_hasPlayedSound = true;
	}

	private void LateUpdate()
	{
		if (RuntimeFollowingTransform == null || !DTMonoHelper.IsActive(_goToFollow))
		{
			StopFollowing();
			return;
		}
		if (!_positionAtClosestColliderPoint)
		{
			Trans.position = RuntimeFollowingTransform.position;
		}
		if (!_isInsideTrigger || _hasPlayedSound || _groupLoadFailed)
		{
			return;
		}
		switch (_groupToPlay.GroupLoadStatus)
		{
		case MasterAudio.InternetFileLoadStatus.Loaded:
			PlaySound();
			break;
		case MasterAudio.InternetFileLoadStatus.Failed:
			if (MasterAudio.LogSoundsEnabled)
			{
				MasterAudio.LogWarning("TransformFollower: '" + base.name + "' not attempting to play Sound Group '" + _soundType + "' because the Sound Group failed to load.");
			}
			_groupLoadFailed = true;
			break;
		case MasterAudio.InternetFileLoadStatus.Loading:
			break;
		}
	}

	public bool RecalcClosestColliderPosition()
	{
		Vector3 position = MasterAudio.ListenerTrans.position;
		bool flag = _lastListenerPos != position;
		Vector3 position2 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		float num = float.MaxValue;
		bool flag2 = false;
		if (_actorColliders.Count > 0)
		{
			if (_actorColliders.Count == 1)
			{
				Collider collider = _actorColliders[0];
				Vector3 position3 = collider.transform.position;
				if (_lastPositionByCollider[collider] == position3 && !flag)
				{
					return false;
				}
				flag2 = true;
				position2 = collider.ClosestPoint(position);
				_lastPositionByCollider[collider] = position3;
			}
			else
			{
				for (int i = 0; i < _actorColliders.Count; i++)
				{
					Collider collider2 = _actorColliders[i];
					Vector3 position4 = collider2.transform.position;
					if (!(_lastPositionByCollider[collider2] == position4) || flag)
					{
						flag2 = true;
						Vector3 vector = collider2.ClosestPoint(position);
						float sqrMagnitude = (position - vector).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							position2 = vector;
							num = sqrMagnitude;
						}
						_lastPositionByCollider[collider2] = position4;
					}
				}
			}
		}
		else
		{
			if (_actorColliders2D.Count <= 0)
			{
				return false;
			}
			if (_actorColliders2D.Count == 1)
			{
				Collider2D collider2D = _actorColliders2D[0];
				Vector3 position5 = collider2D.transform.position;
				if (_lastPositionByCollider2D[collider2D] == position5 && !flag)
				{
					return false;
				}
				flag2 = true;
				position2 = collider2D.bounds.ClosestPoint(position);
				_lastPositionByCollider2D[collider2D] = position5;
			}
			else
			{
				for (int j = 0; j < _actorColliders2D.Count; j++)
				{
					Collider2D collider2D2 = _actorColliders2D[j];
					Vector3 position6 = collider2D2.transform.position;
					if (!(_lastPositionByCollider2D[collider2D2] == position6) || flag)
					{
						flag2 = true;
						Vector3 vector2 = collider2D2.bounds.ClosestPoint(position);
						float sqrMagnitude2 = (position - vector2).sqrMagnitude;
						if (sqrMagnitude2 < num)
						{
							position2 = vector2;
							num = sqrMagnitude2;
						}
						_lastPositionByCollider2D[collider2D2] = position6;
					}
				}
			}
		}
		if (!flag2)
		{
			return false;
		}
		Trans.position = position2;
		Trans.LookAt(MasterAudio.ListenerTrans);
		_lastListenerPos = position;
		return true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!(RuntimeFollowingTransform == null) && !(other == null) && !(other.name != "~ListenerFollower~"))
		{
			_isInsideTrigger = false;
			_hasPlayedSound = false;
			MasterAudio.StopSoundGroupOfTransform(RuntimeFollowingTransform, _soundType);
		}
	}
}
