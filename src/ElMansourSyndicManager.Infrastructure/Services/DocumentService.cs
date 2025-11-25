using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<DocumentService> _logger;
        private readonly string _storagePath;

        public DocumentService(IDocumentRepository documentRepository, IAuthenticationService authService, ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _authService = authService;
            _logger = logger;
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "documents");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var docs = await _documentRepository.GetAllAsync(cancellationToken);
            return docs.Select(MapToDto);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            var docs = await _documentRepository.GetByCategoryAsync(category, cancellationToken);
            return docs.Select(MapToDto);
        }

        public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string query, CancellationToken cancellationToken = default)
        {
            var docs = await _documentRepository.SearchAsync(query, cancellationToken);
            return docs.Select(MapToDto);
        }

        public async Task<DocumentDto> UploadDocumentAsync(string filePath, string category, string? description, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);

            var fileName = Path.GetFileName(filePath);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var destPath = Path.Combine(_storagePath, uniqueFileName);

            // Copy file
            await Task.Run(() => File.Copy(filePath, destPath), cancellationToken);

            var doc = new Document
            {
                FileName = fileName,
                FilePath = destPath,
                FileSize = new FileInfo(filePath).Length,
                ContentType = "application/octet-stream", // Simplified
                Category = category,
                UploadedBy = _authService.CurrentUser?.Username ?? "System",
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _documentRepository.CreateAsync(doc, cancellationToken);
            return MapToDto(doc);
        }

        public async Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await _documentRepository.GetByIdAsync(id, cancellationToken);
            if (doc != null)
            {
                if (File.Exists(doc.FilePath))
                {
                    File.Delete(doc.FilePath);
                }
                await _documentRepository.DeleteAsync(doc, cancellationToken);
            }
        }

        public async Task<string> GetDocumentPathAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doc = await _documentRepository.GetByIdAsync(id, cancellationToken);
            if (doc == null) throw new KeyNotFoundException($"Document {id} not found");
            return doc.FilePath;
        }

        private DocumentDto MapToDto(Document doc)
        {
            return new DocumentDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                FileSize = doc.FileSize,
                ContentType = doc.ContentType,
                Category = doc.Category,
                UploadedBy = doc.UploadedBy,
                Description = doc.Description,
                CreatedAt = doc.CreatedAt
            };
        }
    }
}
