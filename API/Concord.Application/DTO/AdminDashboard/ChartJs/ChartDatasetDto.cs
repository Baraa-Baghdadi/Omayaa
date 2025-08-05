using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.AdminDashboard.ChartJs
{
    public class ChartDatasetDto
    {
        public string Label { get; set; }
        public List<object> Data { get; set; } = new List<object>();
        public List<string> BackgroundColor { get; set; } = new List<string>();
        public List<string> BorderColor { get; set; } = new List<string>();
        public int BorderWidth { get; set; } = 1;
    }
}
