CREATE TABLE status
(
    id   int PRIMARY KEY,
    name varchar NOT NULL
);

CREATE TABLE role
(
    id   int PRIMARY KEY,
    name varchar NOT NULL
);

CREATE TABLE account
(
    id            uuid PRIMARY KEY,
    email         varchar NOT NULL,
    phone         varchar NOT NULL,
    password_hash varchar NOT NULL,
    role_id       int     NOT NULL REFERENCES role,
    status_id     int     NOT NULL REFERENCES status
);

CREATE INDEX email_index ON account (email);

CREATE TABLE pending_confirmation_token
(
    account_id              uuid PRIMARY KEY REFERENCES account ON DELETE CASCADE,
    confirmation_token_hash varchar NOT NULL
);

CREATE TABLE refresh_token_info
(
    id             uuid PRIMARY KEY,
    account_id     uuid                     NOT NULL REFERENCES account ON DELETE CASCADE,
    is_revoked     bool                     NOT NULL,
    issue_datetime timestamp with time zone NOT NULL,
    expires_at     timestamp with time zone NOT NULL
);

CREATE TABLE password_recover_token
(
    id                 uuid PRIMARY KEY,
    account_id         uuid REFERENCES account ON DELETE CASCADE,
    recover_token_hash varchar                  NOT NULL,
    is_already_applied boolean                  NOT NULL,
    expires_at         timestamp with time zone NOT NULL
);

CREATE TABLE outbox
(
    event_id         uuid PRIMARY KEY,
    type             text                     NOT NULL,
    content          text                     NOT NULL,
    occurred_on_utc  timestamp with time zone NOT NULL,
    processed_on_utc timestamp with time zone
);

CREATE INDEX IF NOT EXISTS IX_outbox_messages_unprocessed
    ON outbox (occurred_on_utc, processed_on_utc)
    INCLUDE (event_id, type)
    WHERE processed_on_utc IS NULL;

INSERT INTO role (id, name)
VALUES (1, 'customer'),
       (2, 'admin'),
       (3, 'support'),
       (4, 'maintenance'),
       (5, 'hr');

INSERT INTO status (id, name)
VALUES (1, 'pending confirmation'),
       (2, 'confirmed'),
       (4, 'deactivated'),
       (5, 'deleted');