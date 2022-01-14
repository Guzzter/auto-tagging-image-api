# auto-tagging-image-api

This is a working demo for working with Azure Cognitive Services. It analyzes uploaded images and generates a caption text and tags. As a bonus it generates a smart cropped thumbnail. Project contains Image API storing images and smart AI-cropped thumbnails in Azure Storage with metadata tagging from Azure Vision API

## Smart AI cropped thumbnails vs basic cropped thumbnail

Although the basic cropped thumbnail could be improved with some extra effort, example below shows power of AI. It looks at the image what the focus point should be before creating the thumbnail.

![AI cropped thumbnail](https://github.com/Guzzter/auto-tagging-image-api/raw/master/example-thumbnail-generation.jpg "ai-cropped-thumbnail")

## Swagger interface

![Auto tagging image api with azure!](https://github.com/Guzzter/auto-tagging-image-api/raw/master/api-screenshot.jpg "auto-tagging-image-api")

## Get started

For running this demo you need a (free) Azure account where you need to create the following 2 Azure resources:
- [Storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal)
- [Cognitive service account](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account?tabs=multiservice%2Cwindows)

### Create Storage account 

Needed for storing uploaded images and generated thumbnails. After creation add the connection string to the placeholder in appsettings.Development.json. BTW, the containers 'photos' and 'thumbnails' will be create automatically.

### Create Cognitive services multi-service account. 

Used for Vision API for analyzing image conten to retrieve caption text and all related tags. After creation of account, copy the key and endpoint values to the placeholders in appsettings.Development.json

### Running the application

When running the application locally you can use the Swagger interface to upload an image (see Samples folder). After the image is uploaded you can use List API method to retrieve image data with the recognized captain and tags from Vision API. Example output:

```
{
"caption": "a canal with buildings along it",
"name": "Colmar.jpg",
"tags": "building outdoor house narrow town residential",
"thumbnail": "https://<something>.blob.core.windows.net/thumbnails/Colmar.jpg",
"url": "https://<something>.blob.core.windows.net/photos/Colmar.jpg"
}
```

## Used NuGet packages:

I use the following packages:
- Azure.Storage.Blobs - used for BlobContainerClient for store/retrieve images from storage account
- Microsoft.Azure.CognitiveServices.Vision.ComputerVision - used for sending images to retrieve captain and tags
- Imageflow.AllPlatforms - used for creating thumbnail images
- MimeTypeMapOfficial - used for mapping extension to corresponding mime-type
- Swashbuckle.AspNetCore - used for Swagger API documentation page

## Other notes

This implementation is inspired by the older Microsoft [vision tuturial](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/tutorials/storage-lab-tutorial#use-computer-vision-to-generate-metadata). The tutorial is using .NET core 2(?) with razer pages. I choose to modernize it and use .Net 6.0 with Swagger API.

For resizing with ImageFlow, I used this documentation page: [Querystring documentation for BuildCommandString](https://docs.imageflow.io/querystring/introduction.html)
