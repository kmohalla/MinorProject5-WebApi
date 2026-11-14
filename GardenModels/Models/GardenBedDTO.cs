namespace GardenModels
{
    public class GardenBedDTO
    {
        public int ID { get; set; }
        public string BedName { get; set; } = "";
        public string BedState { get; set; } = "";
        public DateTime? DatePlanted { get; set; }
        public String? GuideTitle { get; set; }
        public String? PlantName { get; set; }
        public String? ToolName { get; set; }
    }
}
