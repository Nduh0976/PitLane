using System.Collections.Concurrent;

namespace PitLane.Core.Concurrency;

/// <summary>
/// Collapsesd concurrent requests for the same key into one piece of work. If two runs ask for the
/// same commit at the same moment, the repository is downlaoded once and both wait on that download.
/// 
/// The shared work must not use any one caller's CancellationToken. If the first caller cancels,
/// the others would fail too. Caller pass their owen token only to stop *waiting*.
/// </summary>
public sealed class SingleFlight<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _inFlight = new();

    public bool IsInFlight(TKey key) => _inFlight.ContainsKey(key);

    public Task<TValue> RunAsync(TKey key, Func<Task<TValue>> work, CancellationToken waitToken = default)
    {
        Lazy<Task<TValue>> mine = null!;
        mine = new Lazy<Task<TValue>>(() => ExecuteAsync(key, mine, work));

        // GetOrAdd may race, but only the instance that ends up in the dictionary is ever evaluated,
        // because  we always call .Value on what GetOrAdd returned.
        var shared = _inFlight.GetOrAdd(key, mine);
        return shared.Value.WaitAsync(waitToken);
    }

    private async Task<TValue> ExecuteAsync(TKey key, Lazy<Task<TValue>> self, Func<Task<TValue>> work)
    {
        try
        {
            return await work().ConfigureAwait(false);
        }
        finally
        {
            // Remove only our own entry. Successful results are cached elsewhere (on disk), and a
            // failure must not be remembered, so the next request get a fresh attempt.

            _inFlight.TryRemove(new KeyValuePair<TKey, Lazy<Task<TValue>>>(key, self));
        }
    }
}
