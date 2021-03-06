using Azure.Storage.Blobs;
using AzureBlob.Api.Models;
using Imageflow.Fluent;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlob.Api.Service
{
    public class FileService : IFileService
    {
        private const bool OVERWRITE_EXISTING_BLOBS = true;
        private const int THUMB_HEIGHT = 192;
        private const int THUMB_WIDTH = 192;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly string _photosContainerName;
        private readonly string _thumbnailsContainerName;
        private string THUMB_RESIZE_CMD = $"width={THUMB_WIDTH}&height={THUMB_HEIGHT}&mode=both&scale=both&format=jpg";

        public FileService(BlobServiceClient blobServiceClient,
                                ComputerVisionClient computerVisionClient,
                                IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _computerVisionClient = computerVisionClient;
            _photosContainerName = configuration["AzureBlobStorage:ContainerNamePhotos"];
            _thumbnailsContainerName = configuration["AzureBlobStorage:ContainerNameThumbnails"];
        }

        public async Task Delete(string imageName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(_photosContainerName);

            var blobClient = blobContainer.GetBlobClient(imageName);

            await blobClient.DeleteAsync();
        }

        public async Task<byte[]> Get(string imageName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(_photosContainerName);

            var blobClient = blobContainer.GetBlobClient(imageName);
            var downloadContent = await blobClient.DownloadAsync();
            using (MemoryStream ms = new MemoryStream())
            {
                await downloadContent.Value.Content.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        public async Task<IEnumerable<TaggedImage>> GetList()
        {
            var photosContainer = _blobServiceClient.GetBlobContainerClient(_photosContainerName);
            photosContainer.CreateIfNotExists();

            // Download all Blobs in container and map to a list of TaggedImages
            var images = new List<TaggedImage>();
            await foreach (var blob in photosContainer.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.Metadata,
                                                      Azure.Storage.Blobs.Models.BlobStates.None))
            {
                string photoUrl = photosContainer.Uri.AbsoluteUri + "/" + blob.Name;
                images.Add(new TaggedImage
                {
                    Name = blob.Name,
                    Url = photoUrl,
                    Thumbnail = photoUrl.Replace($"/{_photosContainerName}/", $"/{_thumbnailsContainerName}/"),
                    Caption = blob.Metadata.ContainsKey("Caption") ? blob.Metadata["Caption"] : "",
                    Tags = string.Join(' ', blob.Metadata.Where(m => m.Key.StartsWith("Tag")).Select(m => m.Value))
                }); ;
            }
            return images;
        }

        public async Task Upload(FileModel model)
        {
            // Save the original image in the "photos" container
            var photosContainer = _blobServiceClient.GetBlobContainerClient(_photosContainerName);
            photosContainer.CreateIfNotExists();
            var blobClient = photosContainer.GetBlobClient(model.ImageFile.FileName);
            await blobClient.UploadAsync(model.ImageFile.OpenReadStream(), OVERWRITE_EXISTING_BLOBS);

            // Generate a 'dumb' thumbnail and save it in the "thumbnails" container
            await UploadThumbnailToContainer(model);

            // Generate a AI smart cropped thumbnails and save it in the thumbnails container as '*-smartcrop.ext'
            await UploadSmartCroppedThumbnailToContainer(model, blobClient.Uri.AbsoluteUri);

            // Submit the image to the Azure Computer Vision API
            await TagWithComputerVision(blobClient);
        }

        private async Task TagWithComputerVision(BlobClient blobClient)
        {
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>() { VisualFeatureTypes.Description };
            var result = await _computerVisionClient.AnalyzeImageAsync(blobClient.Uri.AbsoluteUri, features);

            // Record the image description and tags in blob metadata
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("Caption", result.Description.Captions[0].Text);

            for (int i = 0; i < result.Description.Tags.Count; i++)
            {
                metadata.Add($"Tag{i}", result.Description.Tags[i]);
            }

            await blobClient.SetMetadataAsync(metadata);
        }

        /// <summary>
        /// Resize thumbnail with AI smart cropping and upload to Azure storage container
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        private async Task UploadSmartCroppedThumbnailToContainer(FileModel model, string fileUrl)
        {
            var smartCropFileName = model.ImageFile.FileName.Insert(model.ImageFile.FileName.LastIndexOf('.'), "-smartcrop");
            var thumbnailsContainer = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);
            thumbnailsContainer.CreateIfNotExists();
            var thumbnailClient = thumbnailsContainer.GetBlobClient(smartCropFileName);

            using (Stream stream = await _computerVisionClient.GenerateThumbnailAsync(THUMB_WIDTH, THUMB_HEIGHT, fileUrl, true))
            {
                await thumbnailClient.UploadAsync(stream, OVERWRITE_EXISTING_BLOBS);
            }
        }

        /// <summary>
        /// Resize thumbnail with Imageflow and upload to Azure storage container
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task UploadThumbnailToContainer(FileModel model)
        {
            var thumbnailsContainer = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);
            thumbnailsContainer.CreateIfNotExists();
            var thumbnailClient = thumbnailsContainer.GetBlobClient(model.ImageFile.FileName);

            using (var outputStream = new MemoryStream())
            {
                using (var b = new ImageJob())
                {
                    var r = await b.BuildCommandString(
                        new StreamSource(model.ImageFile.OpenReadStream(), true),
                        new StreamDestination(outputStream, true),
                        THUMB_RESIZE_CMD)
                        .Finish().InProcessAsync();
                    outputStream.Position = 0;
                    await thumbnailClient.UploadAsync(outputStream, OVERWRITE_EXISTING_BLOBS);
                }
            }
        }
    }
}