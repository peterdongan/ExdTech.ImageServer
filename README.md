# ExdTech.ImageServer

Simple image server designed for use with Azure Blob Storage. Requires appsettings.json to be added with authentication configured and a `BlobConnectionString` variable set.

Images are uploaded by authenticated users via the API. Accepts png, bmp, gif, jpg and bmp files up to 5MB serialized as byte arrays. The server throws a bad request if larger files are submitted or if file type validation fails.

**`/api/contentimages` `POST`** Image is resized if largest dimension is greater than 720px. It is compressed at 80% quality if it is resized or if its file size is greater than 50k. Returns a GUID.

**`/api/pageimages` `POST`** Image is resized if largest dimension is greater than 10800px. It is compressed at 80% quality if it is resized or if its file size is greater than 200k. Returns a GUID.

All uploads are reencoded as jpegs. Filenames are discarded.

Images are accessed by:

**`/[GUID].jpg` `GET`**

