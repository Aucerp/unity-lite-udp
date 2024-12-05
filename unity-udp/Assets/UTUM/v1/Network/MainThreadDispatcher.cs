using System;
using System.Collections.Generic;
using UnityEngine;

namespace MENet
{
	public class MainThreadDispatcher : MonoBehaviour
	{
		public MainThreadDispatcher Instance { get; set; }
		private static readonly Queue<Action> ActionQueue = new Queue<Action>();

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(this);
			}
		}

		public static void RunOnMainThread(Action action)
		{
			lock (ActionQueue)
			{
				ActionQueue.Enqueue(action);
			}
		}

		private void Update()
		{
			lock (ActionQueue)
			{
				while (ActionQueue.Count > 0)
				{
					var action = ActionQueue.Dequeue();
					if (action != null) action();
				}
			}
		}
	}
}

