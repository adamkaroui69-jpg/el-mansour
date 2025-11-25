using ElMansourSyndicManager.Core.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentDto>> GetDocumentsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string query, CancellationToken cancellationToken = default);
        Task<DocumentDto> UploadDocumentAsync(string filePath, string category, string? description, CancellationToken cancellationToken = default);
        Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);
        Task<string> GetDocumentPathAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
