using UnityEngine;

namespace DarkTonic.MasterAudio;

public class MechanimStateSounds : StateMachineBehaviour
{
	[Header("Select For Sounds To Follow Object")]
	public bool SoundFollowsObject;

	[Tooltip("Select for sounds to retrigger each time animation loops without exiting state")]
	[Header("Retrigger Sounds Each Time Anim Loops w/o Exiting State")]
	public bool RetriggerWhenStateLoops;

	[Tooltip("Play a Sound Group when state is Entered")]
	[Header("Enter Sound Group")]
	public bool playEnterSound;

	public bool stopEnterSoundOnExit;

	[SoundGroup]
	public string enterSoundGroup = "[None]";

	[Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
	public string enterVariationName;

	private bool wasEnterSoundPlayed;

	[Tooltip("Play a Sound Group when state is Exited")]
	[Header("Exit Sound Group")]
	public bool playExitSound;

	[SoundGroup]
	public string exitSoundGroup = "[None]";

	[Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
	public string exitVariationName;

	[Tooltip("Play a Sound Group (Normal or Looped Chain Variation Mode) timed to the animation state's normalized time.  Normalized time is simply the length in time of the animation.  Time is represented as a float from 0f - 1f.  0f is the beginning, .5f is the middle, 1f is the end...etc.etc.  Select a Start time from 0 - 1.  Select a stop time greater than the start time or leave stop time equals to zero and select Stop Anim Time Sound On Exit.  This can be used for Looped Chain Sound Groups since you have to define a stop time.")]
	[Header("Play Sound Group Timed to Animation")]
	public bool playAnimTimeSound;

	public bool stopAnimTimeSoundOnExit;

	[Tooltip("If selected, When To Stop Sound (below) will be used. Otherwise the sound will not be stopped unless you have Stop Anim Time Sound On Exit selected above.")]
	public bool useStopTime;

	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	[Range(0f, 1f)]
	public float whenToStartSound;

	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	[Range(0f, 1f)]
	public float whenToStopSound;

	[SoundGroup]
	public string TimedSoundGroup = "[None]";

	[Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
	public string timedVariationName;

	private bool playSoundStart = true;

	private bool playSoundStop = true;

	[Header("Play Multiple Sounds Timed to Anim")]
	[Tooltip("Play a Sound Group with each variation timed to the animation.  This allows you to time your sounds to the actions in you animation.  Example: A sword swing combo where you want swoosh soundsor character grunts timed to the acceleration phase of the sword swing.  Select the number of sounds to be played, up to 4.  Then set the time you want each sound to start with each subsequent time greater than the previous time.")]
	public bool playMultiAnimTimeSounds;

	public bool StopMultiAnimTimeSoundsOnExit;

	[Range(0f, 4f)]
	public int numOfMultiSoundsToPlay;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToStartMultiSound1;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToStartMultiSound2;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToStartMultiSound3;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToStartMultiSound4;

	[SoundGroup]
	public string MultiSoundsTimedGroup = "[None]";

	[Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
	public string multiTimedVariationName;

	private bool playMultiSound1 = true;

	private bool playMultiSound2 = true;

	private bool playMultiSound3 = true;

	private bool playMultiSound4 = true;

	private Transform _actorTrans;

	private int _lastRepetition = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_lastRepetition = 0;
		_actorTrans = ActorTrans(animator);
		if (!playEnterSound)
		{
			return;
		}
		string variationName = GetVariationName(enterVariationName);
		if (SoundFollowsObject)
		{
			if (variationName == null)
			{
				MasterAudio.PlaySound3DFollowTransformAndForget(enterSoundGroup, _actorTrans);
			}
			else
			{
				MasterAudio.PlaySound3DFollowTransformAndForget(enterSoundGroup, _actorTrans, 1f, null, 0f, variationName);
			}
		}
		else if (variationName == null)
		{
			MasterAudio.PlaySound3DAtTransformAndForget(enterSoundGroup, _actorTrans);
		}
		else
		{
			MasterAudio.PlaySound3DAtTransformAndForget(enterSoundGroup, _actorTrans, 1f, null, 0f, variationName);
		}
		wasEnterSoundPlayed = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int num = (int)stateInfo.normalizedTime;
		float num2 = stateInfo.normalizedTime - (float)num;
		if (playAnimTimeSound)
		{
			if (!playSoundStart && RetriggerWhenStateLoops && _lastRepetition >= 0 && num > _lastRepetition)
			{
				playSoundStart = true;
			}
			if (playSoundStart && num2 > whenToStartSound)
			{
				playSoundStart = false;
				if (useStopTime && whenToStopSound < whenToStartSound)
				{
					Debug.LogError("Stop time must be greater than start time when Use Stop Time is selected.");
				}
				else
				{
					string variationName = GetVariationName(timedVariationName);
					if (SoundFollowsObject)
					{
						if (variationName == null)
						{
							MasterAudio.PlaySound3DFollowTransformAndForget(TimedSoundGroup, _actorTrans);
						}
						else
						{
							MasterAudio.PlaySound3DFollowTransformAndForget(TimedSoundGroup, _actorTrans, 1f, null, 0f, variationName);
						}
					}
					else if (variationName == null)
					{
						MasterAudio.PlaySound3DAtTransformAndForget(TimedSoundGroup, _actorTrans);
					}
					else
					{
						MasterAudio.PlaySound3DAtTransformAndForget(TimedSoundGroup, _actorTrans, 1f, null, 0f, variationName);
					}
				}
			}
			if (useStopTime && playSoundStop && num2 > whenToStartSound && !stopAnimTimeSoundOnExit && num2 > whenToStopSound)
			{
				playSoundStop = false;
				MasterAudio.StopSoundGroupOfTransform(_actorTrans, TimedSoundGroup);
			}
		}
		if (playMultiAnimTimeSounds)
		{
			if (RetriggerWhenStateLoops)
			{
				if (!playMultiSound1 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					playMultiSound1 = true;
				}
				if (!playMultiSound2 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					playMultiSound2 = true;
				}
				if (!playMultiSound3 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					playMultiSound3 = true;
				}
				if (!playMultiSound4 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					playMultiSound4 = true;
				}
			}
			string variationName2 = GetVariationName(multiTimedVariationName);
			if (playMultiSound1 && num2 > whenToStartMultiSound1 && numOfMultiSoundsToPlay >= 1)
			{
				playMultiSound1 = false;
				if (SoundFollowsObject)
				{
					if (variationName2 == null)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
					}
					else
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
					}
				}
				else if (variationName2 == null)
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
				}
				else
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
				}
			}
			if (playMultiSound2 && num2 > whenToStartMultiSound2 && numOfMultiSoundsToPlay >= 2)
			{
				playMultiSound2 = false;
				if (SoundFollowsObject)
				{
					if (variationName2 == null)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
					}
					else
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
					}
				}
				else if (variationName2 == null)
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
				}
				else
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
				}
			}
			if (playMultiSound3 && num2 > whenToStartMultiSound3 && numOfMultiSoundsToPlay >= 3)
			{
				playMultiSound3 = false;
				if (SoundFollowsObject)
				{
					if (variationName2 == null)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
					}
					else
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
					}
				}
				else if (variationName2 == null)
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
				}
				else
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
				}
			}
			if (playMultiSound4 && num2 > whenToStartMultiSound4 && numOfMultiSoundsToPlay >= 4)
			{
				playMultiSound4 = false;
				if (SoundFollowsObject)
				{
					if (variationName2 == null)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
					}
					else
					{
						MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
					}
				}
				else if (variationName2 == null)
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
				}
				else
				{
					MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, variationName2);
				}
			}
		}
		_lastRepetition = num;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (wasEnterSoundPlayed && stopEnterSoundOnExit)
		{
			MasterAudio.StopSoundGroupOfTransform(_actorTrans, enterSoundGroup);
		}
		wasEnterSoundPlayed = false;
		if (playExitSound)
		{
			string variationName = GetVariationName(exitVariationName);
			if (SoundFollowsObject)
			{
				if (variationName == null)
				{
					MasterAudio.PlaySound3DFollowTransformAndForget(exitSoundGroup, _actorTrans);
				}
				else
				{
					MasterAudio.PlaySound3DFollowTransformAndForget(exitSoundGroup, _actorTrans, 1f, null, 0f, variationName);
				}
			}
			else if (variationName == null)
			{
				MasterAudio.PlaySound3DAtTransformAndForget(exitSoundGroup, _actorTrans);
			}
			else
			{
				MasterAudio.PlaySound3DAtTransformAndForget(exitSoundGroup, _actorTrans, 1f, null, 0f, variationName);
			}
		}
		if (playAnimTimeSound)
		{
			if (stopAnimTimeSoundOnExit)
			{
				MasterAudio.StopSoundGroupOfTransform(_actorTrans, TimedSoundGroup);
			}
			playSoundStart = true;
			playSoundStop = true;
		}
		if (playMultiAnimTimeSounds)
		{
			if (StopMultiAnimTimeSoundsOnExit)
			{
				MasterAudio.StopSoundGroupOfTransform(_actorTrans, MultiSoundsTimedGroup);
			}
			playMultiSound1 = true;
			playMultiSound2 = true;
			playMultiSound3 = true;
			playMultiSound4 = true;
		}
	}

	private Transform ActorTrans(Animator anim)
	{
		if (_actorTrans != null)
		{
			return _actorTrans;
		}
		_actorTrans = anim.transform;
		return _actorTrans;
	}

	private static string GetVariationName(string varName)
	{
		if (string.IsNullOrEmpty(varName))
		{
			return null;
		}
		varName = varName.Trim();
		if (string.IsNullOrEmpty(varName))
		{
			return null;
		}
		return varName;
	}
}
