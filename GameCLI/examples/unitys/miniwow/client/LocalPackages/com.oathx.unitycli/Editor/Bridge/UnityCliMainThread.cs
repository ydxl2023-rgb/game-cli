using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Dispatches bridge work from the named-pipe listener thread onto the Unity editor thread.
    /// </summary>
    internal static class UnityCliMainThread
    {
        /// <summary>
        /// Protects the queued editor-thread work.
        /// </summary>
        static readonly object QueueLock = new object();
        /// <summary>
        /// Work items waiting for the next editor update tick.
        /// </summary>
        static readonly Queue<Action> Queue = new Queue<Action>();
        /// <summary>
        /// Managed thread id captured from the Unity editor thread.
        /// </summary>
        static int mainThreadId;
        /// <summary>
        /// Tracks whether the editor update callback is currently registered.
        /// </summary>
        static bool updateHookRegistered;

        /// <summary>
        /// Captures the current thread as the Unity main thread and starts pumping queued work.
        /// </summary>
        internal static void Initialize()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            if (updateHookRegistered)
            {
                return;
            }

            updateHookRegistered = true;
            EditorApplication.update += ExecuteQueuedWork;
        }

        /// <summary>
        /// Stops pumping queued work during bridge shutdown.
        /// </summary>
        internal static void Shutdown()
        {
            if (!updateHookRegistered)
            {
                return;
            }

            updateHookRegistered = false;
            EditorApplication.update -= ExecuteQueuedWork;
        }

        /// <summary>
        /// Runs a delegate on the Unity editor thread and returns its result.
        /// </summary>
        /// <typeparam name="T">The delegate result type.</typeparam>
        /// <param name="work">The work to run on the editor thread.</param>
        /// <returns>A task that completes with the delegate result.</returns>
        internal static Task<T> RunOnMainThreadAsync<T>(Func<T> work)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
            {
                return Task.FromResult(work());
            }

            var completion = new TaskCompletionSource<T>();
            lock (QueueLock)
            {
                Queue.Enqueue(() =>
                {
                    try
                    {
                        completion.SetResult(work());
                    }
                    catch (Exception exception)
                    {
                        completion.SetException(exception);
                    }
                });
            }

            return completion.Task;
        }

        /// <summary>
        /// Executes all work queued by bridge commands.
        /// </summary>
        static void ExecuteQueuedWork()
        {
            while (true)
            {
                Action action;
                lock (QueueLock)
                {
                    if (Queue.Count == 0)
                    {
                        return;
                    }

                    action = Queue.Dequeue();
                }

                action();
            }
        }
    }
}
