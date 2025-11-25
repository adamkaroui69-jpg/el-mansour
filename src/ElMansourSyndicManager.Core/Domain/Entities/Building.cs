namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Building : BaseEntity
    {
        public string BuildingCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int NumberOfFloors { get; set; }
        public int NumberOfApartments { get; set; }
    }
}
