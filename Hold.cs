using System.Threading;

namespace VidDraw {
    /// <summary>RAII-style lock guard for (interprocess) mutexes.</summary>
    /// <remarks>
    /// Acquiring the mutex due to another thread/process abandoning it is not
    /// conisdered an error.
    /// </remarks>
    internal readonly ref struct Hold {
        /// <summary>Acquires the mutex, waiting if necessary.</summary>
        /// <remarks>
        /// Like <see cref="WaitHandle.WaitOne()"/> but also returns (rather
        /// than throwing an <see cref="AbandonedMutexException"/>) when the
        /// mutex becomes available due to termination of the threading that
        /// held it.
        /// </remarks>
        /// <param name="mutex">The mutex to acquire.</param>
        internal Hold(Mutex mutex)
        {
            try {
                mutex.WaitOne();
            } catch (AbandonedMutexException) {
                // Also consider this success (as the thread holds the mutex).
            }

            _mutex = mutex;
        }

        internal void Dispose() => _mutex.ReleaseMutex();

        private readonly Mutex _mutex;
    }
}
