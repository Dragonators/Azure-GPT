using System.Collections.Concurrent;

namespace OpenAi_API
{
    public class CancelSource
    {
        public static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }
}
