using Rewired;
using UnityEngine;

namespace LazyBearTechnology;

public class Stick
{
	private const float NEW_DIR_DELAY = 0.05f;

	private const float MIN_MAGNITUDE = 0.1f;

	private const float OPPOSITE_DIR_MIN_MAGNITUDE = 0.35f;

	public Vector2 direction;

	private int horizontalAxisId;

	private int verticalAxisId;

	private float prevH;

	private float prevV;

	private float currentH;

	private float currentV;

	private float stickDelay;

	public bool HasDirection => direction.magnitude > 0f;

	public Stick(int horizontalAxisId, int verticalAxisId)
	{
		this.horizontalAxisId = horizontalAxisId;
		this.verticalAxisId = verticalAxisId;
	}

	public void Update(Player player)
	{
		currentH = player.GetAxis(horizontalAxisId);
		currentV = player.GetAxis(verticalAxisId);
		direction = new Vector2(currentH, currentV);
		if (direction.magnitude < 0.1f)
		{
			direction = Vector2.zero;
			currentH = (currentV = 0f);
		}
		stickDelay -= Time.deltaTime;
		if (direction.magnitude > 0f)
		{
			if (((prevH < 0f && currentH > 0f) || (prevH > 0f && currentH < 0f)) && ((prevV < 0f && currentV > 0f) || (prevV > 0f && currentV < 0f)))
			{
				stickDelay = 0.05f;
			}
		}
		else
		{
			stickDelay = 0f;
		}
		if (stickDelay > 0f && direction.magnitude < 0.35f)
		{
			direction = Vector2.zero;
			currentH = (currentV = 0f);
		}
		prevH = direction.x;
		prevV = direction.y;
	}
}
