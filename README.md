# ExdTech.ImageServer
Minimalist image server using a REST API. Designed to be easy to extend and configure.

## Functions
* Compresses images greater than configured/specified filesize.
* Resizes images with dimensions greater than configured/specified limits.
* Throws Bad Request if non-images are uploaded or if limits on accepted filesize or dimensions are exceeded.
* Reencodes images as jpegs. 
* Stores images to Azure Blob Storage. (Or elsewhere with your own implementation of `IImageStore`.)

## Set-up
Configure the following values in appsettings.json:
````
  // Binds to ExdTech.ImageServer.Contract.ImageProcessingOptions.
  "ImageProcessingConfig": {
    "MaxWidthAccepted": 1080,   // Not required. Images are rejected if this is exceeded.
    "MaxHeightAccepted": 1080,  // Not required. Images are rejected if this is exceeded.
    "MaxWidthInPixels": 1080,   // Images are scaled down if this is exceeded. 
    "MaxHeightInPixels": 1080,  // Images are scaled down if this is exceeded.
    "MaxFileSizeNotCompressedInBytes": 200000,  // Images are compressed if this is exceeded.
    "MaxFileSizeAcceptedInBytes": 5000000,      // Images are rejected if this is exceeded.
    "CompressionQualityPercentage": 80          // Jpg compression quality to use if compressing.
  },

  "ImageStoreConnectionString": "<Connection string>",
  "ContainerClient": "<Container>", // Container must already exist
````    
    
### Storage
By default it uses Azure Blob Storage. You can configure this by setting "ImageStoreConnectionString" and "ContainerClient" configuration values. Alternatively you can replace it by implementing the `IImageStore` interface 


### Authentication/Authorization
It's set up for Azure B2C policy-based authorization, using a policy called "access". You can configure this for your own use, or remove it.

## API
Accepts png, bmp, gif, jpg and bmp serialized as byte arrays.

### Schema
POSTs use 
```
{
"Data": byte[],             // Serialized image
"WidthLimitPx": ushort?,    // Image is scaled down if this is exceeded. No effect if it is greater than the server's configured MaxWidthInPixels.
"HeightLimitPx": ushort?,   // Image is scaled down if this is exceeded. No effect if it is greater than the server's configured MaxHeightInPixels.
"ByteLimit": byte?          // Image is compressed if this is exceeded. No effect if it is greater than the server's configured MaxFileSizeNotCompressedInBytes.
}
````

### Endpoints

**`/` `POST`** 
* Image is scaled down if its dimensions exceed configured/specified limits.
* Image is compressed if its size exceeds configured/specified limits.
* Throws Bad Request if a non-image is uploaded.
* Throws Bad Request if any of `MaxFileSizeAcceptedInBytes`, `MaxHeightAccepted` or `MaxWidthAccepted` are configured and exceeded.
* Returns a GUID.

**`/<Id>` `GET`**
* Returns the image for the specified Id. File is named `<Id>.jpeg`.

## Licence
MIT
