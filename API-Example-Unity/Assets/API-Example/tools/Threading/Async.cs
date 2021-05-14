using System;
using System.Collections.Generic;

namespace Threading
{
    public class Async
    {
        private static Dispatcher Dispatcher
        {
            get { return Dispatcher.Instance; }
        }

        private static readonly Dictionary<string, object> Locks = new Dictionary<string, object>();

        public static AsyncTask Run(Action action)
        {
            AsyncTask asyncTask = new AsyncTask(action);
            Dispatcher.RegisterTask(asyncTask);
            return asyncTask;
        }

        public static AsyncTask<T> Run<T>(Func<T> func)
        {
            AsyncTask<T> asyncTask = new AsyncTask<T>(func);
            Dispatcher.RegisterTask(asyncTask);
            return asyncTask;
        }

        public static AsyncTask RunInBackground(string taskName, int runFrequencyMs, Action action)
        {
            if (Dispatcher.HasBackgroundTask(taskName) == false)
            {
                AsyncTask asyncTask = new AsyncTask(action, runFrequencyMs);
                Dispatcher.RegisterBackgroundTask(taskName, asyncTask);
                return asyncTask;
            }
            return Dispatcher.GetBackgroundTask(taskName);
        }

        public static AsyncTask<T> RunInBackground<T>(string taskName, int runFrequencyMs, Func<T> func)
        {
            if (Dispatcher.HasBackgroundTask(taskName) == false)
            {
                AsyncTask<T> asyncTask = new AsyncTask<T>(func, runFrequencyMs);
                Dispatcher.RegisterBackgroundTask(taskName, asyncTask);
                return asyncTask;
            }
            var genericAsyncTask = Dispatcher.GetBackgroundTask(taskName) as AsyncTask<T>;
            if (genericAsyncTask == null)
            {
                throw new InvalidOperationException("Cannot find requested generic AsyncTask with name " + taskName);
            }
            return genericAsyncTask;
        }

        public static object GetLock(string key)
        {
            object lockObj;
            Locks.TryGetValue(key, out lockObj);

            if (lockObj == null)
            {
                lockObj = new object();
                Locks.Add(key, lockObj);
            }

            return lockObj;
        }
    }
}