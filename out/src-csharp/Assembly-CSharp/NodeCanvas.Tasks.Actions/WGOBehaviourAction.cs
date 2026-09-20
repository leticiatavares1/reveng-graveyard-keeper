using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class WGOBehaviourAction : ActionTask
{
	private WorldObjectPart _self_wgo_part;

	private WorldGameObject _self_wgo;

	private WorldGameObject _player_wgo;

	private BaseCharacterComponent _self_ch;

	private ProjectileEmitter _projectile_emitter;

	private bool _has_projectile_emitter;

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

	protected WorldGameObject player_wgo
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _player_wgo;
		}
	}

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

	protected ProjectileEmitter projectile_emitter
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _projectile_emitter;
		}
	}

	protected bool has_projectile_emitter => _has_projectile_emitter;

	private void Cache()
	{
		_self_wgo_part = base.ownerAgent.GetComponent<WorldObjectPart>();
		_self_wgo = _self_wgo_part.parent;
		_self_ch = ((_self_wgo != null) ? _self_wgo.components.character : null);
		_player_wgo = MainGame.me.player;
		_projectile_emitter = _self_wgo_part.GetComponentInChildren<ProjectileEmitter>();
		_has_projectile_emitter = _projectile_emitter != null;
		_cached = _self_wgo != null && _player_wgo != null;
	}
}
