using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace SPCCSHelpers.CustomMetrics;

public class CustomMetric
{
    /// <summary>
    /// Name of metric. Required
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value of metric. Required
    /// </summary>
    public double Value { get; set; }

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