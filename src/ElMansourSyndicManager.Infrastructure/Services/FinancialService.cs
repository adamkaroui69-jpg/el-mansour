using ElMansourSyndicManager.Core.Domain.DTOs.Financial;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;

        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResidentFinancialStateDto?> GetResidentFinancialStateAsync(string houseCode)
        {
            var house = await _context.Houses.FirstOrDefaultAsync(h => h.HouseCode == houseCode);
            if (house == null) return null;

            var payments = await _context.Payments
                .Where(p => p.HouseCode == houseCode && p.Status == "Paid")
                .ToListAsync();

            var totalPaid = payments.Sum(p => p.Amount);
            
            // Calcul théorique
            // Nous commençons le calcul à partir de la date de création de l'enregistrement maison.
            // TODO: Dans le futur, ajouter un champ 'MembershipStartDate' à House pour être plus précis.
            var startDate = house.CreatedAt; 
            
            // Normaliser au 1er du mois pour simplifier
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            var endDate = DateTime.Today;
            
            // Ne pas aller au-delà du mois courant pour le dû
            if (endDate < startDate) endDate = startDate;

            decimal totalDue = 0;
            var unpaidMonths = new List<UnpaidMonthDto>();
            
            // Algorithme FIFO : Les paiements couvrent les mois les plus anciens en premier.
            decimal remainingPaymentMoney = totalPaid; 

            var currentDate = startDate;
            
            // Boucle mois par mois jusqu'à aujourd'hui
            while (currentDate <= endDate)
            {
                // Ignorer les mois futurs si endDate est dans le futur (pas possible avec DateTime.Today mais bon)
                if (currentDate > DateTime.Today) break;

                var monthAmount = house.MonthlyAmount;
                
                // Si cotisation définie
                if (monthAmount > 0)
                {
                    totalDue += monthAmount;

                    if (remainingPaymentMoney >= monthAmount)
                    {
                        // Ce mois est intégralement couvert
                        remainingPaymentMoney -= monthAmount;
                    }
                    else
                    {
                        // Ce mois est impayé ou partiellement payé
                        var coveredAmount = remainingPaymentMoney > 0 ? remainingPaymentMoney : 0;
                        remainingPaymentMoney = 0; // Le stock d'argent est épuisé

                        var dueAmount = monthAmount - coveredAmount;
                        
                        unpaidMonths.Add(new UnpaidMonthDto
                        {
                            Month = currentDate.ToString("yyyy-MM"),
                            Amount = dueAmount,
                            DaysLate = (DateTime.Today - currentDate).Days
                        });
                    }
                }
                
                currentDate = currentDate.AddMonths(1);
            }
            
            // Couleur du statut
            string color = "#4CAF50"; // Vert (OK)
            if (unpaidMonths.Count > 0)
            {
                if (unpaidMonths.Count >= 3)
                    color = "#F44336"; // Rouge (Critique - 3 mois+)
                else
                    color = "#FF9800"; // Orange (Retard léger)
            }
            else if (remainingPaymentMoney > 0) // Il reste de l'argent = Avance
            {
                color = "#2196F3"; // Bleu (En avance)
            }

            return new ResidentFinancialStateDto
            {
                HouseCode = house.HouseCode,
                OwnerName = house.OwnerName,
                TotalDue = totalDue,
                TotalPaid = totalPaid,
                Balance = totalPaid - totalDue, // Négatif = Dette, Positif = Avance
                UnpaidMonths = unpaidMonths,
                UnpaidMonthsCount = unpaidMonths.Count,
                StatusColor = color
            };
        }

        public async Task<IEnumerable<ResidentFinancialStateDto>> GetAllResidentsFinancialStateAsync()
        {
            var houses = await _context.Houses.Where(h => h.IsActive).ToListAsync();
            var results = new List<ResidentFinancialStateDto>();

            foreach (var house in houses)
            {
                var state = await GetResidentFinancialStateAsync(house.HouseCode);
                if (state != null)
                {
                    results.Add(state);
                }
            }

            // Trier par nombre de mois impayés (descendant)
            return results.OrderByDescending(x => x.UnpaidMonthsCount).ThenBy(x => x.HouseCode).ToList();
        }

        public async Task<decimal> GetTotalArrearsAsync()
        {
            var allStates = await GetAllResidentsFinancialStateAsync();
            // Somme des balances négatives (en valeur absolue)
            return allStates.Where(s => s.Balance < 0).Sum(s => Math.Abs(s.Balance));
        }
    }
}
