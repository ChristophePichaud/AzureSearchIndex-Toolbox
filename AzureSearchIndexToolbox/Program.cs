using AzureSearchIndexToolbox.Extractors;
using AzureSearchIndexToolbox.Services;
using AzureSearchIndexToolbox.Models;

namespace AzureSearchIndexToolbox
{
    /// <summary>
    /// Azure Search Index Toolbox
    /// Extracts data from PPTX, PDF, and MD files to create Azure Search Index files.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Azure Search Index Toolbox ===");
            Console.WriteLine("Extracts data from PPTX, PDF, and MD files for Azure Search Index");
            Console.WriteLine();

            // Check if arguments are provided
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            // Parse command line arguments
            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "extract":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Please provide a file or directory path.");
                            ShowUsage();
                            return;
                        }
                        string inputPath = args[1];
                        string outputDir = args.Length > 2 ? args[2] : "./output";
                        ExtractFromPath(inputPath, outputDir);
                        break;

                    case "merge":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Error: Please provide input files and output file path.");
                            ShowUsage();
                            return;
                        }
                        var inputFiles = args.Skip(1).Take(args.Length - 2).ToList();
                        string mergeOutput = args[args.Length - 1];
                        MergeIndexFiles(inputFiles, mergeOutput);
                        break;

                    case "deploy":
                        if (args.Length < 6)
                        {
                            Console.WriteLine("Error: Please provide all required parameters for deployment.");
                            ShowUsage();
                            return;
                        }
                        DeployToAzure(args[1], args[2], args[3], args[4], args[5], args.Length > 6 ? args[6] : "searchindex-media");
                        break;

                    case "chatgpt":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Please provide a configuration file path.");
                            ShowUsage();
                            return;
                        }
                        RunChatGptInteractive(args[1]).GetAwaiter().GetResult();
                        break;

                    case "help":
                    case "--help":
                    case "-h":
                        ShowUsage();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Extracts data from a file or directory and creates Azure Search Index files.
        /// Processes PPTX, PDF, and MD files found in the specified path.
        /// </summary>
        /// <param name="inputPath">Path to a file or directory</param>
        /// <param name="outputDirectory">Directory where extracted data and JSON files will be saved</param>
        static void ExtractFromPath(string inputPath, string outputDirectory)
        {
            Console.WriteLine($"Processing: {inputPath}");
            Console.WriteLine($"Output directory: {outputDirectory}");
            Console.WriteLine();

            // Create output directory if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Create a subdirectory for extracted media files
            string mediaOutputDir = Path.Combine(outputDirectory, "media");
            if (!Directory.Exists(mediaOutputDir))
            {
                Directory.CreateDirectory(mediaOutputDir);
            }

            var documents = new List<SearchIndexDocument>();

            // Check if input is a file or directory
            if (File.Exists(inputPath))
            {
                // Process single file
                var document = ProcessFile(inputPath, mediaOutputDir);
                if (document != null)
                {
                    documents.Add(document);
                }
            }
            else if (Directory.Exists(inputPath))
            {
                // Process all supported files in directory
                var supportedExtensions = new[] { "*.pptx", "*.pdf", "*.md" };
                
                foreach (var extension in supportedExtensions)
                {
                    var files = Directory.GetFiles(inputPath, extension, SearchOption.AllDirectories);
                    Console.WriteLine($"Found {files.Length} {extension} file(s)");
                    
                    foreach (var file in files)
                    {
                        var document = ProcessFile(file, mediaOutputDir);
                        if (document != null)
                        {
                            documents.Add(document);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Path not found: {inputPath}");
                return;
            }

            // Save all documents to a JSON file
            if (documents.Count > 0)
            {
                var indexService = new AzureSearchIndexService();
                string jsonOutputPath = Path.Combine(outputDirectory, "search-index.json");
                indexService.SaveToJson(documents, jsonOutputPath);

                Console.WriteLine();
                Console.WriteLine($"=== Extraction Complete ===");
                Console.WriteLine($"Total documents processed: {documents.Count}");
                Console.WriteLine($"Search index saved to: {jsonOutputPath}");
                Console.WriteLine($"Media files saved to: {mediaOutputDir}");
            }
            else
            {
                Console.WriteLine("No documents were processed.");
            }
        }

        /// <summary>
        /// Processes a single file based on its extension.
        /// Routes to the appropriate extractor (PPTX, PDF, or MD).
        /// </summary>
        /// <param name="filePath">Path to the file to process</param>
        /// <param name="mediaOutputDir">Directory where media files will be saved</param>
        /// <returns>Extracted SearchIndexDocument or null if processing failed</returns>
        static SearchIndexDocument? ProcessFile(string filePath, string mediaOutputDir)
        {
            try
            {
                Console.WriteLine($"Processing: {Path.GetFileName(filePath)}");
                
                string extension = Path.GetExtension(filePath).ToLower();
                SearchIndexDocument? document = null;

                switch (extension)
                {
                    case ".pptx":
                        document = ExtractFromPptx(filePath, mediaOutputDir);
                        break;

                    case ".pdf":
                        document = ExtractFromPdf(filePath, mediaOutputDir);
                        break;

                    case ".md":
                        document = ExtractFromMarkdown(filePath, mediaOutputDir);
                        break;

                    default:
                        Console.WriteLine($"  Unsupported file type: {extension}");
                        return null;
                }

                if (document != null)
                {
                    // Validate the document
                    var indexService = new AzureSearchIndexService();
                    if (indexService.ValidateDocument(document))
                    {
                        Console.WriteLine($"  ✓ Successfully extracted from {Path.GetFileName(filePath)}");
                        Console.WriteLine($"    - Title: {document.Title}");
                        Console.WriteLine($"    - Content length: {document.Content.Length} characters");
                        Console.WriteLine($"    - Images: {document.Images.Count}");
                        Console.WriteLine($"    - Audio files: {document.AudioFiles.Count}");
                        Console.WriteLine($"    - Video files: {document.VideoFiles.Count}");
                        return document;
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Document validation failed for {Path.GetFileName(filePath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error processing {Path.GetFileName(filePath)}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Extracts data from a PowerPoint (PPTX) file.
        /// Extracts text, titles, images, audio, and video files.
        /// </summary>
        /// <param name="filePath">Path to the PPTX file</param>
        /// <param name="mediaOutputDir">Directory for extracted media</param>
        /// <returns>Extracted SearchIndexDocument</returns>
        static SearchIndexDocument ExtractFromPptx(string filePath, string mediaOutputDir)
        {
            var extractor = new PptxExtractor();
            return extractor.ExtractData(filePath, mediaOutputDir);
        }

        /// <summary>
        /// Extracts data from a PDF file.
        /// Extracts text and images.
        /// </summary>
        /// <param name="filePath">Path to the PDF file</param>
        /// <param name="mediaOutputDir">Directory for extracted media</param>
        /// <returns>Extracted SearchIndexDocument</returns>
        static SearchIndexDocument ExtractFromPdf(string filePath, string mediaOutputDir)
        {
            var extractor = new PdfExtractor();
            return extractor.ExtractData(filePath, mediaOutputDir);
        }

        /// <summary>
        /// Extracts data from a Markdown (MD) file.
        /// Extracts text, titles, and image references.
        /// </summary>
        /// <param name="filePath">Path to the MD file</param>
        /// <param name="mediaOutputDir">Directory for extracted media</param>
        /// <returns>Extracted SearchIndexDocument</returns>
        static SearchIndexDocument ExtractFromMarkdown(string filePath, string mediaOutputDir)
        {
            var extractor = new MarkdownExtractor();
            return extractor.ExtractData(filePath, mediaOutputDir);
        }

        /// <summary>
        /// Merges multiple Azure Search Index JSON files into a single file.
        /// </summary>
        /// <param name="inputFiles">List of input JSON files to merge</param>
        /// <param name="outputFile">Path for the merged output file</param>
        static void MergeIndexFiles(List<string> inputFiles, string outputFile)
        {
            Console.WriteLine("Merging index files...");
            var indexService = new AzureSearchIndexService();
            indexService.MergeIndexFiles(inputFiles, outputFile);
        }

        /// <summary>
        /// Deploys the search index and media files to Azure.
        /// </summary>
        /// <param name="jsonIndexPath">Path to the search-index.json file</param>
        /// <param name="mediaDirectory">Path to the media directory</param>
        /// <param name="blobConnectionString">Azure Blob Storage connection string</param>
        /// <param name="searchEndpoint">Azure Cognitive Search service endpoint</param>
        /// <param name="searchApiKey">Azure Cognitive Search admin API key</param>
        /// <param name="containerName">Name of the blob container (optional, defaults to 'searchindex-media')</param>
        static void DeployToAzure(string jsonIndexPath, string mediaDirectory, string blobConnectionString, 
            string searchEndpoint, string searchApiKey, string containerName = "searchindex-media")
        {
            try
            {
                // Validate input paths
                if (!File.Exists(jsonIndexPath))
                {
                    Console.WriteLine($"Error: JSON index file not found: {jsonIndexPath}");
                    return;
                }

                if (!Directory.Exists(mediaDirectory))
                {
                    Console.WriteLine($"Error: Media directory not found: {mediaDirectory}");
                    return;
                }

                // Prompt for index name
                Console.Write("Enter the Azure Search Index name to create/update: ");
                string? indexName = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(indexName))
                {
                    Console.WriteLine("Error: Index name is required.");
                    return;
                }

                // Create deployment service
                var deploymentService = new AzureDeploymentService(
                    blobConnectionString,
                    searchEndpoint,
                    searchApiKey,
                    containerName
                );

                // Validate connections
                Console.WriteLine("Validating Azure connections...");
                var isValid = deploymentService.ValidateConnectionsAsync().GetAwaiter().GetResult();
                
                if (!isValid)
                {
                    Console.WriteLine("Error: Azure connection validation failed. Please check your credentials.");
                    return;
                }

                Console.WriteLine();

                // Deploy to Azure
                deploymentService.DeployAsync(jsonIndexPath, mediaDirectory, indexName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deployment failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Runs an interactive ChatGPT session with Azure Search Index integration.
        /// </summary>
        /// <param name="configFilePath">Path to the ChatGPT configuration JSON file</param>
        static async Task RunChatGptInteractive(string configFilePath)
        {
            Console.WriteLine("=== ChatGPT with Azure Search Index ===");
            Console.WriteLine();

            // Load configuration
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Error: Configuration file not found: {configFilePath}");
                Console.WriteLine("Please create a chatgpt-config.json file with your Azure OpenAI and Search credentials.");
                return;
            }

            ChatGptConfiguration? config;
            try
            {
                string configJson = File.ReadAllText(configFilePath);
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatGptConfiguration>(configJson);
                
                if (config == null)
                {
                    Console.WriteLine("Error: Failed to parse configuration file.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                return;
            }

            // Initialize ChatGPT service
            ChatGptService? chatService = null;
            try
            {
                chatService = new ChatGptService(config);
                Console.WriteLine("✓ ChatGPT service initialized");
                Console.WriteLine($"✓ Using model: {config.DeploymentName}");
                Console.WriteLine($"✓ Connected to search index: {config.SearchIndexName}");
                Console.WriteLine($"✓ Conversation ID: {chatService.GetCurrentConversationId()}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ChatGPT service: {ex.Message}");
                return;
            }

            // Interactive loop
            bool running = true;
            while (running)
            {
                Console.WriteLine("\nCommands:");
                Console.WriteLine("  ask - Ask a single question");
                Console.WriteLine("  multi - Ask multiple questions");
                Console.WriteLine("  new - Start a new conversation");
                Console.WriteLine("  continue - Continue an existing conversation");
                Console.WriteLine("  reset - Reset current conversation");
                Console.WriteLine("  history - View conversation history");
                Console.WriteLine("  exit - Exit the program");
                Console.Write("\nEnter command: ");
                
                string? command = Console.ReadLine()?.ToLower().Trim();

                try
                {
                    switch (command)
                    {
                        case "ask":
                            await HandleAskQuestion(chatService);
                            break;

                        case "multi":
                            await HandleMultipleQuestions(chatService);
                            break;

                        case "new":
                            string newConvId = chatService.StartNewConversation();
                            Console.WriteLine($"Started new conversation: {newConvId}");
                            break;

                        case "continue":
                            await HandleContinueConversation(chatService);
                            break;

                        case "reset":
                            chatService.ResetConversation();
                            Console.WriteLine("Conversation reset.");
                            break;

                        case "history":
                            await HandleViewHistory(chatService);
                            break;

                        case "exit":
                            running = false;
                            Console.WriteLine("Exiting...");
                            break;

                        default:
                            Console.WriteLine("Unknown command. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            chatService?.Dispose();
        }

        /// <summary>
        /// Handles asking a single question.
        /// </summary>
        static async Task HandleAskQuestion(ChatGptService chatService)
        {
            Console.Write("\nEnter your question: ");
            string? question = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(question))
            {
                Console.WriteLine("Question cannot be empty.");
                return;
            }

            Console.WriteLine("\nProcessing...");
            var response = await chatService.AskQuestionAsync(question);
            
            DisplayResponse(response);
        }

        /// <summary>
        /// Handles asking multiple questions.
        /// </summary>
        static async Task HandleMultipleQuestions(ChatGptService chatService)
        {
            Console.WriteLine("\nEnter questions (one per line). Type 'done' when finished:");
            
            var questions = new List<string>();
            while (true)
            {
                Console.Write($"Question {questions.Count + 1}: ");
                string? question = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(question) || question.ToLower() == "done")
                {
                    break;
                }
                
                questions.Add(question);
            }

            if (questions.Count == 0)
            {
                Console.WriteLine("No questions entered.");
                return;
            }

            Console.WriteLine("\nProcessing...");
            var response = await chatService.AskQuestionsAsync(questions);
            
            DisplayResponse(response);
        }

        /// <summary>
        /// Handles continuing an existing conversation.
        /// </summary>
        static async Task HandleContinueConversation(ChatGptService chatService)
        {
            Console.Write("\nEnter conversation ID: ");
            string? conversationId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                Console.WriteLine("Conversation ID cannot be empty.");
                return;
            }

            await chatService.ContinueConversationAsync(conversationId);
        }

        /// <summary>
        /// Handles viewing conversation history.
        /// </summary>
        static async Task HandleViewHistory(ChatGptService chatService)
        {
            var history = await chatService.GetConversationHistoryAsync();

            if (history.Count == 0)
            {
                Console.WriteLine("\nNo conversation history found.");
                return;
            }

            Console.WriteLine($"\n=== Conversation History ({history.Count} exchanges) ===\n");
            
            foreach (var entry in history)
            {
                Console.WriteLine($"[{entry.CreatedAt:yyyy-MM-dd HH:mm:ss}] Q{entry.SequenceNumber}:");
                Console.WriteLine($"Q: {entry.Question}");
                Console.WriteLine($"A: {entry.Answer}");
                
                if (!string.IsNullOrWhiteSpace(entry.Citations))
                {
                    var citations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DocumentCitation>>(entry.Citations);
                    if (citations != null && citations.Count > 0)
                    {
                        Console.WriteLine($"   Sources: {citations.Count} document(s)");
                    }
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Displays a ChatGPT response with formatting.
        /// </summary>
        static void DisplayResponse(ChatGptResponse response)
        {
            Console.WriteLine($"\n=== Response (Conversation: {response.ConversationId}) ===\n");

            foreach (var answer in response.Answers)
            {
                Console.WriteLine($"Question: {answer.Question}");
                Console.WriteLine($"\nAnswer: {answer.Answer}");
                
                if (answer.Citations.Count > 0)
                {
                    Console.WriteLine($"\n--- Sources and Citations ({answer.Citations.Count} document(s)) ---");
                    
                    for (int i = 0; i < answer.Citations.Count; i++)
                    {
                        var citation = answer.Citations[i];
                        Console.WriteLine($"\n[{i + 1}] {citation.Title}");
                        Console.WriteLine($"    Source: {citation.SourcePath}");
                        Console.WriteLine($"    Type: {citation.FileType}");
                        Console.WriteLine($"    Relevance Score: {citation.Score:F4}");
                    }
                    
                    Console.WriteLine("\nHow the answer was found:");
                    Console.WriteLine("The assistant searched the Azure Search Index for relevant documents based on your question,");
                    Console.WriteLine("retrieved the most relevant content, and used it as context to generate the answer.");
                    Console.WriteLine("The citations above show which documents were used and their relevance scores.");
                }
                else
                {
                    Console.WriteLine("\nNo specific documents were found in the search index for this question.");
                    Console.WriteLine("The answer was generated based on the model's general knowledge.");
                }
                
                Console.WriteLine("\n" + new string('=', 80));
            }
        }

        /// <summary>
        /// Displays usage information for the application.
        /// </summary>
        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  AzureSearchIndexToolbox extract <file-or-directory> [output-directory]");
            Console.WriteLine("    Extracts data from PPTX, PDF, or MD files and creates a search index");
            Console.WriteLine();
            Console.WriteLine("  AzureSearchIndexToolbox merge <file1> <file2> ... <output-file>");
            Console.WriteLine("    Merges multiple search index JSON files into one");
            Console.WriteLine();
            Console.WriteLine("  AzureSearchIndexToolbox deploy <json-path> <media-dir> <blob-conn> <search-endpoint> <search-key> [container-name]");
            Console.WriteLine("    Deploys search index and media files to Azure");
            Console.WriteLine();
            Console.WriteLine("  AzureSearchIndexToolbox chatgpt <config-file-path>");
            Console.WriteLine("    Runs interactive ChatGPT session with Azure Search Index integration");
            Console.WriteLine();
            Console.WriteLine("  AzureSearchIndexToolbox help");
            Console.WriteLine("    Shows this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AzureSearchIndexToolbox extract presentation.pptx");
            Console.WriteLine("  AzureSearchIndexToolbox extract ./documents ./output");
            Console.WriteLine("  AzureSearchIndexToolbox merge index1.json index2.json merged.json");
            Console.WriteLine("  AzureSearchIndexToolbox deploy ./output/search-index.json ./output/media \"<connection-string>\" \"https://myservice.search.windows.net\" \"<api-key>\"");
            Console.WriteLine("  AzureSearchIndexToolbox chatgpt ./chatgpt-config.json");
            Console.WriteLine();
            Console.WriteLine("Supported file types:");
            Console.WriteLine("  - PowerPoint presentations (.pptx)");
            Console.WriteLine("  - PDF documents (.pdf)");
            Console.WriteLine("  - Markdown files (.md)");
        }
    }
}
