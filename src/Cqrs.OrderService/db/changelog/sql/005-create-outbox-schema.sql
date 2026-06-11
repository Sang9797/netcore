CREATE TABLE outbox_messages (
    outbox_message_id VARCHAR(64) PRIMARY KEY,
    event_type VARCHAR(200) NOT NULL,
    routing_key VARCHAR(200) NOT NULL,
    payload TEXT NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    published_at TIMESTAMPTZ NULL,
    publish_attempts INTEGER NOT NULL DEFAULT 0,
    last_error TEXT NULL
);

CREATE INDEX idx_outbox_messages_unpublished
    ON outbox_messages (published_at, created_at);
