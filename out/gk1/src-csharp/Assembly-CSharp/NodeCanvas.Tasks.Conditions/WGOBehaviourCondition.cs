using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class WGOBehaviourCondition : ConditionTask
{
	private WorldObjectPart _self_wgo_part;

	private WorldGameObject _self_wgo;

	private BaseCharacterComponent _self_ch;

	private bool _cached;

	protected WorldGameObject self_wgo
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _self_wgo;
		}
	}

	protected WorldGameObject player_wgo => MainGame.me.player;

	public BaseCharacterComponent self_ch
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _self_ch;
		}
	}

	private void Cache()
	{
		_self_wgo_part = base.ownerAgent.GetComponent<WorldObjectPart>();
		_self_wgo = _self_wgo_part.parent;
		_self_ch = ((_self_wgo != null) ? _self_wgo.components.character : null);
		_cached = _self_wgo != null && player_wgo != null;
	}
}
