using System.Collections.Generic;
using UnityEngine;

namespace NGTools.Network;

public abstract class BaseServer : MonoBehaviour
{
	private static List<BaseServer> instances = new List<BaseServer>();

	[Header("Starts the server when awaking.")]
	public bool autoStart = true;

	[Header("Keep the server alive between scenes.")]
	public bool dontDestroyOnLoad = true;

	[Header("[Required] A listener to communicate via network.")]
	public NetworkListener listener;

	public PacketExecuter executer { get; private set; }

	protected virtual void Awake()
	{
		if (executer != null)
		{
			return;
		}
		for (int i = 0; i < instances.Count; i++)
		{
			if (instances[i].GetType() == GetType())
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		executer = CreatePacketExecuter();
		if (dontDestroyOnLoad)
		{
			instances.Add(this);
			Object.DontDestroyOnLoad(base.transform.root.gameObject);
		}
	}

	protected virtual void Start()
	{
		if (listener == null)
		{
			Debug.LogError("A NetworkListener is required.", this);
			return;
		}
		listener.SetServer(this);
		if (autoStart)
		{
			StartServer();
		}
	}

	protected virtual void OnDestroy()
	{
		if (listener != null)
		{
			listener.StopServer();
			listener = null;
		}
	}

	public void StartServer()
	{
		listener.StartServer();
	}

	public void StopServer()
	{
		OnDestroy();
	}

	protected abstract PacketExecuter CreatePacketExecuter();
}
