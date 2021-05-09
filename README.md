# ExdTech.ImageServer

Simple image server using a REST API.

## Set-up

### Storage
The storage repository is injected. It uses the following interface:

**`IImageStore`**
* `RetrievedImage GetImage (Guid id);`
* `Guid AddImage (byte[] data, string docType);`
        
**`RetrievedImage`**
* `public Stream FileContent { get; set; }`
* `public Guid Id { get; set; }`
* `public string DocType { get; set; }`
        
An implementation using Azure Blob storage is included. You need to configure its connection string to use it.

### Authentication/Authorization
The POST methods use policy-based authorization, using a policy called "access". You will need to configure, change or remove this as required. Image download is unprotected.

## Usage 
Images are uploaded by authenticated users via the API. Accepts png, bmp, gif, jpg and bmp files up to 5MB serialized as byte arrays. The server throws a bad request if larger files are submitted or if file type validation fails.

### Schema
* POSTS use `{"Data": byte[]}`

**`/contentimages` `POST`** 
* Image is resized if largest dimension is greater than 720px. It is compressed at 80% quality if it is resized or if its file size is greater than 50k. Returns a GUID.

**`/pageimages` `POST`** 
* Image is resized if largest dimension is greater than 1080px. It is compressed at 80% quality if it is resized or if its file size is greater than 200k. Returns a GUID.

**`/` `POST`** 
* Uses same code as a POST to /contentimages. Image is resized if largest dimension is greater than 720px. It is compressed at 80% quality if it is resized or if its file size is greater than 50k. Returns a GUID.

All uploads are reencoded as jpegs. Filenames are discarded. Downloaded files are named `[Id].jpg`.

Images are accessed by:

**`/[Id]` `GET`**
**`/pageimages/[Id]` `GET`** 
**`/contentimages/[Id]` `GET`** 

All of these point to the same resource. There is no difference between them.
