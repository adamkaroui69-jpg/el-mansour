using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories
{
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Document>()
                .Where(d => d.Category == category)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Document>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Document>()
                .Where(d => d.FileName.Contains(query) || (d.Description != null && d.Description.Contains(query)))
                .ToListAsync(cancellationToken);
        }
    }
}
