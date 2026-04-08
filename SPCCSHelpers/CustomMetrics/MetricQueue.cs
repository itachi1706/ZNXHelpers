using System.Threading.Channels;
using Serilog;

namespace SPCCSHelpers.CustomMetrics;

/// <summary>
/// Queue for processing metrics. Initialize by calling the following to ensure all requests uses the same queue
/// <code>
/// builder.Services.AddSingleton&lt;MetricQueue&gt;();
/// </code>
/// </summary>
public class MetricQueue
{
    private readonly Channel<CustomMetric> _metricsQueue = Channel.CreateUnbounded<CustomMetric>();
    
    private readonly bool _awsCustomMetrics = EnvHelper.GetBool("AWS_CUSTOM_METRICS", false);

    /// <summary>
    /// Use this function to queue metric to be published to cloudwatch
    /// Note: This will only run if AWS_CUSTOM_METRICS is enabled
    /// </summary>
    /// <param name="customMetric">Metric Object</param>
    public void Enqueue(CustomMetric customMetric)
    {
        if (_awsCustomMetrics)
        {
            // TryWrite because it is okay to fail. We just do not want it to block execution
            _metricsQueue.Writer.TryWrite(customMetric);
        }
    }

    public ChannelReader<CustomMetric> Reader => _metricsQueue.Reader;
}