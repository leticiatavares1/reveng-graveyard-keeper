using UnityEngine;

public class OverheadGUI : MonoBehaviour
{
	public UI2DSprite bar;

	private WorldGameObject _linked_obj;

	private long _linked_obj_id;

	private float _linked_obj_max_hp;

	private Transform _tf;

	private Camera _world_cam;

	private Camera _gui_cam;

	public long linked_obj_id => _linked_obj_id;

	public void CustomUpdate()
	{
		if (MainGame.game_started)
		{
			bar.fillAmount = MainGame.me.save.GetHPPercentage();
			base.gameObject.SetActive(bar.fillAmount < 1f);
			if (!(bar.fillAmount >= 1f))
			{
				base.transform.SetGUIPosToWorldPos(MainGame.me.player.pos3, MainGame.me.world_cam, MainGame.me.gui_cam);
			}
		}
	}

	public void LinkToObj(WorldGameObject obj)
	{
		_linked_obj = obj;
		_linked_obj_id = _linked_obj.unique_id;
		_linked_obj_max_hp = _linked_obj.obj_def.hp.EvaluateFloat(_linked_obj);
		_tf = base.transform;
		_world_cam = MainGame.me.world_cam;
		_gui_cam = MainGame.me.gui_cam;
	}

	public bool IsNotNeededAnymore()
	{
		if (!(_linked_obj == null))
		{
			return _linked_obj.hp.EqualsTo(_linked_obj_max_hp);
		}
		return true;
	}

	public void UpdateForLinkedObj()
	{
		bar.fillAmount = _linked_obj.hp / _linked_obj_max_hp;
		_tf.SetGUIPosToWorldPos(_linked_obj.pos3, _world_cam, _gui_cam);
	}

	public void SetActive(bool is_active)
	{
		base.gameObject.SetActive(is_active);
	}
}
