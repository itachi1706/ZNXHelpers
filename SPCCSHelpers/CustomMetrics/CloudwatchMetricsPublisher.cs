using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SPCCSHelpers.CustomMetrics;

// ReSharper disable once UnusedType.Global

/// <summary>
/// The background service that will push the metrics up to Cloudwatch. Initialize with
/// <code>
///builder.Services.AddSingleton&lt;AwsHelperV3&gt;();
/// builder.Services.AddHostedService&lt;CloudWatchMetricPublisher&gt;();
/// </code>
/// </summary>
public class CloudwatchMetricsPublisher(MetricQueue queue, AwsHelperV3 awsHelper) : BackgroundService
{
    private readonly bool _verboseLogEnabled = EnvHelper.GetBool("METRICS_VERBOSE_LOGGING", false);

    private readonly ILogger _logger = Log.ForContext<CloudwatchMetricsPublisher>();

    // AWS Limit (Ref: https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_PutMetricData.html)
    private const int MaxBatchSize = 1000;

    private void VerboseLog(string? log)
    {
        if (log == null) return; // NO-OP
        if (_verboseLogEnabled)
        {
            _logger.Debug("{Log}", log);
        }
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Cloudwatch Metrics Publisher started");
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        var batch = new List<MetricDatum>();

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            VerboseLog("Checking for metrics in queue...");
            // Drain queue
            while (queue.Reader.TryRead(out var metricData))
            {
                if (metricData.Dimensions.Count < 1)
                {
                    // Add default dimension
                    metricData.Dimensions.Add(new Dimension
                    {
                        Name = "UniqueIdentifier",
                        Value = AwsHelperV3.GetUniqueInstanceName()
                    });
                }

                batch.Add(new MetricDatum
                {
                    MetricName = metricData.Name,
                    Value = metricData.Value,
                    Dimensions = metricData.Dimensions,
                    Unit = metricData.Unit
                });

                if (batch.Count >= MaxBatchSize)
                {
                    VerboseLog($"Batch size limit reached. Flushing batch of {batch.Count} metrics...");
                    await FlushBatchAsync(batch);
                }
            }

            VerboseLog($"Batch size: {batch.Count}");
            if (batch.Count > 0)
            {
                VerboseLog("Flushing batch...");
                await FlushBatchAsync(batch);
            }

            VerboseLog("Batch flushed. Waiting for next tick...");
        }
    }

    private async Task FlushBatchAsync(List<MetricDatum> batch)
    {
        VerboseLog($"Flushing {batch.Count} metrics to CloudWatch...");
        try
        {
            await awsHelper.PushMetric(batch.ToList());
            batch.Clear();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to push metrics to CloudWatch. Discarding metrics... {Message}", e.Message);
            batch.Clear();
        }
    }
}