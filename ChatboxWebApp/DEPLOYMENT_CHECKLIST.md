# Deployment Checklist for Web Chatbox

Use this checklist to ensure a successful deployment of the Azure Search ChatGPT Web Chatbox.

## Pre-Deployment Checklist

### Prerequisites
- [ ] .NET 8.0 SDK installed
- [ ] PostgreSQL database server available (local or cloud)
- [ ] Azure OpenAI account with API key and deployed GPT model
- [ ] Azure Cognitive Search service with indexed documents
- [ ] IIS 10.0 or later installed (for production)
- [ ] ASP.NET Core Hosting Bundle 8.0 installed on IIS server

### Azure Resources
- [ ] Azure OpenAI resource created
- [ ] GPT model deployed (e.g., gpt-35-turbo, gpt-4)
- [ ] API key obtained from Azure OpenAI
- [ ] Endpoint URL noted (https://your-resource.openai.azure.com/)
- [ ] Azure Cognitive Search service created
- [ ] Search service admin API key obtained
- [ ] Search index created and populated with documents

### Database Setup
- [ ] PostgreSQL server installed/accessible
- [ ] Database created (e.g., `chatgpt_conversations`)
- [ ] User account with CREATE TABLE permissions
- [ ] Connection string prepared
- [ ] Network access configured (firewall rules if needed)

## Configuration Checklist

### Step 1: Clone or Download Code
- [ ] Code downloaded from repository
- [ ] Navigate to `ChatboxWebApp` directory
- [ ] Both projects present: ChatboxWebApp and ChatboxWebApp.Client

### Step 2: Create Configuration File
- [ ] Copy `chatgpt-config.template.json` to `chatgpt-config.json`
- [ ] OR configure settings in `appsettings.json`

### Step 3: Fill Configuration Values
- [ ] `apiKey` - Azure OpenAI API key entered
- [ ] `endpoint` - Azure OpenAI endpoint URL entered
- [ ] `deploymentName` - Model deployment name entered
- [ ] `searchEndpoint` - Azure Search endpoint entered
- [ ] `searchApiKey` - Azure Search API key entered
- [ ] `searchIndexName` - Search index name entered
- [ ] `postgresConnectionString` - PostgreSQL connection string entered
- [ ] `maxQuestionsCount` - Set desired limit (e.g., 10)
- [ ] `systemContext` - Customize assistant personality (optional)

### Step 4: Verify Configuration
- [ ] All required fields have values (no "YOUR_" placeholders)
- [ ] Connection strings are properly formatted
- [ ] API keys are valid and not expired
- [ ] Endpoints use HTTPS

## Local Testing Checklist

### Build and Run
- [ ] Navigate to `ChatboxWebApp` directory
- [ ] Run `dotnet restore` (if needed)
- [ ] Run `dotnet build` - succeeds with no errors
- [ ] Run `dotnet run --project ChatboxWebApp`
- [ ] Application starts without errors
- [ ] Note the URLs (http://localhost:5001 and https://localhost:7001)

### Test Functionality
- [ ] Open browser to http://localhost:5001/chatbox
- [ ] Chatbox page loads successfully
- [ ] Conversation ID displayed
- [ ] Question count shows (e.g., "0/10")

#### Test Single Question Mode
- [ ] Select "Single Question" mode
- [ ] Enter a test question
- [ ] Click "Send Question"
- [ ] Response received with answer
- [ ] Citations displayed (if available)
- [ ] Question count incremented (e.g., "1/10")

#### Test Multiple Questions Mode
- [ ] Select "Multiple Questions" mode
- [ ] Enter 2-3 questions
- [ ] Click "Add Question" to add more fields
- [ ] Click "Send All Questions"
- [ ] All responses received
- [ ] Question count updated correctly

#### Test Conversation Management
- [ ] Click "Reset" button
- [ ] Message history clears
- [ ] Conversation ID stays the same
- [ ] Question count resets to 0
- [ ] Click "New Conversation" button
- [ ] New conversation ID generated
- [ ] Question count resets to 0

#### Test Error Handling
- [ ] Try sending empty question - error displayed
- [ ] Try exceeding max questions - error displayed
- [ ] Disconnect PostgreSQL - error handled gracefully

### Database Verification
- [ ] Connect to PostgreSQL database
- [ ] Verify `conversation_history` table exists
- [ ] Check that questions and answers are being saved
- [ ] Verify conversation IDs are stored correctly

## IIS Deployment Checklist

### IIS Server Preparation
- [ ] ASP.NET Core Hosting Bundle 8.0 installed
- [ ] IIS restarted after hosting bundle installation
- [ ] .NET 8.0 Runtime installed
- [ ] Application pool created or identified

### Build for Production
- [ ] Open command prompt/terminal
- [ ] Navigate to ChatboxWebApp directory
- [ ] Run: `dotnet publish ChatboxWebApp -c Release -o C:\inetpub\wwwroot\chatbox`
- [ ] Build succeeds
- [ ] Published files created in output directory

### Configuration Files
- [ ] Copy `chatgpt-config.json` to deployment directory
- [ ] OR ensure `appsettings.json` has production settings
- [ ] Verify `web.config` is present
- [ ] Update environment variables in `web.config` if needed

### IIS Site Creation
- [ ] Open IIS Manager
- [ ] Right-click "Sites" → "Add Website"
- [ ] Site name: ChatboxWebApp (or your choice)
- [ ] Physical path: C:\inetpub\wwwroot\chatbox
- [ ] Binding type: http or https
- [ ] Port: 80 (or custom)
- [ ] Host name: (optional)
- [ ] Click OK

### Application Pool Configuration
- [ ] Select Application Pools → ChatboxWebApp
- [ ] Set .NET CLR version: **No Managed Code**
- [ ] Set Managed pipeline mode: **Integrated**
- [ ] Set Identity to appropriate account
- [ ] Set Start Mode: **AlwaysRunning** (optional)
- [ ] Click Apply

### Permissions
- [ ] Right-click deployment folder → Properties → Security
- [ ] Add IIS_IUSRS group
- [ ] Grant Read & Execute permissions
- [ ] Apply changes

### Firewall Configuration (if needed)
- [ ] Open Windows Firewall
- [ ] Create inbound rule for port 80/443
- [ ] Allow traffic from appropriate sources

### Test IIS Deployment
- [ ] Browse to site URL (e.g., http://localhost or http://yourserver)
- [ ] Home page loads
- [ ] Navigate to /chatbox
- [ ] Chatbox interface loads
- [ ] Test single question functionality
- [ ] Test multiple questions functionality
- [ ] Test conversation management
- [ ] Verify responses are correct

### Troubleshooting (if needed)
- [ ] Check IIS logs in C:\inetpub\logs\LogFiles
- [ ] Enable stdout logging in web.config
- [ ] Check application logs
- [ ] Verify database connectivity from IIS server
- [ ] Verify Azure OpenAI API access from IIS server
- [ ] Check application pool is running
- [ ] Restart application pool if needed

## Production Readiness Checklist

### Security
- [ ] HTTPS enabled with valid SSL certificate
- [ ] API keys not in source control
- [ ] Configuration files secured with proper permissions
- [ ] Database credentials encrypted or in secure vault
- [ ] CORS configured for production (not AllowAll)
- [ ] Consider implementing authentication/authorization

### Performance
- [ ] Application pool set to AlwaysRunning
- [ ] Response caching configured (if applicable)
- [ ] Database indexes verified
- [ ] Connection pooling enabled
- [ ] Static file compression enabled in IIS

### Monitoring
- [ ] Logging configured in appsettings.json
- [ ] Application Insights set up (optional)
- [ ] Health checks configured (optional)
- [ ] Error notification system in place
- [ ] Database monitoring configured

### Backup
- [ ] Database backup schedule configured
- [ ] Configuration files backed up
- [ ] Deployment process documented
- [ ] Rollback procedure defined

### Documentation
- [ ] Deployment notes recorded
- [ ] Configuration settings documented
- [ ] Known issues documented
- [ ] Contact information for support updated

## Post-Deployment Verification

### Functional Testing
- [ ] Test all features in production environment
- [ ] Verify question submission works
- [ ] Check citation display
- [ ] Test conversation management
- [ ] Verify database persistence
- [ ] Test from different browsers
- [ ] Test from different devices (mobile, tablet)

### Performance Testing
- [ ] Check response times are acceptable
- [ ] Test with multiple concurrent users (if applicable)
- [ ] Monitor server resources (CPU, memory)
- [ ] Check database query performance

### User Acceptance
- [ ] Have users test the application
- [ ] Collect feedback
- [ ] Document any issues
- [ ] Plan for improvements

## Maintenance Checklist

### Regular Tasks
- [ ] Monitor application logs weekly
- [ ] Review database size monthly
- [ ] Check for Azure service updates
- [ ] Update NuGet packages quarterly
- [ ] Backup configuration files regularly
- [ ] Test disaster recovery procedure quarterly

### Updates
- [ ] Plan for .NET updates
- [ ] Monitor Azure OpenAI API changes
- [ ] Track Azure Search API updates
- [ ] Update documentation as needed

## Rollback Procedure

If deployment fails:
- [ ] Stop IIS site
- [ ] Restore previous version from backup
- [ ] Restore configuration files
- [ ] Restore database (if schema changed)
- [ ] Test restored version
- [ ] Document issues encountered

## Support Contacts

Document your support contacts:
- Azure OpenAI Support: _______________________
- Azure Support: _______________________
- Database Admin: _______________________
- DevOps Team: _______________________
- Application Owner: _______________________

## Sign-Off

- [ ] Development team approves
- [ ] QA team approves
- [ ] Operations team approves
- [ ] Security team approves (if required)
- [ ] Product owner approves

**Deployment Date:** _______________
**Deployed By:** _______________
**Version:** _______________
**Notes:** _______________

---

## Quick Reference

**Local Development:**
```bash
cd ChatboxWebApp
dotnet run --project ChatboxWebApp
# Browse to http://localhost:5001/chatbox
```

**Build for Production:**
```bash
dotnet publish ChatboxWebApp -c Release -o C:\inetpub\wwwroot\chatbox
```

**Key URLs:**
- Development: http://localhost:5001/chatbox
- Production: http://yourserver/chatbox
- API Base: /api/chat

**Configuration Location:**
- Development: ChatboxWebApp/chatgpt-config.json
- Production: C:\inetpub\wwwroot\chatbox\chatgpt-config.json

**Logs Location:**
- IIS: C:\inetpub\logs\LogFiles
- Application: C:\inetpub\wwwroot\chatbox\logs (if stdout enabled)

**Database:**
- Table: conversation_history
- Connection: Check PostgreSQL connection string in config

---

✅ **Checklist Complete** - Ready for deployment!
