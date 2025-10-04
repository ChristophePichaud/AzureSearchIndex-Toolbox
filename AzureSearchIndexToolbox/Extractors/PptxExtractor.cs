using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using AzureSearchIndexToolbox.Models;
using A = DocumentFormat.OpenXml.Drawing;

namespace AzureSearchIndexToolbox.Extractors
{
    /// <summary>
    /// Extractor class for PowerPoint (.pptx) files.
    /// Provides methods to extract text, titles, images, audio, and video from presentations.
    /// </summary>
    public class PptxExtractor
    {
        /// <summary>
        /// Extracts all data from a PowerPoint presentation file.
        /// </summary>
        /// <param name="filePath">Path to the .pptx file</param>
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
                FileType = "PPTX",
                Title = Path.GetFileNameWithoutExtension(filePath)
            };

            // Create output directory for media files if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (PresentationDocument presentationDocument = PresentationDocument.Open(filePath, false))
            {
                // Extract text content and titles from all slides
                ExtractTextFromSlides(presentationDocument, document);

                // Extract images from the presentation
                ExtractImages(presentationDocument, document, outputDirectory);

                // Extract audio and video files
                ExtractMediaFiles(presentationDocument, document, outputDirectory);

                // Extract presentation metadata
                ExtractMetadata(presentationDocument, document);
            }

            return document;
        }

        /// <summary>
        /// Extracts text content from all slides in the presentation.
        /// Separates slide titles from content and combines all text.
        /// </summary>
        /// <param name="presentationDocument">The presentation document to extract from</param>
        /// <param name="document">The document model to populate with extracted data</param>
        private void ExtractTextFromSlides(PresentationDocument presentationDocument, SearchIndexDocument document)
        {
            if (presentationDocument.PresentationPart == null)
            {
                return;
            }

            var slideTexts = new List<string>();
            var slideParts = presentationDocument.PresentationPart.SlideParts;

            foreach (var slidePart in slideParts)
            {
                if (slidePart.Slide != null)
                {
                    // Extract all text from the slide
                    var slideText = ExtractTextFromSlide(slidePart);
                    if (!string.IsNullOrWhiteSpace(slideText))
                    {
                        slideTexts.Add(slideText);
                    }
                }
            }

            // Combine all slide texts into the content field
            document.Content = string.Join("\n\n", slideTexts);
        }

        /// <summary>
        /// Extracts text from a single slide, including all text boxes and shapes.
        /// </summary>
        /// <param name="slidePart">The slide part to extract text from</param>
        /// <returns>Extracted text from the slide</returns>
        private string ExtractTextFromSlide(SlidePart slidePart)
        {
            var texts = new List<string>();

            // Get all text from shapes in the slide
            var shapes = slidePart.Slide.Descendants<Shape>();
            foreach (var shape in shapes)
            {
                if (shape.TextBody != null)
                {
                    var paragraphs = shape.TextBody.Descendants<A.Paragraph>();
                    foreach (var paragraph in paragraphs)
                    {
                        var runs = paragraph.Descendants<A.Run>();
                        foreach (var run in runs)
                        {
                            if (run.Text != null && !string.IsNullOrWhiteSpace(run.Text.Text))
                            {
                                texts.Add(run.Text.Text);
                            }
                        }
                    }
                }
            }

            return string.Join(" ", texts);
        }

        /// <summary>
        /// Extracts all images from the presentation and saves them to the output directory.
        /// </summary>
        /// <param name="presentationDocument">The presentation document to extract from</param>
        /// <param name="document">The document model to populate with image paths</param>
        /// <param name="outputDirectory">Directory where images will be saved</param>
        private void ExtractImages(PresentationDocument presentationDocument, SearchIndexDocument document, string outputDirectory)
        {
            if (presentationDocument.PresentationPart == null)
            {
                return;
            }

            int imageCounter = 1;
            var slideParts = presentationDocument.PresentationPart.SlideParts;

            foreach (var slidePart in slideParts)
            {
                // Get all image parts from the slide
                var imageParts = slidePart.ImageParts;
                foreach (var imagePart in imageParts)
                {
                    // Determine file extension based on content type
                    string extension = GetImageExtension(imagePart.ContentType);
                    string fileName = $"{Path.GetFileNameWithoutExtension(document.SourcePath)}_image_{imageCounter}{extension}";
                    string imagePath = Path.Combine(outputDirectory, fileName);

                    // Save the image to disk
                    using (var imageStream = imagePart.GetStream())
                    using (var fileStream = File.Create(imagePath))
                    {
                        imageStream.CopyTo(fileStream);
                    }

                    document.Images.Add(imagePath);
                    imageCounter++;
                }
            }
        }

        /// <summary>
        /// Extracts audio and video files from the presentation.
        /// </summary>
        /// <param name="presentationDocument">The presentation document to extract from</param>
        /// <param name="document">The document model to populate with media file paths</param>
        /// <param name="outputDirectory">Directory where media files will be saved</param>
        private void ExtractMediaFiles(PresentationDocument presentationDocument, SearchIndexDocument document, string outputDirectory)
        {
            if (presentationDocument.PresentationPart == null)
            {
                return;
            }

            int audioCounter = 1;
            int videoCounter = 1;
            var slideParts = presentationDocument.PresentationPart.SlideParts;

            foreach (var slidePart in slideParts)
            {
                // Extract audio files
                var audioParts = slidePart.DataPartReferenceRelationships
                    .Where(r => r.DataPart.ContentType.Contains("audio"));

                foreach (var audioPart in audioParts)
                {
                    string extension = GetMediaExtension(audioPart.DataPart.ContentType);
                    string fileName = $"{Path.GetFileNameWithoutExtension(document.SourcePath)}_audio_{audioCounter}{extension}";
                    string audioPath = Path.Combine(outputDirectory, fileName);

                    using (var audioStream = audioPart.DataPart.GetStream())
                    using (var fileStream = File.Create(audioPath))
                    {
                        audioStream.CopyTo(fileStream);
                    }

                    document.AudioFiles.Add(audioPath);
                    audioCounter++;
                }

                // Extract video files
                var videoParts = slidePart.DataPartReferenceRelationships
                    .Where(r => r.DataPart.ContentType.Contains("video"));

                foreach (var videoPart in videoParts)
                {
                    string extension = GetMediaExtension(videoPart.DataPart.ContentType);
                    string fileName = $"{Path.GetFileNameWithoutExtension(document.SourcePath)}_video_{videoCounter}{extension}";
                    string videoPath = Path.Combine(outputDirectory, fileName);

                    using (var videoStream = videoPart.DataPart.GetStream())
                    using (var fileStream = File.Create(videoPath))
                    {
                        videoStream.CopyTo(fileStream);
                    }

                    document.VideoFiles.Add(videoPath);
                    videoCounter++;
                }
            }
        }

        /// <summary>
        /// Extracts metadata from the presentation (author, created date, etc.).
        /// </summary>
        /// <param name="presentationDocument">The presentation document to extract from</param>
        /// <param name="document">The document model to populate with metadata</param>
        private void ExtractMetadata(PresentationDocument presentationDocument, SearchIndexDocument document)
        {
            var coreProperties = presentationDocument.PackageProperties;

            if (coreProperties != null)
            {
                if (!string.IsNullOrWhiteSpace(coreProperties.Creator))
                {
                    document.Metadata["Author"] = coreProperties.Creator;
                }

                if (coreProperties.Created.HasValue)
                {
                    document.Metadata["CreatedDate"] = coreProperties.Created.Value.ToString("yyyy-MM-dd");
                }

                if (coreProperties.Modified.HasValue)
                {
                    document.Metadata["ModifiedDate"] = coreProperties.Modified.Value.ToString("yyyy-MM-dd");
                }

                if (!string.IsNullOrWhiteSpace(coreProperties.Title))
                {
                    document.Metadata["DocumentTitle"] = coreProperties.Title;
                    // Use document title as the main title if available
                    if (string.IsNullOrWhiteSpace(document.Title) || document.Title == Path.GetFileNameWithoutExtension(document.SourcePath))
                    {
                        document.Title = coreProperties.Title;
                    }
                }
            }

            // Add slide count
            if (presentationDocument.PresentationPart != null)
            {
                var slideCount = presentationDocument.PresentationPart.SlideParts.Count();
                document.Metadata["SlideCount"] = slideCount.ToString();
            }
        }

        /// <summary>
        /// Determines the file extension for an image based on its content type.
        /// </summary>
        /// <param name="contentType">MIME content type of the image</param>
        /// <returns>File extension including the dot (e.g., ".png")</returns>
        private string GetImageExtension(string contentType)
        {
            return contentType.ToLower() switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/tiff" => ".tiff",
                _ => ".bin"
            };
        }

        /// <summary>
        /// Determines the file extension for a media file based on its content type.
        /// </summary>
        /// <param name="contentType">MIME content type of the media file</param>
        /// <returns>File extension including the dot (e.g., ".mp3")</returns>
        private string GetMediaExtension(string contentType)
        {
            return contentType.ToLower() switch
            {
                var ct when ct.Contains("mp3") => ".mp3",
                var ct when ct.Contains("wav") => ".wav",
                var ct when ct.Contains("mp4") => ".mp4",
                var ct when ct.Contains("avi") => ".avi",
                var ct when ct.Contains("wmv") => ".wmv",
                var ct when ct.Contains("mpeg") => ".mpeg",
                _ => ".bin"
            };
        }
    }
}
