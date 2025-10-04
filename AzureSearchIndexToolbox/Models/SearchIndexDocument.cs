using Newtonsoft.Json;

namespace AzureSearchIndexToolbox.Models
{
    /// <summary>
    /// Represents a document in the Azure Search Index.
    /// This model contains all the extracted data from various file formats (PPTX, PDF, MD).
    /// </summary>
    public class SearchIndexDocument
    {
        /// <summary>
        /// Unique identifier for the document in the search index.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title of the document (extracted from file name or content).
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Main text content extracted from the document.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Source file path of the document.
        /// </summary>
        [JsonProperty("sourcePath")]
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Type of the source file (e.g., "PPTX", "PDF", "MD").
        /// </summary>
        [JsonProperty("fileType")]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the document was indexed.
        /// </summary>
        [JsonProperty("indexedDate")]
        public DateTime IndexedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of image paths extracted from the document.
        /// </summary>
        [JsonProperty("images")]
        public List<string> Images { get; set; } = new List<string>();

        /// <summary>
        /// Collection of audio file paths extracted from the document (e.g., MP3).
        /// </summary>
        [JsonProperty("audioFiles")]
        public List<string> AudioFiles { get; set; } = new List<string>();

        /// <summary>
        /// Collection of video file paths extracted from the document.
        /// </summary>
        [JsonProperty("videoFiles")]
        public List<string> VideoFiles { get; set; } = new List<string>();

        /// <summary>
        /// Additional metadata extracted from the document.
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
