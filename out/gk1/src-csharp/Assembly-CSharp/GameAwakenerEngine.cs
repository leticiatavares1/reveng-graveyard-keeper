using System.Collections.Generic;
using System.Diagnostics;
using LinqTools;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class GameAwakenerEngine : MonoBehaviour
{
	private struct ObjectData
	{
		public SimplifiedObject obj;

		public float x;

		public float y;
	}

	private const bool REALTIME_AWAKENING = false;

	private static GameAwakenerEngine _me = null;

	private static bool _inited = false;

	private List<ObjectData> _objs = new List<ObjectData>();

	private Stopwatch _sw = new Stopwatch();

	private float _frame_len;

	public float activation_sqr_radius = 3400000f;

	private static int _prewarm_iterator = 0;

	public static bool prewarm_finished = true;

	public static int left_objects_to_prewarm = 0;

	public static int was_objects_to_prewarm = 0;

	protected static GameAwakenerEngine me
	{
		get
		{
			if (!_inited)
			{
				_inited = true;
				_me = new GameObject("GameAwakenerEngine").AddComponent<GameAwakenerEngine>();
			}
			return _me;
		}
	}

	public static void Init()
	{
		UnityEngine.Debug.Log("GameAwakenerEngine.Init", me);
	}

	public static void ScanMap()
	{
		List<SimplifiedObject> list = MainGame.me.world_root.GetComponentsInChildren<SimplifiedObject>(includeInactive: true).ToList();
		me._objs.Clear();
		foreach (SimplifiedObject item in list)
		{
			me._objs.Add(new ObjectData
			{
				obj = item,
				x = item.transform.position.x,
				y = item.transform.position.y
			});
		}
		me.gameObject.SetActive(value: true);
	}

	public static void StartRestoringSimplifiedObjects()
	{
	}

	public static void Stop()
	{
		me._objs.Clear();
		me.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		if (!prewarm_finished)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (_prewarm_iterator < me._objs.Count)
			{
				me._objs[_prewarm_iterator].obj.Restore();
				left_objects_to_prewarm--;
				_prewarm_iterator++;
				if (stopwatch.ElapsedMilliseconds > 200)
				{
					return;
				}
			}
			me._objs.Clear();
			prewarm_finished = true;
		}
		base.gameObject.SetActive(value: false);
	}

	private int RestoreNearObjects()
	{
		return -1;
	}

	public static void OnPlayerMoved()
	{
	}

	public static void PreWarm()
	{
		was_objects_to_prewarm = (left_objects_to_prewarm = me._objs.Count);
		_prewarm_iterator = 0;
		prewarm_finished = false;
		me.gameObject.SetActive(value: true);
	}
}
