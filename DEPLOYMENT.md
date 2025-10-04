# Azure Deployment Guide

This guide explains how to deploy your extracted search index and media files to Azure using the Azure Search Index Toolbox.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Azure Services Setup](#azure-services-setup)
- [Deployment Process](#deployment-process)
- [Command Reference](#command-reference)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

## Prerequisites

Before deploying to Azure, ensure you have:

1. **Azure Subscription**: An active Azure subscription
2. **Extracted Data**: Completed the extraction process using the `extract` command
3. **Azure Resources**: 
   - Azure Cognitive Search service
   - Azure Storage Account with Blob Storage

## Azure Services Setup

### Step 1: Create Azure Cognitive Search Service

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Click **Create a resource** > **Azure Cognitive Search**
3. Fill in the required information:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new or use existing
   - **Service name**: Choose a unique name (e.g., `my-search-service`)
   - **Location**: Select a region close to your users
   - **Pricing tier**: Select appropriate tier (Free tier available for testing)
4. Click **Review + Create** > **Create**
5. Wait for deployment to complete

### Step 2: Get Azure Cognitive Search Credentials

1. Navigate to your Search service in the Azure Portal
2. From the left menu, select **Keys**
3. Copy the following:
   - **URL**: The service endpoint (e.g., `https://my-search-service.search.windows.net`)
   - **Primary admin key**: Your API key for authentication

### Step 3: Create Azure Storage Account

1. In the Azure Portal, click **Create a resource** > **Storage account**
2. Fill in the required information:
   - **Subscription**: Select your subscription
   - **Resource Group**: Use the same resource group as your Search service
   - **Storage account name**: Choose a unique name (e.g., `mysearchstorage`)
   - **Location**: Use the same region as your Search service
   - **Performance**: Standard
   - **Redundancy**: Choose based on your needs (LRS is sufficient for testing)
3. Click **Review + Create** > **Create**
4. Wait for deployment to complete

### Step 4: Get Azure Storage Connection String

1. Navigate to your Storage Account in the Azure Portal
2. From the left menu, select **Access keys**
3. Copy the **Connection string** for key1 or key2

## Deployment Process

### Step 1: Extract Your Data

First, extract data from your documents:

```bash
cd AzureSearchIndexToolbox
dotnet run -- extract ./mydocuments ./output
```

This creates:
- `./output/search-index.json` - Search index file
- `./output/media/` - Extracted media files

### Step 2: Deploy to Azure

Deploy your extracted data to Azure:

```bash
dotnet run -- deploy "./output/search-index.json" "./output/media" "<blob-connection-string>" "<search-endpoint>" "<search-api-key>"
```

**Parameters:**
- `<json-path>`: Path to your search-index.json file
- `<media-dir>`: Path to your media directory
- `<blob-connection-string>`: Azure Storage connection string
- `<search-endpoint>`: Azure Cognitive Search endpoint URL
- `<search-api-key>`: Azure Cognitive Search admin API key
- `[container-name]`: (Optional) Blob container name (defaults to `searchindex-media`)

### Step 3: Interactive Deployment

When you run the deploy command, you'll be prompted to:

1. **Enter Index Name**: Provide a name for your search index (e.g., `documents-index`)
2. The tool will:
   - Validate Azure connections
   - Upload all media files to Blob Storage
   - Update document references with Blob URLs
   - Create/update the search index schema
   - Upload all documents to the search index

### Example Deployment Session

```
=== Azure Search Index Toolbox ===
Extracts data from PPTX, PDF, and MD files for Azure Search Index

Validating Azure connections...
✓ Blob Storage connection successful
✓ Azure Cognitive Search connection successful

Enter the Azure Search Index name to create/update: documents-index

=== Starting Azure Deployment ===

Step 1: Loading documents from JSON...
✓ Loaded 10 document(s)

Step 2: Uploading media files to Azure Blob Storage...
Found 25 unique media file(s) to upload
  ✓ Uploaded: document1_image_1.png
  ✓ Uploaded: document1_image_2.png
  ...
✓ Uploaded 25 media file(s)

Step 3: Updating document references with Blob Storage URLs...
✓ Document references updated

Step 4: Creating/updating Azure Search Index...
  Created new search index: documents-index
✓ Search index 'documents-index' ready

Step 5: Uploading documents to Azure Search Index...
  ✓ Uploaded batch of 10 document(s)
✓ Uploaded 10 document(s) to search index

=== Deployment Complete ===
Search Index: documents-index
Blob Container: searchindex-media
```

## Command Reference

### Full Deploy Command

```bash
dotnet run -- deploy <json-path> <media-dir> <blob-connection-string> <search-endpoint> <search-api-key> [container-name]
```

### Deploy with Custom Container Name

```bash
dotnet run -- deploy "./output/search-index.json" "./output/media" "DefaultEndpointsProtocol=https;AccountName=..." "https://myservice.search.windows.net" "ABC123..." "my-custom-container"
```

## What Happens During Deployment

### 1. Media File Upload

All media files (images, audio, video) are uploaded to Azure Blob Storage:
- Files are uploaded to the specified container (default: `searchindex-media`)
- Container is created if it doesn't exist
- Public access is enabled for direct file access
- Each file retains its original name

### 2. Document Reference Update

All local file paths in the search index are replaced with Blob Storage URLs:
- Images: `./output/media/image1.png` → `https://storage.blob.core.windows.net/searchindex-media/image1.png`
- Audio: Local paths → Blob URLs
- Video: Local paths → Blob URLs

### 3. Search Index Creation

The tool creates an Azure Cognitive Search index with the following schema:

| Field | Type | Properties |
|-------|------|------------|
| id | String | Key, Filterable |
| title | String | Searchable, Filterable, Sortable |
| content | String | Searchable (with English analyzer) |
| sourcePath | String | Filterable |
| fileType | String | Filterable, Facetable |
| indexedDate | DateTimeOffset | Filterable, Sortable |
| images | Collection(String) | - |
| audioFiles | Collection(String) | - |
| videoFiles | Collection(String) | - |

### 4. Document Upload

All documents are uploaded to the search index:
- Documents are uploaded in batches of 100
- Metadata is preserved
- Documents are immediately searchable

## Troubleshooting

### Connection Validation Failed

**Error**: `Azure connection validation failed`

**Solutions**:
- Verify your connection string is correct and complete
- Ensure your Search service endpoint URL is correct (includes `https://`)
- Check that your API key is the **admin key**, not a query key
- Verify your Azure services are in the **same region** for optimal performance

### File Not Found During Upload

**Error**: `Warning: File not found: <path>`

**Solutions**:
- Ensure you're providing the correct path to the media directory
- Verify that the extraction process completed successfully
- Check that the `search-index.json` file references exist

### Index Already Exists

**Info**: `Updated existing search index: <name>`

This is normal behavior. The tool will:
- Update the existing index schema if needed
- Keep existing documents unless you upload documents with the same ID

### Insufficient Permissions

**Error**: `Unauthorized` or `403 Forbidden`

**Solutions**:
- Verify you're using the **admin API key** (not a query key)
- Check that your Azure account has sufficient permissions
- Ensure the Storage Account has proper access settings

### Upload Batch Errors

**Error**: `Error uploading batch: <message>`

**Solutions**:
- Check that your search index tier supports the number of documents
- Verify document data is valid (no null required fields)
- Ensure documents don't exceed size limits (16 MB per document)

## Best Practices

### 1. Use Environment Variables

Store sensitive credentials in environment variables instead of command line:

**Windows (PowerShell):**
```powershell
$env:AZURE_STORAGE_CONNECTION="<your-connection-string>"
$env:AZURE_SEARCH_ENDPOINT="<your-search-endpoint>"
$env:AZURE_SEARCH_KEY="<your-api-key>"

dotnet run -- deploy "./output/search-index.json" "./output/media" $env:AZURE_STORAGE_CONNECTION $env:AZURE_SEARCH_ENDPOINT $env:AZURE_SEARCH_KEY
```

**Linux/Mac (Bash):**
```bash
export AZURE_STORAGE_CONNECTION="<your-connection-string>"
export AZURE_SEARCH_ENDPOINT="<your-search-endpoint>"
export AZURE_SEARCH_KEY="<your-api-key>"

dotnet run -- deploy "./output/search-index.json" "./output/media" "$AZURE_STORAGE_CONNECTION" "$AZURE_SEARCH_ENDPOINT" "$AZURE_SEARCH_KEY"
```

### 2. Test with Small Datasets

Before deploying large datasets:
1. Test with a few documents first
2. Verify the search index works correctly
3. Check that media files are accessible
4. Then deploy the full dataset

### 3. Use Consistent Naming

- Use lowercase for container names
- Use descriptive index names (e.g., `product-docs-2024`)
- Avoid special characters

### 4. Monitor Azure Costs

- Azure Cognitive Search has different pricing tiers
- Blob Storage costs depend on storage size and access patterns
- Consider using the Free tier for testing
- Monitor usage in the Azure Portal

### 5. Backup Your Data

Before deployment:
- Keep local copies of extracted data
- Export existing search indexes if updating
- Document your Azure resource configuration

### 6. Organize Multiple Deployments

For multiple projects or environments:
- Use different container names for each project
- Create separate search indexes for different datasets
- Use Azure resource groups to organize resources

### 7. Security Considerations

- **Never commit credentials to source control**
- Use Azure Key Vault for production deployments
- Rotate API keys regularly
- Use managed identities where possible
- Consider private endpoints for enhanced security

## Verifying Deployment

### 1. Check Blob Storage

1. Navigate to your Storage Account in Azure Portal
2. Select **Containers**
3. Open the container (e.g., `searchindex-media`)
4. Verify all media files are uploaded

### 2. Test Search Index

1. Navigate to your Search service in Azure Portal
2. Select **Indexes**
3. Click on your index name
4. Click **Search explorer**
5. Try a simple search query: `*` (returns all documents)
6. Test specific searches: `search=<keyword>`

### 3. Access Media Files

Test that media files are publicly accessible:
```
https://<storage-account>.blob.core.windows.net/<container-name>/<filename>
```

Example:
```
https://mysearchstorage.blob.core.windows.net/searchindex-media/document1_image_1.png
```

## Advanced Scenarios

### Updating an Existing Index

To update documents in an existing index:
1. Run the deploy command with the same index name
2. Documents with matching IDs will be updated
3. New documents will be added
4. Existing documents not in the new set remain unchanged

### Deploying Multiple Indexes

Deploy different document sets to separate indexes:

```bash
# Deploy technical documentation
dotnet run -- deploy "./tech-docs/search-index.json" "./tech-docs/media" "..." "..." "..." "tech-docs-media"

# Deploy marketing materials
dotnet run -- deploy "./marketing/search-index.json" "./marketing/media" "..." "..." "..." "marketing-media"
```

### Integration with CI/CD

Create a deployment script for automation:

```bash
#!/bin/bash
# deploy-to-azure.sh

JSON_PATH="./output/search-index.json"
MEDIA_DIR="./output/media"
INDEX_NAME="automated-index"

# Load credentials from secure storage
BLOB_CONN=$(az keyvault secret show --name blob-connection --vault-name myvault --query value -o tsv)
SEARCH_ENDPOINT=$(az keyvault secret show --name search-endpoint --vault-name myvault --query value -o tsv)
SEARCH_KEY=$(az keyvault secret show --name search-key --vault-name myvault --query value -o tsv)

# Deploy
dotnet run -- deploy "$JSON_PATH" "$MEDIA_DIR" "$BLOB_CONN" "$SEARCH_ENDPOINT" "$SEARCH_KEY"
```

## Next Steps

After successful deployment:

1. **Configure Search Features**:
   - Set up analyzers for better search results
   - Configure scoring profiles
   - Add synonym maps

2. **Build Applications**:
   - Create a web interface for searching
   - Integrate with Azure.AI.OpenAI for semantic search
   - Build mobile apps using the search API

3. **Monitor and Optimize**:
   - Use Azure Monitor for performance tracking
   - Analyze search queries
   - Optimize index configuration

4. **Enhance Security**:
   - Implement authentication
   - Use Azure AD for identity management
   - Set up private endpoints

## Support and Resources

- [Azure Cognitive Search Documentation](https://docs.microsoft.com/azure/search/)
- [Azure Blob Storage Documentation](https://docs.microsoft.com/azure/storage/blobs/)
- [Azure SDK for .NET](https://docs.microsoft.com/dotnet/azure/)
- [Azure Search REST API](https://docs.microsoft.com/rest/api/searchservice/)

## Related Documentation

- [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md) - General usage examples
- [README.md](README.md) - Project overview
- [SOLUTION_SUMMARY.md](SOLUTION_SUMMARY.md) - Technical architecture
