# Quick Start Guide: ChatGPT Chatbox Web Application

Get your Azure Search ChatGPT chatbox web application running in minutes!

## 5-Minute Setup

### Step 1: Prerequisites

Ensure you have:
- ✅ .NET 8.0 SDK: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ PostgreSQL running locally or access to cloud instance
- ✅ Azure OpenAI API key and endpoint
- ✅ Azure Search service with indexed documents

### Step 2: Create Configuration File

Navigate to the ChatboxWebApp directory and create `chatgpt-config.json`:

```bash
cd ChatboxWebApp/ChatboxWebApp
cp chatgpt-config.template.json chatgpt-config.json
```

Edit `chatgpt-config.json` with your credentials:

```json
{
  "apiKey": "your-azure-openai-api-key",
  "endpoint": "https://your-resource.openai.azure.com/",
  "deploymentName": "gpt-35-turbo",
  "maxQuestionsCount": 10,
  "systemContext": "I am a helpful AI assistant.",
  "searchEndpoint": "https://your-search.search.windows.net",
  "searchApiKey": "your-search-api-key",
  "searchIndexName": "your-index-name",
  "postgresConnectionString": "Host=localhost;Database=chatgpt_conversations;Username=postgres;Password=yourpassword",
  "temperature": 0.7,
  "maxTokens": 800
}
```

### Step 3: Create Database

Create the PostgreSQL database:

```sql
CREATE DATABASE chatgpt_conversations;
```

The application will automatically create the required tables on first run.

### Step 4: Run the Application

```bash
cd ChatboxWebApp
dotnet run --project ChatboxWebApp
```

### Step 5: Open Your Browser

Navigate to: **http://localhost:5001/chatbox** (or https://localhost:7001/chatbox)

🎉 **You're ready to chat!**

## Using the Chatbox

### Single Question Mode

1. Select "Single Question" mode (default)
2. Type your question in the text area
3. Click "Send Question"
4. Wait for the response with citations

### Multiple Questions Mode

1. Select "Multiple Questions" mode
2. Enter multiple questions (one per field)
3. Click "Add Question" to add more fields
4. Click "Send All Questions"
5. Receive answers for all questions

### Managing Conversations

- **New Conversation**: Start fresh with a new conversation ID
- **Reset**: Clear messages but keep the same conversation ID
- **Question Counter**: Monitor your usage (e.g., "3/10" means 3 questions asked out of 10 maximum)

## Common Issues

### Issue: "Configuration not found"
**Solution**: Ensure `chatgpt-config.json` exists in the ChatboxWebApp directory or configuration is in `appsettings.json`

### Issue: "Cannot connect to PostgreSQL"
**Solution**: 
- Check if PostgreSQL is running: `pg_isready`
- Verify connection string in configuration
- Ensure database exists

### Issue: "Azure OpenAI error"
**Solution**:
- Verify API key and endpoint are correct
- Check deployment name matches your Azure OpenAI model
- Ensure you have quota available

### Issue: "Search service unavailable"
**Solution**:
- Verify search endpoint and API key
- Ensure search index exists and has documents
- Check Azure Search service status

## Next Steps

### For Development

```bash
# Watch mode with hot reload
cd ChatboxWebApp
dotnet watch --project ChatboxWebApp
```

### For Production

See [IIS Deployment Guide](README.md#iis-deployment) in the main README.

### Customization

1. **Change System Context**: Edit the `systemContext` in your configuration to change the assistant's personality
2. **Adjust Question Limit**: Change `maxQuestionsCount` to allow more or fewer questions per conversation
3. **Modify Styling**: Edit `Chatbox.razor.css` to customize the UI appearance

## Features at a Glance

| Feature | Description |
|---------|-------------|
| 🔵 Single Question | Ask one question at a time |
| 🔵 Multiple Questions | Batch multiple questions |
| 🔄 Reset | Clear conversation history |
| 🆕 New Conversation | Start with new ID |
| 📊 Question Counter | Track usage limits |
| 💾 Database Storage | All conversations saved |
| 📚 Citations | View source documents |
| 🎨 Modern UI | Responsive design |

## Keyboard Shortcuts

- **Enter** (in single question mode): Submit question
- **Escape**: Clear error messages

## Tips for Best Results

1. **Be Specific**: Ask clear, detailed questions
2. **Use Context**: The assistant has access to your indexed documents
3. **Review Citations**: Check source documents for accuracy
4. **Manage Questions**: Keep track of your question count limit
5. **New Conversations**: Start a new conversation for unrelated topics

## Getting Help

- 📖 Full Documentation: [README.md](README.md)
- 🔧 Console Version: [CHATGPT_SERVICE.md](../CHATGPT_SERVICE.md)
- 💡 Azure OpenAI Docs: [Microsoft Learn](https://learn.microsoft.com/azure/ai-services/openai/)
- 🔍 Azure Search Docs: [Microsoft Learn](https://learn.microsoft.com/azure/search/)

## Example Session

```
1. Open http://localhost:5001/chatbox
2. Type: "What are the main features of the product?"
3. Click "Send Question"
4. Review the answer and source citations
5. Ask follow-up: "Can you provide more details on feature X?"
6. Continue conversation or click "New Conversation" for a fresh start
```

## Development Mode Features

When running with `dotnet watch`:
- ✅ Automatic rebuild on code changes
- ✅ Hot reload for UI updates
- ✅ Detailed logging in console
- ✅ CORS enabled for API testing

## Architecture Overview

```
User Browser (Blazor WebAssembly)
    ↓ HTTP/HTTPS
ASP.NET Core Server (API)
    ↓ 
ChatGptService
    ↓
Azure OpenAI ←→ Azure Search
    ↓
PostgreSQL Database
```

## Configuration Checklist

Before running, ensure you have:

- [ ] Created `chatgpt-config.json` or configured `appsettings.json`
- [ ] PostgreSQL database created
- [ ] Azure OpenAI API key
- [ ] Azure OpenAI endpoint URL
- [ ] Azure OpenAI model deployed
- [ ] Azure Search service endpoint
- [ ] Azure Search API key
- [ ] Azure Search index with documents
- [ ] .NET 8.0 SDK installed

## Security Reminder

⚠️ **Never commit `chatgpt-config.json` with real API keys to source control!**

The `.gitignore` file is already configured to exclude sensitive configuration files.

## What's Next?

1. ✅ **You have a working chatbox!**
2. Deploy to IIS for production use
3. Add authentication for multi-user scenarios
4. Customize the UI to match your branding
5. Integrate with your existing applications
6. Scale with Azure App Service or Azure Kubernetes Service

---

**Need Help?** Check the [full README](README.md) or the [main project documentation](../README.md).
