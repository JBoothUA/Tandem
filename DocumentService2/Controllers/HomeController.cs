using System;
using DocumentService.Models;
using DoucmentService.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace DocumentService.Controllers
{
    public class HomeController : Controller
    {
        private const string DocumentContainerName = "jessebooth-dev-documents";

        private readonly ILogger<HomeController> _logger;

        private readonly DocumentServiceConfig _configuration;

        public HomeController(ILogger<HomeController> logger, DocumentServiceConfig configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var container = GetBlobContainer();
            var resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                true, BlobListingDetails.Metadata, 100, null, null, null);
            
            var documents = new List<Document>();
            foreach (var blobItem in resultSegment.Results)
            {
                var blob = (CloudBlob)blobItem;
                documents.Add(new Document()
                {
                    FileName = blob.Name,
                    FileSize = Math.Round((blob.Properties.Length / 1024f) / 1024f, 2).ToString(),
                    ModifiedOn = DateTime.Parse(blob.Properties.LastModified.ToString()).ToLocalTime().ToString()
                });
            }

            return View("Index", documents);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile files)
        {
            try
            {
                var container = GetBlobContainer();
                var blob = container.GetBlockBlobReference(files.FileName);
                await using (var data = files.OpenReadStream())
                {
                    await blob.UploadFromStreamAsync(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return await Index();
        }

        public async Task<IActionResult> Download(string documentName)
        {
            CloudBlockBlob blockBlob;
            var container = GetBlobContainer();
            await using (var memoryStream = new MemoryStream())
            {
                blockBlob = container.GetBlockBlobReference(documentName);
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }

            var blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }

        public async Task<IActionResult> Delete(string documentName)
        {
            var container = GetBlobContainer();
            var blob = container.GetBlobReference(documentName);
            await blob.DeleteIfExistsAsync();
            return await Index();
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_configuration.DocumentBlobStorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(DocumentContainerName);
        }
    }
}
