using ElMansourSyndicManager.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Repositories
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<Document>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
