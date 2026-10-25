using PitLane.Core.Concurrency;

namespace PitLane.Tests;

public class SingleFlightTests
{
    [Fact]
    public async Task Concurrent_callers_share_one_execution()
    {
        var flight = new SingleFlight<string, int>();
        var executions = 0;
        var release = new TaskCompletionSource();

        async Task<int> Work()
        {
            Interlocked.Increment(ref executions);
            await release.Task;
            return 42;
        }

        var callers = Enumerable.Range(0, 20).Select(_ => flight.RunAsync("key", Work)).ToArray();
        Assert.True(flight.IsInFlight("key"));
        release.SetResult();

        var results = await Task.WhenAll(callers);
        Assert.All(results, r => Assert.Equal(42, r));
        Assert.Equal(1, executions);
        Assert.False(flight.IsInFlight("key"));
    }

    [Fact]
    public async Task Failures_are_not_remembered()
    {
        var flight = new SingleFlight<string, int>();
        var attempts = 0;

        Task<int> Work() => ++attempts == 1 ? throw new InvalidOperationException("first try fails") : Task.FromResult(7);

        await Assert.ThrowsAsync<InvalidOperationException>(() => flight.RunAsync("key", Work));
        Assert.Equal(7, await flight.RunAsync("key", Work));
     }

    [Fact]
    public async Task One_caller_giving_up_does_not_cancel_the_shared_work()
    {
        var flight = new SingleFlight<string, int>();
        var release = new TaskCompletionSource();
        using var impatient = new CancellationTokenSource();

        var first = flight.RunAsync("key", async () => { await release.Task; return 1; }, impatient.Token);
        var second = flight.RunAsync("key", () => Task.FromResult(2));

        impatient.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

        release.SetResult();
        Assert.Equal(1, await second); // The second caller joined the first execution and still gets its result.
    }
}
