using UnityEngine;

public interface IMovable
{
	Vector3 MovablePosition { get; set; }

	Vector3 MovablePositionWithoutDirectionChange { get; set; }

	Vector2 MovableDirection { get; set; }

	string MovableObjectId { get; }

	void OnTransitionReached(string currentWorldId, string destinationWorldId)
	{
	}

	void OnPathStart()
	{
	}

	void OnPathComplete(MovementComponent component)
	{
	}

	void OnTeleportToTransitPoint(Vector3 from, Vector3 to)
	{
	}
}
