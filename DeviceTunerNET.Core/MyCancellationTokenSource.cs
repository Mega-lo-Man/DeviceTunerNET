using System.Threading;

namespace DeviceTunerNET.Core
{
    public class MyCancellationTokenSource : CancellationTokenSource
    {
        public bool IsDisposed { get; private set; }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }
}
