# Quick Deployment Reference

This is a quick reference for deploying extracted data to Azure. For complete documentation, see [DEPLOYMENT.md](./DEPLOYMENT.md).

## Prerequisites

1. Azure Subscription
2. Azure Cognitive Search service
3. Azure Storage Account with Blob Storage
4. Extracted data (from `extract` command)

## Quick Start

### 1. Get Azure Credentials

**Azure Cognitive Search:**
- Navigate to your Search service in Azure Portal
- Go to **Keys** section
- Copy:
  - **URL** (e.g., `https://my-search-service.search.windows.net`)
  - **Primary admin key**

**Azure Storage:**
- Navigate to your Storage Account in Azure Portal
- Go to **Access keys** section
- Copy the **Connection string**

### 2. Run Deployment Command

```bash
cd AzureSearchIndexToolbox
dotnet run -- deploy <json-path> <media-dir> <blob-connection> <search-endpoint> <search-key>
```

**Example:**
```bash
dotnet run -- deploy "./output/search-index.json" "./output/media" "DefaultEndpointsProtocol=https;AccountName=mystorage;AccountKey=ABC123..." "https://myservice.search.windows.net" "XYZ789..."
```

### 3. Enter Index Name

When prompted, enter a name for your search index (e.g., `documents-index`)

## What Happens

1. ✅ Validates Azure connections
2. ✅ Uploads all media files to Blob Storage
3. ✅ Updates document references with Blob URLs
4. ✅ Creates/updates search index schema
5. ✅ Uploads documents to search index

## Testing Your Deployment

### Test Blob Storage
Visit: `https://<storage-account>.blob.core.windows.net/<container-name>/<filename>`

### Test Search Index
1. Go to Azure Portal → Your Search Service
2. Click **Search explorer**
3. Try search query: `*` (returns all documents)

## Environment Variables (Recommended)

**Windows PowerShell:**
```powershell
$env:AZURE_STORAGE_CONNECTION="<your-connection-string>"
$env:AZURE_SEARCH_ENDPOINT="<your-endpoint>"
$env:AZURE_SEARCH_KEY="<your-api-key>"

dotnet run -- deploy "./output/search-index.json" "./output/media" $env:AZURE_STORAGE_CONNECTION $env:AZURE_SEARCH_ENDPOINT $env:AZURE_SEARCH_KEY
```

**Linux/Mac:**
```bash
export AZURE_STORAGE_CONNECTION="<your-connection-string>"
export AZURE_SEARCH_ENDPOINT="<your-endpoint>"
export AZURE_SEARCH_KEY="<your-api-key>"

dotnet run -- deploy "./output/search-index.json" "./output/media" "$AZURE_STORAGE_CONNECTION" "$AZURE_SEARCH_ENDPOINT" "$AZURE_SEARCH_KEY"
```

## Common Issues

### Connection validation failed
- Verify connection string is complete
- Check endpoint URL includes `https://`
- Ensure you're using admin key (not query key)

### File not found during upload
- Verify media directory path is correct
- Check extraction completed successfully

### Insufficient permissions
- Use admin API key for deployment
- Verify Azure account permissions

## Complete Workflow Example

```bash
# Step 1: Extract data
dotnet run -- extract ./documents ./output

# Step 2: Deploy to Azure
dotnet run -- deploy "./output/search-index.json" "./output/media" "<blob-conn>" "<search-endpoint>" "<search-key>"

# Step 3: Verify in Azure Portal
# - Check Blob Storage container for media files
# - Check Search Index for documents
# - Test search queries
```

## Next Steps

- Set up automated deployments with CI/CD
- Integrate search into your application
- Configure advanced search features (analyzers, scoring profiles)
- Monitor usage with Azure Monitor

## Full Documentation

- [DEPLOYMENT.md](./DEPLOYMENT.md) - Complete deployment guide
- [USAGE_EXAMPLES.md](./USAGE_EXAMPLES.md) - Usage examples
- [README.md](./README.md) - Project overview
