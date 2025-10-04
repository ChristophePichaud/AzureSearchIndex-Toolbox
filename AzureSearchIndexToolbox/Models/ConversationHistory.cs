using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureSearchIndexToolbox.Models
{
    /// <summary>
    /// Entity model for storing conversation history in PostgreSQL.
    /// </summary>
    [Table("conversation_history")]
    public class ConversationHistory
    {
        /// <summary>
        /// Unique identifier for the conversation entry.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Conversation session identifier to group related questions and answers.
        /// </summary>
        [Required]
        [Column("conversation_id")]
        [MaxLength(100)]
        public string ConversationId { get; set; } = string.Empty;

        /// <summary>
        /// Question asked by the user.
        /// </summary>
        [Required]
        [Column("question")]
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// Answer provided by ChatGPT.
        /// </summary>
        [Required]
        [Column("answer")]
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Citations and sources used to generate the answer.
        /// Stored as JSON string.
        /// </summary>
        [Column("citations")]
        public string? Citations { get; set; }

        /// <summary>
        /// Timestamp when the question was asked.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Order of the question in the conversation.
        /// </summary>
        [Column("sequence_number")]
        public int SequenceNumber { get; set; }
    }
}
