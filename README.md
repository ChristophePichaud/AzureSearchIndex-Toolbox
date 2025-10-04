# AzureSearchIndex-Toolbox

A C# console application that extracts data from PowerPoint (PPTX), PDF, and Markdown (MD) files to create Azure Search Index files in JSON format. Ready to use with Azure.AI.OpenAI and Azure Cognitive Search.

## Features

- **PowerPoint (PPTX) Extraction**: Extracts text content, titles, images, audio files (MP3), and video files from presentations
- **PDF Extraction**: Extracts text content and images from PDF documents  
- **Markdown (MD) Extraction**: Parses markdown files to extract text, titles, and image references
- **Azure Search Index Format**: Outputs data in JSON format compatible with Azure Cognitive Search
- **Batch Processing**: Process individual files or entire directories
- **File Merging**: Merge multiple search index JSON files into a single file
- **Azure Deployment**: Deploy search indexes and media files directly to Azure Cognitive Search and Azure Blob Storage
- **ChatGPT Integration**: Interactive Q&A service using Azure OpenAI with Azure Search Index for context-aware responses (NEW!)

## Quick Start

### Build the Project

```bash
cd AzureSearchIndexToolbox
dotnet build
```

### Extract Data from Files

```bash
# Extract from a single file
dotnet run -- extract presentation.pptx

# Extract from a directory
dotnet run -- extract ./documents ./output

# Get help
dotnet run -- help
```

### Deploy to Azure

```bash
# Deploy extracted data to Azure
dotnet run -- deploy ./output/search-index.json ./output/media "<blob-connection-string>" "https://myservice.search.windows.net" "<search-api-key>"
```

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed deployment instructions.

### Use ChatGPT with Your Indexed Data

```bash
# Ask questions about your indexed documents using ChatGPT
dotnet run -- chatgpt ./chatgpt-config.json
```

See [CHATGPT_SERVICE.md](./CHATGPT_SERVICE.md) for complete ChatGPT integration documentation.

## Documentation

See the [detailed documentation](./AzureSearchIndexToolbox/README.md) in the AzureSearchIndexToolbox folder for complete usage instructions, examples, and architecture details.

## Architecture

The solution follows a clean, modular architecture:

- **Models**: Data structures for search index documents and ChatGPT configuration
- **Extractors**: Specialized extractors for each file type (PPTX, PDF, MD)
- **Services**: Azure Search Index service, Azure Deployment service, and ChatGPT service
- **Program**: Main orchestration and CLI interface

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

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.
