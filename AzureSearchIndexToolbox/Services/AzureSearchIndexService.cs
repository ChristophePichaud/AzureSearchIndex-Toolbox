using Newtonsoft.Json;
using AzureSearchIndexToolbox.Models;

namespace AzureSearchIndexToolbox.Services
{
    /// <summary>
    /// Service for creating and managing Azure Search Index files.
    /// Handles JSON serialization and file operations for search index documents.
    /// </summary>
    public class AzureSearchIndexService
    {
        /// <summary>
        /// Saves a collection of documents to a JSON file in Azure Search Index format.
        /// </summary>
        /// <param name="documents">Collection of search index documents to save</param>
        /// <param name="outputFilePath">Path where the JSON file will be saved</param>
        public void SaveToJson(List<SearchIndexDocument> documents, string outputFilePath)
        {
            if (documents == null || documents.Count == 0)
            {
                throw new ArgumentException("Documents collection cannot be null or empty.", nameof(documents));
            }

            // Create the directory if it doesn't exist
            string? directoryPath = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Serialize documents to JSON with proper formatting
            string json = SerializeDocuments(documents);

            // Write to file
            File.WriteAllText(outputFilePath, json);

            Console.WriteLine($"Successfully saved {documents.Count} document(s) to {outputFilePath}");
        }

        /// <summary>
        /// Saves a single document to a JSON file.
        /// </summary>
        /// <param name="document">The search index document to save</param>
        /// <param name="outputFilePath">Path where the JSON file will be saved</param>
        public void SaveToJson(SearchIndexDocument document, string outputFilePath)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            SaveToJson(new List<SearchIndexDocument> { document }, outputFilePath);
        }

        /// <summary>
        /// Serializes a collection of documents to JSON format.
        /// Uses indented formatting for readability.
        /// </summary>
        /// <param name="documents">Documents to serialize</param>
        /// <returns>JSON string representation of the documents</returns>
        public string SerializeDocuments(List<SearchIndexDocument> documents)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"
            };

            // Wrap documents in a value array for Azure Search format
            var azureSearchFormat = new
            {
                value = documents
            };

            return JsonConvert.SerializeObject(azureSearchFormat, settings);
        }

        /// <summary>
        /// Loads documents from a JSON file.
        /// </summary>
        /// <param name="inputFilePath">Path to the JSON file to load</param>
        /// <returns>List of search index documents</returns>
        public List<SearchIndexDocument> LoadFromJson(string inputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException($"File not found: {inputFilePath}");
            }

            string json = File.ReadAllText(inputFilePath);
            return DeserializeDocuments(json);
        }

        /// <summary>
        /// Deserializes a JSON string to a collection of documents.
        /// Supports both single document and array formats.
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>List of search index documents</returns>
        public List<SearchIndexDocument> DeserializeDocuments(string json)
        {
            try
            {
                // Try to deserialize as Azure Search format (with "value" wrapper)
                var azureSearchFormat = JsonConvert.DeserializeObject<AzureSearchIndexFormat>(json);
                if (azureSearchFormat?.Value != null && azureSearchFormat.Value.Count > 0)
                {
                    return azureSearchFormat.Value;
                }
            }
            catch
            {
                // If that fails, try other formats
            }

            try
            {
                // Try to deserialize as a direct array
                var documents = JsonConvert.DeserializeObject<List<SearchIndexDocument>>(json);
                if (documents != null && documents.Count > 0)
                {
                    return documents;
                }
            }
            catch
            {
                // If that fails too, try single document
            }

            // Try to deserialize as a single document
            var document = JsonConvert.DeserializeObject<SearchIndexDocument>(json);
            if (document != null)
            {
                return new List<SearchIndexDocument> { document };
            }

            throw new JsonException("Unable to deserialize JSON to SearchIndexDocument format.");
        }

        /// <summary>
        /// Validates that a document has required fields populated.
        /// </summary>
        /// <param name="document">Document to validate</param>
        /// <returns>True if the document is valid, false otherwise</returns>
        public bool ValidateDocument(SearchIndexDocument document)
        {
            if (document == null)
            {
                return false;
            }

            // Check required fields
            if (string.IsNullOrWhiteSpace(document.Id))
            {
                Console.WriteLine("Validation failed: Document ID is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(document.Title))
            {
                Console.WriteLine("Validation failed: Document Title is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(document.Content))
            {
                Console.WriteLine("Validation failed: Document Content is required.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Merges multiple search index JSON files into a single file.
        /// </summary>
        /// <param name="inputFiles">Collection of input JSON file paths</param>
        /// <param name="outputFilePath">Path for the merged output file</param>
        public void MergeIndexFiles(List<string> inputFiles, string outputFilePath)
        {
            var allDocuments = new List<SearchIndexDocument>();

            foreach (var inputFile in inputFiles)
            {
                try
                {
                    var documents = LoadFromJson(inputFile);
                    allDocuments.AddRange(documents);
                    Console.WriteLine($"Loaded {documents.Count} document(s) from {inputFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {inputFile}: {ex.Message}");
                }
            }

            if (allDocuments.Count > 0)
            {
                SaveToJson(allDocuments, outputFilePath);
                Console.WriteLine($"Merged {allDocuments.Count} total document(s) into {outputFilePath}");
            }
            else
            {
                Console.WriteLine("No documents to merge.");
            }
        }

        /// <summary>
        /// Helper class for deserializing Azure Search Index format.
        /// </summary>
        private class AzureSearchIndexFormat
        {
            [JsonProperty("value")]
            public List<SearchIndexDocument> Value { get; set; } = new List<SearchIndexDocument>();
        }
    }
}
