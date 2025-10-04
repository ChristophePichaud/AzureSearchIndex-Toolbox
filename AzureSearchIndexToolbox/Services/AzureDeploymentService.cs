using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureSearchIndexToolbox.Models;
using Newtonsoft.Json;

namespace AzureSearchIndexToolbox.Services
{
    /// <summary>
    /// Service for deploying Azure Search Index and uploading files to Azure Blob Storage.
    /// Handles the complete deployment workflow including media file uploads and search index creation.
    /// </summary>
    public class AzureDeploymentService
    {
        private readonly string _blobStorageConnectionString;
        private readonly string _searchServiceEndpoint;
        private readonly string _searchServiceApiKey;
        private readonly string _containerName;

        /// <summary>
        /// Initializes a new instance of the AzureDeploymentService.
        /// </summary>
        /// <param name="blobStorageConnectionString">Azure Blob Storage connection string</param>
        /// <param name="searchServiceEndpoint">Azure Cognitive Search service endpoint URL</param>
        /// <param name="searchServiceApiKey">Azure Cognitive Search admin API key</param>
        /// <param name="containerName">Name of the blob container for media files</param>
        public AzureDeploymentService(
            string blobStorageConnectionString,
            string searchServiceEndpoint,
            string searchServiceApiKey,
            string containerName = "searchindex-media")
        {
            _blobStorageConnectionString = blobStorageConnectionString;
            _searchServiceEndpoint = searchServiceEndpoint;
            _searchServiceApiKey = searchServiceApiKey;
            _containerName = containerName;
        }

        /// <summary>
        /// Deploys the complete solution to Azure including media files and search index.
        /// </summary>
        /// <param name="jsonIndexPath">Path to the search-index.json file</param>
        /// <param name="mediaDirectory">Path to the media directory containing files to upload</param>
        /// <param name="indexName">Name of the Azure Search Index to create/update</param>
        public async Task DeployAsync(string jsonIndexPath, string mediaDirectory, string indexName)
        {
            Console.WriteLine("=== Starting Azure Deployment ===");
            Console.WriteLine();

            // Step 1: Load documents from JSON
            Console.WriteLine("Step 1: Loading documents from JSON...");
            var indexService = new AzureSearchIndexService();
            var documents = indexService.LoadFromJson(jsonIndexPath);
            Console.WriteLine($"✓ Loaded {documents.Count} document(s)");
            Console.WriteLine();

            // Step 2: Upload media files to Azure Blob Storage
            Console.WriteLine("Step 2: Uploading media files to Azure Blob Storage...");
            var blobUrlMap = await UploadMediaFilesAsync(documents, mediaDirectory);
            Console.WriteLine($"✓ Uploaded {blobUrlMap.Count} media file(s)");
            Console.WriteLine();

            // Step 3: Update document references with Blob Storage URLs
            Console.WriteLine("Step 3: Updating document references with Blob Storage URLs...");
            UpdateDocumentReferences(documents, blobUrlMap);
            Console.WriteLine("✓ Document references updated");
            Console.WriteLine();

            // Step 4: Create or update the Azure Search Index
            Console.WriteLine("Step 4: Creating/updating Azure Search Index...");
            await CreateOrUpdateSearchIndexAsync(indexName);
            Console.WriteLine($"✓ Search index '{indexName}' ready");
            Console.WriteLine();

            // Step 5: Upload documents to Azure Search Index
            Console.WriteLine("Step 5: Uploading documents to Azure Search Index...");
            await UploadDocumentsToSearchIndexAsync(documents, indexName);
            Console.WriteLine($"✓ Uploaded {documents.Count} document(s) to search index");
            Console.WriteLine();

            Console.WriteLine("=== Deployment Complete ===");
            Console.WriteLine($"Search Index: {indexName}");
            Console.WriteLine($"Blob Container: {_containerName}");
        }

        /// <summary>
        /// Uploads all media files from documents to Azure Blob Storage.
        /// </summary>
        /// <param name="documents">List of documents containing media file paths</param>
        /// <param name="mediaDirectory">Base directory containing the media files</param>
        /// <returns>Dictionary mapping local file paths to blob URLs</returns>
        private async Task<Dictionary<string, string>> UploadMediaFilesAsync(List<SearchIndexDocument> documents, string mediaDirectory)
        {
            var blobUrlMap = new Dictionary<string, string>();
            var blobServiceClient = new BlobServiceClient(_blobStorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Create container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Collect all unique media file paths
            var allMediaFiles = new HashSet<string>();
            foreach (var doc in documents)
            {
                allMediaFiles.UnionWith(doc.Images);
                allMediaFiles.UnionWith(doc.AudioFiles);
                allMediaFiles.UnionWith(doc.VideoFiles);
            }

            Console.WriteLine($"Found {allMediaFiles.Count} unique media file(s) to upload");

            // Upload each file
            foreach (var filePath in allMediaFiles)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"  ⚠ Warning: File not found: {filePath}");
                        continue;
                    }

                    string fileName = Path.GetFileName(filePath);
                    var blobClient = containerClient.GetBlobClient(fileName);

                    // Upload the file
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        await blobClient.UploadAsync(fileStream, overwrite: true);
                    }

                    // Store the blob URL
                    blobUrlMap[filePath] = blobClient.Uri.ToString();
                    Console.WriteLine($"  ✓ Uploaded: {fileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error uploading {filePath}: {ex.Message}");
                }
            }

            return blobUrlMap;
        }

        /// <summary>
        /// Updates document references to use Azure Blob Storage URLs instead of local paths.
        /// </summary>
        /// <param name="documents">List of documents to update</param>
        /// <param name="blobUrlMap">Mapping of local paths to blob URLs</param>
        private void UpdateDocumentReferences(List<SearchIndexDocument> documents, Dictionary<string, string> blobUrlMap)
        {
            foreach (var doc in documents)
            {
                // Update image references
                doc.Images = doc.Images.Select(path => 
                    blobUrlMap.ContainsKey(path) ? blobUrlMap[path] : path
                ).ToList();

                // Update audio file references
                doc.AudioFiles = doc.AudioFiles.Select(path => 
                    blobUrlMap.ContainsKey(path) ? blobUrlMap[path] : path
                ).ToList();

                // Update video file references
                doc.VideoFiles = doc.VideoFiles.Select(path => 
                    blobUrlMap.ContainsKey(path) ? blobUrlMap[path] : path
                ).ToList();
            }
        }

        /// <summary>
        /// Creates or updates the Azure Search Index with the appropriate schema.
        /// </summary>
        /// <param name="indexName">Name of the search index</param>
        private async Task CreateOrUpdateSearchIndexAsync(string indexName)
        {
            var credential = new AzureKeyCredential(_searchServiceApiKey);
            var indexClient = new SearchIndexClient(new Uri(_searchServiceEndpoint), credential);

            // Define the search index schema
            var index = new SearchIndex(indexName)
            {
                Fields =
                {
                    new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                    new SearchableField("title") { IsFilterable = true, IsSortable = true },
                    new SearchableField("content") { AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                    new SimpleField("sourcePath", SearchFieldDataType.String) { IsFilterable = true },
                    new SimpleField("fileType", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                    new SimpleField("indexedDate", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                    new SimpleField("images", SearchFieldDataType.Collection(SearchFieldDataType.String)),
                    new SimpleField("audioFiles", SearchFieldDataType.Collection(SearchFieldDataType.String)),
                    new SimpleField("videoFiles", SearchFieldDataType.Collection(SearchFieldDataType.String))
                }
            };

            try
            {
                // Try to create the index
                await indexClient.CreateIndexAsync(index);
                Console.WriteLine($"  Created new search index: {indexName}");
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // Index already exists, update it
                await indexClient.CreateOrUpdateIndexAsync(index);
                Console.WriteLine($"  Updated existing search index: {indexName}");
            }
        }

        /// <summary>
        /// Uploads documents to the Azure Search Index.
        /// </summary>
        /// <param name="documents">List of documents to upload</param>
        /// <param name="indexName">Name of the search index</param>
        private async Task UploadDocumentsToSearchIndexAsync(List<SearchIndexDocument> documents, string indexName)
        {
            var credential = new AzureKeyCredential(_searchServiceApiKey);
            var searchClient = new SearchClient(new Uri(_searchServiceEndpoint), indexName, credential);

            // Convert documents to Azure Search format
            var searchDocuments = documents.Select(doc => new
            {
                id = doc.Id,
                title = doc.Title,
                content = doc.Content,
                sourcePath = doc.SourcePath,
                fileType = doc.FileType,
                indexedDate = doc.IndexedDate,
                images = doc.Images.ToArray(),
                audioFiles = doc.AudioFiles.ToArray(),
                videoFiles = doc.VideoFiles.ToArray()
            }).ToList();

            // Upload in batches of 100 (Azure Search limit)
            const int batchSize = 100;
            for (int i = 0; i < searchDocuments.Count; i += batchSize)
            {
                var batch = searchDocuments.Skip(i).Take(batchSize).ToList();
                try
                {
                    var response = await searchClient.UploadDocumentsAsync(batch);
                    Console.WriteLine($"  ✓ Uploaded batch of {batch.Count} document(s)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error uploading batch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Validates Azure connection settings by attempting to connect to services.
        /// </summary>
        /// <returns>True if connections are valid, false otherwise</returns>
        public async Task<bool> ValidateConnectionsAsync()
        {
            bool isValid = true;

            try
            {
                // Test Blob Storage connection
                var blobServiceClient = new BlobServiceClient(_blobStorageConnectionString);
                await blobServiceClient.GetPropertiesAsync();
                Console.WriteLine("✓ Blob Storage connection successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Blob Storage connection failed: {ex.Message}");
                isValid = false;
            }

            try
            {
                // Test Search Service connection
                var credential = new AzureKeyCredential(_searchServiceApiKey);
                var indexClient = new SearchIndexClient(new Uri(_searchServiceEndpoint), credential);
                await indexClient.GetServiceStatisticsAsync();
                Console.WriteLine("✓ Azure Cognitive Search connection successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Azure Cognitive Search connection failed: {ex.Message}");
                isValid = false;
            }

            return isValid;
        }
    }
}
