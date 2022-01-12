# auto-tagging-image-api

This is a working demo. Project contains Image API storing images and thumbnails in Azure Storage with metadata tagging from Azure Vision API

## Get started

For running this demo you need a (free) Azure account where you need to create the following 2 Azure resources:
- Storage account
- Cognitive service account

### Create Storage account 

Needed for storing uploaded images and generated thumbnails. After creation add the connection string to the placeholder in appsettings.Development.json. BTW, the containers 'photos' and 'thumbnails' will be create automatically.

### Create Cognitive services multi-service account. 

Used for Vision API for analyzing image conten to retrieve caption text and all related tags. After creation of account, copy the key and endpoint values to the placeholders in appsettings.Development.json

### Running the application

When running the application locally you can use the Swagger interface to upload an image (see Samples folder). After the image is uploaded you can use List API method to retrieve image data with the recognized captain and tags from Vision API.
