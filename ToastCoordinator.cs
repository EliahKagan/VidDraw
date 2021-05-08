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

using System;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    /// <summary>
    /// Coordinates toast notification initialization and cleanup/uninstall.
    /// </summary>
    internal sealed class ToastCoordinator : IDisposable {
        /// <summary>
        /// Creates a ToastCoordinator if toasts are supported.
        /// </summary>
        internal static ToastCoordinator? TryCreate(OnActivated onActivated)
            => Platform.CanToast ? new(onActivated) : null;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static string CheckerName => $"checker-{Platform.XStyleArch}";

        private ToastCoordinator(OnActivated onActivated)
        {
            using (var hold = new Hold(_locker))
                _checker = Sync.CreateMutex(CheckerName);

            ToastNotificationManagerCompat.OnActivated += onActivated;
        }

        private void Dispose(bool disposing)
        {
            if (_checker is not null) {
                using var hold = new Hold(_locker);

                _checker.Dispose();
                _checker = null; // In case re-creation throws an exception.

                _checker = Sync.CreateMutex(CheckerName, out var createdNew);
                if (createdNew) ToastNotificationManagerCompat.Uninstall();

                // The OS cleans up mutex handles, so this isn't really needed
                // for synchronization, though it does prevent cleanup from
                // being attempted twice.
                _checker.Dispose();
                _checker = null;
            }

            if (disposing) _locker.Dispose();
        }

        ~ToastCoordinator() => Dispose(false);

        private readonly Mutex _locker = Sync.CreateMutex("main");

        private Mutex? _checker;
    }
}
