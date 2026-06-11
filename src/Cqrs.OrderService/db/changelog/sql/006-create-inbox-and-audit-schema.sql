CREATE TABLE inbox_messages (
    inbox_message_id VARCHAR(64) PRIMARY KEY,
    event_type VARCHAR(200) NOT NULL,
    routing_key VARCHAR(200) NOT NULL,
    received_at TIMESTAMPTZ NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE integration_event_audits (
    audit_id VARCHAR(64) PRIMARY KEY,
    message_id VARCHAR(64) NOT NULL,
    event_type VARCHAR(200) NOT NULL,
    routing_key VARCHAR(200) NOT NULL,
    aggregate_id VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_integration_event_audits_message_id
    ON integration_event_audits (message_id);
