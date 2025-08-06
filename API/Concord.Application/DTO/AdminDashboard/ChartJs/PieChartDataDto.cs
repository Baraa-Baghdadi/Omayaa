using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.AdminDashboard.ChartJs
{
    public class PieChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> BackgroundColors { get; set; } = new List<string>();
    }
}
