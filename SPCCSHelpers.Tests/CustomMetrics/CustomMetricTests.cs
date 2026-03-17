using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using SPCCSHelpers.CustomMetrics;

namespace SPCCSHelpers.Tests;

public class CustomMetricTests
{
    [Fact]
    public void CustomMetric_initializesWithProvidedRequiredValues()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 42
        };

        Assert.Equal("RequestCount", metric.Name);
        Assert.Equal(42, metric.Value);
    }

    [Fact]
    public void CustomMetric_defaultsDimensionsToEmptyList()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };

        Assert.NotNull(metric.Dimensions);
        Assert.Empty(metric.Dimensions);
    }

    [Fact]
    public void CustomMetric_defaultsUnitToCount()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };

        Assert.Equal(StandardUnit.Count, metric.Unit);
    }

    [Fact]
    public void CustomMetric_defaultsTimestampToCurrentUtcTime()
    {
        var before = DateTime.UtcNow;

        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };

        var after = DateTime.UtcNow;

        Assert.Equal(DateTimeKind.Utc, metric.Timestamp.Kind);
        Assert.InRange(metric.Timestamp, before, after);
    }

    [Fact]
    public void CustomMetric_usesIndependentDimensionListsAcrossInstances()
    {
        var first = new CustomMetric
        {
            Name = "MetricOne",
            Value = 1
        };
        var second = new CustomMetric
        {
            Name = "MetricTwo",
            Value = 2
        };

        first.Dimensions.Add(new Dimension { Name = "Region", Value = "ap-southeast-1" });

        Assert.Single(first.Dimensions);
        Assert.Empty(second.Dimensions);
    }

    [Fact]
    public void CustomMetric_nameSetter_updatesName()
    {
        var metric = new CustomMetric
        {
            Name = "Initial",
            Value = 1
        };

        metric.Name = "Updated";

        Assert.Equal("Updated", metric.Name);
    }

    [Fact]
    public void CustomMetric_valueSetter_updatesValue()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };

        metric.Value = -12.5;

        Assert.Equal(-12.5, metric.Value);
    }

    [Fact]
    public void CustomMetric_dimensionsSetter_replacesDimensionsCollection()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };
        var replacement = new List<Dimension>
        {
            new() { Name = "Service", Value = "api" }
        };

        metric.Dimensions = replacement;

        Assert.Same(replacement, metric.Dimensions);
        Assert.Single(metric.Dimensions);
    }

    [Fact]
    public void CustomMetric_unitSetter_updatesUnit()
    {
        var metric = new CustomMetric
        {
            Name = "Latency",
            Value = 1
        };

        metric.Unit = StandardUnit.Milliseconds;

        Assert.Equal(StandardUnit.Milliseconds, metric.Unit);
    }

    [Fact]
    public void CustomMetric_timestampSetter_updatesTimestamp()
    {
        var metric = new CustomMetric
        {
            Name = "RequestCount",
            Value = 1
        };
        var timestamp = new DateTime(2026, 03, 17, 10, 15, 00, DateTimeKind.Utc);

        metric.Timestamp = timestamp;

        Assert.Equal(timestamp, metric.Timestamp);
    }
}