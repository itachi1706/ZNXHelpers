using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.KeyManagementService;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Moq;
using SPCCSHelpers.CustomMetrics;

namespace SPCCSHelpers.Tests.CustomMetrics;

[Collection("EnvironmentVariableDependent")]
public class CloudwatchMetricsPublisherTests
{
    [Fact]
    public async Task StartAsync_withCustomMetricsDisabled_doesNotPublishAndClearsQueue()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "false");
        Environment.SetEnvironmentVariable("METRICS_ALWAYS_INSTANCE_ID", "true");

        var queue = new MetricQueue();
        queue.Enqueue(new CustomMetric { Name = "Requests", Value = 1 });

        var mockCwClient = new Mock<AmazonCloudWatchClient>(RegionEndpoint.APSoutheast1);
        var helper = new AwsHelperV3(
            new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1).Object,
            mockCwClient.Object);

        var publisher = new CloudwatchMetricsPublisher(queue, helper);

        await publisher.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await publisher.StopAsync(CancellationToken.None);

        mockCwClient.Verify(
            x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        var hasQueuedMetric = queue.Reader.TryRead(out var queuedMetric);
        Assert.False(hasQueuedMetric);
        Assert.Null(queuedMetric);
    }

    [Fact]
    public async Task ExecuteAsync_withMetricWithoutDimensions_addsUniqueIdentifierDimension()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "true");
        Environment.SetEnvironmentVariable("METRICS_ALWAYS_INSTANCE_ID", "true");
        Environment.SetEnvironmentVariable("APP_ID", "cw-publisher-instance");

        var queue = new MetricQueue();
        queue.Enqueue(new CustomMetric { Name = "Requests", Value = 3 });

        var mockCwClient = new Mock<AmazonCloudWatchClient>(RegionEndpoint.APSoutheast1);
        PutMetricDataRequest? capturedRequest = null;
        var flushTcs = new TaskCompletionSource<bool>();
        mockCwClient
            .Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutMetricDataRequest, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
                flushTcs.TrySetResult(true);
            })
            .ReturnsAsync(new PutMetricDataResponse());

        var helper = new AwsHelperV3(
            new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1).Object,
            mockCwClient.Object);

        var publisher = new CloudwatchMetricsPublisher(queue, helper);

        await publisher.StartAsync(CancellationToken.None);
        await flushTcs.Task.WaitAsync(TimeSpan.FromSeconds(8));
        await publisher.StopAsync(CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Single(capturedRequest!.MetricData);
        var sentMetric = capturedRequest.MetricData[0];
        Assert.Single(sentMetric.Dimensions);
        Assert.Equal("UniqueIdentifier", sentMetric.Dimensions[0].Name);
        Assert.Equal("cw-publisher-instance", sentMetric.Dimensions[0].Value);
    }

    [Fact]
    public async Task ExecuteAsync_withAlwaysAddInstanceIdDisabled_keepsExistingNonInstanceDimensionsOnly()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "true");
        Environment.SetEnvironmentVariable("METRICS_ALWAYS_INSTANCE_ID", "false");
        Environment.SetEnvironmentVariable("APP_ID", "cw-publisher-instance");

        var queue = new MetricQueue();
        queue.Enqueue(new CustomMetric
        {
            Name = "Latency",
            Value = 150,
            Dimensions =
            [
                new Dimension { Name = "Route", Value = "/health" }
            ]
        });

        var mockCwClient = new Mock<AmazonCloudWatchClient>(RegionEndpoint.APSoutheast1);
        PutMetricDataRequest? capturedRequest = null;
        var flushTcs = new TaskCompletionSource<bool>();
        mockCwClient
            .Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutMetricDataRequest, CancellationToken>((request, _) =>
            {
                capturedRequest = request;
                flushTcs.TrySetResult(true);
            })
            .ReturnsAsync(new PutMetricDataResponse());

        var helper = new AwsHelperV3(
            new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1).Object,
            new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1).Object,
            mockCwClient.Object);

        var publisher = new CloudwatchMetricsPublisher(queue, helper);

        await publisher.StartAsync(CancellationToken.None);
        await flushTcs.Task.WaitAsync(TimeSpan.FromSeconds(8));
        await publisher.StopAsync(CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Single(capturedRequest!.MetricData);
        var sentMetric = capturedRequest.MetricData[0];
        Assert.Single(sentMetric.Dimensions);
        Assert.Equal("Route", sentMetric.Dimensions[0].Name);
        Assert.Equal("/health", sentMetric.Dimensions[0].Value);
        Assert.DoesNotContain(sentMetric.Dimensions, d => d.Name == "UniqueIdentifier");
    }
}