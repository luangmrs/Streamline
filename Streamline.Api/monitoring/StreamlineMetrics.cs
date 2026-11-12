using Prometheus;

namespace Streamline.API.Monitoring;

public static class StreamlineMetrics
{
    public static readonly Counter OrderListRequests = Metrics.CreateCounter(
        "streamline_order_list_requests_total",
        "Total successful requests to list Streamline orders.");
}
