using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using AzureSearchIndexToolbox.Models;

namespace AzureSearchIndexToolbox.Extractors
{
    /// <summary>
    /// Extractor class for PDF files.
    /// Provides methods to extract text and images from PDF documents.
    /// </summary>
    public class PdfExtractor
    {
        /// <summary>
        /// Extracts all data from a PDF file.
        /// </summary>
        /// <param name="filePath">Path to the PDF file</param>
        /// <param name="outputDirectory">Directory where extracted media files will be saved</param>
        /// <returns>SearchIndexDocument containing all extracted data</returns>
        public SearchIndexDocument ExtractData(string filePath, string outputDirectory)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var document = new SearchIndexDocument
            {
                Id = Guid.NewGuid().ToString(),
                SourcePath = filePath,
                FileType = "PDF",
                Title = Path.GetFileNameWithoutExtension(filePath)
            };

            // Create output directory for media files if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (PdfReader pdfReader = new PdfReader(filePath))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
            {
                // Extract text content from all pages
                ExtractTextFromPages(pdfDocument, document);

                // Extract images from the PDF
                ExtractImages(pdfDocument, document, outputDirectory);

                // Extract metadata
                ExtractMetadata(pdfDocument, document);
            }

            return document;
        }

        /// <summary>
        /// Extracts text content from all pages in the PDF document.
        /// </summary>
        /// <param name="pdfDocument">The PDF document to extract from</param>
        /// <param name="document">The document model to populate with extracted data</param>
        private void ExtractTextFromPages(PdfDocument pdfDocument, SearchIndexDocument document)
        {
            var pageTexts = new List<string>();
            int numberOfPages = pdfDocument.GetNumberOfPages();

            for (int pageNumber = 1; pageNumber <= numberOfPages; pageNumber++)
            {
                // Extract text from the current page
                string pageText = ExtractTextFromPage(pdfDocument, pageNumber);
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    pageTexts.Add(pageText);
                }
            }

            // Combine all page texts into the content field
            document.Content = string.Join("\n\n", pageTexts);
        }

        /// <summary>
        /// Extracts text from a single page in the PDF.
        /// </summary>
        /// <param name="pdfDocument">The PDF document</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <returns>Extracted text from the page</returns>
        private string ExtractTextFromPage(PdfDocument pdfDocument, int pageNumber)
        {
            var page = pdfDocument.GetPage(pageNumber);
            var strategy = new SimpleTextExtractionStrategy();
            string text = PdfTextExtractor.GetTextFromPage(page, strategy);
            return text;
        }

        /// <summary>
        /// Extracts images from the PDF and saves them to the output directory.
        /// Note: Image extraction from PDF is complex and may require additional libraries
        /// for complete implementation. This is a basic implementation.
        /// </summary>
        /// <param name="pdfDocument">The PDF document to extract from</param>
        /// <param name="document">The document model to populate with image paths</param>
        /// <param name="outputDirectory">Directory where images will be saved</param>
        private void ExtractImages(PdfDocument pdfDocument, SearchIndexDocument document, string outputDirectory)
        {
            // Note: Full image extraction from PDF requires more complex processing
            // This is a placeholder for the basic structure
            // For production use, consider using libraries like iText7 with additional image extraction capabilities

            int numberOfPages = pdfDocument.GetNumberOfPages();
            int imageCounter = 1;

            for (int pageNumber = 1; pageNumber <= numberOfPages; pageNumber++)
            {
                var page = pdfDocument.GetPage(pageNumber);
                var resources = page.GetResources();

                if (resources != null)
                {
                    var xObjects = resources.GetResourceNames();
                    
                    foreach (var name in xObjects)
                    {
                        var xObject = resources.GetResource(name);
                        
                        if (xObject != null && xObject.IsStream())
                        {
                            var stream = (iText.Kernel.Pdf.PdfStream)xObject;
                            
                            // Check if this is an image XObject
                            var subtype = stream.GetAsName(iText.Kernel.Pdf.PdfName.Subtype);
                            if (subtype != null && subtype.Equals(iText.Kernel.Pdf.PdfName.Image))
                            {
                                try
                                {
                                    var imageBytes = stream.GetBytes();
                                    if (imageBytes != null && imageBytes.Length > 0)
                                    {
                                        string fileName = $"{Path.GetFileNameWithoutExtension(document.SourcePath)}_image_{imageCounter}.png";
                                        string imagePath = Path.Combine(outputDirectory, fileName);

                                        File.WriteAllBytes(imagePath, imageBytes);
                                        document.Images.Add(imagePath);
                                        imageCounter++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle image extraction errors
                                    Console.WriteLine($"Error extracting image from page {pageNumber}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts metadata from the PDF document.
        /// </summary>
        /// <param name="pdfDocument">The PDF document to extract from</param>
        /// <param name="document">The document model to populate with metadata</param>
        private void ExtractMetadata(PdfDocument pdfDocument, SearchIndexDocument document)
        {
            var info = pdfDocument.GetDocumentInfo();

            if (info != null)
            {
                // Extract author
                string? author = info.GetAuthor();
                if (!string.IsNullOrWhiteSpace(author))
                {
                    document.Metadata["Author"] = author;
                }

                // Extract title
                string? title = info.GetTitle();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    document.Metadata["DocumentTitle"] = title;
                    // Use document title as the main title if available
                    if (string.IsNullOrWhiteSpace(document.Title) || document.Title == Path.GetFileNameWithoutExtension(document.SourcePath))
                    {
                        document.Title = title;
                    }
                }

                // Extract subject
                string? subject = info.GetSubject();
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    document.Metadata["Subject"] = subject;
                }

                // Extract keywords
                string? keywords = info.GetKeywords();
                if (!string.IsNullOrWhiteSpace(keywords))
                {
                    document.Metadata["Keywords"] = keywords;
                }

                // Extract creator
                string? creator = info.GetCreator();
                if (!string.IsNullOrWhiteSpace(creator))
                {
                    document.Metadata["Creator"] = creator;
                }

                // Extract producer
                string? producer = info.GetProducer();
                if (!string.IsNullOrWhiteSpace(producer))
                {
                    document.Metadata["Producer"] = producer;
                }
            }

            // Add page count
            int pageCount = pdfDocument.GetNumberOfPages();
            document.Metadata["PageCount"] = pageCount.ToString();
        }
    }
}
