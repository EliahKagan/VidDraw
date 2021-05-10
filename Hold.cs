// Copyright (c) 2021 Eliah Kagan
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

using System.Threading;

namespace VidDraw {
    /// <summary>RAII-style lock guard for (interprocess) mutexes.</summary>
    /// <remarks>
    /// Acquiring the mutex due to another thread/process abandoning it is not
    /// considered an error.
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
