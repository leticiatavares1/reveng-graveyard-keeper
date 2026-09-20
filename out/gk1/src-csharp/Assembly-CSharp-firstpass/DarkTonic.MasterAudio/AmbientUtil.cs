using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class AmbientUtil
{
	public const string FollowerHolderName = "_Followers";

	public const string ListenerFollowerName = "~ListenerFollower~";

	public const float ListenerFollowerTrigRadius = 0.01f;

	private static Transform _followerHolder;

	private static ListenerFollower _listenerFollower;

	private static Rigidbody _listenerFollowerRB;

	public static ListenerFollower ListenerFollower
	{
		get
		{
			if (_listenerFollower != null)
			{
				return _listenerFollower;
			}
			if (FollowerHolder == null)
			{
				return null;
			}
			Transform transform = FollowerHolder.GetChildTransform("~ListenerFollower~");
			if (transform == null)
			{
				transform = new GameObject("~ListenerFollower~").transform;
				transform.parent = FollowerHolder;
				transform.gameObject.layer = FollowerHolder.gameObject.layer;
			}
			_listenerFollower = transform.GetComponent<ListenerFollower>();
			if (_listenerFollower == null)
			{
				_listenerFollower = transform.gameObject.AddComponent<ListenerFollower>();
			}
			if (MasterAudio.Instance.listenerFollowerHasRigidBody)
			{
				Rigidbody rigidbody = transform.gameObject.GetComponent<Rigidbody>();
				if (rigidbody == null)
				{
					rigidbody = transform.gameObject.AddComponent<Rigidbody>();
				}
				rigidbody.useGravity = false;
				_listenerFollowerRB = rigidbody;
			}
			return _listenerFollower;
		}
	}

	public static Transform FollowerHolder
	{
		get
		{
			if (!Application.isPlaying || MasterAudio.SafeInstance == null)
			{
				return null;
			}
			if (_followerHolder != null)
			{
				return _followerHolder;
			}
			Transform trans = MasterAudio.SafeInstance.Trans;
			_followerHolder = trans.GetChildTransform("_Followers");
			if (_followerHolder != null)
			{
				return _followerHolder;
			}
			_followerHolder = new GameObject("_Followers").transform;
			_followerHolder.parent = trans;
			_followerHolder.gameObject.layer = trans.gameObject.layer;
			return _followerHolder;
		}
	}

	public static bool HasListenerFollower => _listenerFollower != null;

	public static bool HasListenerFolowerRigidBody => _listenerFollowerRB != null;

	public static void InitFollowerHolder()
	{
		Transform followerHolder = FollowerHolder;
		if (followerHolder != null)
		{
			followerHolder.DestroyAllChildren();
		}
	}

	public static bool InitListenerFollower()
	{
		Transform listenerTrans = MasterAudio.ListenerTrans;
		if (listenerTrans == null)
		{
			return false;
		}
		ListenerFollower listenerFollower = ListenerFollower;
		if (listenerFollower == null)
		{
			return false;
		}
		listenerFollower.StartFollowing(listenerTrans, "[None]", 0.01f);
		return true;
	}

	public static Transform InitAudioSourceFollower(Transform transToFollow, string followerName, string soundGroupName, bool willFollowSource, bool willPositionOnClosestColliderPoint, bool useTopCollider, bool useChildColliders)
	{
		if (ListenerFollower == null || FollowerHolder == null)
		{
			return null;
		}
		MasterAudioGroup masterAudioGroup = MasterAudio.GrabGroup(soundGroupName);
		if (masterAudioGroup == null)
		{
			return null;
		}
		if (masterAudioGroup.groupVariations.Count == 0)
		{
			return null;
		}
		float maxDistance = masterAudioGroup.groupVariations[0].VarAudio.maxDistance;
		GameObject gameObject = new GameObject(followerName);
		Transform childTransform = FollowerHolder.GetChildTransform(followerName);
		if (childTransform != null)
		{
			Object.Destroy(childTransform.gameObject);
		}
		gameObject.transform.parent = FollowerHolder;
		gameObject.gameObject.layer = FollowerHolder.gameObject.layer;
		gameObject.gameObject.AddComponent<TransformFollower>().StartFollowing(transToFollow, soundGroupName, maxDistance, willFollowSource, willPositionOnClosestColliderPoint, useTopCollider, useChildColliders);
		return gameObject.transform;
	}
}
