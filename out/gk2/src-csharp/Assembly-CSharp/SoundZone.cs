using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public class SoundZone : MonoBehaviour
{
	[Serializable]
	private class SegmentPoint
	{
		public int segmentIndex;

		public float distance;

		public float positionOnLine;

		public SegmentPoint(int segmentIndex, float distance, float positionOnLine)
		{
			this.segmentIndex = segmentIndex;
			this.distance = distance;
			this.positionOnLine = positionOnLine;
		}
	}

	[SerializeField]
	private SoundZoneTrackType trackType;

	[SerializeField]
	private string soundId;

	[Tooltip("Sound would be attached to object")]
	[SerializeField]
	private bool playSoundAs3DSound;

	[SerializeField]
	private float playlistTransitionDuration = 2f;

	[SerializeField]
	private Ease playlistTransitionEase = Ease.InOutSine;

	[SerializeField]
	private bool customValue = true;

	[SerializeField]
	private SoundZonePreset preset;

	[SerializeField]
	private SoundZoneValuesType soundType;

	[SerializeField]
	private AnimationCurve curve;

	[SerializeField]
	private float minDistance;

	[SerializeField]
	private float maxDistance;

	[Space]
	[SerializeField]
	private float currentDistance;

	[SerializeField]
	private float currentVolumeCoef;

	[SerializeField]
	private SoundZoneType type = SoundZoneType.Line;

	[SerializeField]
	private bool overrideAmbientSound;

	[Tooltip("If true, the ambient override switches instantly instead of crossfading over the environment's switch duration.")]
	[SerializeField]
	private bool hardCutOverride;

	[SerializeField]
	private List<SoundPoint> points = new List<SoundPoint>();

	[SerializeField]
	private List<SoundZoneCollider> colliders = new List<SoundZoneCollider>();

	[SerializeField]
	private List<bool> isMainCameraInsideList = new List<bool>();

	[SerializeField]
	private float[] segmentLengths;

	[SerializeField]
	private Vector3[] segmentDirections;

	[SerializeField]
	private int curClosestSpIndex;

	[SerializeField]
	private Vector3 closestPosOnLine;

	[SerializeField]
	private float totalLineLength;

	[SerializeField]
	private float curPosOnLine;

	[SerializeField]
	private SegmentPoint p1;

	[SerializeField]
	private SegmentPoint p2;

	[SerializeField]
	private Vector3 polygonCenter;

	private SoundHandler soundHandler;

	private PlaylistController playlistController;

	private PlaylistController previousPlaylistController;

	private Transform sound3DTransform;

	private bool isMainCameraInsideZone;

	private float maxMoveDistance = 10f;

	private bool IsCustomAndCurve
	{
		get
		{
			if (customValue)
			{
				return soundType == SoundZoneValuesType.Curve;
			}
			return false;
		}
	}

	public bool PlaySoundAs3DSound => playSoundAs3DSound;

	public float MinDistance
	{
		get
		{
			if (!customValue)
			{
				return preset.minDistance;
			}
			return minDistance;
		}
	}

	public float MaxDistance
	{
		get
		{
			if (!customValue)
			{
				return preset.maxDistance;
			}
			return maxDistance;
		}
	}

	public bool OverrideAmbientSound => overrideAmbientSound;

	private float CalcVolumeCoef(float coef)
	{
		if (customValue)
		{
			if (soundType == SoundZoneValuesType.Curve)
			{
				return curve.Evaluate(coef);
			}
			return coef;
		}
		return preset.Evaluate(coef);
	}

	public void OnPlayerEnter(int colliderIndex)
	{
		isMainCameraInsideList[colliderIndex] = true;
		EnsurePlayerInside();
	}

	public void OnPlayerExit(int colliderIndex)
	{
		isMainCameraInsideList[colliderIndex] = false;
		EnsurePlayerInside();
	}

	private void EnsurePlayerInside()
	{
		bool num = isMainCameraInsideZone;
		isMainCameraInsideZone = isMainCameraInsideList.Contains(item: true);
		if (num == isMainCameraInsideZone)
		{
			return;
		}
		if (isMainCameraInsideZone)
		{
			if (overrideAmbientSound && EnvironmentEngine.Instance != null)
			{
				EnvironmentEngine.Instance.PushOverrodeAmbientSound(this, soundId, !hardCutOverride);
			}
			switch (trackType)
			{
			case SoundZoneTrackType.Sound:
				soundHandler?.Stop();
				if (playSoundAs3DSound)
				{
					if (sound3DTransform == null)
					{
						sound3DTransform = new GameObject("SoundTransform").transform;
						sound3DTransform.SetParent(base.transform);
					}
					soundHandler = LazyAudio.PlayAtGameObject(soundId, sound3DTransform, SpatialType.sound3D);
				}
				else
				{
					soundHandler = LazyAudio.Play(soundId);
				}
				break;
			case SoundZoneTrackType.Playlist:
			{
				PlaylistController activePlaylistController = GetActivePlaylistController();
				if (!(activePlaylistController != null) || !(activePlaylistController.Id == soundId))
				{
					previousPlaylistController = activePlaylistController;
					previousPlaylistController?.Pause(playlistTransitionDuration, playlistTransitionEase);
					playlistController = LazyAudio.PlayPlaylist(soundId);
				}
				break;
			}
			case SoundZoneTrackType.None:
				break;
			}
			return;
		}
		ReleaseAmbientOverride();
		switch (trackType)
		{
		case SoundZoneTrackType.Sound:
			soundHandler?.Stop();
			if (playSoundAs3DSound && sound3DTransform != null)
			{
				UnityEngine.Object.Destroy(sound3DTransform.gameObject);
			}
			break;
		case SoundZoneTrackType.Playlist:
			playlistController?.Stop();
			playlistController = null;
			if (previousPlaylistController == null)
			{
				playlistController = LazyAudio.PlayPlaylist("gameplay");
				break;
			}
			previousPlaylistController?.UnPause(playlistTransitionDuration, playlistTransitionEase);
			previousPlaylistController = null;
			break;
		case SoundZoneTrackType.None:
			break;
		}
	}

	private void ReleaseAmbientOverride()
	{
		if (overrideAmbientSound && EnvironmentEngine.Instance != null)
		{
			EnvironmentEngine.Instance.ClearOverrodeAmbientSound(this, !hardCutOverride);
		}
	}

	private void OnDisable()
	{
		ReleaseAmbientOverride();
		for (int i = 0; i < isMainCameraInsideList.Count; i++)
		{
			isMainCameraInsideList[i] = false;
		}
		isMainCameraInsideZone = false;
	}

	private PlaylistController GetActivePlaylistController()
	{
		foreach (PlaylistController playlistController in LazyAudio.GetPlaylistControllers())
		{
			if (!(playlistController.Id == soundId) && playlistController.IsActive && !playlistController.IsPaused)
			{
				return playlistController;
			}
		}
		return null;
	}

	private void Start()
	{
		CalculatePathData();
	}

	private void Update()
	{
		if (!isMainCameraInsideZone)
		{
			return;
		}
		switch (type)
		{
		case SoundZoneType.Line:
			if (playSoundAs3DSound && sound3DTransform != null)
			{
				sound3DTransform.position = CalculateSourcePosition();
			}
			DoLineCalculations();
			break;
		case SoundZoneType.Point:
			DoPointCalculations();
			break;
		case SoundZoneType.Polygon:
			if (points.Count >= 3)
			{
				DoPolygonCalculations();
			}
			break;
		}
	}

	private void OnDestroy()
	{
		ReleaseAmbientOverride();
		soundHandler?.Stop();
		if (isMainCameraInsideZone && trackType == SoundZoneTrackType.Playlist)
		{
			playlistController?.Stop();
			previousPlaylistController?.UnPause();
		}
	}

	private void CalculatePathData()
	{
		if (points.Count < 2)
		{
			return;
		}
		if (type == SoundZoneType.Polygon && points.Count >= 3)
		{
			Vector3 zero = Vector3.zero;
			foreach (SoundPoint point in points)
			{
				zero += point.transform.position;
			}
			polygonCenter = zero / points.Count;
			return;
		}
		segmentLengths = new float[points.Count - 1];
		segmentDirections = new Vector3[points.Count - 1];
		totalLineLength = 0f;
		for (int i = 0; i < points.Count - 1; i++)
		{
			Vector3 vector = points[i + 1].transform.position - points[i].transform.position;
			segmentLengths[i] = vector.magnitude;
			segmentDirections[i] = vector.normalized;
			totalLineLength += segmentLengths[i];
		}
	}

	private Vector3 CalculateSourcePosition()
	{
		Vector3 position = LazyAudio.Microphone.position;
		float num = FindWeightedTargetPosOnLine(position);
		float num2 = maxMoveDistance * Time.deltaTime;
		float f = num - curPosOnLine;
		if (Mathf.Abs(f) > num2)
		{
			curPosOnLine += Mathf.Sign(f) * num2;
		}
		else
		{
			curPosOnLine = num;
		}
		curPosOnLine = Mathf.Clamp(curPosOnLine, 0f, totalLineLength);
		return GetPositionAtDistance(curPosOnLine);
	}

	private float FindWeightedTargetPosOnLine(Vector3 playerPos)
	{
		List<SegmentPoint> list = new List<SegmentPoint>();
		float num = 0f;
		for (int i = 0; i < points.Count - 1; i++)
		{
			Vector3 position = points[i].transform.position;
			Vector3 vector = segmentDirections[i];
			float value = Vector3.Dot(playerPos - position, vector);
			value = Mathf.Clamp(value, 0f, segmentLengths[i]);
			Vector3 pt = position + vector * value;
			float distance = CalcDistanceBetween(playerPos, pt);
			float positionOnLine = num + value;
			list.Add(new SegmentPoint(i, distance, positionOnLine));
			num += segmentLengths[i];
		}
		list.Sort((SegmentPoint a, SegmentPoint b) => a.distance.CompareTo(b.distance));
		p1 = list[0];
		p2 = list[1];
		if (p2.positionOnLine.EqualsTo(GetSegmentStartLength(p2.segmentIndex)) || p2.positionOnLine.EqualsTo(GetSegmentEndLength(p2.segmentIndex)))
		{
			p2 = null;
			return p1.positionOnLine;
		}
		float num2 = 0.001f;
		float num3 = 1f / (p1.distance - minDistance + num2);
		float num4 = 1f / (p2.distance - minDistance + num2);
		float num5 = num3 + num4;
		num3 /= num5;
		num4 /= num5;
		return p1.positionOnLine * num3 + p2.positionOnLine * num4;
	}

	private float GetSegmentStartLength(int index)
	{
		if (index == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < index; i++)
		{
			num += segmentLengths[i];
		}
		return num;
	}

	private float GetSegmentEndLength(int index)
	{
		float num = 0f;
		for (int i = 0; i <= index; i++)
		{
			num += segmentLengths[i];
		}
		return num;
	}

	public Vector3 GetPositionAtDistance(float distance)
	{
		float num = 0f;
		for (int i = 0; i < segmentLengths.Length; i++)
		{
			if (num + segmentLengths[i] >= distance)
			{
				float t = (distance - num) / segmentLengths[i];
				return Vector3.Lerp(points[i].transform.position, points[i + 1].transform.position, t);
			}
			num += segmentLengths[i];
		}
		List<SoundPoint> list = points;
		return list[list.Count - 1].transform.position;
	}

	private void DoLineCalculations()
	{
		Vector3 position = LazyAudio.Microphone.position;
		float magnitude3;
		if (!playSoundAs3DSound)
		{
			curClosestSpIndex = FindClosestToPlayerPointIndex();
			Vector3 vector;
			if (curClosestSpIndex == 0 || curClosestSpIndex == points.Count - 1)
			{
				int index = ((curClosestSpIndex == 0) ? (curClosestSpIndex + 1) : (curClosestSpIndex - 1));
				vector = GetNearestPointToPlayerOnFiniteLine(points[index].transform.position, points[curClosestSpIndex].transform.position);
			}
			else
			{
				Vector3 nearestPointToPlayerOnFiniteLine = GetNearestPointToPlayerOnFiniteLine(points[curClosestSpIndex - 1].transform.position, points[curClosestSpIndex].transform.position);
				Vector3 nearestPointToPlayerOnFiniteLine2 = GetNearestPointToPlayerOnFiniteLine(points[curClosestSpIndex + 1].transform.position, points[curClosestSpIndex].transform.position);
				float magnitude = (position - nearestPointToPlayerOnFiniteLine).magnitude;
				float magnitude2 = (position - nearestPointToPlayerOnFiniteLine2).magnitude;
				vector = ((magnitude < magnitude2) ? nearestPointToPlayerOnFiniteLine : nearestPointToPlayerOnFiniteLine2);
			}
			closestPosOnLine = vector;
			magnitude3 = (position - closestPosOnLine).magnitude;
		}
		else
		{
			closestPosOnLine = GetPositionAtDistance(p1.positionOnLine);
			magnitude3 = (position - closestPosOnLine).magnitude;
		}
		ApplyCoefByDistance(magnitude3);
	}

	private void DoPointCalculations()
	{
		CalcDistanceAndApplyCoef(base.transform.position);
	}

	private void DoPolygonCalculations()
	{
		Vector3 position = LazyAudio.Microphone.position;
		Vector3 closestPointOnPolygonBoundary = GetClosestPointOnPolygonBoundary(position);
		bool flag = IsPointInsidePolygon(position);
		if (playSoundAs3DSound && sound3DTransform != null)
		{
			sound3DTransform.position = (flag ? position : closestPointOnPolygonBoundary);
		}
		closestPosOnLine = closestPointOnPolygonBoundary;
		if (flag)
		{
			ApplyCoefByDistance(0f);
		}
		else
		{
			CalcDistanceAndApplyCoef(closestPointOnPolygonBoundary);
		}
	}

	private bool IsPointInsidePolygon(Vector3 point)
	{
		if (points == null || points.Count < 3)
		{
			return false;
		}
		bool flag = false;
		int num = 0;
		int index = points.Count - 1;
		while (num < points.Count)
		{
			Vector3 position = points[num].transform.position;
			Vector3 position2 = points[index].transform.position;
			if (position.z > point.z != position2.z > point.z && point.x < (position2.x - position.x) * (point.z - position.z) / (position2.z - position.z) + position.x)
			{
				flag = !flag;
			}
			index = num++;
		}
		return flag;
	}

	private Vector3 GetClosestPointOnPolygonBoundary(Vector3 position)
	{
		if (points == null || points.Count < 3)
		{
			return polygonCenter;
		}
		Vector3 result = points[0].transform.position;
		float num = float.MaxValue;
		for (int i = 0; i < points.Count; i++)
		{
			int index = (i + 1) % points.Count;
			Vector3 nearestPointToPlayerOnFiniteLine = GetNearestPointToPlayerOnFiniteLine(points[i].transform.position, points[index].transform.position);
			float sqrMagnitude = (position - nearestPointToPlayerOnFiniteLine).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = nearestPointToPlayerOnFiniteLine;
			}
		}
		return result;
	}

	private void CalcDistanceAndApplyCoef(Vector3 closestPoint)
	{
		ApplyCoefByDistance((LazyAudio.Microphone.position - closestPoint).magnitude);
	}

	private void ApplyCoefByDistance(float distance)
	{
		if (distance < MinDistance)
		{
			currentVolumeCoef = 1f;
		}
		else
		{
			currentVolumeCoef = Mathf.Clamp01(1f - (distance - MinDistance) / (MaxDistance - MinDistance));
			currentVolumeCoef = CalcVolumeCoef(currentVolumeCoef);
		}
		soundHandler?.SetVolume(currentVolumeCoef);
		currentDistance = distance;
	}

	private int FindClosestToPlayerPointIndex()
	{
		Vector3 position = LazyAudio.Microphone.position;
		int result = 0;
		float num = CalcDistanceBetween(position, points[0].transform.position);
		for (int i = 1; i < points.Count; i++)
		{
			float num2 = CalcDistanceBetween(position, points[i].transform.position);
			if (num2 <= num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private float CalcDistanceBetween(Vector3 pt1, Vector3 pt2)
	{
		return Vector3.Distance(pt1, pt2);
	}

	private Vector3 GetNearestPointOnLine(Vector3 startPt, Vector3 lineDir)
	{
		lineDir.Normalize();
		float num = Vector3.Dot(LazyAudio.Microphone.position - startPt, lineDir);
		return startPt + lineDir * num;
	}

	private Vector3 GetNearestPointToPlayerOnFiniteLine(Vector3 startPt, Vector3 endPt)
	{
		Vector3 vector = endPt - startPt;
		float magnitude = vector.magnitude;
		vector.Normalize();
		float value = Vector3.Dot(LazyAudio.Microphone.position - startPt, vector);
		value = Mathf.Clamp(value, 0f, magnitude);
		return startPt + vector * value;
	}
}
