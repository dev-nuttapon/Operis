namespace Operis_API.Modules.Documents.Application;

public interface IDocumentTemplateCacheCommands
{
    Task<int> RefreshAsync(CancellationToken cancellationToken);
}
