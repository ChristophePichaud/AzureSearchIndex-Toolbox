using Newtonsoft.Json;

namespace ChatboxWebApp.Models
{
    /// <summary>
    /// Configuration settings for the ChatGPT service with Azure Search Index integration.
    /// </summary>
    public class ChatGptConfiguration
    {
        /// <summary>
        /// Azure OpenAI API key for authentication.
        /// </summary>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Azure OpenAI endpoint URL.
        /// </summary>
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Deployment name for the GPT model (e.g., "gpt-35-turbo").
        /// </summary>
        [JsonProperty("deploymentName")]
        public string DeploymentName { get; set; } = "gpt-35-turbo";

        /// <summary>
        /// Maximum number of questions allowed in a conversation.
        /// </summary>
        [JsonProperty("maxQuestionsCount")]
        public int MaxQuestionsCount { get; set; } = 10;

        /// <summary>
        /// System context/prompt for the assistant.
        /// </summary>
        [JsonProperty("systemContext")]
        public string SystemContext { get; set; } = "Je suis un assistant français et je vais vous donner des informations sur les fichiers d'index personnalisés.";

        /// <summary>
        /// Azure Search service endpoint.
        /// </summary>
        [JsonProperty("searchEndpoint")]
        public string SearchEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Azure Search service API key.
        /// </summary>
        [JsonProperty("searchApiKey")]
        public string SearchApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Azure Search index name.
        /// </summary>
        [JsonProperty("searchIndexName")]
        public string SearchIndexName { get; set; } = string.Empty;

        /// <summary>
        /// PostgreSQL connection string for storing conversation history.
        /// </summary>
        [JsonProperty("postgresConnectionString")]
        public string PostgresConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Maximum temperature for GPT responses (0.0 to 1.0).
        /// </summary>
        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Maximum tokens for GPT responses.
        /// </summary>
        [JsonProperty("maxTokens")]
        public int MaxTokens { get; set; } = 800;
    }
}
