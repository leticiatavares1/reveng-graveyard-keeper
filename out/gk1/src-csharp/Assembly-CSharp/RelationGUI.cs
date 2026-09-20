using UnityEngine;

public class RelationGUI : MonoBehaviour
{
	public UI2DSprite face;

	public UILabel label_name;

	public UILabel label_description;

	public UILabel label_relation;

	public Color color_neutral;

	public Color color_negative;

	public Color color_positive;

	public UIWidget back;

	public Transform anchor_zone_name;

	public Transform anchor_zone_info;

	public Transform anchor_no_zone;

	private WorldGameObject _npc;

	private int _current_relation;

	private int _hide_frame;

	private GameObject _zone_info;

	private GameObject _hud_obj;

	private UIRect _hud_rect;

	private UIRect _self_rect;

	public UIProgressBar progress_bar;

	public Transform bubble_pos_tf;

	private ObjectDefinition _npc_obj_def;

	public GameObject relation_bar_go;

	private string _npc_id = "";

	public HUDTasksGUI npc_tasks;

	public bool additional;

	public Transform additional_point;

	public string npc_id => _npc_id;

	public void Init()
	{
		base.transform.parent.gameObject.SetActive(value: true);
		Hide();
		_self_rect = GetComponent<UIRect>();
		_zone_info = GUIElements.me.hud.zone_descr_object;
		_hud_obj = GUIElements.me.hud.gameObject;
		_hud_rect = _hud_obj.GetComponent<UIRect>();
		npc_tasks.Init();
	}

	public void Open(WorldGameObject npc, bool animated = true)
	{
		_npc_obj_def = (string.IsNullOrEmpty(npc.obj_def.npc_alias) ? npc.obj_def : GameBalance.me.GetData<ObjectDefinition>(npc.obj_def.npc_alias));
		_npc_id = _npc_obj_def.id;
		bool flag = npc.IsWorker();
		if (!_npc_obj_def.IsNPC() && !flag)
		{
			return;
		}
		if (_npc_obj_def.npc_in_list && !flag)
		{
			MainGame.me.save.OnMetNPC(_npc_id);
		}
		Sprite headSprite = npc.GetHeadSprite();
		face.sprite2D = headSprite;
		face.transform.parent.gameObject.SetActive(headSprite != null);
		relation_bar_go.SetActive(!flag);
		label_description.gameObject.SetActive(flag);
		if (flag)
		{
			label_description.text = npc.worker.GetWorkerEfficiencyText();
			npc.data.GetBodySkulls(out var _, out var positive, out var _, dont_count_self: true);
			label_name.text = "";
			for (int i = 0; i < positive; i++)
			{
				label_name.text += "(skull)";
			}
		}
		else
		{
			label_name.text = GJL.L(_npc_id);
		}
		GJL.L("desc_" + _npc_id);
		back.height = 64;
		_current_relation = int.MaxValue;
		_npc = npc;
		base.gameObject.Activate();
		Update();
		npc_tasks.Draw(_npc_id);
		GJL.EnsureChildLabelsHasCorrectFont(base.gameObject);
		GameObject gameObject = progress_bar.transform.parent.gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(!flag);
		}
		label_relation.SetActive(!flag);
	}

	public void Update()
	{
		if (additional)
		{
			if (!(_zone_info == null) && base.gameObject.activeSelf)
			{
				Transform transform = (GUIElements.me.relation.gameObject.activeSelf ? additional_point : ((_hud_obj.activeSelf && !_hud_rect.alpha.EqualsTo(0f)) ? (_zone_info.activeSelf ? anchor_zone_info : anchor_zone_name) : anchor_no_zone));
				base.transform.position = transform.position;
			}
		}
		else if (!(_zone_info == null) && base.gameObject.activeSelf)
		{
			Transform transform = ((!_self_rect.alpha.EqualsTo(1f) || (_hud_obj.activeSelf && !_hud_rect.alpha.EqualsTo(0f))) ? (_zone_info.activeSelf ? anchor_zone_info : anchor_zone_name) : anchor_no_zone);
			base.transform.position = transform.position;
			RedrawRelation();
		}
	}

	public void Hide()
	{
		base.gameObject.Deactivate();
		_hide_frame = Time.frameCount;
		_npc = null;
		_npc_obj_def = null;
		npc_tasks.Draw(string.Empty);
	}

	public void OnShownRelationBubble(WorldGameObject npc)
	{
		ObjectDefinition objectDefinition = (string.IsNullOrEmpty(npc.obj_def.npc_alias) ? npc.obj_def : GameBalance.me.GetData<ObjectDefinition>(npc.obj_def.npc_alias));
		if (objectDefinition == _npc_obj_def)
		{
			_current_relation = WorldGameObject.GetRelation(objectDefinition.id);
		}
		RedrawRelation();
	}

	public void RedrawRelation()
	{
		int relation = WorldGameObject.GetRelation(_npc_id);
		if (relation == _current_relation)
		{
			return;
		}
		label_relation.text = relation.ToString();
		progress_bar.value = (float)relation / 100f;
		if (_current_relation != int.MaxValue)
		{
			if (_npc.GetParam("it_is_a_copy") == 1f)
			{
				return;
			}
			int delta = relation - _current_relation;
			_npc.ShowRelationChangeBubble(delta);
		}
		_current_relation = relation;
	}

	public void ChangeHUDAlpha(bool show, bool animated)
	{
		Debug.Log("ChangeHUDAlpha show=" + show);
		base.gameObject.TryFinishAlphaTween();
		if (!animated)
		{
			_self_rect.alpha = (show ? 1f : 0f);
		}
		else
		{
			_self_rect.ChangeAlpha(_self_rect.alpha, show ? 1f : 0f, 0.2f);
		}
	}

	public ObjectDefinition GetCurrentInteractiveNPC()
	{
		if (!(_npc == null))
		{
			return _npc_obj_def;
		}
		return null;
	}
}
