using Microsoft.EntityFrameworkCore;

namespace AzureSearchIndexToolbox.Models
{
    /// <summary>
    /// Database context for managing conversation history in PostgreSQL.
    /// </summary>
    public class ConversationDbContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Conversation history entries.
        /// </summary>
        public DbSet<ConversationHistory> ConversationHistories { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the ConversationDbContext.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string</param>
        public ConversationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Configures the database connection.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        /// <summary>
        /// Configures the entity models.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create index on conversation_id for faster queries
            modelBuilder.Entity<ConversationHistory>()
                .HasIndex(ch => ch.ConversationId)
                .HasDatabaseName("ix_conversation_history_conversation_id");

            // Create index on created_at for sorting
            modelBuilder.Entity<ConversationHistory>()
                .HasIndex(ch => ch.CreatedAt)
                .HasDatabaseName("ix_conversation_history_created_at");
        }
    }
}
