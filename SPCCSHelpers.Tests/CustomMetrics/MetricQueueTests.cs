using SPCCSHelpers.CustomMetrics;

namespace SPCCSHelpers.Tests.CustomMetrics;

[Collection("EnvironmentVariableDependent")]
public class MetricQueueTests
{
    private static IDisposable SetAwsCustomMetrics(string value)
    {
        return new EnvironmentVariableScope("AWS_CUSTOM_METRICS", value);
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _key;
        private readonly string? _originalValue;

        public EnvironmentVariableScope(string key, string value)
        {
            _key = key;
            _originalValue = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_key, _originalValue);
        }
    }

    [Fact]
    public async Task Enqueue_withCustomMetricsEnabled_allowsReadingQueuedMetric()
    {
        using var _ = SetAwsCustomMetrics("true");
        var queue = new MetricQueue();
        var metric = new CustomMetric { Name = "RequestCount", Value = 1 };

        queue.Enqueue(metric);

        var readMetric = await queue.Reader.ReadAsync();

        Assert.Same(metric, readMetric);
    }

    [Fact]
    public async Task Enqueue_withCustomMetricsEnabled_multipleMetricsAreReadInFifoOrder()
    {
        using var _ = SetAwsCustomMetrics("true");
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
    public async Task Reader_withCustomMetricsEnabled_waitsForDataAndThenReturnsQueuedMetric()
    {
        using var _ = SetAwsCustomMetrics("true");
        var queue = new MetricQueue();
        var metric = new CustomMetric { Name = "Delayed", Value = 10 };

        var readTask = queue.Reader.ReadAsync().AsTask();
        Assert.False(readTask.IsCompleted);

        queue.Enqueue(metric);

        var readMetric = await readTask;

        Assert.Same(metric, readMetric);
    }

    [Fact]
    public void Reader_withCustomMetricsEnabled_isEmptyBeforeAnyMetricIsQueued()
    {
        using var _ = SetAwsCustomMetrics("true");
        var queue = new MetricQueue();

        var hasValue = queue.Reader.TryRead(out var metric);

        Assert.False(hasValue);
        Assert.Null(metric);
    }

    [Fact]
    public async Task Enqueue_withCustomMetricsEnabled_acceptsNullMetricValue()
    {
        using var _ = SetAwsCustomMetrics("true");
        var queue = new MetricQueue();

        queue.Enqueue(null!);

        var readMetric = await queue.Reader.ReadAsync();

        Assert.Null(readMetric);
    }

    [Fact]
    public void Enqueue_withCustomMetricsDisabled_doesNotQueueMetric()
    {
        using var _ = SetAwsCustomMetrics("false");
        var queue = new MetricQueue();

        queue.Enqueue(new CustomMetric { Name = "Suppressed", Value = 1 });

        var hasValue = queue.Reader.TryRead(out var metric);

        Assert.False(hasValue);
        Assert.Null(metric);
    }
}