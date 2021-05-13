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

    "ImageStoreConnectionString": "<YourAzureBlobStore>",

    // Name of container in Azure Blob Storage - remove this if using different storage.
    // The container must exist. (It is not created programmtically.)
    "ContainerClient": "imagefiles",

    // Optional values. Can be used to prevent resizing.
    "MaxWidthAccepted": 1080,
    "MaxHeightAccepted": 1080,

    "MaxWidthInPixels": 1080,
    "MaxHeightInPixels": 1080,
    "MaxFileSizeNotCompressedInBytes": 200000,
    "MaxFileSizeAcceptedInBytes": 5000000,
    "CompressionQualityPercentage": 80,
    
### Storage
By default it uses Azure Blob Storage. You can configure this by setting "ImageStoreConnectionString" and "ContainerClient" configuration values. Alternatively you can replace it by implementing the following interface:

**`IImageStore`**
* `RetrievedImage GetImage (Guid id);`
* `Guid AddImage (byte[] data, string docType);`
        
**`RetrievedImage`**
* `public Stream FileContent { get; set; }`
* `public Guid Id { get; set; }`
* `public string DocType { get; set; }`


### Authentication/Authorization
It's set up for Azure B2C policy-based authorization, using a policy called "access". You can configure this for your own use, or remove it.

## API
Accepts png, bmp, gif, jpg and bmp serialized as byte arrays.

### Schema
POSTs use 
```
{
"Data": byte[],
"WidthLimitPx": ushort?,
"HeightLimitPx": ushort?,
"ByteLimit": byte?
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
