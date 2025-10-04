using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using AzureSearchIndexToolbox.Models;

namespace AzureSearchIndexToolbox.Extractors
{
    /// <summary>
    /// Extractor class for Markdown (.md) files.
    /// Provides methods to extract text, titles, and links from markdown documents.
    /// </summary>
    public class MarkdownExtractor
    {
        /// <summary>
        /// Extracts all data from a Markdown file.
        /// </summary>
        /// <param name="filePath">Path to the .md file</param>
        /// <param name="outputDirectory">Directory where extracted media files will be saved (for future use)</param>
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
                FileType = "MD",
                Title = Path.GetFileNameWithoutExtension(filePath)
            };

            // Create output directory if it doesn't exist (for potential future use)
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Read the markdown file
            string markdownContent = File.ReadAllText(filePath);

            // Parse the markdown document
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var markdownDocument = Markdown.Parse(markdownContent, pipeline);

            // Extract title from the first heading
            ExtractTitle(markdownDocument, document);

            // Extract all text content
            ExtractContent(markdownContent, document);

            // Extract image references
            ExtractImageReferences(markdownDocument, document, filePath);

            // Extract metadata from the markdown
            ExtractMetadata(markdownDocument, document);

            return document;
        }

        /// <summary>
        /// Extracts the title from the markdown document.
        /// Uses the first heading (H1) as the title if available.
        /// </summary>
        /// <param name="markdownDocument">The parsed markdown document</param>
        /// <param name="document">The document model to populate with title</param>
        private void ExtractTitle(MarkdownDocument markdownDocument, SearchIndexDocument document)
        {
            // Find the first heading
            var firstHeading = markdownDocument.Descendants<HeadingBlock>().FirstOrDefault();
            
            if (firstHeading != null)
            {
                // Extract text from the heading
                var headingText = GetTextFromBlock(firstHeading);
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    document.Title = headingText.Trim();
                }
            }
        }

        /// <summary>
        /// Extracts all text content from the markdown file.
        /// Preserves the structure while removing markdown syntax.
        /// </summary>
        /// <param name="markdownContent">Raw markdown content</param>
        /// <param name="document">The document model to populate with content</param>
        private void ExtractContent(string markdownContent, SearchIndexDocument document)
        {
            // Convert markdown to plain text by removing common markdown syntax
            // This is a simple approach; for more sophisticated parsing, use the Markdig AST
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            
            // Convert to HTML first, then strip HTML tags for plain text
            string html = Markdown.ToHtml(markdownContent, pipeline);
            string plainText = StripHtmlTags(html);
            
            document.Content = plainText.Trim();
        }

        /// <summary>
        /// Extracts image references from the markdown document.
        /// </summary>
        /// <param name="markdownDocument">The parsed markdown document</param>
        /// <param name="document">The document model to populate with image references</param>
        /// <param name="markdownFilePath">Path to the markdown file (for resolving relative paths)</param>
        private void ExtractImageReferences(MarkdownDocument markdownDocument, SearchIndexDocument document, string markdownFilePath)
        {
            var imageLinks = markdownDocument.Descendants<LinkInline>()
                .Where(link => link.IsImage);

            foreach (var imageLink in imageLinks)
            {
                if (!string.IsNullOrWhiteSpace(imageLink.Url))
                {
                    // If the image path is relative, make it absolute based on the markdown file location
                    string imagePath = imageLink.Url;
                    
                    if (!Uri.IsWellFormedUriString(imagePath, UriKind.Absolute))
                    {
                        // Resolve relative path
                        string? markdownDirectory = Path.GetDirectoryName(markdownFilePath);
                        if (markdownDirectory != null)
                        {
                            imagePath = Path.Combine(markdownDirectory, imagePath);
                            imagePath = Path.GetFullPath(imagePath);
                        }
                    }
                    
                    document.Images.Add(imagePath);
                }
            }
        }

        /// <summary>
        /// Extracts metadata from the markdown document.
        /// Counts headings, code blocks, and other structural elements.
        /// </summary>
        /// <param name="markdownDocument">The parsed markdown document</param>
        /// <param name="document">The document model to populate with metadata</param>
        private void ExtractMetadata(MarkdownDocument markdownDocument, SearchIndexDocument document)
        {
            // Count headings by level
            var headings = markdownDocument.Descendants<HeadingBlock>().ToList();
            document.Metadata["HeadingCount"] = headings.Count.ToString();

            var headingsByLevel = headings.GroupBy(h => h.Level);
            foreach (var group in headingsByLevel)
            {
                document.Metadata[$"H{group.Key}Count"] = group.Count().ToString();
            }

            // Count code blocks
            var codeBlocks = markdownDocument.Descendants<CodeBlock>().Count();
            document.Metadata["CodeBlockCount"] = codeBlocks.ToString();

            // Count links
            var links = markdownDocument.Descendants<LinkInline>().Where(l => !l.IsImage).Count();
            document.Metadata["LinkCount"] = links.ToString();

            // Count images
            document.Metadata["ImageCount"] = document.Images.Count.ToString();

            // Extract all headings as a table of contents
            var tocEntries = new List<string>();
            foreach (var heading in headings)
            {
                string headingText = GetTextFromBlock(heading);
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    string indent = new string(' ', (heading.Level - 1) * 2);
                    tocEntries.Add($"{indent}- {headingText.Trim()}");
                }
            }

            if (tocEntries.Any())
            {
                document.Metadata["TableOfContents"] = string.Join("\n", tocEntries);
            }
        }

        /// <summary>
        /// Extracts text content from a markdown block element.
        /// </summary>
        /// <param name="block">The block element to extract text from</param>
        /// <returns>Plain text content</returns>
        private string GetTextFromBlock(Block block)
        {
            var textBuilder = new System.Text.StringBuilder();
            
            if (block is LeafBlock leafBlock)
            {
                if (leafBlock.Inline != null)
                {
                    foreach (var inline in leafBlock.Inline)
                    {
                        if (inline is LiteralInline literal)
                        {
                            textBuilder.Append(literal.Content.ToString());
                        }
                        else if (inline is CodeInline code)
                        {
                            textBuilder.Append(code.Content);
                        }
                        else if (inline is LinkInline link)
                        {
                            // Extract text from link
                            foreach (var child in link)
                            {
                                if (child is LiteralInline linkText)
                                {
                                    textBuilder.Append(linkText.Content.ToString());
                                }
                            }
                        }
                    }
                }
            }

            return textBuilder.ToString();
        }

        /// <summary>
        /// Strips HTML tags from a string to get plain text.
        /// </summary>
        /// <param name="html">HTML content</param>
        /// <returns>Plain text without HTML tags</returns>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            // Simple HTML tag removal using regex
            string noTags = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
            
            // Replace HTML entities
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, "&nbsp;", " ");
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, "&lt;", "<");
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, "&gt;", ">");
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, "&amp;", "&");
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, "&quot;", "\"");
            
            // Normalize whitespace
            noTags = System.Text.RegularExpressions.Regex.Replace(noTags, @"\s+", " ");
            
            return noTags.Trim();
        }
    }
}
