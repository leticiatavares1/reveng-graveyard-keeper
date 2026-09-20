using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedWorkersList
{
	[SerializeField]
	private List<Worker> _workers = new List<Worker>();

	[SerializeField]
	private long last_unique_id;

	public Worker CreateNewWorker(WorldGameObject zombie_wgo, string worker_id, Item base_body)
	{
		last_unique_id++;
		Worker worker = new Worker(zombie_wgo, last_unique_id);
		worker.id = worker_id;
		_workers.Add(worker);
		zombie_wgo.data.inventory = base_body.inventory;
		return worker;
	}

	public Worker GetWorker(long worker_unique_id)
	{
		foreach (Worker worker in _workers)
		{
			if (worker.worker_unique_id == worker_unique_id)
			{
				return worker;
			}
		}
		return null;
	}
}
