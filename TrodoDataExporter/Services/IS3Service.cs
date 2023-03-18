using Amazon.S3.Model;
using TrodoDataExporter.Models;

namespace TrodoDataExporter.Services
{
    public interface IS3Service
    {
        Task<GetObjectResponse> GetLatestS3Object();
        Task<Product[]> GetLatestS3ObjectDeserialized();
        Task<Product[]> DeserializeS3Object(GetObjectResponse objectResponse);
    }
}
