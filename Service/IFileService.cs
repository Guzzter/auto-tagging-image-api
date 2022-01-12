using AzureBlob.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureBlob.Api.Service
{
    public interface IFileService
    {
        Task Delete(string imageName);

        Task<byte[]> Get(string imageName);

        Task<IEnumerable<TaggedImage>> GetList();

        Task Upload(FileModel model);
    }
}