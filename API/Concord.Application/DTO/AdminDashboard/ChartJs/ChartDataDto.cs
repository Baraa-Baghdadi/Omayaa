namespace Concord.Application.DTO.AdminDashboard.ChartJs
{
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDatasetDto> Datasets { get; set; } = new List<ChartDatasetDto>();
    }
}
