using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Static
{
    public class NotificationDto
    {
        public string Type { get; set; } // success, warning, info, danger
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionText { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Icon => Type switch
        {
            "warning" => "fas fa-exclamation-triangle",
            "success" => "fas fa-check-circle",
            "info" => "fas fa-info-circle",
            "danger" => "fas fa-times-circle",
            _ => "fas fa-bell"
        };
    }
}
