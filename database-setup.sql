-- ================================================
-- PostgreSQL Database Setup for ChatGPT Service
-- ================================================
-- This script creates the necessary database schema
-- for storing ChatGPT conversation history
-- ================================================

-- Create database (run as postgres superuser)
-- Uncomment if you want to create a new database
-- CREATE DATABASE chatgpt_conversations;

-- Connect to the database
\c chatgpt_conversations;

-- ================================================
-- Create conversation_history table
-- ================================================
CREATE TABLE IF NOT EXISTS conversation_history (
    id SERIAL PRIMARY KEY,
    conversation_id VARCHAR(100) NOT NULL,
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    citations TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sequence_number INTEGER NOT NULL
);

-- ================================================
-- Create indexes for better query performance
-- ================================================

-- Index on conversation_id for filtering conversations
CREATE INDEX IF NOT EXISTS ix_conversation_history_conversation_id 
ON conversation_history(conversation_id);

-- Index on created_at for sorting by date
CREATE INDEX IF NOT EXISTS ix_conversation_history_created_at 
ON conversation_history(created_at);

-- Composite index for conversation queries with ordering
CREATE INDEX IF NOT EXISTS ix_conversation_history_conversation_sequence 
ON conversation_history(conversation_id, sequence_number);

-- ================================================
-- Table comments for documentation
-- ================================================
COMMENT ON TABLE conversation_history IS 'Stores ChatGPT conversation history with questions, answers, and citations';
COMMENT ON COLUMN conversation_history.id IS 'Auto-incrementing primary key';
COMMENT ON COLUMN conversation_history.conversation_id IS 'GUID identifying a conversation session';
COMMENT ON COLUMN conversation_history.question IS 'User question';
COMMENT ON COLUMN conversation_history.answer IS 'ChatGPT response';
COMMENT ON COLUMN conversation_history.citations IS 'JSON array of source documents used';
COMMENT ON COLUMN conversation_history.created_at IS 'Timestamp when the question was asked';
COMMENT ON COLUMN conversation_history.sequence_number IS 'Order of the question in the conversation';

-- ================================================
-- Sample Queries
-- ================================================

-- Get all conversations for a specific conversation_id
-- SELECT * FROM conversation_history 
-- WHERE conversation_id = 'your-conversation-id' 
-- ORDER BY sequence_number;

-- Get recent conversations
-- SELECT conversation_id, COUNT(*) as question_count, MAX(created_at) as last_activity
-- FROM conversation_history
-- GROUP BY conversation_id
-- ORDER BY last_activity DESC
-- LIMIT 10;

-- Get conversations with their questions count
-- SELECT conversation_id, 
--        COUNT(*) as exchanges,
--        MIN(created_at) as started_at,
--        MAX(created_at) as last_activity
-- FROM conversation_history
-- GROUP BY conversation_id
-- ORDER BY last_activity DESC;

-- Search for questions containing specific text
-- SELECT conversation_id, question, answer, created_at
-- FROM conversation_history
-- WHERE question ILIKE '%search term%'
-- ORDER BY created_at DESC;

-- ================================================
-- Maintenance
-- ================================================

-- Delete old conversations (older than 90 days)
-- DELETE FROM conversation_history 
-- WHERE created_at < NOW() - INTERVAL '90 days';

-- Vacuum the table to reclaim space
-- VACUUM ANALYZE conversation_history;

-- ================================================
-- Grant Permissions (adjust as needed)
-- ================================================

-- Create a user for the application if needed
-- CREATE USER chatgpt_user WITH PASSWORD 'secure_password';

-- Grant permissions to the application user
-- GRANT SELECT, INSERT, UPDATE, DELETE ON conversation_history TO chatgpt_user;
-- GRANT USAGE, SELECT ON SEQUENCE conversation_history_id_seq TO chatgpt_user;

-- ================================================
-- Verification
-- ================================================

-- Verify table creation
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'conversation_history'
ORDER BY ordinal_position;

-- Verify indexes
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'conversation_history';

PRINT '✓ Database schema created successfully!';
