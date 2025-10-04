# AzureSearchIndex-Toolbox

A comprehensive toolset for extracting data from PowerPoint (PPTX), PDF, and Markdown (MD) files to create Azure Search Index files, with both console and web interfaces for ChatGPT-powered Q&A.

## Features

- **PowerPoint (PPTX) Extraction**: Extracts text content, titles, images, audio files (MP3), and video files from presentations
- **PDF Extraction**: Extracts text content and images from PDF documents  
- **Markdown (MD) Extraction**: Parses markdown files to extract text, titles, and image references
- **Azure Search Index Format**: Outputs data in JSON format compatible with Azure Cognitive Search
- **Batch Processing**: Process individual files or entire directories
- **File Merging**: Merge multiple search index JSON files into a single file
- **Azure Deployment**: Deploy search indexes and media files directly to Azure Cognitive Search and Azure Blob Storage
- **Console ChatGPT Integration**: Interactive Q&A service using Azure OpenAI with Azure Search Index for context-aware responses
- **Web Chatbox Application**: Modern Blazor WebAssembly chatbox for web-based Q&A with IIS support (NEW!)

## Quick Start

### Console Application

#### Build the Project

```bash
cd AzureSearchIndexToolbox
dotnet build
```

#### Extract Data from Files

```bash
# Extract from a single file
dotnet run -- extract presentation.pptx

# Extract from a directory
dotnet run -- extract ./documents ./output

# Get help
dotnet run -- help
```

#### Deploy to Azure

```bash
# Deploy extracted data to Azure
dotnet run -- deploy ./output/search-index.json ./output/media "<blob-connection-string>" "https://myservice.search.windows.net" "<search-api-key>"
```

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed deployment instructions.

#### Use ChatGPT with Your Indexed Data (Console)

```bash
# Ask questions about your indexed documents using ChatGPT
dotnet run -- chatgpt ./chatgpt-config.json
```

See [CHATGPT_SERVICE.md](./CHATGPT_SERVICE.md) for complete ChatGPT integration documentation.

### Web Chatbox Application (NEW!)

#### Quick Start

```bash
cd ChatboxWebApp
cp ChatboxWebApp/chatgpt-config.template.json ChatboxWebApp/chatgpt-config.json
# Edit chatgpt-config.json with your credentials
dotnet run --project ChatboxWebApp
# Open browser to http://localhost:5001/chatbox
```

See [ChatboxWebApp/QUICKSTART.md](./ChatboxWebApp/QUICKSTART.md) for detailed web app setup.

See [ChatboxWebApp/README.md](./ChatboxWebApp/README.md) for complete web app documentation including IIS deployment.

## Documentation

See the [detailed documentation](./AzureSearchIndexToolbox/README.md) in the AzureSearchIndexToolbox folder for complete usage instructions, examples, and architecture details.

## Architecture

The solution follows a clean, modular architecture:

### Console Application
- **Models**: Data structures for search index documents and ChatGPT configuration
- **Extractors**: Specialized extractors for each file type (PPTX, PDF, MD)
- **Services**: Azure Search Index service, Azure Deployment service, and ChatGPT service
- **Program**: Main orchestration and CLI interface

### Web Application (NEW!)
- **Backend**: ASP.NET Core 8.0 with RESTful API controllers
- **Frontend**: Blazor WebAssembly for interactive client-side UI
- **Services**: Shared ChatGptService for Azure OpenAI integration
- **Models**: Entity Framework Core models for PostgreSQL storage
- **Deployment**: IIS-ready with web.config included

Every component is fully commented to help users understand how it works.

## Requirements

- .NET 8.0 or higher
- NuGet packages (automatically restored):
  - DocumentFormat.OpenXml
  - iText7
  - Markdig
  - Newtonsoft.Json
  - Azure.Search.Documents (for deployment)
  - Azure.Storage.Blobs (for deployment)
  - Azure.AI.OpenAI (for ChatGPT integration)
  - Microsoft.EntityFrameworkCore (for conversation storage)
  - Npgsql.EntityFrameworkCore.PostgreSQL (for PostgreSQL)
- PostgreSQL (optional, for ChatGPT conversation history)

## Output Format

Generates Azure Search Index compatible JSON:

```json
{
  "value": [
    {
      "id": "unique-guid",
      "title": "Document Title",
      "content": "Extracted text...",
      "sourcePath": "/path/to/file",
      "fileType": "PPTX",
      "images": ["image1.png"],
      "audioFiles": ["audio1.mp3"],
      "videoFiles": ["video1.mp4"],
      "metadata": {...}
    }
  ]
}
```

## Key Features in Detail

### Console Application
- **Extract**: Process PPTX, PDF, and MD files to create search indexes
- **Deploy**: Upload indexes and media to Azure Search and Blob Storage
- **ChatGPT CLI**: Interactive command-line Q&A with conversation history
- **Batch Processing**: Handle entire directories of documents

### Web Chatbox Application
- **Single Question Mode**: Ask questions one at a time with real-time responses
- **Multiple Questions Mode**: Submit multiple questions in batch
- **Conversation Management**: New, reset, and continue conversations
- **Question Tracking**: Monitor usage against configurable limits
- **Citation Display**: View source documents used for each answer
- **Modern UI**: Responsive Blazor WebAssembly interface
- **IIS Deployment**: Production-ready with included web.config
- **PostgreSQL Storage**: Complete conversation history with EF Core

## Use Cases

1. **Document Knowledge Base**: Extract and index your documentation for AI-powered search
2. **Training Materials**: Make PowerPoint presentations searchable and queryable
3. **Research Papers**: Index PDF documents for intelligent Q&A
4. **Corporate Wiki**: Convert Markdown documentation into searchable knowledge
5. **Internal Chatbot**: Deploy the web chatbox on IIS for company-wide access
6. **Customer Support**: Use the web interface for support teams to query documentation
7. **Educational Content**: Make course materials searchable and interactive

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.
