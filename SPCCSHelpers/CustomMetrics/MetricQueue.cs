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
    private readonly ILogger _logger = Log.ForContext<MetricQueue>();

    /// <summary>
    /// Use this function to queue metric to be published to cloudwatch
    /// </summary>
    /// <param name="customMetric">Metric Object</param>
    public void Enqueue(CustomMetric customMetric)
    {
        if (_awsCustomMetrics)
        {
            _logger.Debug("Attempting to enqueue custom metric");
            // TryWrite because it is okay to fail. We just do not want it to block execution
            _metricsQueue.Writer.TryWrite(customMetric);
        }
        else
        {
            _logger.Debug("Not adding to queue as metrics is disabled");
        }
        
    }

    public ChannelReader<CustomMetric> Reader => _metricsQueue.Reader;
}