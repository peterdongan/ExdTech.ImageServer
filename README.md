# ExdTech.ImageServer

Simple image server using a REST API. Developed for and used by ExdPic.

## Set-up

### Storage
By default it uses Azure Blob Storage. You can configure this by setting a "ImageStoreConnectionString" configuration value. Alternatively you can replace it by implementing the following interface:

**`IImageStore`**
* `RetrievedImage GetImage (Guid id);`
* `Guid AddImage (byte[] data, string docType);`
        
**`RetrievedImage`**
* `public Stream FileContent { get; set; }`
* `public Guid Id { get; set; }`
* `public string DocType { get; set; }`


### Authentication/Authorization
The POST methods use policy-based authorization, using a policy called "access". You should remove or replace this as required. Image download is unprotected.

## API
Images are uploaded by authenticated users via the API. Accepts png, bmp, gif, jpg and bmp files up to 5MB serialized as byte arrays. The server throws a bad request if larger files are submitted or if file type validation fails. All uploads are reencoded as jpegs. Images are resized if they exceed the maximum dimensions. Images are compressed if they are resized or the filesize if greater than the specified limit. Filenames are discarded. Downloaded files are named `[Id].jpg`.

### Schema
POSTS use `{"Data": byte[]}`. 

### Endpoints
**`/contentimages` `POST`** 
* Image is resized if width/height >720px.
* Image is compressed if resized or if filesize >50k
* Returns a GUID.

**`/pageimages` `POST`** 
* Image is resized if width/height >1080px.
* Image is compressed if resized or if filesize >200k
* Returns a GUID.
* 
**`/` `POST`** 
* _Uses same code as a POST to /contentimages._
* Image is resized if width/height >720px.
* Image is compressed if resized or if filesize >50k
* Returns a GUID.


Images are accessed by:

**`/[Id]` `GET`**

**`/pageimages/[Id]` `GET`** 

**`/contentimages/[Id]` `GET`** 

All of these point to the same resource. There is no difference between them.
