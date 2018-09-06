namespace PackageUpdater
{
    using System;
    using System.Diagnostics;

    internal static class ProcessExt
    {
        internal static void OnExit(this Process process, Action action)
        {
            // Kept alive by the subscription
            _ = new Listener(process, action);
        }

        private class Listener
        {
            private readonly Process process;
            private readonly Action action;

            public Listener(Process process, Action action)
            {
                this.process = process;
                this.action = action;
                this.process.Exited += this.OnExit;
            }

            private void OnExit(object sender, EventArgs e)
            {
                this.process.Exited -= this.OnExit;
                this.action();
            }
        }
    }
}