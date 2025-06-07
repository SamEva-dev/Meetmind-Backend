
namespace Meetmind.Application.Dto;

public class StorageStatsDto
{
    public double UsedGB { get; set; }
    public double UsedMB { get; set; }
    public long UsedBytes { get; set; }
    public double UsagePercent { get; set; }
    public double DiskUsedGB { get; set; }
    public double DiskFreeGB { get; set; }
    public double DiskTotalGB { get; set; }
}