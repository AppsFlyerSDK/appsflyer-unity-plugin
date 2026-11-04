#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AppsFlyerSDK
{
    /// <summary>
    /// Runs Android JNI calls that need to block for a real result (awaitResponse: true, via
    /// QueryAsync) on a single, long-lived, explicitly JNI-attached background thread - instead
    /// of Unity's main game-loop thread (ANR risk: a slow/stalled server round trip freezes
    /// Update()/rendering) or Awaitable.BackgroundThreadAsync()'s transient thread-pool workers
    /// (those aren't guaranteed to have an attached JNI environment and caused the "Empty
    /// response from native" failures under headless Run In Background - see QueryAsync).
    /// One dedicated OS thread, attached once, reused for every call, sidesteps both problems
    /// without touching the native bridge/protocol at all.
    /// </summary>
    internal static class AppsFlyerJniWorker
    {
        private static readonly BlockingCollection<Action> workQueue = new BlockingCollection<Action>();
        private static int started;

        private static void EnsureStarted()
        {
            if (Interlocked.CompareExchange(ref started, 1, 0) != 0) return;
            var thread = new Thread(RunLoop) { IsBackground = true, Name = "AppsFlyerJniWorker" };
            thread.Start();
        }

        private static void RunLoop()
        {
            AndroidJNI.AttachCurrentThread();
            try
            {
                foreach (var work in workQueue.GetConsumingEnumerable())
                {
                    work();
                }
            }
            finally
            {
                AndroidJNI.DetachCurrentThread();
            }
        }

        /// <summary>Runs <paramref name="work"/> on the dedicated JNI thread and returns its result.</summary>
        public static Task<object> EnqueueExecute(Func<object> work)
        {
            EnsureStarted();
            var tcs = new TaskCompletionSource<object>();
            workQueue.Add(() =>
            {
                try { tcs.SetResult(work()); }
                catch (Exception e) { tcs.SetException(e); }
            });
            return tcs.Task;
        }
    }
}
#endif
