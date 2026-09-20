using UnityEngine;

namespace DarkTonic.MasterAudio;

[AddComponentMenu("Dark Tonic/Master Audio/Ambient Sound")]
[AudioScriptOrder(-20)]
public class AmbientSound : MonoBehaviour
{
	[SoundGroup]
	public string AmbientSoundGroup = "[None]";

	[Tooltip("This option is useful if your caller ever moves, as it will make the Audio Source follow to the location of the caller every frame.")]
	public bool FollowCaller;

	[Tooltip("Using this option, the Audio Source will be updated every frame to the closest position on the caller's collider, if any. This will override the Follow Caller option above and happen instead.")]
	public bool UseClosestColliderPosition;

	public bool UseTopCollider = true;

	public bool IncludeChildColliders;

	[Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
	public Transform RuntimeFollower;

	private Transform _trans;

	public bool IsValidSoundGroup => !MasterAudio.SoundGroupHardCodedNames.Contains(AmbientSoundGroup);

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

	protected void OnEnable()
	{
		StartTrackers();
	}

	private void OnDisable()
	{
		if (!MasterAudio.AppIsShuttingDown && IsValidSoundGroup && !(MasterAudio.SafeInstance == null))
		{
			MasterAudio.StopSoundGroupOfTransform(Trans, AmbientSoundGroup);
			RuntimeFollower = null;
		}
	}

	private void StartTrackers()
	{
		if (IsValidSoundGroup && AmbientUtil.InitListenerFollower())
		{
			if (!AmbientUtil.HasListenerFolowerRigidBody)
			{
				MasterAudio.LogWarning("Your Ambient Sound script on Game Object '" + base.name + "' will not function because you have turned off the Listener Follower RigidBody in Advanced Settings.");
			}
			string followerName = base.name + "_" + AmbientSoundGroup + "_" + Random.Range(0, 9) + "_Follower";
			RuntimeFollower = AmbientUtil.InitAudioSourceFollower(Trans, followerName, AmbientSoundGroup, FollowCaller, UseClosestColliderPosition, UseTopCollider, IncludeChildColliders);
		}
	}
}
