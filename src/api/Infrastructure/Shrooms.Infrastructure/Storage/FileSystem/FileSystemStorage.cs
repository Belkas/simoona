﻿using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Shrooms.Infrastructure.Storage.FileSystem
{
    public class FileSystemStorage : IStorage
    {
        public Task RemovePicture(string blobKey, string tenantPicturesContainer)
        {
            var filePath = HostingEnvironment.MapPath("~/storage/" + tenantPicturesContainer + "/" + blobKey);
            File.Delete(filePath);

            return Task.FromResult<object>(null);
        }

        public async Task UploadPicture(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var filePath = HostingEnvironment.MapPath("~/storage/" + tenantPicturesContainer + "/");
            var fullPath = Path.Combine(filePath, blobKey);
            Directory.CreateDirectory(filePath);

            var destinationStream = File.Create(fullPath);
            await stream.CopyToAsync(destinationStream);
        }
    }
}
