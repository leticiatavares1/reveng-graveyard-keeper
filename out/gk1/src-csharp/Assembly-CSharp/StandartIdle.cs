using UnityEngine;

public class StandartIdle : BaseCharacterIdle
{
	[Space]
	public float speed = 1f;

	public float max_moving_range = 1f;

	protected override void MoveToRandomPos()
	{
		Vector2 vector = GetNextDest();
		if (ch != null && !(ch.wgo == null))
		{
			Vector2 vector2 = ch.wgo.pos - vector;
			if (vector2.magnitude > 96f * max_moving_range)
			{
				vector2 *= 96f * max_moving_range / vector2.magnitude;
				vector = ch.wgo.pos - vector2;
			}
			_state = IdleState.Moving;
			ch.GoTo(vector, snap_to_node: false, ChangeState, ChangeState);
			ch.SetSpeed(speed);
		}
	}
}
