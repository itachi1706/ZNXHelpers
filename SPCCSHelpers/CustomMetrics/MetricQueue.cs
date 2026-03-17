using System.Threading.Channels;

namespace SPCCSHelpers.CustomMetrics;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Queue for processing metrics. Initialize by calling the following to ensure all requests uses the same queue
/// <code>
/// builder.Services.AddSingleton&lt;MetricQueue&gt;();
/// </code>
/// </summary>
public class MetricQueue
{
    private readonly Channel<CustomMetric> _metricsQueue = Channel.CreateUnbounded<CustomMetric>();

    /// <summary>
    /// Use this function to queue metric to be published to cloudwatch
    /// </summary>
    /// <param name="customMetric">Metric Object</param>
    public void Enqueue(CustomMetric customMetric)
    {
        // TryWrite because it is okay to fail. We just do not want it to block execution
        _metricsQueue.Writer.TryWrite(customMetric);
    }

    public ChannelReader<CustomMetric> Reader => _metricsQueue.Reader;
}