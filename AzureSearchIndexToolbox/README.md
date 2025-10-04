# Azure Search Index Toolbox

A C# console application that extracts data from PowerPoint (PPTX), PDF, and Markdown (MD) files to create Azure Search Index files in JSON format. The extracted data is ready to use with Azure.AI.OpenAI and Azure Cognitive Search.

## Features

- **PowerPoint (PPTX) Extraction**: Extracts text content, titles, images, audio files (MP3), and video files from presentations
- **PDF Extraction**: Extracts text content and images from PDF documents
- **Markdown (MD) Extraction**: Parses markdown files to extract text, titles, and image references
- **Azure Search Index Format**: Outputs data in JSON format compatible with Azure Cognitive Search
- **Batch Processing**: Process individual files or entire directories
- **File Merging**: Merge multiple search index JSON files into a single file

## Requirements

- .NET 8.0 or higher
- NuGet packages (automatically restored):
  - DocumentFormat.OpenXml (for PPTX files)
  - iText7 (for PDF files)
  - Markdig (for Markdown files)
  - Newtonsoft.Json (for JSON serialization)

## Installation

1. Clone this repository:
```bash
git clone https://github.com/ChristophePichaud/AzureSearchIndex-Toolbox.git
cd AzureSearchIndex-Toolbox/AzureSearchIndexToolbox
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run -- <command> <arguments>
```

## Usage

### Extract Data from Files

Extract data from a single file:
```bash
dotnet run -- extract presentation.pptx
```

Extract from a single file with custom output directory:
```bash
dotnet run -- extract presentation.pptx ./my-output
```

Extract from all supported files in a directory:
```bash
dotnet run -- extract ./documents ./output
```

### Merge Index Files

Merge multiple search index JSON files:
```bash
dotnet run -- merge index1.json index2.json merged.json
```

### Help

Display usage information:
```bash
dotnet run -- help
```

## Output Format

The tool creates a JSON file in Azure Search Index format with the following structure:

```json
{
  "value": [
    {
      "id": "unique-guid",
      "title": "Document Title",
      "content": "Extracted text content...",
      "sourcePath": "/path/to/source/file.pptx",
      "fileType": "PPTX",
      "indexedDate": "2024-01-15T10:30:00Z",
      "images": [
        "/path/to/extracted/image1.png",
        "/path/to/extracted/image2.jpg"
      ],
      "audioFiles": [
        "/path/to/extracted/audio1.mp3"
      ],
      "videoFiles": [
        "/path/to/extracted/video1.mp4"
      ],
      "metadata": {
        "Author": "John Doe",
        "CreatedDate": "2024-01-01",
        "SlideCount": "10"
      }
    }
  ]
}
```

## Architecture

The solution is organized into distinct, well-commented modules:

### Models
- **SearchIndexDocument**: Data model representing a document in the Azure Search Index

### Extractors
- **PptxExtractor**: Extracts data from PowerPoint presentations
  - `ExtractData()`: Main method to extract all data
  - `ExtractTextFromSlides()`: Extracts text from all slides
  - `ExtractImages()`: Extracts and saves images
  - `ExtractMediaFiles()`: Extracts audio and video files
  - `ExtractMetadata()`: Extracts document metadata

- **PdfExtractor**: Extracts data from PDF documents
  - `ExtractData()`: Main method to extract all data
  - `ExtractTextFromPages()`: Extracts text from all pages
  - `ExtractImages()`: Extracts and saves images
  - `ExtractMetadata()`: Extracts document metadata

- **MarkdownExtractor**: Extracts data from Markdown files
  - `ExtractData()`: Main method to extract all data
  - `ExtractTitle()`: Extracts title from first heading
  - `ExtractContent()`: Converts markdown to plain text
  - `ExtractImageReferences()`: Extracts image references
  - `ExtractMetadata()`: Extracts structural metadata

### Services
- **AzureSearchIndexService**: Manages JSON serialization and file operations
  - `SaveToJson()`: Saves documents to JSON files
  - `LoadFromJson()`: Loads documents from JSON files
  - `ValidateDocument()`: Validates document structure
  - `MergeIndexFiles()`: Merges multiple index files

### Program
- Main orchestration logic with command-line interface
- Processes individual files or entire directories
- Provides clear console output and error handling

## Examples

### Processing a PowerPoint Presentation

```bash
dotnet run -- extract presentation.pptx ./output
```

Output:
```
=== Azure Search Index Toolbox ===
Extracts data from PPTX, PDF, and MD files for Azure Search Index

Processing: presentation.pptx
Output directory: ./output

Processing: presentation.pptx
  ✓ Successfully extracted from presentation.pptx
    - Title: My Presentation
    - Content length: 1500 characters
    - Images: 5
    - Audio files: 1
    - Video files: 2

=== Extraction Complete ===
Total documents processed: 1
Search index saved to: ./output/search-index.json
Media files saved to: ./output/media
```

### Processing Multiple Files

```bash
dotnet run -- extract ./documents ./output
```

This will process all PPTX, PDF, and MD files in the `./documents` directory.

## Integration with Azure Cognitive Search

The generated JSON files are ready to be uploaded to Azure Cognitive Search. You can use the Azure Portal, Azure CLI, or Azure SDKs to upload the data:

```bash
# Example using Azure CLI
az search index data import \
  --index-name my-index \
  --datasource-name my-datasource \
  --data-file output/search-index.json
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.

## Author

Christophe Pichaud
