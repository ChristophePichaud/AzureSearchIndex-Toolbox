# AzureSearchIndex-Toolbox

A C# console application that extracts data from PowerPoint (PPTX), PDF, and Markdown (MD) files to create Azure Search Index files in JSON format. Ready to use with Azure.AI.OpenAI and Azure Cognitive Search.

## Features

- **PowerPoint (PPTX) Extraction**: Extracts text content, titles, images, audio files (MP3), and video files from presentations
- **PDF Extraction**: Extracts text content and images from PDF documents  
- **Markdown (MD) Extraction**: Parses markdown files to extract text, titles, and image references
- **Azure Search Index Format**: Outputs data in JSON format compatible with Azure Cognitive Search
- **Batch Processing**: Process individual files or entire directories
- **File Merging**: Merge multiple search index JSON files into a single file

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

## Documentation

See the [detailed documentation](./AzureSearchIndexToolbox/README.md) in the AzureSearchIndexToolbox folder for complete usage instructions, examples, and architecture details.

## Architecture

The solution follows a clean, modular architecture:

- **Models**: Data structures for search index documents
- **Extractors**: Specialized extractors for each file type (PPTX, PDF, MD)
- **Services**: Azure Search Index service for JSON serialization
- **Program**: Main orchestration and CLI interface

Every component is fully commented to help users understand how it works.

## Requirements

- .NET 8.0 or higher
- NuGet packages (automatically restored):
  - DocumentFormat.OpenXml
  - iText7
  - Markdig
  - Newtonsoft.Json

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
