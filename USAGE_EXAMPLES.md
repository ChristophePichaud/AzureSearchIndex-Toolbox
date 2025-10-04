# Usage Examples

This document provides practical examples of using the Azure Search Index Toolbox.

## Example 1: Extract from a Single PowerPoint File

```bash
cd AzureSearchIndexToolbox
dotnet run -- extract ~/Documents/Presentation.pptx ~/Output
```

**Expected Output:**
```
=== Azure Search Index Toolbox ===
Processing: Presentation.pptx
  ✓ Successfully extracted from Presentation.pptx
    - Title: My Presentation
    - Content length: 2500 characters
    - Images: 8
    - Audio files: 2
    - Video files: 1

=== Extraction Complete ===
Search index saved to: ~/Output/search-index.json
Media files saved to: ~/Output/media
```

## Example 2: Extract from Multiple PDF Files

```bash
cd AzureSearchIndexToolbox
dotnet run -- extract ~/Documents/Reports ./output-reports
```

This will process all `.pdf` files in the `~/Documents/Reports` directory.

## Example 3: Extract from Markdown Documentation

```bash
cd AzureSearchIndexToolbox
dotnet run -- extract ~/Projects/my-docs ./search-output
```

Processes all `.md` files, extracting:
- Headings and titles
- Plain text content
- Image references
- Code blocks (counted in metadata)
- Table of contents

## Example 4: Merge Multiple Index Files

After creating multiple index files from different sources:

```bash
cd AzureSearchIndexToolbox
dotnet run -- merge ./output1/search-index.json ./output2/search-index.json ./combined/merged-index.json
```

## Example 5: Process Mixed File Types

```bash
cd AzureSearchIndexToolbox
dotnet run -- extract ~/Documents/ProjectFiles ./output
```

If `~/Documents/ProjectFiles` contains:
- `presentation.pptx`
- `report.pdf`
- `README.md`
- `notes.md`

All files will be processed and combined into a single `search-index.json` file.

## Example Output JSON

Here's what the extracted data looks like:

```json
{
  "value": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "title": "Quarterly Sales Report",
      "content": "Q4 2024 Sales Report Executive Summary Total revenue increased...",
      "sourcePath": "/home/user/Documents/q4-report.pdf",
      "fileType": "PDF",
      "indexedDate": "2025-10-04T10:30:00Z",
      "images": [
        "/home/user/output/media/q4-report_image_1.png",
        "/home/user/output/media/q4-report_image_2.png"
      ],
      "audioFiles": [],
      "videoFiles": [],
      "metadata": {
        "Author": "Jane Smith",
        "CreatedDate": "2024-12-15",
        "PageCount": "45"
      }
    }
  ]
}
```

## Integration with Azure Cognitive Search

Once you have the JSON output, you can upload it to Azure:

### Using Azure CLI

```bash
# First, create a search index
az search index create \
  --name my-document-index \
  --service-name my-search-service \
  --resource-group my-resource-group

# Upload the documents
az search index data import \
  --index-name my-document-index \
  --service-name my-search-service \
  --resource-group my-resource-group \
  --documents @search-index.json
```

### Using Azure Portal

1. Navigate to your Azure Cognitive Search service
2. Select "Indexes" from the left menu
3. Create a new index with fields matching the SearchIndexDocument schema
4. Use "Import data" to upload the `search-index.json` file

### Using Azure SDK for .NET

```csharp
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

var endpoint = new Uri("https://my-search-service.search.windows.net");
var credential = new AzureKeyCredential("your-api-key");

var indexClient = new SearchIndexClient(endpoint, credential);
var searchClient = indexClient.GetSearchClient("my-document-index");

// Load and upload documents
var json = File.ReadAllText("search-index.json");
var documents = JsonConvert.DeserializeObject<AzureSearchFormat>(json);

await searchClient.UploadDocumentsAsync(documents.Value);
```

## Tips and Best Practices

1. **Use Absolute Paths**: When specifying file paths, use absolute paths to avoid confusion
2. **Output Directory**: Always specify an output directory to keep extracted media organized
3. **Batch Processing**: Process entire directories for efficiency
4. **Merge When Needed**: Use merge to combine indexes from different sources
5. **Check Output**: Always review the generated JSON to ensure quality

## Troubleshooting

### Issue: "File not found"
**Solution**: Use absolute paths or verify the file exists

### Issue: "No documents processed"
**Solution**: Check that your directory contains `.pptx`, `.pdf`, or `.md` files

### Issue: "Image extraction failed"
**Solution**: This is normal for some complex PDFs; text extraction will still work

## Command Reference

```bash
# Extract from file
dotnet run -- extract <file-path> [output-directory]

# Extract from directory
dotnet run -- extract <directory-path> [output-directory]

# Merge index files
dotnet run -- merge <file1> <file2> ... <output-file>

# Show help
dotnet run -- help
```

## Next Steps

- Integrate with Azure Cognitive Search
- Use with Azure.AI.OpenAI for semantic search
- Build a web interface for the extracted data
- Set up automated extraction pipelines
