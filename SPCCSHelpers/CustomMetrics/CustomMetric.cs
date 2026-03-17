using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace SPCCSHelpers.CustomMetrics;

// ReSharper disable once ClassNeverInstantiated.Global

public class CustomMetric
{
    /// <summary>
    /// Name of metric. Required
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Value of metric. Required
    /// </summary>
    public required double Value { get; set; }

    /// <summary>
    /// Dimensions of metric. Optional
    /// If not specified, unique instance id will be obtained
    /// </summary>
    public List<Dimension> Dimensions { get; set; } = new();

    /// <summary>
    /// Unit of metric measurement, defaults to Count
    /// </summary>
    public StandardUnit Unit { get; set; } = StandardUnit.Count;
}