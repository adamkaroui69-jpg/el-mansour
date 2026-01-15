using ElMansourSyndicManager.Core.Domain.DTOs.Financial;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IFinancialService
    {
        Task<ResidentFinancialStateDto?> GetResidentFinancialStateAsync(string houseCode);
        Task<IEnumerable<ResidentFinancialStateDto>> GetAllResidentsFinancialStateAsync();
        
        /// <summary>
        /// Calcule le montant total des arriérés pour l'ensemble de la résidence.
        /// </summary>
        Task<decimal> GetTotalArrearsAsync();
    }
}
