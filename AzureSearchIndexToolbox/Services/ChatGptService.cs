using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSearchIndexToolbox.Models;
using Newtonsoft.Json;

namespace AzureSearchIndexToolbox.Services
{
    /// <summary>
    /// Service for interacting with Azure OpenAI ChatGPT using Azure Search Index for context.
    /// Provides conversation management and stores history in PostgreSQL.
    /// </summary>
    public class ChatGptService
    {
        private readonly ChatGptConfiguration _config;
        private readonly OpenAIClient _openAIClient;
        private readonly SearchClient _searchClient;
        private readonly ConversationDbContext _dbContext;
        private List<ChatRequestMessage> _conversationHistory;
        private string _currentConversationId;

        /// <summary>
        /// Initializes a new instance of the ChatGptService.
        /// </summary>
        /// <param name="config">ChatGPT configuration settings</param>
        public ChatGptService(ChatGptConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            // Initialize OpenAI client
            _openAIClient = new OpenAIClient(
                new Uri(_config.Endpoint),
                new AzureKeyCredential(_config.ApiKey));

            // Initialize Azure Search client
            _searchClient = new SearchClient(
                new Uri(_config.SearchEndpoint),
                _config.SearchIndexName,
                new AzureKeyCredential(_config.SearchApiKey));

            // Initialize database context
            _dbContext = new ConversationDbContext(_config.PostgresConnectionString);
            
            // Initialize conversation history
            _conversationHistory = new List<ChatRequestMessage>();
            _currentConversationId = Guid.NewGuid().ToString();

            // Add system message
            _conversationHistory.Add(new ChatRequestSystemMessage(_config.SystemContext));
        }

        /// <summary>
        /// Starts a new conversation with a fresh context.
        /// </summary>
        /// <returns>New conversation ID</returns>
        public string StartNewConversation()
        {
            _conversationHistory.Clear();
            _conversationHistory.Add(new ChatRequestSystemMessage(_config.SystemContext));
            _currentConversationId = Guid.NewGuid().ToString();
            
            Console.WriteLine($"Started new conversation: {_currentConversationId}");
            return _currentConversationId;
        }

        /// <summary>
        /// Resets the current conversation but keeps the same conversation ID.
        /// </summary>
        public void ResetConversation()
        {
            _conversationHistory.Clear();
            _conversationHistory.Add(new ChatRequestSystemMessage(_config.SystemContext));
            Console.WriteLine($"Reset conversation: {_currentConversationId}");
        }

        /// <summary>
        /// Gets the current conversation ID.
        /// </summary>
        public string GetCurrentConversationId()
        {
            return _currentConversationId;
        }

        /// <summary>
        /// Sends a single question to ChatGPT with context from Azure Search Index.
        /// </summary>
        /// <param name="question">User's question</param>
        /// <returns>Response with answer and citations</returns>
        public async Task<ChatGptResponse> AskQuestionAsync(string question)
        {
            return await AskQuestionsAsync(new List<string> { question });
        }

        /// <summary>
        /// Sends multiple questions to ChatGPT with context from Azure Search Index.
        /// </summary>
        /// <param name="questions">List of user questions</param>
        /// <returns>Response with answers and citations</returns>
        public async Task<ChatGptResponse> AskQuestionsAsync(List<string> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                throw new ArgumentException("Questions list cannot be null or empty.", nameof(questions));
            }

            // Check if we're at max questions
            int currentQuestionCount = _conversationHistory.Count(m => m is ChatRequestUserMessage);
            if (currentQuestionCount + questions.Count > _config.MaxQuestionsCount)
            {
                throw new InvalidOperationException(
                    $"Maximum questions count ({_config.MaxQuestionsCount}) would be exceeded. " +
                    $"Current: {currentQuestionCount}, Attempting to add: {questions.Count}");
            }

            var response = new ChatGptResponse
            {
                ConversationId = _currentConversationId,
                Answers = new List<ChatGptAnswer>()
            };

            foreach (var question in questions)
            {
                var answer = await ProcessSingleQuestionAsync(question);
                response.Answers.Add(answer);
            }

            return response;
        }

        /// <summary>
        /// Continues an existing conversation by loading history from the database.
        /// </summary>
        /// <param name="conversationId">Conversation ID to continue</param>
        public async Task ContinueConversationAsync(string conversationId)
        {
            _currentConversationId = conversationId;
            _conversationHistory.Clear();
            
            // Add system message
            _conversationHistory.Add(new ChatRequestSystemMessage(_config.SystemContext));

            // Load conversation history from database
            var history = await Task.Run(() => _dbContext.ConversationHistories
                .Where(ch => ch.ConversationId == conversationId)
                .OrderBy(ch => ch.SequenceNumber)
                .ToList());

            foreach (var entry in history)
            {
                _conversationHistory.Add(new ChatRequestUserMessage(entry.Question));
                _conversationHistory.Add(new ChatRequestAssistantMessage(entry.Answer));
            }

            Console.WriteLine($"Continued conversation {conversationId} with {history.Count} previous exchanges");
        }

        /// <summary>
        /// Processes a single question with context from Azure Search Index.
        /// </summary>
        private async Task<ChatGptAnswer> ProcessSingleQuestionAsync(string question)
        {
            Console.WriteLine($"\nProcessing question: {question}");

            // Step 1: Search for relevant documents in Azure Search Index
            var citations = await SearchRelevantDocumentsAsync(question);

            // Step 2: Build context from search results
            string searchContext = BuildSearchContext(citations);

            // Step 3: Add question with context to conversation
            string questionWithContext = string.IsNullOrEmpty(searchContext)
                ? question
                : $"{question}\n\nContext from search index:\n{searchContext}";

            _conversationHistory.Add(new ChatRequestUserMessage(questionWithContext));

            // Step 4: Call ChatGPT
            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = _config.DeploymentName,
                Temperature = _config.Temperature,
                MaxTokens = _config.MaxTokens
            };

            foreach (var message in _conversationHistory)
            {
                chatCompletionsOptions.Messages.Add(message);
            }

            Response<ChatCompletions> chatResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            string answer = chatResponse.Value.Choices[0].Message.Content;

            // Step 5: Add assistant response to conversation
            _conversationHistory.Add(new ChatRequestAssistantMessage(answer));

            // Step 6: Save to database
            int sequenceNumber = _conversationHistory.Count(m => m is ChatRequestUserMessage);
            await SaveToDatabase(question, answer, citations, sequenceNumber);

            Console.WriteLine($"Answer generated with {citations.Count} citation(s)");

            return new ChatGptAnswer
            {
                Question = question,
                Answer = answer,
                Citations = citations
            };
        }

        /// <summary>
        /// Searches for relevant documents in Azure Search Index.
        /// </summary>
        private async Task<List<DocumentCitation>> SearchRelevantDocumentsAsync(string query)
        {
            var citations = new List<DocumentCitation>();

            try
            {
                var searchOptions = new SearchOptions
                {
                    Size = 5, // Top 5 results
                    IncludeTotalCount = true
                };

                // Include relevant fields
                searchOptions.Select.Add("id");
                searchOptions.Select.Add("title");
                searchOptions.Select.Add("content");
                searchOptions.Select.Add("sourcePath");
                searchOptions.Select.Add("fileType");

                SearchResults<SearchDocument> results = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);

                await foreach (SearchResult<SearchDocument> result in results.GetResultsAsync())
                {
                    citations.Add(new DocumentCitation
                    {
                        DocumentId = result.Document.ContainsKey("id") ? result.Document["id"]?.ToString() ?? "" : "",
                        Title = result.Document.ContainsKey("title") ? result.Document["title"]?.ToString() ?? "" : "",
                        Content = result.Document.ContainsKey("content") ? result.Document["content"]?.ToString() ?? "" : "",
                        SourcePath = result.Document.ContainsKey("sourcePath") ? result.Document["sourcePath"]?.ToString() ?? "" : "",
                        FileType = result.Document.ContainsKey("fileType") ? result.Document["fileType"]?.ToString() ?? "" : "",
                        Score = result.Score ?? 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Search failed: {ex.Message}");
            }

            return citations;
        }

        /// <summary>
        /// Builds context string from search citations.
        /// </summary>
        private string BuildSearchContext(List<DocumentCitation> citations)
        {
            if (citations.Count == 0)
            {
                return string.Empty;
            }

            var contextBuilder = new System.Text.StringBuilder();
            
            for (int i = 0; i < citations.Count; i++)
            {
                var citation = citations[i];
                contextBuilder.AppendLine($"[Document {i + 1}]");
                contextBuilder.AppendLine($"Title: {citation.Title}");
                contextBuilder.AppendLine($"Source: {citation.SourcePath}");
                contextBuilder.AppendLine($"Type: {citation.FileType}");
                
                // Include a snippet of content (first 500 characters)
                string contentSnippet = citation.Content.Length > 500 
                    ? citation.Content.Substring(0, 500) + "..." 
                    : citation.Content;
                contextBuilder.AppendLine($"Content: {contentSnippet}");
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString();
        }

        /// <summary>
        /// Saves question and answer to the database.
        /// </summary>
        private async Task SaveToDatabase(string question, string answer, List<DocumentCitation> citations, int sequenceNumber)
        {
            try
            {
                // Ensure database exists
                await Task.Run(() => _dbContext.Database.EnsureCreated());

                var entry = new ConversationHistory
                {
                    ConversationId = _currentConversationId,
                    Question = question,
                    Answer = answer,
                    Citations = JsonConvert.SerializeObject(citations),
                    CreatedAt = DateTime.UtcNow,
                    SequenceNumber = sequenceNumber
                };

                _dbContext.ConversationHistories.Add(entry);
                await Task.Run(() => _dbContext.SaveChanges());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to save to database: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the conversation history for the current conversation.
        /// </summary>
        public async Task<List<ConversationHistory>> GetConversationHistoryAsync()
        {
            return await Task.Run(() => _dbContext.ConversationHistories
                .Where(ch => ch.ConversationId == _currentConversationId)
                .OrderBy(ch => ch.SequenceNumber)
                .ToList());
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }

    /// <summary>
    /// Response from ChatGPT service containing answers and citations.
    /// </summary>
    public class ChatGptResponse
    {
        public string ConversationId { get; set; } = string.Empty;
        public List<ChatGptAnswer> Answers { get; set; } = new List<ChatGptAnswer>();
    }

    /// <summary>
    /// Individual answer with citations.
    /// </summary>
    public class ChatGptAnswer
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<DocumentCitation> Citations { get; set; } = new List<DocumentCitation>();
    }

    /// <summary>
    /// Citation information from Azure Search Index documents.
    /// </summary>
    public class DocumentCitation
    {
        public string DocumentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
