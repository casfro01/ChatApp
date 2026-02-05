DROP SCHEMA IF EXISTS chat CASCADE
CREATE SCHEMA IF NOT EXISTS chat;

CREATE TABLE chat.user(
    id TEXT PRIMARY KEY NOT NULL,
    username TEXT NOT NULL UNIQUE,
    passwordhash TEXT NOT NULL,
    createdAt TIMESTAMP WITH TIME ZONE
);

CREATE TABLE chat.rooms(
    id TEXT PRIMARY KEY NOT NULL,
    chatName TEXT UNIQUE
);

CREATE TABLE chat.messages(
    id SERIAL PRIMARY KEY,
    userId TEXT REFERENCES chat.user(id) ON DELETE CASCADE,
    roomId TEXT REFERENCES chat.rooms(id) ON DELETE CASCADE,
    chatMessage TEXT
);

CREATE TABLE chat.userRooms(
    userId TEXT REFERENCES chat.user(id) ON DELETE CASCADE,
    roomId TEXT REFERENCES chat.rooms(id) ON DELETE CASCADE,
    PRIMARY KEY (userId, roomId)
);