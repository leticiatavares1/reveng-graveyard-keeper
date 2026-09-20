using DG.Tweening;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Playlist Control", 0)]
[Category("Game/Sound")]
[Color("f5da42")]
public class Flow_PlaylistControl : GKCustomFlowNode
{
	public enum ControlType
	{
		Play,
		Pause,
		Stop,
		NextTrack,
		PlayTrack,
		SetWeight,
		AddWeight,
		PauseWithEase,
		UnPause,
		UnPauseWithEase
	}

	[GatherPortsCallback]
	public ControlType controlType;

	private static string previousPlaylistId;

	private static string previousTrackId;

	private static string activePlaylistId;

	private static string pausedPlaylistId;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> playlistId;

	private ValueInput<string> trackId;

	private ValueInput<float> weight;

	private ValueInput<float> duration;

	private ValueInput<Ease> ease;

	private ValueInput<bool> restorePreviousTrack;

	public override string name => $"{base.name} \n<color=#58BF2B>{controlType}</color>" + (playlistId.isDefaultValue ? "<color=#F73B3B>\nPlaylistId is NULL</color>" : string.Empty);

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Control);
		@out = AddFlowOutput("out".CapitalizeFirst());
		playlistId = AddValueInput<string>("playlistId".CapitalizeFirst());
		switch (controlType)
		{
		case ControlType.Play:
			duration = AddValueInput<float>("duration");
			break;
		case ControlType.Stop:
			restorePreviousTrack = AddValueInput<bool>("restorePreviousTrack");
			duration = AddValueInput<float>("duration");
			break;
		case ControlType.PlayTrack:
			trackId = AddValueInput<string>("trackId");
			duration = AddValueInput<float>("duration");
			break;
		case ControlType.SetWeight:
		case ControlType.AddWeight:
			trackId = AddValueInput<string>("trackId");
			weight = AddValueInput<float>("weight");
			break;
		case ControlType.Pause:
		case ControlType.UnPause:
			duration = AddValueInput<float>("duration");
			break;
		case ControlType.PauseWithEase:
		case ControlType.UnPauseWithEase:
			duration = AddValueInput<float>("duration");
			ease = AddValueInput<Ease>("ease");
			break;
		case ControlType.NextTrack:
			break;
		}
	}

	private void Control(Flow flow)
	{
		switch (controlType)
		{
		case ControlType.Play:
			if (((duration.value <= 0f) ? LazyAudio.PlayPlaylist(playlistId.value) : LazyAudio.PlayPlaylist(playlistId.value, duration.value)) != null)
			{
				PausePreviousActivePlaylist(playlistId.value, duration.value);
			}
			break;
		case ControlType.Pause:
			if (duration.value <= 0f)
			{
				LazyAudio.PausePlaylist(playlistId.value);
			}
			else
			{
				LazyAudio.PausePlaylist(playlistId.value, duration.value);
			}
			break;
		case ControlType.PauseWithEase:
			if (duration.value <= 0f)
			{
				LazyAudio.PausePlaylist(playlistId.value);
			}
			else
			{
				LazyAudio.PausePlaylist(playlistId.value, duration.value, ease.value);
			}
			break;
		case ControlType.UnPause:
			if (duration.value <= 0f)
			{
				LazyAudio.UnPausePlaylist(playlistId.value);
			}
			else
			{
				LazyAudio.UnPausePlaylist(playlistId.value, duration.value);
			}
			break;
		case ControlType.UnPauseWithEase:
			if (duration.value <= 0f)
			{
				LazyAudio.UnPausePlaylist(playlistId.value);
			}
			else
			{
				LazyAudio.UnPausePlaylist(playlistId.value, duration.value, ease.value);
			}
			break;
		case ControlType.Stop:
			if (restorePreviousTrack.value)
			{
				if (TryRestorePreviousTrack(playlistId.value))
				{
					break;
				}
			}
			else
			{
				ClearPreviousTrack();
			}
			if (duration.value <= 0f)
			{
				LazyAudio.StopPlaylist(playlistId.value);
			}
			else
			{
				LazyAudio.StopPlaylist(playlistId.value, duration.value);
			}
			UnpausePreviousActivePlaylist(playlistId.value, duration.value);
			break;
		case ControlType.NextTrack:
			LazyAudio.PlayNextTrack(playlistId.value);
			break;
		case ControlType.PlayTrack:
			RememberCurrentTrack(playlistId.value, trackId.value);
			if (duration.value <= 0f)
			{
				LazyAudio.PlayTrackInPlaylist(trackId.value, playlistId.value);
			}
			else
			{
				LazyAudio.PlayTrackInPlaylist(trackId.value, playlistId.value, duration.value);
			}
			break;
		case ControlType.SetWeight:
			LazyAudio.SetWeightTrackInPlaylist(trackId.value, playlistId.value, weight.value);
			break;
		case ControlType.AddWeight:
			LazyAudio.AddWeightTrackInPlaylist(trackId.value, playlistId.value, weight.value);
			break;
		}
		@out.Call(flow);
	}

	private static void PausePreviousActivePlaylist(string newPlaylistId, float fadeDuration)
	{
		if (!string.IsNullOrEmpty(activePlaylistId) && activePlaylistId != newPlaylistId)
		{
			if (fadeDuration <= 0f)
			{
				LazyAudio.PausePlaylist(activePlaylistId);
			}
			else
			{
				LazyAudio.PausePlaylist(activePlaylistId, fadeDuration);
			}
			pausedPlaylistId = activePlaylistId;
		}
		activePlaylistId = newPlaylistId;
	}

	private static void UnpausePreviousActivePlaylist(string stoppedPlaylistId, float fadeDuration)
	{
		if (activePlaylistId != stoppedPlaylistId)
		{
			return;
		}
		string value = pausedPlaylistId;
		pausedPlaylistId = null;
		activePlaylistId = value;
		if (!string.IsNullOrEmpty(value))
		{
			if (fadeDuration <= 0f)
			{
				LazyAudio.UnPausePlaylist(value);
			}
			else
			{
				LazyAudio.UnPausePlaylist(value, fadeDuration);
			}
		}
	}

	private static PlaylistController FindPlaylistController(string id)
	{
		foreach (PlaylistController playlistController in LazyAudio.GetPlaylistControllers())
		{
			if (playlistController.Id == id)
			{
				return playlistController;
			}
		}
		return null;
	}

	private static void RememberCurrentTrack(string playlistId, string newTrackId)
	{
		if (!string.IsNullOrEmpty(playlistId))
		{
			Track track = FindPlaylistController(playlistId)?.LastTrack;
			if (track != null && !string.IsNullOrEmpty(track.id) && !(track.id == newTrackId))
			{
				previousPlaylistId = playlistId;
				previousTrackId = track.id;
			}
		}
	}

	private static void ClearPreviousTrack()
	{
		previousPlaylistId = null;
		previousTrackId = null;
	}

	private static bool TryRestorePreviousTrack(string playlistId)
	{
		if (string.IsNullOrEmpty(previousTrackId) || previousPlaylistId != playlistId)
		{
			return false;
		}
		string text = previousTrackId;
		ClearPreviousTrack();
		LazyAudio.PlayTrackInPlaylist(text, playlistId);
		return true;
	}
}
