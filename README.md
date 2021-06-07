# ExdTech.ImageServer
This is the image server used by [exd pic](https://exdpic.com). It is live at [images.exdpic.com](https://images.exdpic.com). It is a simple image server that uses a REST API. It is designed to be easy to extend and configure. 

## Functions
* High quality image resizing and compression.
* Throws Bad Request if non-images are uploaded or if limits on accepted filesize or dimensions are exceeded.
* Reencodes images as jpegs. (This behaviour should not be removed without understanding and consideration of the security aspect.) 
* Stores images to Azure Blob Storage. (Or elsewhere with your own implementation of `IImageStore`.)

## Processing
It uses [Magick.Net](https://github.com/dlemstra/Magick.NET]) for processing, which is a .net wrapper for ImageMagick. The quality of processed images is very good.

## Set-up
It is set up to use an Azure blob store for the images and SQL server for image information. You can change it to use different stores by using a new implementation of IImageStorageService or IInfoStorageService respectively. 

The following packages are available on nuget to facilitate client development:

* [ExdTech.ImageServer.Common](https://www.nuget.org/packages/ExdTech.ImageServer.Common/) has definitions for the DTOs and the interfaces.
* [ExdTech.ImageServer.Persistence.ImageInfo](https://www.nuget.org/packages/ExdTech.ImageServer.Persistence.ImageInfo/) has an EF model for the image info data store and an implementation of a data access service to access it. It is used by the Exd Pic Windows app to store the image info locally.

### Configuration
Configure appsettings.json. You must configure an image store and an info store for it to work.

````
  // Binds to ExdTech.ImageServer.Contract.ImageProcessingOptions.
  "ImageProcessingConfig": {
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
By default it uses Azure Blob Storage. You can configure this by setting "ImageStoreConnectionString" and "ContainerClient" configuration values. Alternatively you can replace it by implementing the `IImageStorageService` interface.

### Authentication/Authorization
It uses Azure AD B2C authentication for POSTs. You will probably want to remove or change this. It can be turned off by removing the  Authorize(Policy = "access") attribute from the POST method in the API contrller. 

## API and Schema
It uses JSON. See the swagger documentation for the live version at https://images.exdpic.com/index.html. 

### /{id} GET
Get an object containing the info and the image serialized as a byte array

### /images/{id} GET
Get the image file. `/images/{id}.jpeg` and `/images/{id}.jpg` will also work.

### /info/{id} GET
Get the image info only.

### / POST
Send the image serialized as a byte array as well as information about the image.

## Licence
MIT
