# Azure Search ChatGPT Chatbox Web Application

A modern Blazor WebAssembly chatbox application with ASP.NET Core backend that integrates Azure OpenAI ChatGPT with Azure Search Index for context-aware conversations. All conversations are stored in PostgreSQL database with Entity Framework.

## Features

- 🔵 **Single Question Mode**: Ask one question at a time with context from Azure Search
- 🔵 **Multiple Questions Mode**: Ask several questions in batch
- 🔄 **Reset Conversation**: Clear the current conversation while keeping the same ID
- 🆕 **New Conversation**: Start a fresh conversation with a new ID
- 📊 **Question Count Tracking**: Display current question count vs. maximum allowed
- 💾 **PostgreSQL Storage**: All questions, answers, and conversation IDs stored in database
- 🎨 **Modern UI**: Clean, responsive interface with real-time updates
- 📚 **Citation Support**: Display source documents used for answers

## Architecture

### Backend (ChatboxWebApp)
- **ASP.NET Core 8.0** web application
- **Controllers**: RESTful API endpoints for chat operations
- **Services**: ChatGptService for Azure OpenAI integration
- **Models**: EF Core models for PostgreSQL storage
- **Configuration**: appsettings.json or external chatgpt-config.json

### Frontend (ChatboxWebApp.Client)
- **Blazor WebAssembly** for client-side interactivity
- **Razor Components**: Reusable UI components
- **HTTP Client**: API communication with backend
- **CSS**: Scoped component styles

## Prerequisites

- .NET 8.0 SDK or later
- Azure OpenAI account with deployed GPT model
- Azure Cognitive Search service with indexed documents
- PostgreSQL database (local or cloud)
- IIS 10.0 or later (for production deployment)
- ASP.NET Core Hosting Bundle (for IIS)

## Configuration

### Option 1: Using appsettings.json

Edit `appsettings.json` in the ChatboxWebApp project:

```json
{
  "ChatGpt": {
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
}
```

### Option 2: Using chatgpt-config.json

Create `chatgpt-config.json` in the ChatboxWebApp project root:

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

### Configuration Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `apiKey` | Azure OpenAI API key | Required |
| `endpoint` | Azure OpenAI endpoint URL | Required |
| `deploymentName` | GPT model deployment name | gpt-35-turbo |
| `maxQuestionsCount` | Maximum questions per conversation | 10 |
| `systemContext` | Assistant's personality/role | French assistant |
| `searchEndpoint` | Azure Search service URL | Required |
| `searchApiKey` | Azure Search API key | Required |
| `searchIndexName` | Azure Search index name | Required |
| `postgresConnectionString` | PostgreSQL connection string | Required |
| `temperature` | Response creativity (0.0-1.0) | 0.7 |
| `maxTokens` | Maximum response length | 800 |

## Database Setup

The application automatically creates the required database schema. Ensure:

1. PostgreSQL is installed and running
2. The database specified in the connection string exists
3. The user has CREATE TABLE permissions

The service creates a `conversation_history` table with:
- `id`: Auto-incrementing primary key
- `conversation_id`: Groups related Q&A pairs
- `question`: User's question
- `answer`: ChatGPT's response
- `citations`: JSON array of source documents
- `created_at`: Timestamp
- `sequence_number`: Order in conversation

## Running Locally

### Development Mode

```bash
cd ChatboxWebApp
dotnet run --project ChatboxWebApp
```

The application will start at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001

Navigate to `/chatbox` to access the chatbox interface.

### Watch Mode (Auto-rebuild)

```bash
cd ChatboxWebApp
dotnet watch --project ChatboxWebApp
```

## Building for Production

```bash
cd ChatboxWebApp
dotnet publish ChatboxWebApp -c Release -o ./publish
```

This creates a self-contained deployment in the `./publish` directory.

## IIS Deployment

### Prerequisites

1. Install [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Restart IIS after installation: `iisreset`

### Deployment Steps

1. **Build the application**:
   ```bash
   cd ChatboxWebApp
   dotnet publish ChatboxWebApp -c Release -o C:\inetpub\wwwroot\chatbox
   ```

2. **Copy configuration**:
   - Copy your `chatgpt-config.json` to `C:\inetpub\wwwroot\chatbox\`
   - Or update `appsettings.json` with your configuration

3. **Create IIS Site**:
   - Open IIS Manager
   - Right-click "Sites" → "Add Website"
   - Site name: `ChatboxWebApp`
   - Physical path: `C:\inetpub\wwwroot\chatbox`
   - Port: `80` (or your preferred port)
   - Host name: (optional) `chatbox.yourdomain.com`

4. **Configure Application Pool**:
   - Select Application Pools → ChatboxWebApp
   - Set .NET CLR version to: **No Managed Code**
   - Set Managed pipeline mode to: **Integrated**
   - Set Start Mode to: **AlwaysRunning** (optional, for better performance)

5. **Set Permissions**:
   - Grant `IIS_IUSRS` read access to the deployment folder
   - Grant write access to logs folder if logging is enabled

6. **Test the deployment**:
   - Browse to http://localhost (or your configured URL)
   - Navigate to `/chatbox`

### IIS Configuration Options

The `web.config` file is included and pre-configured. You can modify it to:

- Change the hosting model (in-process vs out-of-process)
- Enable/disable stdout logging
- Set environment variables
- Configure request limits

### Troubleshooting IIS Deployment

**Issue**: 500.19 error - Cannot read configuration file
- **Solution**: Ensure ASP.NET Core Hosting Bundle is installed

**Issue**: 502.5 error - Process failure
- **Solution**: Check that .NET 8.0 runtime is installed

**Issue**: 404 error on /api endpoints
- **Solution**: Verify the application pool is set to "No Managed Code"

**Issue**: Database connection failures
- **Solution**: Check PostgreSQL connection string and ensure the database server is accessible from IIS

## API Endpoints

### POST /api/chat/ask
Ask a single question.

**Request Body**:
```json
{
  "question": "What is the main topic?"
}
```

**Response**:
```json
{
  "conversationId": "guid",
  "answers": [
    {
      "question": "What is the main topic?",
      "answer": "The main topic is...",
      "citations": [
        {
          "title": "Document Title",
          "filePath": "path/to/doc",
          "score": 0.95,
          "content": "excerpt..."
        }
      ]
    }
  ]
}
```

### POST /api/chat/ask-multiple
Ask multiple questions in batch.

**Request Body**:
```json
{
  "questions": [
    "Question 1?",
    "Question 2?",
    "Question 3?"
  ]
}
```

### POST /api/chat/new-conversation
Start a new conversation.

**Response**:
```json
{
  "conversationId": "new-guid",
  "message": "New conversation started."
}
```

### POST /api/chat/reset
Reset the current conversation.

**Response**:
```json
{
  "conversationId": "same-guid",
  "message": "Conversation reset."
}
```

### GET /api/chat/conversation-info
Get current conversation information.

**Response**:
```json
{
  "conversationId": "guid",
  "questionCount": 3,
  "maxQuestions": 10,
  "message": "Questions: 3/10"
}
```

### POST /api/chat/continue
Continue an existing conversation by loading history from database.

**Request Body**:
```json
{
  "conversationId": "existing-guid"
}
```

## User Interface

### Navigation
- **Home**: Landing page
- **Chatbox**: Main chat interface
- **Counter**: Sample counter page
- **Weather**: Sample weather page

### Chatbox Features

1. **Conversation Info Panel**:
   - Displays current conversation ID
   - Shows question count vs. max allowed
   - Buttons for new conversation and reset

2. **Chat Messages Area**:
   - Scrollable message history
   - User messages on the right (blue)
   - Assistant messages on the left (gray)
   - Citations displayed below answers
   - Loading indicator during processing

3. **Input Section**:
   - Toggle between single/multiple question modes
   - Text area for single questions
   - Dynamic list for multiple questions
   - Add/remove question fields
   - Send button (disabled when loading or empty)

4. **Error Handling**:
   - Toast notifications for errors
   - Dismissible error messages
   - Graceful degradation

## Security Considerations

1. **API Keys**: Never commit configuration files with real API keys
2. **HTTPS**: Always use HTTPS in production
3. **CORS**: Configure CORS policies appropriately for production
4. **Database**: Use secure connection strings and strong passwords
5. **Rate Limiting**: Consider implementing rate limiting for API endpoints
6. **Authentication**: Add authentication/authorization for production use

## Development Tips

### Hot Reload
Use `dotnet watch` for automatic rebuilding on file changes.

### Debugging
- Set breakpoints in the backend controllers and services
- Use browser DevTools for frontend debugging
- Check browser console for client-side errors
- Check server logs for backend errors

### Custom Styling
Component-specific styles are in `.razor.css` files (scoped styles).

### Adding Features
1. Add new API endpoints in `Controllers/ChatController.cs`
2. Create new Razor components in `ChatboxWebApp.Client/Pages`
3. Update navigation in `Components/Layout/NavMenu.razor`

## Performance Optimization

- **Singleton Service**: ChatGptService is registered as Singleton to maintain conversation state
- **Response Caching**: Consider adding response caching for repeated questions
- **Database Indexing**: Conversation history table has indexes on conversation_id and created_at
- **Static Assets**: WebAssembly assets are automatically compressed

## Monitoring and Logging

Enable logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ChatboxWebApp": "Debug"
    }
  }
}
```

For IIS, enable stdout logging in `web.config`:

```xml
<aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout">
```

## Support

For issues or questions:
1. Check the [main repository documentation](../README.md)
2. Review [ChatGPT Service documentation](../CHATGPT_SERVICE.md)
3. Check [Azure OpenAI documentation](https://learn.microsoft.com/azure/ai-services/openai/)
4. Review [Blazor documentation](https://learn.microsoft.com/aspnet/core/blazor/)

## License

This project is part of the Azure Search Index Toolbox and follows the same license.

## Next Steps

- Add authentication/authorization
- Implement conversation history view
- Add export functionality for conversations
- Integrate with Azure AD for user management
- Add support for file uploads
- Implement streaming responses for better UX
