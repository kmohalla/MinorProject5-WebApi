namespace GardenModels
{
    public class GuideDTO
    {
        public int ID { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string PlantingSeason { get; set; } = "";
        public string Difficulty { get; set; } = "";
    }
}
