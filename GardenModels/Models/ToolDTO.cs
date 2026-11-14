namespace GardenModels
{
    public class ToolDTO
    {
        public int ID { get; set; }
        public decimal ToolWeightMetricKg { get; set; }
        public decimal ToolWeightImperialLbs { get; set; }
        public string ToolName { get; set; } = "";
        public string Material { get; set; } = "";
        public int ToolLengthMetricCm { get; set; }
        public int ToolLengthImperialIn { get; set; }
    }
}
