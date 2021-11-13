using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour {

	public int maxDequeuePerFrame = 100;

	static ThreadedDataRequester instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	void Awake() {
		instance = FindObjectOfType<ThreadedDataRequester> ();
	}

	public static void RequestData(Func<object> generateData, Action<object> callback) {
        void threadStart()
        {
            instance.DataThread(generateData, callback);
        }

        Thread thread = new Thread(threadStart)
        {
            Priority = System.Threading.ThreadPriority.Lowest,
            IsBackground = true
        };

        thread.Start();
	}

	void DataThread(Func<object> generateData, Action<object> callback) {
		object data = generateData ();
		lock (dataQueue) {
			dataQueue.Enqueue(new ThreadInfo (callback, data));
		}
	}
		

	void Update() {
		if (dataQueue.Count > 0) {
			for (int i = 0; i < Mathf.Min(maxDequeuePerFrame, dataQueue.Count); i++) {
				ThreadInfo threadInfo = dataQueue.Dequeue();
				try
				{
					threadInfo.callback(threadInfo.parameter);
				}
				catch { }
			}
		}
	}

	struct ThreadInfo {
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo (Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

	}
}
