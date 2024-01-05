DELIMITER //
CREATE DATABASE IF NOT EXISTS EazyNotesDB CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE EazyNotesDB;

CREATE TABLE IF NOT EXISTS Users (
    Id binary(16) PRIMARY KEY UNIQUE,
    Username varchar(255) NOT NULL UNIQUE,
    License TEXT,
    DisplayName varchar(255) NOT NULL,
    Email varchar(255) NOT NULL,
    EmailVerified BIT NOT NULL DEFAULT 0,
    RegistrationDate DATETIME NOT NULL,
    PasswordSalt varchar(127) NOT NULL,
    PasswordHash varchar(127) NOT NULL,
    RSAPublicKey varchar(2047) NOT NULL,
    RSAPrivateKeyCrypt varchar(4095) NOT NULL,
    AlgorithmIdentifier varchar(511) NOT NULL, 
    INDEX (Id),
    INDEX (Username)
) ENGINE=InnoDB DEFAULT CHARSET = utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Topics (
    Id binary(16) PRIMARY KEY UNIQUE,
    UserId binary(16) NOT NULL,
    Title varchar(511) NOT NULL,
    Symbol varchar(4) NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateModifiedHeader DATETIME NOT NULL,
    DateModified DATETIME NOT NULL,
    DateDeleted DATETIME DEFAULT NULL,
    IVKey varchar(511) NOT NULL,
    Color varchar(31) NOT NULL,
    Position SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    FOREIGN KEY(UserId) REFERENCES Users(Id), 
    INDEX (Id),
    INDEX (UserId),
    INDEX (DateModifiedHeader),
    INDEX (DateDeleted)
) ENGINE=InnoDB DEFAULT CHARSET = utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Notes (
    Id binary(16) PRIMARY KEY UNIQUE,
    UserId binary(16) NOT NULL,
    TopicId binary(16) NOT NULL,
    Title varchar(511) NOT NULL,
    Content TEXT NOT NULL,
    NoteType TINYINT NOT NULL DEFAULT 0,
    DateCreated DATETIME NOT NULL,
    DateModifiedHeader DATETIME NOT NULL,
    DateModified DATETIME NOT NULL,
    DateDeleted DATETIME DEFAULT NULL,
    Pinned BIT DEFAULT 0,
    GloballyPinned BIT DEFAULT 0,
    Options varchar(127) NULL,
    IVKey varchar(511) NOT NULL,
    FOREIGN KEY(TopicId) REFERENCES Topics(Id),
    FOREIGN KEY(UserId) REFERENCES Users(Id),
    INDEX (Id),
    INDEX (TopicId),
    INDEX (UserId),
    INDEX (DateModifiedHeader),
    INDEX (DateDeleted)
) ENGINE=InnoDB DEFAULT CHARSET = utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Clients (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY UNIQUE,
    Username varchar(255) NOT NULL,
    DeviceName varchar(127) NOT NULL,
    Platform varchar(63) NOT NULL,
    Country varchar(63),
    Registered DATETIME NOT NULL,
    FOREIGN KEY(Username) REFERENCES Users(Username),
    INDEX (Username), 
    INDEX (DeviceName),
    INDEX (Platform)
) ENGINE=InnoDB DEFAULT CHARSET = utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Feedback (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY UNIQUE,
    Title varchar(95) NOT NULL,
    Body varchar(4095) NOT NULL,
    Category TINYINT NOT NULL,
    AppVersion varchar(32) NOT NULL,
    AddressedInVersion varchar(32) NULL,
    DeviceName varchar(127) NULL,
    Platform varchar(63) NOT NULL,
    Submitted DATETIME NOT NULL, 
    INDEX (Id)
) ENGINE=InnoDB DEFAULT CHARSET = utf8mb4 COLLATE utf8mb4_0900_ai_ci;




CREATE PROCEDURE spClientsGetAll() 
BEGIN 
    SELECT * FROM Clients;
END;

CREATE PROCEDURE spClientInsert(IN Id BIGINT, IN Username varchar(255), IN DeviceName varchar(127), IN Platform varchar(63), IN Country varchar(63), IN Registered DATETIME)
BEGIN 
    IF NOT EXISTS (
        SELECT Id FROM Clients
        WHERE Clients.Username = Username AND Clients.DeviceName = DeviceName AND Clients.Platform = Platform)
    THEN BEGIN 
        INSERT INTO Clients(Id, Username, DeviceName, Platform, Country, Registered)
        VALUES (Id, Username, DeviceName, Platform, Country, Registered);
        SELECT LAST_INSERT_ID();
    END;
    END IF;
END;

CREATE PROCEDURE spClientDelete(IN Id BIGINT)
BEGIN 
    DELETE FROM Clients 
    WHERE Clients.Id = Id;
END;




CREATE PROCEDURE spFeedbackGetAll() 
BEGIN 
    SELECT * FROM Feedback;
END;

CREATE PROCEDURE spFeedbackInsert(IN Id BIGINT, IN Title varchar(511), IN Body varchar(4095), IN Category TINYINT, 
    IN AppVersion varchar(32), IN AddressedInVersion varchar(32), IN DeviceName varchar(127), IN Platform varchar(63), IN Submitted DATETIME) 
BEGIN
    INSERT INTO Feedback(Id, Title, Body, Category, AppVersion, AddressedInVersion, DeviceName, Platform, Submitted)
    VALUES (Id, Title, Body, Category, AppVersion, AddressedInVersion, DeviceName, Platform, Submitted);
    SELECT LAST_INSERT_ID();
END;

CREATE PROCEDURE spFeedbackDelete(IN Id BIGINT)
BEGIN 
    DELETE FROM Feedback 
    WHERE Feedback.Id = Id;
END;

CREATE PROCEDURE spFeedbackUpdate(IN Id BIGINT, IN AddressedInVersion varchar(32)) 
BEGIN 
    UPDATE Feedback 
    SET Feedback.AddressedInVersion = AddressedInVersion 
    WHERE Feedback.Id = Id;
END;




CREATE PROCEDURE spGetUserIdByNoteId(IN `Id` varchar(36)) 
BEGIN 
    SELECT BIN_TO_UUID(UserId) AS UserId
    FROM Notes 
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
END;

CREATE PROCEDURE spNotesGetUntrashedByUserId(IN `UserId` varchar(36))
BEGIN 
    SELECT *
    FROM Notes 
    WHERE Notes.UserId = UUID_TO_BIN(`UserId`) AND Notes.DateDeleted IS NULL;
END;

CREATE PROCEDURE spNotesGetTrashedByUserId(IN `UserId` varchar(36))
BEGIN 
    SELECT *
    FROM Notes 
    WHERE Notes.UserId = UUID_TO_BIN(`UserId`) AND Notes.DateDeleted IS NOT NULL;
END;

CREATE PROCEDURE spNotesGetByTopicId(IN `TopicId` varchar(36))
BEGIN 
    SELECT *
    FROM Notes 
    WHERE Notes.TopicId = UUID_TO_BIN(`TopicId`);
END;

CREATE PROCEDURE spNoteGetById(IN `Id` varchar(36))
BEGIN 
    SELECT *
    FROM Notes
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
END;

CREATE PROCEDURE spNoteInsertOrUpdate(IN `Id` varchar(36), IN `UserId` varchar(36), IN `TopicId` varchar(36), IN `Title` varchar(511), 
    IN `Content` TEXT, IN `NoteType` TINYINT, IN `Pinned` BIT, IN `GloballyPinned` BIT, IN `Options` varchar(127), 
    IN `DateCreated` DATETIME, IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME, IN `DateDeleted` DATETIME, IN `IVKey` varchar(511)) 
BEGIN 
    INSERT INTO Notes(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey)
    VALUES (UUID_TO_BIN(`Id`), UUID_TO_BIN(`UserId`), UUID_TO_BIN(`TopicId`), `Title`, `Content`, `NoteType`, `Pinned`, `GloballyPinned`, `Options`, `DateCreated`, `DateModifiedHeader`, `DateModified`, `DateDeleted`, `IVKey`)
    ON DUPLICATE KEY UPDATE 
    Title=`Title`, Content=`Content`, Options=`Options`, DateModifiedHeader=`DateModifiedHeader`, DateModified=`DateModified`;
    SELECT Id;
END;

CREATE PROCEDURE spNoteInsert(IN `Id` varchar(36), IN `UserId` varchar(36), IN `TopicId` varchar(36), IN `Title` varchar(511), 
    IN `Content` TEXT, IN `NoteType` TINYINT, IN `Pinned` BIT, IN `GloballyPinned` BIT, IN `Options` varchar(127), 
    IN `DateCreated` DATETIME, IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME, IN `DateDeleted` DATETIME, IN `IVKey` varchar(511)) 
BEGIN 
    INSERT INTO Notes(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey)
    VALUES (UUID_TO_BIN(`Id`), UUID_TO_BIN(`UserId`), UUID_TO_BIN(`TopicId`), `Title`, `Content`, `NoteType`, `Pinned`, `GloballyPinned`, `Options`, `DateCreated`, `DateModifiedHeader`, `DateModified`, `DateDeleted`, `IVKey`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteUpdateBody(IN `Id` varchar(36), IN `Title` varchar(511), IN `Content` TEXT, IN `Options` varchar(127), IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME) 
BEGIN 
    UPDATE Notes 
    SET Title=`Title`, Content=`Content`, Options=`Options`, DateModifiedHeader=`DateModifiedHeader`, DateModified=`DateModified`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteUpdateHeader(IN `Id` varchar(36), IN `TopicId` varchar(36), IN `Options` varchar(127), IN `Pinned` BIT, 
    IN `GloballyPinned` BIT, IN `DateModifiedHeader` DATETIME, IN `DateDeleted` DATETIME) 
BEGIN 
    UPDATE Notes 
    SET TopicId=UUID_TO_BIN(`TopicId`), Options=`Options`, Pinned = `Pinned`, GloballyPinned = `GloballyPinned`, DateModifiedHeader = `DateModifiedHeader`, DateDeleted = `DateDeleted`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT `Id`;
END;

CREATE PROCEDURE spNoteUpdateTopicId(IN `Id` varchar(36), IN `TopicId` varchar(36), IN `DateModifiedHeader` DATETIME)
BEGIN 
    UPDATE Notes 
    SET TopicId=UUID_TO_BIN(`TopicId`), DateModifiedHeader=`DateModifiedHeader`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteTrashUntrash(IN `Id` varchar(36), IN `DateDeleted` DATETIME, IN `DateModifiedHeader` DATETIME) 
BEGIN 
    UPDATE Notes 
    SET DateDeleted=`DateDeleted`, DateModifiedHeader=`DateModifiedHeader`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteTogglePinned(IN `Id` varchar(36), IN `Pinned` BIT, IN `DateModifiedHeader` DATETIME) 
BEGIN 
    UPDATE Notes 
    SET Pinned = `Pinned`, DateModifiedHeader=`DateModifiedHeader`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteToggleGloballyPinned(IN `Id` varchar(36), IN `GloballyPinned` BIT, IN `DateModifiedHeader` DATETIME) 
BEGIN 
    UPDATE Notes 
    SET GloballyPinned = `GloballyPinned`, DateModifiedHeader=`DateModifiedHeader`
    WHERE Notes.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spNoteDelete(IN `Id` varchar(36)) 
BEGIN 
    DELETE FROM Notes
    WHERE Notes.Id = UUID_TO_BIN(Id);
END;




CREATE PROCEDURE spGetUserIdByTopicId(IN `Id` varchar(36)) 
BEGIN 
    SELECT BIN_TO_UUID(UserId) AS UserId
    FROM Topics 
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
END;

CREATE PROCEDURE spTopicsGetUntrashedByUserId(IN `UserId` varchar(36)) 
BEGIN 
    SELECT *
    FROM Topics 
    WHERE Topics.UserId = UUID_TO_BIN(`UserId`) AND Topics.DateDeleted IS NULL;
END;

CREATE PROCEDURE spTopicsGetTrashedByUserId(IN `UserId` varchar(36)) 
BEGIN 
    SELECT *
    FROM Topics 
    WHERE Topics.UserId = UUID_TO_BIN(`UserId`) AND Topics.DateDeleted IS NOT NULL;
END;

CREATE PROCEDURE spTopicsGetByUserId(IN `UserId` varchar(36)) 
BEGIN 
    SELECT *
    FROM Topics 
    WHERE Topics.UserId = UUID_TO_BIN(`UserId`);
END;

CREATE PROCEDURE spTopicGetById(IN `Id` varchar(36))
BEGIN 
    SELECT *
    FROM Topics
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
END;

CREATE PROCEDURE spTopicInsertOrUpdate(IN `Id` varchar(36), IN `UserId` varchar(36), IN `Title` varchar(511), IN `Symbol` varchar(4), 
    IN `DateCreated` DATETIME, IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME, IN `DateDeleted` DATETIME,
    IN `IVKey` varchar(511), IN `Color` varchar(31), IN `Position` SMALLINT) 
BEGIN 
    INSERT INTO Topics(Id, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position)
    VALUES (UUID_TO_BIN(`Id`), UUID_TO_BIN(`UserId`), `Title`, `Symbol`, `DateCreated`, `DateModifiedHeader`, `DateModified`, `DateDeleted`, `IVKey`, `Color`, `Position`)
    ON DUPLICATE KEY UPDATE
    Title=`Title`, Symbol=`Symbol`, DateModifiedHeader=`DateModified`, DateModified=`DateModified`, Color=`Color`;
    SELECT Id;
END;

CREATE PROCEDURE spTopicInsert(IN `Id` varchar(36), IN `UserId` varchar(36), IN `Title` varchar(511), IN `Symbol` varchar(4), 
    IN `DateCreated` DATETIME, IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME, IN `DateDeleted` DATETIME,
    IN `IVKey` varchar(511), IN `Color` varchar(31), IN `Position` SMALLINT) 
BEGIN 
    INSERT INTO Topics(Id, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position)
    VALUES (UUID_TO_BIN(`Id`), UUID_TO_BIN(`UserId`), `Title`, `Symbol`, `DateCreated`, `DateModifiedHeader`, `DateModified`, `DateDeleted`, `IVKey`, `Color`, `Position`);
    SELECT Id;
END;

CREATE PROCEDURE spTopicUpdateBody(IN `Id` varchar(36), IN `Title` varchar(511), IN `Symbol` varchar(4), IN `DateModifiedHeader` DATETIME, IN `DateModified` DATETIME, IN `Color` varchar(31)) 
BEGIN 
    UPDATE Topics
    SET Title=`Title`, Symbol=`Symbol`, DateModifiedHeader=`DateModified`, DateModified=`DateModified`, Color=`Color`
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spTopicUpdateHeader(IN `Id` varchar(36), IN `Symbol` varchar(4), IN `DateModifiedHeader` DATETIME, IN `DateDeleted` DATETIME, IN `Color` varchar(31), IN `Position` SMALLINT) 
BEGIN 
    UPDATE Topics
    SET Symbol=`Symbol`, DateModifiedHeader=`DateModifiedHeader`, DateDeleted=`DateDeleted`, Color=`Color`, Position=`Position`
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spTopicUpdatePosition(IN `Id` varchar(36), IN `Position` SMALLINT, IN `DateModifiedHeader` DATETIME) 
BEGIN 
    UPDATE Topics 
    SET Position = `Position`, DateModifiedHeader=`DateModifiedHeader`
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spTopicTrashUntrash(IN `Id` varchar(36), IN `DateDeleted` DATETIME, IN `DateModifiedHeader` DATETIME) 
BEGIN 
    UPDATE Topics 
    SET DateDeleted = `DateDeleted`, DateModifiedHeader=`DateModifiedHeader`
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;

CREATE PROCEDURE spTopicDelete(IN `Id` varchar(36)) 
BEGIN 
	DELETE FROM Topics
    WHERE Topics.Id = UUID_TO_BIN(`Id`);
    SELECT Id;
END;





CREATE PROCEDURE spUserGetById(IN `Id` varchar(36))
BEGIN
    SELECT *
    FROM Users 
    WHERE Users.Id = UUID_TO_BIN(`Id`);
END;

CREATE PROCEDURE spUserGetByEmail(IN `Email` varchar(255)) 
BEGIN 
    SELECT *
    FROM Users 
    WHERE Users.Email = `Email`;
END;

CREATE PROCEDURE spUserGetByUsername(IN `Username` varchar(255)) 
BEGIN 
    SELECT *
    FROM Users 
    WHERE Users.Username = `Username`;
END;

CREATE PROCEDURE spUserInsert(IN `Id` varchar(36), IN `Username` varchar(255), IN `DisplayName` varchar(255), 
    IN `Email` varchar(255), IN `EmailVerified` BIT, IN `RegistrationDate` DATETIME, IN `PasswordSalt` varchar(127), 
    IN `PasswordHash` varchar(127), IN `RSAPublicKey` varchar(2047), IN `RSAPrivateKeyCrypt` varchar(4095), 
    IN `AlgorithmIdentifier` varchar(511)) 
BEGIN 
    INSERT INTO Users(Id, Username, DisplayName, Email, EmailVerified, RegistrationDate, PasswordSalt, PasswordHash, RSAPublicKey, RSAPrivateKeyCrypt, AlgorithmIdentifier)
    VALUES (UUID_TO_BIN(`Id`), `Username`, `DisplayName`, `Email`, `EmailVerified`, `RegistrationDate`, `PasswordSalt`, `PasswordHash`, `RSAPublicKey`, `RSAPrivateKeyCrypt`, `AlgorithmIdentifier`);
END;

CREATE PROCEDURE spUserUpdateProfile(IN `Id` varchar(36), IN `Username` varchar(255), IN `DisplayName` varchar(255),
    IN `Email` varchar(255), IN `EmailVerified` BIT) 
BEGIN 
    UPDATE Users 
    SET Username=`Username`, DisplayName=`DisplayName`, Email=`Email`, EmailVerified=`EmailVerified`
    WHERE Users.Id = UUID_TO_BIN(`Id`);
    SELECT `Id`;
END;

CREATE PROCEDURE spUserUpdateKeys(IN `Id` varchar(36), IN `PasswordHash` varchar(127), IN `RSAPrivateKeyCrypt` varchar(4095))
BEGIN 
    UPDATE Users 
    SET PasswordHash = `PasswordHash`, RSAPrivateKeyCrypt = `RSAPrivateKeyCrypt`
    WHERE Users.Id = UUID_TO_BIN(`Id`);
    SELECT `Id`;
END;

//

DELIMITER ;