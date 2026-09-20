using UnityEngine;

public class TutorialArrowGUI : MonoBehaviour
{
	private WorldGameObject _attached_wgo;

	private bool _visible;

	public Transform inside_obj;

	public float time_k = 7f;

	public float w;

	public float h;

	public void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	public void AttachToWGO(WorldGameObject wgo)
	{
		_attached_wgo = wgo;
		_visible = wgo != null;
		base.gameObject.SetActive(_visible);
	}

	public void Update()
	{
		if (_visible)
		{
			Vector3 position = _attached_wgo.pos3;
			if (_attached_wgo.bubble_pos_tf != null)
			{
				position = _attached_wgo.bubble_pos_tf.transform.position;
			}
			base.transform.SetGUIPosToWorldPos(position, MainGame.me.world_cam, MainGame.me.gui_cam);
			Vector2 vector = base.transform.localPosition;
			Vector2 vector2 = vector;
			w = (float)Screen.width / 6.6f;
			h = (float)Screen.height / 6.4f;
			bool flag = false;
			if (vector.x > w)
			{
				vector.x = w;
				flag = true;
			}
			else if (vector.x < 0f - w)
			{
				vector.x = 0f - w;
				flag = true;
			}
			if (vector.y > h)
			{
				vector.y = h;
				flag = true;
			}
			else if (vector.y < 0f - h)
			{
				vector.y = 0f - h;
				flag = true;
			}
			if (flag)
			{
				base.transform.localPosition = vector;
				base.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector2.y, vector2.x) * 57.29578f + 90f);
			}
			else
			{
				base.transform.localRotation = Quaternion.identity;
			}
			inside_obj.localPosition = new Vector3(0f, Mathf.Sin(Time.fixedTime * time_k) * 4f + 4f);
		}
	}
}
