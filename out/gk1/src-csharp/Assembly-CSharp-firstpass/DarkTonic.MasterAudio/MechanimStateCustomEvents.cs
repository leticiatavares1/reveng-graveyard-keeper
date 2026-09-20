using UnityEngine;

namespace DarkTonic.MasterAudio;

public class MechanimStateCustomEvents : StateMachineBehaviour
{
	[Tooltip("Select for event to re-fire each time animation loops without exiting state")]
	[Header("Retrigger Events Each Time Anim Loops w/o Exiting State")]
	public bool RetriggerWhenStateLoops;

	[Tooltip("Fire A Custom Event When State Is Entered")]
	[Header("Enter Custom Event")]
	public bool fireEnterEvent;

	[MasterCustomEvent]
	public string enterCustomEvent = "[None]";

	[Tooltip("Fire a Custom Event when state is Exited")]
	[Header("Exit Custom Event")]
	public bool fireExitEvent;

	[MasterCustomEvent]
	public string exitCustomEvent = "[None]";

	[Header("Fire Custom EventTimed to Animation")]
	[Tooltip("Fire a Custom Event timed to the animation state's normalized time.  Normalized time is simply the length in time of the animation.  Time is represented as a float from 0f - 1f.  0f is the beginning, .5f is the middle, 1f is the end...etc.etc.  Select a Start time from 0 - 1.")]
	public bool fireAnimTimeEvent;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToFireEvent;

	[MasterCustomEvent]
	public string timedCustomEvent = "[None]";

	[Tooltip("Fire a Custom Event with timed to the animation.  This allows you to time your Custom Events to the actions in you animation. Select the number of Custom Events to be fired, up to 4. Then set the time you want each Custom Event to fire with each subsequent time greater than the previous time.")]
	[Header("Fire Multiple Custom Events Timed to Anim")]
	public bool fireMultiAnimTimeEvent;

	[Range(0f, 4f)]
	public int numOfMultiEventsToFire;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToFireMultiEvent1;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToFireMultiEvent2;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToFireMultiEvent3;

	[Range(0f, 1f)]
	[Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
	public float whenToFireMultiEvent4;

	[MasterCustomEvent]
	public string MultiTimedEvent = "[None]";

	private bool _playMultiEvent1 = true;

	private bool _playMultiEvent2 = true;

	private bool _playMultiEvent3 = true;

	private bool _playMultiEvent4 = true;

	private bool _fireTimedEvent = true;

	private Transform _actorTrans;

	private int _lastRepetition = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_lastRepetition = 0;
		_actorTrans = ActorTrans(animator);
		if (fireEnterEvent && !(enterCustomEvent == "[None]") && !string.IsNullOrEmpty(enterCustomEvent))
		{
			MasterAudio.FireCustomEvent(enterCustomEvent, _actorTrans);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int num = (int)stateInfo.normalizedTime;
		float num2 = stateInfo.normalizedTime - (float)num;
		if (fireAnimTimeEvent)
		{
			if (!_fireTimedEvent && RetriggerWhenStateLoops && _lastRepetition >= 0 && num > _lastRepetition)
			{
				_fireTimedEvent = true;
			}
			if (_fireTimedEvent && num2 > whenToFireEvent)
			{
				_fireTimedEvent = false;
				MasterAudio.FireCustomEvent(timedCustomEvent, _actorTrans);
			}
		}
		if (fireMultiAnimTimeEvent)
		{
			if (RetriggerWhenStateLoops)
			{
				if (!_playMultiEvent1 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					_playMultiEvent1 = true;
				}
				if (!_playMultiEvent2 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					_playMultiEvent2 = true;
				}
				if (!_playMultiEvent3 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					_playMultiEvent3 = true;
				}
				if (!_playMultiEvent4 && _lastRepetition >= 0 && num > _lastRepetition)
				{
					_playMultiEvent4 = true;
				}
			}
			if (_playMultiEvent1 && !(num2 < whenToFireMultiEvent1) && numOfMultiEventsToFire >= 1)
			{
				_playMultiEvent1 = false;
				MasterAudio.FireCustomEvent(MultiTimedEvent, _actorTrans);
			}
			if (_playMultiEvent2 && !(num2 < whenToFireMultiEvent2) && numOfMultiEventsToFire >= 2)
			{
				_playMultiEvent2 = false;
				MasterAudio.FireCustomEvent(MultiTimedEvent, _actorTrans);
			}
			if (_playMultiEvent3 && !(num2 < whenToFireMultiEvent3) && numOfMultiEventsToFire >= 3)
			{
				_playMultiEvent3 = false;
				MasterAudio.FireCustomEvent(MultiTimedEvent, _actorTrans);
			}
			if (_playMultiEvent4 && !(num2 < whenToFireMultiEvent4) && numOfMultiEventsToFire >= 4)
			{
				_playMultiEvent4 = false;
				MasterAudio.FireCustomEvent(MultiTimedEvent, _actorTrans);
			}
		}
		_lastRepetition = num;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (fireExitEvent && exitCustomEvent != "[None]" && !string.IsNullOrEmpty(exitCustomEvent))
		{
			MasterAudio.FireCustomEvent(exitCustomEvent, _actorTrans);
		}
		if (fireMultiAnimTimeEvent)
		{
			_playMultiEvent1 = true;
			_playMultiEvent2 = true;
			_playMultiEvent3 = true;
			_playMultiEvent4 = true;
		}
		if (fireAnimTimeEvent)
		{
			_fireTimedEvent = true;
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
}
