BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Meta" (
	"Version"	INTEGER DEFAULT 1
);
CREATE TABLE IF NOT EXISTS "Notes" (
	"Id"	TEXT NOT NULL UNIQUE,
	"UserId"	TEXT NOT NULL,
	"TopicId"	TEXT NOT NULL,
	"Title"	TEXT,
	"Content"	TEXT,
	"NoteType"	INTEGER,
	"Pinned"	INTEGER DEFAULT 0,
	"GloballyPinned"	INTEGER DEFAULT 0,
	"Options"	TEXT,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT NOT NULL,
	"DateDeleted"	TEXT,
	"IVKey"	TEXT,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Topics" (
	"Id"	TEXT NOT NULL UNIQUE,
	"UserId"	TEXT NOT NULL,
	"Title"	TEXT,
	"Symbol"	TEXT,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT NOT NULL,
	"DateDeleted"	TEXT,
	"IVKey"	TEXT,
	"Color"	TEXT DEFAULT 'ff808080',
	"Position"	INTEGER DEFAULT 0,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "DeletedTopics" (
	"Id"	TEXT NOT NULL UNIQUE,
	"UserId"	TEXT NOT NULL,
	"Title"	TEXT,
	"Symbol"	TEXT,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT NOT NULL,
	"DateDeleted"	TEXT,
	"IVKey"	TEXT,
	"Color"	TEXT DEFAULT 'ff808080',
	"Position"	INTEGER DEFAULT 0,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "DeletedNotes" (
	"Id"	TEXT NOT NULL UNIQUE,
	"UserId"	TEXT NOT NULL,
	"TopicId"	TEXT NOT NULL,
	"Title"	TEXT,
	"Content"	TEXT,
	"NoteType"	INTEGER,
	"Pinned"	INTEGER DEFAULT 0,
	"GloballyPinned"	INTEGER DEFAULT 0,
	"Options"	TEXT,
	"DateCreated"	TEXT NOT NULL,
	"DateLastEdited"	TEXT NOT NULL,
	"DateDeleted"	TEXT,
	"IVKey"	TEXT,
	PRIMARY KEY("Id")
);
CREATE INDEX IF NOT EXISTS "Idx_NoteId" ON "Notes" (
	"Id"	ASC
);
CREATE INDEX IF NOT EXISTS "Idx_TopicId" ON "Topics" (
	"Id"	ASC
);
CREATE INDEX IF NOT EXISTS "Idx_DelTopicId" ON "DeletedTopics" (
	"Id"	ASC
);
CREATE INDEX IF NOT EXISTS "Idx_DelNoteId" ON "DeletedNotes" (
	"Id"	ASC
);
COMMIT;
