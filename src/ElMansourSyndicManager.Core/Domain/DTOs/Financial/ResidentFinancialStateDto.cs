using System.Collections.Generic;

namespace ElMansourSyndicManager.Core.Domain.DTOs.Financial
{
    public class ResidentFinancialStateDto
    {
        public string HouseCode { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal TotalDue { get; set; }
        public decimal TotalPaid { get; set; }
        /// <summary>
        /// Solde du compte. Positif = En règle/Avance. Négatif = Arriérés.
        /// </summary>
        public decimal Balance { get; set; }
        public int UnpaidMonthsCount { get; set; }
        public List<UnpaidMonthDto> UnpaidMonths { get; set; } = new List<UnpaidMonthDto>();
        public string StatusColor { get; set; } = "#4CAF50"; // Green default
    }
}
