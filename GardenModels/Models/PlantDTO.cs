namespace GardenModels
{
    public class PlantDTO
    {
        public int ID { get; set; }
        public string CommonName { get; set; } = "";
        public string Family { get; set; } = "";
        public string Type { get; set; } = "";
        public string Color { get; set; } = "";
        public String? SourceName { get; set; }
    }
}
