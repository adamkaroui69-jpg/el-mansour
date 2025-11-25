namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class House : BaseEntity
    {
        public string HouseCode { get; set; } = string.Empty;
        public string BuildingCode { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal MonthlyAmount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
