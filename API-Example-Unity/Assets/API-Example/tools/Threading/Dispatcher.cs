using System.Collections.Generic;
using UnityEngine;

namespace Threading
{
    public class Dispatcher : MonoBehaviour
    {
        private static readonly HashSet<AsyncTask> ThreadedTasks = new HashSet<AsyncTask>();
        private static readonly Dictionary<string, AsyncTask> BackgroundTasks = new Dictionary<string, AsyncTask>();

        private static Dispatcher _instance;
        public static Dispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var dispatcherObject = new GameObject("TheadDispatcher");
                    _instance = dispatcherObject.AddComponent<Dispatcher>();
                    DontDestroyOnLoad(dispatcherObject);
                }
                return _instance;
            }
        }

        private readonly List<AsyncTask> _deadTasks = new List<AsyncTask>();
        private void Update()
        {
            _deadTasks.Clear();
            foreach (AsyncTask threadedTask in ThreadedTasks)
            {
                if (threadedTask.IsFinished)
                {
                    threadedTask.OnTaskFinished();
                    _deadTasks.Add(threadedTask);
                }
            }
            foreach (AsyncTask threadedTask in _deadTasks)
            {
                ThreadedTasks.Remove(threadedTask);
            }

            foreach (KeyValuePair<string, AsyncTask> backgroundTask in BackgroundTasks)
            {
                backgroundTask.Value.OnTaskFinished();
            }
        }

        public void Reset()
        {
            foreach (AsyncTask threadedTask in ThreadedTasks)
            {
                threadedTask.Thread.Abort();
            }
            ThreadedTasks.Clear();

            foreach (KeyValuePair<string, AsyncTask> backgroundTask in BackgroundTasks)
            {
                backgroundTask.Value.Thread.Abort();
            }
            BackgroundTasks.Clear();
        }

        public void RegisterTask(AsyncTask asyncTask)
        {
            ThreadedTasks.Add(asyncTask);
        }

        public bool HasBackgroundTask(string taskName)
        {
            return BackgroundTasks.ContainsKey(taskName);
        }

        public AsyncTask GetBackgroundTask(string taskName)
        {
            AsyncTask asyncTask;
            BackgroundTasks.TryGetValue(taskName, out asyncTask);
            return asyncTask;
        }

        public void RegisterBackgroundTask(string taskName, AsyncTask asyncTask)
        {
            BackgroundTasks.Add(taskName, asyncTask);
        }
    }
}
