using Amazon.S3;
using Amazon.S3.Model;
using Business.Abstract;
using Business.Settings;
using Core.Utilities.Results;
using Microsoft.Extensions.Options;

namespace Business.Concrete
{
    public class R2StorageService : IR2StorageService
    {
        private static readonly HashSet<string> CommonAssets = new(StringComparer.OrdinalIgnoreCase)
        {
            "nophoto.jpg",
            "Quattro-logo.png"
        };

        private readonly IAmazonS3 _s3Client;
        private readonly R2Options _options;

        public R2StorageService(IAmazonS3 s3Client, IOptions<R2Options> options)
        {
            _s3Client = s3Client;
            _options = options.Value;
        }

        public string BuildProductImageKey(string tenantSlug, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));
            }

            if (CommonAssets.Contains(fileName))
            {
                return $"common/{fileName}";
            }

            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                throw new ArgumentException("Tenant slug cannot be empty.", nameof(tenantSlug));
            }

            return $"{tenantSlug}/products/{fileName}";
        }

        public async Task<IDataResult<string>> UploadProductImageAsync(string tenantSlug, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            if (content is null)
            {
                return new ErrorDataResult<string>("Dosya icerigi bulunamadi.");
            }

            var key = BuildProductImageKey(tenantSlug, fileName);

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/webp" : contentType,
                AutoCloseStream = false,
                UseChunkEncoding = false,
                DisablePayloadSigning = true
            };

            if (content.CanSeek)
            {
                request.Headers.ContentLength = content.Length - content.Position;
            }

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return new SuccessDataResult<string>(key, "Dosya yuklendi.");
        }

        public async Task<IResult> DeleteProductImageAsync(string tenantSlug, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) || CommonAssets.Contains(fileName))
            {
                return new SuccessResult();
            }

            var key = BuildProductImageKey(tenantSlug, fileName);

            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return new SuccessResult();
        }

        public async Task<IDataResult<(byte[] Content, string ContentType)>> GetProductImageAsync(string tenantSlug, string fileName, CancellationToken cancellationToken = default)
        {
            var key = BuildProductImageKey(tenantSlug, fileName);

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
                await using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

                var contentType = string.IsNullOrWhiteSpace(response.Headers.ContentType)
                    ? "application/octet-stream"
                    : response.Headers.ContentType;

                return new SuccessDataResult<(byte[] Content, string ContentType)>(
                    (memoryStream.ToArray(), contentType));
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ErrorDataResult<(byte[] Content, string ContentType)>("Dosya bulunamadi.");
            }
        }
    }
}
