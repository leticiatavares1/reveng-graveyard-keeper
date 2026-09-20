using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

[CreateAssetMenu]
public class TrailTypeDefinition : ScriptableObject
{
	public Ground.GroudType type;

	public Color color = Color.white;

	public List<Sprite> hor_b;

	public List<Sprite> hor_t;

	public List<Sprite> vert_l;

	public List<Sprite> vert_r;

	public List<Sprite> diag_lt;

	public List<Sprite> diag_rb;

	public bool custom_trail_dist;

	public float leave_trail_dist = 370f;

	public bool custom_trail_decrease;

	public float trail_decrease = 0.9f;

	[SoundGroup]
	public string sound = "[None]";

	public Sprite GetByDirection(Vector2 dir, bool is_left_foot, out bool flip)
	{
		List<Sprite> list = hor_b;
		float num = Mathf.Atan2(dir.y, dir.x) * 57.29578f;
		flip = false;
		switch (Mathf.RoundToInt(num / 45f))
		{
		case 0:
			list = (is_left_foot ? hor_b : hor_t);
			break;
		case -4:
		case 4:
			list = ((!is_left_foot) ? hor_b : hor_t);
			break;
		case -2:
			list = (is_left_foot ? vert_l : vert_r);
			break;
		case 2:
			list = ((!is_left_foot) ? vert_l : vert_r);
			break;
		case -3:
			list = (is_left_foot ? diag_lt : diag_rb);
			break;
		case 1:
			list = ((!is_left_foot) ? diag_lt : diag_rb);
			break;
		case -1:
			flip = true;
			list = (is_left_foot ? diag_rb : diag_lt);
			break;
		case 3:
			flip = true;
			list = ((!is_left_foot) ? diag_rb : diag_lt);
			break;
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[NGUITools.RandomRange(0, list.Count - 1)];
	}
}
