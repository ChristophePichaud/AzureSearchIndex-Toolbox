# Quick Start Guide

Get started with the Azure Search Index Toolbox in 5 minutes!

## Prerequisites

- .NET 8.0 SDK or higher ([Download here](https://dotnet.microsoft.com/download))
- Git (for cloning the repository)
- A terminal/command prompt

## Step 1: Clone and Build

```bash
# Clone the repository
git clone https://github.com/ChristophePichaud/AzureSearchIndex-Toolbox.git

# Navigate to the project
cd AzureSearchIndex-Toolbox/AzureSearchIndexToolbox

# Build the project
dotnet build
```

Expected output:
```
Build succeeded in X.Xs
```

## Step 2: Test with a Sample File

Create a simple markdown file to test:

```bash
# Create a test file
echo "# My First Document" > test.md
echo "" >> test.md
echo "This is a test document for the Azure Search Index Toolbox." >> test.md

# Extract data from it
dotnet run -- extract test.md
```

You should see:
```
=== Azure Search Index Toolbox ===
Processing: test.md
  ✓ Successfully extracted from test.md
    - Title: My First Document
    - Content length: XX characters
    ...
```

## Step 3: Check the Output

```bash
# View the generated JSON
cat ./output/search-index.json
```

The JSON file will contain your extracted data in Azure Search Index format!

## Step 4: Extract from Your Documents

Now try with your own files:

```bash
# Extract from a single file
dotnet run -- extract /path/to/your/presentation.pptx ./my-output

# Or extract from an entire directory
dotnet run -- extract /path/to/documents ./my-output
```

## What Gets Extracted?

### From PowerPoint (.pptx)
- ✅ All text from slides
- ✅ Images
- ✅ Audio files (MP3, WAV)
- ✅ Video files (MP4, AVI, WMV)
- ✅ Metadata (author, dates, slide count)

### From PDF (.pdf)
- ✅ All text content
- ✅ Images
- ✅ Metadata (author, title, keywords)

### From Markdown (.md)
- ✅ All text content
- ✅ Titles and headings
- ✅ Image references
- ✅ Table of contents

## Output Format

The tool creates a JSON file in Azure Search Index format:

```json
{
  "value": [
    {
      "id": "unique-guid",
      "title": "Document Title",
      "content": "Your extracted text...",
      "images": ["image1.png"],
      "audioFiles": ["audio1.mp3"],
      "videoFiles": ["video1.mp4"],
      "metadata": { ... }
    }
  ]
}
```

## Common Commands

```bash
# Get help
dotnet run -- help

# Extract from a single file
dotnet run -- extract file.pptx

# Extract from a directory
dotnet run -- extract ./docs ./output

# Merge multiple index files
dotnet run -- merge index1.json index2.json merged.json
```

## Next Steps

1. **Read the detailed documentation**: See [README.md](./AzureSearchIndexToolbox/README.md)
2. **Explore examples**: Check [USAGE_EXAMPLES.md](./USAGE_EXAMPLES.md)
3. **Understand the architecture**: Read [SOLUTION_SUMMARY.md](./SOLUTION_SUMMARY.md)
4. **Integrate with Azure**: Follow the Azure integration guide in USAGE_EXAMPLES.md

## Troubleshooting

### Build fails
Make sure you have .NET 8.0 SDK installed:
```bash
dotnet --version
```

### "File not found" error
Use absolute paths:
```bash
dotnet run -- extract /full/path/to/file.pptx
```

### No output generated
Check that your directory contains `.pptx`, `.pdf`, or `.md` files

## Need Help?

- Check the [comprehensive documentation](./AzureSearchIndexToolbox/README.md)
- Review [usage examples](./USAGE_EXAMPLES.md)
- Examine the [solution architecture](./SOLUTION_SUMMARY.md)

## Success! 🎉

You're now ready to extract data from your documents and create Azure Search Index files!

Happy indexing! 🚀
