using System;
using System.Collections.Generic;

namespace pwa_camera_poc_blazor.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<int> UnitIds { get; set; } = new();
        public int? CurrentUnitId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
