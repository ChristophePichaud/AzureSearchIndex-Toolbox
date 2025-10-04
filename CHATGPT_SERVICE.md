# ChatGPT Service with Azure Search Index Integration

This document describes the ChatGPT service that integrates Azure OpenAI with Azure Search Index, enabling intelligent question-answering with context from your indexed documents.

## Overview

The ChatGPT service provides:
- **Intelligent Q&A**: Ask questions about your indexed documents
- **Context-aware responses**: Automatically retrieves relevant information from Azure Search Index
- **Conversation management**: Start, continue, and reset conversations
- **Source tracking**: Shows which documents were used to answer questions
- **Persistent storage**: Stores conversation history in PostgreSQL
- **French language support**: Configured by default as a French assistant

## Configuration

### 1. Create Configuration File

Create a `chatgpt-config.json` file with your Azure credentials:

```json
{
  "apiKey": "YOUR_AZURE_OPENAI_API_KEY",
  "endpoint": "https://YOUR_RESOURCE_NAME.openai.azure.com/",
  "deploymentName": "gpt-35-turbo",
  "maxQuestionsCount": 10,
  "systemContext": "Je suis un assistant français et je vais vous donner des informations sur les fichiers d'index personnalisés.",
  "searchEndpoint": "https://YOUR_SEARCH_SERVICE.search.windows.net",
  "searchApiKey": "YOUR_SEARCH_API_KEY",
  "searchIndexName": "YOUR_INDEX_NAME",
  "postgresConnectionString": "Host=localhost;Database=chatgpt_conversations;Username=postgres;Password=YOUR_PASSWORD",
  "temperature": 0.7,
  "maxTokens": 800
}
```

### 2. Configuration Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `apiKey` | Azure OpenAI API key | Required |
| `endpoint` | Azure OpenAI endpoint URL | Required |
| `deploymentName` | GPT model deployment name | gpt-35-turbo |
| `maxQuestionsCount` | Maximum questions per conversation | 10 |
| `systemContext` | System prompt for the assistant | French assistant prompt |
| `searchEndpoint` | Azure Search service endpoint | Required |
| `searchApiKey` | Azure Search API key | Required |
| `searchIndexName` | Name of your search index | Required |
| `postgresConnectionString` | PostgreSQL connection string | Required |
| `temperature` | Response creativity (0.0-1.0) | 0.7 |
| `maxTokens` | Maximum response length | 800 |

### 3. Setup PostgreSQL Database

The service automatically creates the required database schema. Just ensure:
1. PostgreSQL is installed and running
2. The database specified in the connection string exists
3. The user has CREATE TABLE permissions

The service will create a `conversation_history` table with:
- `id`: Auto-incrementing primary key
- `conversation_id`: Groups related Q&A pairs
- `question`: User's question
- `answer`: ChatGPT's response
- `citations`: JSON array of source documents
- `created_at`: Timestamp
- `sequence_number`: Order in conversation

## Usage

### Starting the Interactive Session

```bash
cd AzureSearchIndexToolbox
dotnet run -- chatgpt ./chatgpt-config.json
```

### Available Commands

#### 1. Ask a Single Question
```
Command: ask
```
Prompts you to enter one question and provides an answer with citations.

**Example:**
```
Enter command: ask
Enter your question: What is the main topic of the quarterly report?

Processing...

=== Response ===
Question: What is the main topic of the quarterly report?

Answer: The main topic of the quarterly report is Q4 2024 sales performance...

--- Sources and Citations (2 document(s)) ---
[1] Quarterly Sales Report
    Source: /home/user/Documents/q4-report.pdf
    Type: PDF
    Relevance Score: 0.8542
```

#### 2. Ask Multiple Questions
```
Command: multi
```
Allows you to enter multiple questions in one batch.

**Example:**
```
Enter command: multi
Enter questions (one per line). Type 'done' when finished:
Question 1: What was the total revenue?
Question 2: Who was the top performer?
Question 3: done

Processing...
```

#### 3. Start New Conversation
```
Command: new
```
Starts a fresh conversation with a new conversation ID.

#### 4. Continue Existing Conversation
```
Command: continue
```
Loads a previous conversation from the database to continue where you left off.

**Example:**
```
Enter command: continue
Enter conversation ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Continued conversation a1b2c3d4-e5f6-7890-abcd-ef1234567890 with 5 previous exchanges
```

#### 5. Reset Conversation
```
Command: reset
```
Clears the conversation history but keeps the same conversation ID.

#### 6. View History
```
Command: history
```
Displays all questions and answers from the current conversation.

**Example:**
```
Enter command: history

=== Conversation History (3 exchanges) ===

[2024-01-15 10:30:00] Q1:
Q: What is the main topic?
A: The main topic is...
   Sources: 2 document(s)

[2024-01-15 10:31:00] Q2:
Q: Who is the author?
A: The author is...
   Sources: 1 document(s)
```

#### 7. Exit
```
Command: exit
```
Exits the interactive session.

## How It Works

### Information Retrieval Process

1. **Question Analysis**: Your question is analyzed to extract key terms
2. **Search Query**: The service searches Azure Search Index for relevant documents
3. **Context Building**: Top 5 most relevant documents are retrieved
4. **Answer Generation**: ChatGPT uses the document context to generate an accurate answer
5. **Citation Tracking**: All source documents are tracked and displayed
6. **Storage**: Question, answer, and citations are saved to PostgreSQL

### Source Attribution

The service provides detailed information about how answers were generated:

- **Document Citations**: Shows which documents were used
- **Relevance Scores**: Indicates how relevant each document was
- **Source Paths**: Links back to original files
- **Explanation**: Describes the retrieval process

### Example Output

```
=== Response (Conversation: 12345678-abcd-...) ===

Question: What are the key features of the new product?

Answer: The new product features include advanced analytics, 
real-time reporting, and automated workflows. It's designed to 
streamline operations and improve efficiency by 40%...

--- Sources and Citations (3 document(s)) ---

[1] Product Launch Presentation
    Source: /documents/product-launch.pptx
    Type: PPTX
    Relevance Score: 0.9234

[2] Technical Specifications
    Source: /documents/tech-specs.pdf
    Type: PDF
    Relevance Score: 0.8567

[3] Feature Overview
    Source: /documents/features.md
    Type: MD
    Relevance Score: 0.7891

How the answer was found:
The assistant searched the Azure Search Index for relevant documents based on your question,
retrieved the most relevant content, and used it as context to generate the answer.
The citations above show which documents were used and their relevance scores.
```

## Entity Framework Models

### ConversationHistory Model

```csharp
public class ConversationHistory
{
    public int Id { get; set; }
    public string ConversationId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public string? Citations { get; set; }  // JSON string
    public DateTime CreatedAt { get; set; }
    public int SequenceNumber { get; set; }
}
```

### Database Schema

```sql
CREATE TABLE conversation_history (
    id SERIAL PRIMARY KEY,
    conversation_id VARCHAR(100) NOT NULL,
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    citations TEXT,
    created_at TIMESTAMP NOT NULL,
    sequence_number INTEGER NOT NULL
);

CREATE INDEX ix_conversation_history_conversation_id ON conversation_history(conversation_id);
CREATE INDEX ix_conversation_history_created_at ON conversation_history(created_at);
```

## Advanced Features

### Maximum Questions Limit

The service enforces a maximum number of questions per conversation (default: 10) to:
- Manage API costs
- Maintain conversation quality
- Prevent token limit issues

When the limit is reached, start a new conversation with the `new` command.

### Temperature Control

Adjust the `temperature` parameter (0.0 to 1.0) to control response creativity:
- **0.0-0.3**: Focused, deterministic responses
- **0.4-0.7**: Balanced creativity and accuracy (recommended)
- **0.8-1.0**: More creative but potentially less accurate

### Custom System Context

Modify the `systemContext` parameter to change the assistant's behavior:

```json
{
  "systemContext": "You are a technical documentation expert who provides detailed, accurate answers with code examples."
}
```

## Troubleshooting

### Connection Issues

**Problem**: "Error initializing ChatGPT service"
**Solution**: 
- Verify your API keys are correct
- Check that endpoints are accessible
- Ensure PostgreSQL is running

### No Search Results

**Problem**: "No specific documents were found in the search index"
**Solution**:
- Verify your search index contains documents
- Check that the index name is correct
- Ensure documents are properly indexed

### Database Errors

**Problem**: "Failed to save to database"
**Solution**:
- Check PostgreSQL connection string
- Verify database exists
- Ensure user has proper permissions

### Token Limit Exceeded

**Problem**: Response cuts off mid-sentence
**Solution**:
- Reduce `maxTokens` in configuration
- Ask more specific questions
- Break complex questions into smaller ones

## Best Practices

1. **Specific Questions**: Ask focused questions for better results
2. **Context Matters**: The more relevant documents in your index, the better the answers
3. **Review Citations**: Always check the source documents for accuracy
4. **Conversation Limits**: Start new conversations when changing topics
5. **Security**: Never commit configuration files with real API keys to version control

## Integration Examples

### Programmatic Usage

```csharp
var config = new ChatGptConfiguration
{
    ApiKey = "your-api-key",
    Endpoint = "https://your-resource.openai.azure.com/",
    DeploymentName = "gpt-35-turbo",
    SearchEndpoint = "https://your-search.search.windows.net",
    SearchApiKey = "your-search-key",
    SearchIndexName = "your-index",
    PostgresConnectionString = "Host=localhost;Database=conversations;..."
};

using var chatService = new ChatGptService(config);

// Ask a question
var response = await chatService.AskQuestionAsync("What is the main topic?");

// Display answer and citations
Console.WriteLine($"Answer: {response.Answers[0].Answer}");
foreach (var citation in response.Answers[0].Citations)
{
    Console.WriteLine($"Source: {citation.Title} ({citation.Score:F4})");
}
```

## Security Considerations

1. **API Keys**: Store configuration files securely, never in source control
2. **Database Credentials**: Use strong passwords and secure connections
3. **Network Security**: Use HTTPS for all endpoints
4. **Data Privacy**: Be aware that conversation history is stored in the database
5. **Access Control**: Implement proper authentication for production use

## Next Steps

- Extract and index your documents using the `extract` command
- Deploy your index to Azure using the `deploy` command
- Start asking questions with the `chatgpt` command
- Review conversation history in PostgreSQL for analytics
- Customize the system context for your specific use case
