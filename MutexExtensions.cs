using System.Threading;

namespace VidDraw {
    /// <summary>Extensions for mutexes.</summary>
    internal static class MutexExtensions {
        /// <summary>Acquires the mutex, waiting if necessary.</summary>
        /// <remarks>
        /// Like <see cref="WaitHandle.WaitOne()"/> but also returns (rather
        /// than throwing an <see cref="AbandonedMutexException"/>) when the
        /// mutex becomes available due to termination of the threading that
        /// held it.
        /// </remarks>
        /// <param name="mutex">The mutex to acquire.</param>
        internal static void Acquire(this Mutex mutex)
        {
            try {
                mutex.WaitOne();
            } catch (AbandonedMutexException) {
                // Also consider this success (as the thread holds the mutex).
            }
        }
    }
}
