namespace pwa_camera_poc_blazor.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class City
    {
        public int Id { get; set; }
        public string StateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class State
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
