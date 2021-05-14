using System;
using System.Collections.Generic;
using System.Threading;

namespace Threading
{
    public class AsyncTask
    {
        public Thread Thread { get; protected set; }

        protected readonly object LockObject = new object();
        private readonly Queue<Action> _callBackQueue;

        private Queue<Action> CallBackQueue
        {
            get
            {
                lock (LockObject)
                {
                    return _callBackQueue;
                }
            }
        }

        protected AsyncTask() { }

        public AsyncTask(Action action)
        {
            Thread thread = new Thread(() =>
            {
                action();
            });
            Thread = thread;
            Thread.Start();
        }

        public AsyncTask(Action action, int runFrequencyMs)
        {
            _callBackQueue = new Queue<Action>();
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    action();
                    CallBackQueue.Enqueue(action);
                    Thread.Sleep(runFrequencyMs);
                }
            });
            Thread = thread;
            Thread.Start();
        }

        public bool IsFinished
        {
            get { return Thread.IsAlive == false; }
        }

        protected virtual bool IsBackgroundTask
        {
            get { return CallBackQueue != null; }
        }

        public void ContinueInMainThread(Action action)
        {
            _onTaskFinished = action;
        }

        private Action _onTaskFinished;
        public virtual void OnTaskFinished()
        {
            if (_onTaskFinished != null)
            {
                if (IsBackgroundTask)
                {
                    while (CallBackQueue.Count > 0)
                    {
                        CallBackQueue.Dequeue();
                        _onTaskFinished();
                    }
                }
                else
                {
                    _onTaskFinished();
                }
            }
        }
    }

    public class AsyncTask<T> : AsyncTask
    {
        public T Result { get; private set; }

        private readonly Queue<T> _backgroundResults;

        private Queue<T> BackgroundResults
        {
            get
            {
                lock (LockObject)
                {
                    return _backgroundResults;
                }
            }
        }

        private Action<T> _onTaskFinishedResult;

        protected override bool IsBackgroundTask
        {
            get { return BackgroundResults != null; }
        }

        public AsyncTask(Func<T> func)
        {
            Thread thread = new Thread(() =>
            {
                Result = func();
            });
            Thread = thread;
            Thread.Start();
        }

        public AsyncTask(Func<T> func, int runFrequencyMs)
        {
            _backgroundResults = new Queue<T>();
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    BackgroundResults.Enqueue(func());
                    Thread.Sleep(runFrequencyMs);
                }
            });
            Thread = thread;
            Thread.Start();
        }

        public void ContinueInMainThread(Action<T> action)
        {
            _onTaskFinishedResult = action;
        }

        public override void OnTaskFinished()
        {
            if (_onTaskFinishedResult != null)
            {
                if (IsBackgroundTask)
                {
                    while (BackgroundResults.Count > 0)
                    {
                        T result = BackgroundResults.Dequeue();
                        _onTaskFinishedResult(result);
                    }
                }
                else
                {
                    _onTaskFinishedResult(Result);
                }
            }
            else base.OnTaskFinished();
        }
    }
}