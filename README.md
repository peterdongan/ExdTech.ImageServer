# ExdTech.ImageServer
Simple image server using a REST API. Developed for and used by ExdPic. 

## Set-up
Refer to appsettingsTemplate.json to set up your appsettings file.

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
POSTS use `{"Data": byte[]}`. 

### Endpoints

**`/` `POST`** 
* Image is resized if width/height is greater than `<MaxWidthInPixels>`/`<MaxHeightInPixels>`.
* Image is compressed if resized or if filesize is greater than `<MaxFileSizeNotCompressedInBytes>`. Compression quality is `<CompressionQualityPercentage>`.
* Throws bad request if invalid file is added or if its size is greater than `<MaxFileSizeAcceptedInBytes>`.
* Returns a GUID.

**`/<Id>` `GET`**
* Returns the image for the specified Id. File is named `<Id>.jpeg`.


