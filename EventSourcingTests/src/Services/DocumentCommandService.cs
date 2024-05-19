using Core.Infrastructure;
using System.Reflection;
using System.Text.Json;
using EventSourcingTests.Events;

namespace Core.Services;

public class DocumentCommandService
{
    private readonly IEventStore _eventStore;
    private readonly IFileSystem _fileSystem;
    
    
    public DocumentCommandService(IEventStore eventStore, IFileSystem fileSystem)
    {
        _eventStore = eventStore;
        _fileSystem = fileSystem;
    }
    
    public async Task UploadDocument(string userId, int adminId, Guid tenderId, Guid documentId, string documentName,
        string contents)
    {
        var documentPath = FindFilePath(documentName);
        // Combine the assembly location with the 'TenderDocuments' folder and the document name
        if (!_fileSystem.FileExists(documentPath))
        {
            try
            {
                _fileSystem.CreateFile(documentPath, contents);
            }
            catch (Exception e)
            {
                throw new Exception("Error creating file. Name already exists", e);
            }
            
            await _eventStore.AppendEventAsync(new UploadDocument(userId, adminId, tenderId, documentId, 1,
                DateTimeOffset.UtcNow));
        }
        else
        {
            throw new Exception("File already exists or path is invalid");
        }
    }

    public async Task EditDocument(string userId, int adminId, Guid tenderId, Guid documentId, string documentName,
        string reason, string userLocation,string contents)
    {
        var documentPath = FindFilePath(documentName);
        if (_fileSystem.FileExists(documentPath))
        {
            try
            {
                _fileSystem.CreateFile(documentPath, contents);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
            await _eventStore.AppendEventAsync(new EditDocument(userId, adminId, tenderId, documentId, 1, reason, userLocation,
                DateTimeOffset.UtcNow));
        }
        else
        {
            throw new Exception("File does not exist");
        }
    }

    public async Task DeleteDocument(string userId, int adminId, Guid tenderId, Guid documentId, string documentName)
    {
        var documentPath = FindFilePath(documentName);
        if (_fileSystem.FileExists(documentPath))
        {
            try
            {
                _fileSystem.DeleteFile(documentPath);
            }
            catch (Exception e)
            {
                throw new Exception("Error deleting file", e);
            }
            
            await _eventStore.AppendEventAsync(new DeleteDocument(userId, adminId, tenderId, documentId, 1,
                DateTimeOffset.UtcNow));
        }
        else
        {
            throw new Exception("File does not exist");
        }
    }

    public string FindFilePath(string documentName)
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Navigate up to the root project directory (assuming the bin folder is two levels down from the root)
        var projectRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", ".."));

        // Combine the project root with the 'src' and 'TenderDocuments' folders and the document name
        var documentPath = Path.Combine(projectRoot, "src", "TenderDocuments", documentName);

        return documentPath;
    }
}