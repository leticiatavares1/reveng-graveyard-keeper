using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

public class DungeonRoomInteriorEditor : MonoBehaviour
{
	public string current_interior_name = "interior_name";

	public DungeonRoomInterior.BiomType biom_type = DungeonRoomInterior.BiomType.Unknown;

	public string room_type = "";

	public DungeonRoomInterior.RoomSize room_size = DungeonRoomInterior.RoomSize.Unknown;

	[HideInInspector]
	public string saved_possible_enters_log = "";

	public const float CHECK_COLLIDER_DEPTH = 0.5f;

	public const float DOWN_CHECK_COLLIDER_DEPTH = 1.25f;

	private int _room_height = 1;

	private int _room_width = 1;

	public int[,] GenerateInteriorMatrix(WorldSimpleObject[] wsos)
	{
		int[,] array = new int[_room_width, _room_height];
		foreach (WorldSimpleObject obj in wsos)
		{
			Vector2 vector = obj.transform.localPosition;
			if (obj.wso_type == WorldSimpleObject.WSOType.WallStraight)
			{
				array[Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y)] = 1;
			}
		}
		return array;
	}

	public List<List<IntVector2>> FindPossibleEnters(int[,] t_room_interior_matrix, int t_corridor_width = 3)
	{
		if (t_corridor_width < 1)
		{
			Debug.LogError("Corridor width can not be less than 1!");
			return null;
		}
		if (_room_height < 2 + t_corridor_width && _room_width < 2 + t_corridor_width)
		{
			Debug.LogError("Corridor width can not be more than (room_height/2 - 1)! " + t_corridor_width);
			return null;
		}
		List<List<IntVector2>> possible_enters = new List<List<IntVector2>>();
		for (int i = 0; i < 4; i++)
		{
			possible_enters.Add(new List<IntVector2>());
		}
		for (int j = 1; j < _room_width - t_corridor_width; j++)
		{
			int num = 0;
			while (t_room_interior_matrix[j + num, 0] == 1)
			{
				num++;
				if (num == t_corridor_width)
				{
					break;
				}
			}
			if (num == t_corridor_width)
			{
				possible_enters[3].Add(new IntVector2(j));
			}
			int num2 = 0;
			while (t_room_interior_matrix[j + num2, _room_height - 1] == 1)
			{
				num2++;
				if (num2 == t_corridor_width)
				{
					break;
				}
			}
			if (num2 == t_corridor_width)
			{
				possible_enters[1].Add(new IntVector2(j, _room_height - 1));
			}
		}
		for (int k = 1; k < _room_height - t_corridor_width; k++)
		{
			int num3 = 0;
			while (t_room_interior_matrix[0, k + num3] == 1)
			{
				num3++;
				if (num3 == t_corridor_width)
				{
					break;
				}
			}
			if (num3 == t_corridor_width)
			{
				possible_enters[0].Add(new IntVector2(0, k));
			}
			int num4 = 0;
			while (t_room_interior_matrix[_room_width - 1, k + num4] == 1)
			{
				num4++;
				if (num4 == t_corridor_width)
				{
					break;
				}
			}
			if (num4 == t_corridor_width)
			{
				possible_enters[2].Add(new IntVector2(_room_width - 1, k));
			}
		}
		CheckPossibleEntersWithColliders(ref possible_enters, t_corridor_width);
		return possible_enters;
	}

	public bool IsCorrectCoords()
	{
		WorldGameObject[] componentsInChildren = base.gameObject.GetComponentsInChildren<WorldGameObject>();
		foreach (WorldGameObject worldGameObject in componentsInChildren)
		{
			if (worldGameObject.transform.localPosition.x < 0f || worldGameObject.transform.localPosition.y < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0f, 0.4f, 0f, 0.1f);
		Gizmos.DrawCube(base.transform.position + new Vector3(336f, 336f), new Vector3(768f, 768f, 0f));
		Gizmos.color = new Color(0.4f, 0f, 0f, 0.1f);
		Gizmos.DrawCube(base.transform.position + new Vector3(480f, 480f), new Vector3(1056f, 1056f, 0f));
		Gizmos.color = new Color(0f, 0f, 0.4f, 0.1f);
		Gizmos.DrawCube(base.transform.position + new Vector3(624f, 624f), new Vector3(1344f, 1344f, 0f));
		Gizmos.color = new Color(0.2f, 0f, 0.2f, 0.1f);
		Gizmos.DrawCube(base.transform.position + new Vector3(768f, 768f), new Vector3(1632f, 1632f, 0f));
		Gizmos.color = new Color(0f, 0.2f, 0.2f, 0.1f);
		Gizmos.DrawCube(base.transform.position + new Vector3(912f, 912f), new Vector3(1920f, 1920f, 0f));
	}

	public void CheckPossibleEntersWithColliders(ref List<List<IntVector2>> possible_enters, int corridor_width)
	{
		OptimizedCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		List<List<IntVector2>> list = new List<List<IntVector2>>();
		for (int j = 0; j < 4; j++)
		{
			list.Add(new List<IntVector2>());
		}
		for (int k = 0; k < 4; k++)
		{
			foreach (IntVector2 item in possible_enters[k])
			{
				Vector2 zero = Vector2.zero;
				Vector2 zero2 = Vector2.zero;
				Vector2 vector = base.gameObject.transform.position;
				switch (k)
				{
				case 0:
					zero.x = vector.x + (float)item.x * 96f;
					zero.y = vector.y + (float)item.y * 96f;
					zero2.x = zero.x + 48f;
					zero2.y = zero.y + (float)(corridor_width - 1) * 96f;
					break;
				case 1:
					zero.x = vector.x + (float)item.x * 96f;
					zero.y = vector.y + (float)item.y * 96f;
					zero2.x = zero.x + (float)(corridor_width - 1) * 96f;
					zero2.y = zero.y - 48f;
					break;
				case 2:
					zero.x = vector.x + (float)item.x * 96f;
					zero.y = vector.y + (float)item.y * 96f;
					zero2.x = zero.x - 48f;
					zero2.y = zero.y + (float)(corridor_width - 1) * 96f;
					break;
				case 3:
					zero.x = vector.x + (float)item.x * 96f;
					zero.y = vector.y + (float)item.y * 96f;
					zero2.x = zero.x + (float)(corridor_width - 1) * 96f;
					zero2.y = zero.y + 120f;
					break;
				default:
					Debug.LogError("This is IMPOSSIBRU!!!!");
					break;
				}
				if (zero == Vector2.zero || zero2 == Vector2.zero)
				{
					continue;
				}
				Collider2D[] array = Physics2D.OverlapAreaAll(zero, zero2, 1);
				if (array == null || array.Length == 0)
				{
					continue;
				}
				string text = "";
				int num = 0;
				Collider2D[] array2 = array;
				foreach (Collider2D collider2D in array2)
				{
					if (collider2D.gameObject.layer != 0)
					{
						Debug.LogError("This is can't be.");
						continue;
					}
					WorldSimpleObject componentInParent = collider2D.transform.GetComponentInParent<WorldSimpleObject>();
					if (componentInParent != null)
					{
						if (componentInParent.wso_type != WorldSimpleObject.WSOType.WallCorner && componentInParent.wso_type != 0 && componentInParent.wso_type != WorldSimpleObject.WSOType.Floor)
						{
							if (!list[k].Contains(item))
							{
								list[k].Add(item);
							}
							text = text + componentInParent.name + "=>" + collider2D.name + "\n";
							num++;
						}
						continue;
					}
					WorldObjectPart componentInParent2 = collider2D.transform.GetComponentInParent<WorldObjectPart>();
					if (componentInParent2 != null)
					{
						if (!list[k].Contains(item))
						{
							list[k].Add(item);
						}
						text = text + componentInParent2.name + "=>" + collider2D.name + "\n";
						num++;
					}
				}
				if (num > 0)
				{
					string[] obj = new string[8] { "[", null, null, null, null, null, null, null };
					Vector2 vector2 = zero;
					obj[1] = vector2.ToString();
					obj[2] = "]:[";
					vector2 = zero2;
					obj[3] = vector2.ToString();
					obj[4] = "]:: Found ";
					obj[5] = num.ToString();
					obj[6] = " overlaping colliders: \n";
					obj[7] = text;
					Debug.Log(string.Concat(obj));
				}
			}
		}
		string text2 = "Removed wrong enter: ";
		for (int l = 0; l < 4; l++)
		{
			string text3 = text2;
			DungeonWalker.Direction direction = (DungeonWalker.Direction)l;
			text2 = text3 + "\n" + direction.ToString() + ": ";
			if (list[l].Count == 0)
			{
				text2 += "None";
				continue;
			}
			foreach (IntVector2 item2 in list[l])
			{
				possible_enters[l].Remove(item2);
				text2 = text2 + item2?.ToString() + "; ";
			}
		}
		Debug.Log(text2);
	}

	public void PreparePossibleEntersLog(DungeonRoomInterior asset)
	{
		if (asset == null)
		{
			return;
		}
		saved_possible_enters_log = "";
		for (int i = 3; i <= 7; i++)
		{
			List<IntVectors> list = null;
			switch (i)
			{
			case 3:
				list = asset.possible_enters_3;
				break;
			case 4:
				list = asset.possible_enters_4;
				break;
			case 5:
				list = asset.possible_enters_5;
				break;
			case 6:
				list = asset.possible_enters_6;
				break;
			case 7:
				list = asset.possible_enters_7;
				break;
			}
			if (list == null)
			{
				continue;
			}
			string text = "";
			foreach (IntVectors item in list)
			{
				if (item == null || item.list == null || item.list.Count == 0)
				{
					text = text + " " + (DungeonWalker.Direction)list.IndexOf(item);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				saved_possible_enters_log = saved_possible_enters_log + "\n" + i + "." + text + ";";
			}
		}
	}
}
