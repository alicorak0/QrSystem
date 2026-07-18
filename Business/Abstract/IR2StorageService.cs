using Core.Utilities.Results;

namespace Business.Abstract
{
    public interface IR2StorageService
    {
        Task<IDataResult<string>> UploadProductImageAsync(string tenantSlug, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);

        Task<IResult> DeleteProductImageAsync(string tenantSlug, string fileName, CancellationToken cancellationToken = default);

        Task<IDataResult<(byte[] Content, string ContentType)>> GetProductImageAsync(string tenantSlug, string fileName, CancellationToken cancellationToken = default);

        string BuildProductImageKey(string tenantSlug, string fileName);
    }
}
