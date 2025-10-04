# Quick Start Guide: ChatGPT Service

This guide will help you get started with the ChatGPT service for Azure Search Index.

## Prerequisites

1. **Azure OpenAI Service**
   - Azure subscription
   - Azure OpenAI resource created
   - GPT-3.5-Turbo or GPT-4 deployment

2. **Azure Cognitive Search**
   - Azure Search service created
   - Search index with your documents deployed

3. **PostgreSQL Database**
   - PostgreSQL server (local or cloud)
   - Database created for storing conversation history

## Step-by-Step Setup

### 1. Prepare Your Search Index

First, extract and deploy your documents to Azure Search Index:

```bash
# Extract documents
cd AzureSearchIndexToolbox
dotnet run -- extract ./documents ./output

# Deploy to Azure
dotnet run -- deploy ./output/search-index.json ./output/media \
  "<blob-connection-string>" \
  "https://myservice.search.windows.net" \
  "<search-api-key>"
```

### 2. Setup PostgreSQL Database

Run the provided SQL script to create the required database schema:

```bash
psql -U postgres -d chatgpt_conversations -f database-setup.sql
```

Or manually execute the SQL commands in `database-setup.sql`.

### 3. Create Configuration File

Copy the template and fill in your credentials:

```bash
cp chatgpt-config.template.json chatgpt-config.json
```

Edit `chatgpt-config.json`:

```json
{
  "apiKey": "your-azure-openai-api-key-here",
  "endpoint": "https://your-resource.openai.azure.com/",
  "deploymentName": "gpt-35-turbo",
  "maxQuestionsCount": 10,
  "systemContext": "Je suis un assistant français et je vais vous donner des informations sur les fichiers d'index personnalisés.",
  "searchEndpoint": "https://your-search-service.search.windows.net",
  "searchApiKey": "your-search-api-key-here",
  "searchIndexName": "your-index-name",
  "postgresConnectionString": "Host=localhost;Database=chatgpt_conversations;Username=postgres;Password=your-password",
  "temperature": 0.7,
  "maxTokens": 800
}
```

**Important**: Never commit `chatgpt-config.json` to version control!

### 4. Run the ChatGPT Service

```bash
dotnet run -- chatgpt ./chatgpt-config.json
```

### 5. Start Asking Questions

```
=== ChatGPT with Azure Search Index ===

Commands:
  ask - Ask a single question
  multi - Ask multiple questions
  new - Start a new conversation
  continue - Continue an existing conversation
  reset - Reset current conversation
  history - View conversation history
  exit - Exit the program

Enter command: ask
Enter your question: What are the main topics in the documents?

Processing...
```

## Configuration Details

### Required Settings

| Setting | Where to Find It |
|---------|------------------|
| `apiKey` | Azure Portal → Your OpenAI Resource → Keys and Endpoint |
| `endpoint` | Azure Portal → Your OpenAI Resource → Keys and Endpoint |
| `deploymentName` | Azure Portal → Your OpenAI Resource → Model deployments |
| `searchEndpoint` | Azure Portal → Your Search Service → Overview → Url |
| `searchApiKey` | Azure Portal → Your Search Service → Keys |
| `searchIndexName` | Azure Portal → Your Search Service → Indexes |
| `postgresConnectionString` | Your PostgreSQL server connection details |

### Optional Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `maxQuestionsCount` | Max questions per conversation | 10 |
| `systemContext` | Assistant's personality/role | French assistant |
| `temperature` | Response creativity (0.0-1.0) | 0.7 |
| `maxTokens` | Maximum response length | 800 |

## Common Use Cases

### Use Case 1: Document Q&A

Perfect for asking questions about your indexed documents:

```
Q: What are the key features mentioned in the product documentation?
A: Based on the product documentation, the key features include...
   [Citations from product-features.pdf]
```

### Use Case 2: Multi-Document Analysis

Ask questions that require information from multiple documents:

```
Q: Compare the Q1 and Q2 sales reports
A: Comparing the reports, Q1 had $2M in revenue while Q2 reached $2.5M...
   [Citations from q1-sales.pdf and q2-sales.pdf]
```

### Use Case 3: Technical Support

Use as a knowledge base for technical questions:

```
Q: How do I configure the authentication module?
A: To configure authentication, follow these steps...
   [Citations from technical-guide.md]
```

## Troubleshooting

### Error: "Configuration file not found"
- Ensure `chatgpt-config.json` exists in the correct location
- Use absolute paths if relative paths don't work

### Error: "Error initializing ChatGPT service"
- Verify all API keys are correct
- Check that endpoints are accessible
- Ensure your Azure OpenAI deployment is active

### Error: "Failed to save to database"
- Check PostgreSQL is running: `pg_isready`
- Verify connection string is correct
- Ensure database exists and user has permissions

### No Search Results
- Verify your search index contains documents
- Check the index name in configuration
- Ensure documents are properly indexed

## Tips for Best Results

1. **Be Specific**: Ask clear, focused questions
2. **Use Context**: The service works best with well-indexed documents
3. **Review Citations**: Always check the source documents
4. **Conversation Management**: Start new conversations for different topics
5. **Temperature Tuning**: Lower temperature (0.3-0.5) for factual answers

## Security Best Practices

1. **Never commit** `chatgpt-config.json` to version control
2. **Use environment variables** for production deployments
3. **Rotate API keys** regularly
4. **Use secure connections** (HTTPS) for all endpoints
5. **Implement authentication** if exposing as a service

## Next Steps

1. Index your documents with the `extract` and `deploy` commands
2. Set up your configuration file
3. Start asking questions!
4. Review the full documentation in [CHATGPT_SERVICE.md](./CHATGPT_SERVICE.md)

## Getting Help

- Full documentation: [CHATGPT_SERVICE.md](./CHATGPT_SERVICE.md)
- Database setup: [database-setup.sql](./database-setup.sql)
- Example queries: See the interactive commands

## Example Session

```
$ dotnet run -- chatgpt ./chatgpt-config.json

=== ChatGPT with Azure Search Index ===
✓ ChatGPT service initialized
✓ Using model: gpt-35-turbo
✓ Connected to search index: my-documents
✓ Conversation ID: 12345678-abcd-...

Commands:
  ask - Ask a single question
  multi - Ask multiple questions
  new - Start a new conversation
  continue - Continue an existing conversation
  reset - Reset current conversation
  history - View conversation history
  exit - Exit the program

Enter command: ask
Enter your question: What are the quarterly sales figures?

Processing...

=== Response (Conversation: 12345678-abcd-...) ===

Question: What are the quarterly sales figures?

Answer: According to the Q4 2024 Sales Report, the quarterly 
sales figures are: Q1: $2.0M, Q2: $2.5M, Q3: $2.8M, Q4: $3.2M. 
Total annual revenue reached $10.5M, representing a 40% increase 
from the previous year.

--- Sources and Citations (2 document(s)) ---

[1] Quarterly Sales Report
    Source: /documents/q4-sales-report.pdf
    Type: PDF
    Relevance Score: 0.9234

[2] Annual Financial Summary
    Source: /documents/annual-summary.pdf
    Type: PDF
    Relevance Score: 0.8567

How the answer was found:
The assistant searched the Azure Search Index for relevant documents 
based on your question, retrieved the most relevant content, and used 
it as context to generate the answer. The citations above show which 
documents were used and their relevance scores.

================================================================================

Enter command: exit
Exiting...
```
