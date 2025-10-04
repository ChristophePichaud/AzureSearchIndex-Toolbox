# Azure Search Index Toolbox - Solution Summary

## Overview

This solution implements a comprehensive C# console application that extracts data from PowerPoint (PPTX), PDF, and Markdown (MD) files to create Azure Search Index files in JSON format. The extracted data is ready for use with Azure Cognitive Search and Azure.AI.OpenAI.

## Architecture

The solution follows a clean, modular architecture with distinct components:

### 1. Models (`Models/`)
- **SearchIndexDocument.cs**: Core data model representing a document in the Azure Search Index
  - Contains fields for ID, title, content, source path, file type, indexed date
  - Supports collections for images, audio files, and video files
  - Includes metadata dictionary for additional properties

### 2. Extractors (`Extractors/`)

Each extractor is a specialized class handling a specific file format:

#### **PptxExtractor.cs** - PowerPoint Extraction
Key methods:
- `ExtractData()`: Main entry point for extraction
- `ExtractTextFromSlides()`: Extracts text content from all slides
- `ExtractTextFromSlide()`: Extracts text from individual slide
- `ExtractImages()`: Extracts and saves all images
- `ExtractMediaFiles()`: Extracts audio (MP3, WAV) and video (MP4, AVI, WMV) files
- `ExtractMetadata()`: Extracts document properties (author, dates, slide count)

Features:
- Parses all text from slide shapes and text boxes
- Extracts embedded images in various formats (PNG, JPG, GIF, BMP, TIFF)
- Extracts audio files (MP3, WAV)
- Extracts video files (MP4, AVI, WMV, MPEG)
- Preserves document metadata

#### **PdfExtractor.cs** - PDF Extraction
Key methods:
- `ExtractData()`: Main entry point for extraction
- `ExtractTextFromPages()`: Extracts text from all pages
- `ExtractTextFromPage()`: Extracts text from individual page
- `ExtractImages()`: Extracts and saves images from PDF
- `ExtractMetadata()`: Extracts PDF metadata (author, title, subject, keywords)

Features:
- Uses iText7 library for robust PDF parsing
- Extracts text using SimpleTextExtractionStrategy
- Extracts images as XObjects
- Preserves PDF metadata and document properties

#### **MarkdownExtractor.cs** - Markdown Extraction
Key methods:
- `ExtractData()`: Main entry point for extraction
- `ExtractTitle()`: Extracts title from first H1 heading
- `ExtractContent()`: Converts markdown to plain text
- `ExtractImageReferences()`: Extracts image paths and URLs
- `ExtractMetadata()`: Generates structural metadata (heading counts, table of contents)

Features:
- Uses Markdig library for markdown parsing
- Converts markdown to plain text while preserving structure
- Extracts all heading levels
- Counts code blocks and links
- Generates table of contents from headings
- Resolves relative image paths

### 3. Services (`Services/`)

#### **AzureSearchIndexService.cs** - JSON Management
Key methods:
- `SaveToJson()`: Saves documents to Azure Search Index JSON format
- `LoadFromJson()`: Loads documents from JSON files
- `SerializeDocuments()`: Converts documents to JSON
- `DeserializeDocuments()`: Parses JSON to documents
- `ValidateDocument()`: Validates document structure
- `MergeIndexFiles()`: Combines multiple index files

Features:
- Azure Search Index compatible JSON format with "value" wrapper
- Proper date formatting (ISO 8601)
- Document validation
- Support for merging multiple index files

### 4. Program (`Program.cs`)

Main orchestration with CLI:
- Command-line argument parsing
- File and directory processing
- Progress reporting with detailed console output
- Error handling and validation
- User-friendly help system

## Key Features

1. **Multi-Format Support**: Handles PPTX, PDF, and MD files
2. **Comprehensive Extraction**: Text, titles, images, audio, video, metadata
3. **Batch Processing**: Process entire directories recursively
4. **Azure Search Compatible**: Outputs JSON in Azure Search Index format
5. **Modular Design**: Each component is independent and reusable
6. **Well Documented**: Every method has XML documentation comments
7. **Error Handling**: Graceful error handling with informative messages

## Usage Examples

### Extract from Single File
```bash
dotnet run -- extract presentation.pptx
```

### Extract from Directory
```bash
dotnet run -- extract ./documents ./output
```

### Merge Multiple Index Files
```bash
dotnet run -- merge index1.json index2.json merged.json
```

## Output Format

The tool generates JSON in Azure Search Index format:

```json
{
  "value": [
    {
      "id": "unique-guid",
      "title": "Document Title",
      "content": "Extracted text content...",
      "sourcePath": "/path/to/file.pptx",
      "fileType": "PPTX",
      "indexedDate": "2024-01-15T10:30:00Z",
      "images": ["image1.png", "image2.jpg"],
      "audioFiles": ["audio1.mp3"],
      "videoFiles": ["video1.mp4"],
      "metadata": {
        "Author": "John Doe",
        "CreatedDate": "2024-01-01",
        "SlideCount": "10"
      }
    }
  ]
}
```

## Technology Stack

- **.NET 8.0**: Modern C# with latest language features
- **DocumentFormat.OpenXml 3.3.0**: PPTX parsing and manipulation
- **iText7 9.3.0**: PDF text and image extraction
- **Markdig 0.42.0**: Advanced markdown parsing
- **Newtonsoft.Json 13.0.4**: JSON serialization

## Testing

The solution has been tested with:
- ✅ Markdown file extraction (single file)
- ✅ Markdown file extraction (directory batch)
- ✅ JSON output validation
- ✅ Metadata extraction
- ✅ Image reference extraction
- ✅ Help command
- ✅ Build process

Sample output from test:
```
Processing: sample.md
  ✓ Successfully extracted from sample.md
    - Title: Azure Search Index Toolbox
    - Content length: 555 characters
    - Images: 1
    - Audio files: 0
    - Video files: 0
```

## Design Principles

1. **Separation of Concerns**: Each extractor handles one file type
2. **Single Responsibility**: Each method has a clear, focused purpose
3. **Dependency Injection Ready**: Classes can be easily integrated into DI containers
4. **Open/Closed Principle**: Easy to add new extractors without modifying existing code
5. **Documentation First**: All public methods have XML documentation
6. **Error Recovery**: Graceful handling of extraction errors

## Future Enhancements (Optional)

While the current implementation meets all requirements, potential enhancements could include:

1. DOCX (Word document) extraction
2. XLSX (Excel spreadsheet) extraction
3. Async/await for better performance with large files
4. Progress callbacks for long-running operations
5. Direct Azure Search Index upload
6. OCR for scanned PDFs
7. Video thumbnail generation
8. Audio transcription

## Conclusion

This solution provides a robust, well-documented, and production-ready toolbox for extracting data from multiple document formats and creating Azure Search Index files. Every component is designed with clarity and maintainability in mind, making it easy for users to understand and extend.
