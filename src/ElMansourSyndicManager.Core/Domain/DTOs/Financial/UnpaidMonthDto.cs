namespace ElMansourSyndicManager.Core.Domain.DTOs.Financial
{
    public class UnpaidMonthDto
    {
        public string Month { get; set; } = string.Empty; // YYYY-MM
        public decimal Amount { get; set; }
        public int DaysLate { get; set; }
        public decimal PenaltyAmount { get; set; } // Optionnel
    }
}
