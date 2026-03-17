using SPCCSHelpers.CustomMetrics;

namespace SPCCSHelpers.Tests;

public class MetricQueueTests
{
    [Fact]
    public async Task Enqueue_allowsReadingQueuedMetric()
    {
        var queue = new MetricQueue();
        var metric = new CustomMetric { Name = "RequestCount", Value = 1 };

        queue.Enqueue(metric);

        var readMetric = await queue.Reader.ReadAsync();

        Assert.Same(metric, readMetric);
    }

    [Fact]
    public async Task Enqueue_multipleMetrics_areReadInFifoOrder()
    {
        var queue = new MetricQueue();
        var first = new CustomMetric { Name = "First", Value = 1 };
        var second = new CustomMetric { Name = "Second", Value = 2 };

        queue.Enqueue(first);
        queue.Enqueue(second);

        var firstRead = await queue.Reader.ReadAsync();
        var secondRead = await queue.Reader.ReadAsync();

        Assert.Same(first, firstRead);
        Assert.Same(second, secondRead);
    }

    [Fact]
    public async Task Reader_waitsForDataAndThenReturnsQueuedMetric()
    {
        var queue = new MetricQueue();
        var metric = new CustomMetric { Name = "Delayed", Value = 10 };

        var readTask = queue.Reader.ReadAsync().AsTask();
        Assert.False(readTask.IsCompleted);

        queue.Enqueue(metric);

        var readMetric = await readTask;

        Assert.Same(metric, readMetric);
    }

    [Fact]
    public void Reader_isEmptyBeforeAnyMetricIsQueued()
    {
        var queue = new MetricQueue();

        var hasValue = queue.Reader.TryRead(out var metric);

        Assert.False(hasValue);
        Assert.Null(metric);
    }

    [Fact]
    public async Task Enqueue_acceptsNullMetricValue()
    {
        var queue = new MetricQueue();

        queue.Enqueue(null!);

        var readMetric = await queue.Reader.ReadAsync();

        Assert.Null(readMetric);
    }
}