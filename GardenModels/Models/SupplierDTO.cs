namespace GardenModels
{
    public class SupplierDTO
    {
        public int ID { get; set; }
        public string Supplier { get; set; } = "";
        public string Location { get; set; } = "";
        public List<String> Plants { get; set; } = new();
    }

}
