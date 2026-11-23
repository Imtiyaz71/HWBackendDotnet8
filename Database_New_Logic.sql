-- =====================================================================
-- OURHOMEWORKS: NEW LOGIC DATABASE SCHEMA & SEED DATA
-- Supports Public Registration, General & English Medium Curriculums,
-- Homework Problem Feed, Teacher Collaboration & Notifications
-- =====================================================================

-- 1. Curriculums Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Curriculums' AND xtype='U')
BEGIN
    CREATE TABLE Curriculums (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NULL
    );

    INSERT INTO Curriculums (Name, Description) VALUES 
    (N'General', N'Bangladesh National Curriculum (NCTB - Bangla & English Version)'),
    (N'English Medium', N'International Curriculum (Cambridge / Pearson Edexcel)');
END
GO

-- 2. Curriculum Subjects Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CurriculumSubjects' AND xtype='U')
BEGIN
    CREATE TABLE CurriculumSubjects (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CurriculumType NVARCHAR(100) NOT NULL,
        ClassName NVARCHAR(100) NOT NULL,
        SubjectName NVARCHAR(100) NOT NULL
    );

    -- General Curriculum (Class 1 to 5: Primary)
    INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
    ('General', 'Class 1', 'Bangla'), ('General', 'Class 1', 'English'), ('General', 'Class 1', 'Mathematics'),
    ('General', 'Class 2', 'Bangla'), ('General', 'Class 2', 'English'), ('General', 'Class 2', 'Mathematics'),
    ('General', 'Class 3', 'Bangla'), ('General', 'Class 3', 'English'), ('General', 'Class 3', 'Mathematics'), ('General', 'Class 3', 'Elementary Science'), ('General', 'Class 3', 'BGS'), ('General', 'Class 3', 'Religion'),
    ('General', 'Class 4', 'Bangla'), ('General', 'Class 4', 'English'), ('General', 'Class 4', 'Mathematics'), ('General', 'Class 4', 'Elementary Science'), ('General', 'Class 4', 'BGS'), ('General', 'Class 4', 'Religion'),
    ('General', 'Class 5', 'Bangla'), ('General', 'Class 5', 'English'), ('General', 'Class 5', 'Mathematics'), ('General', 'Class 5', 'Elementary Science'), ('General', 'Class 5', 'BGS'), ('General', 'Class 5', 'Religion');

    -- General Curriculum (Class 6 to 8: Junior)
    INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
    ('General', 'Class 6', 'Bangla'), ('General', 'Class 6', 'English'), ('General', 'Class 6', 'Mathematics'), ('General', 'Class 6', 'Science'), ('General', 'Class 6', 'BGS'), ('General', 'Class 6', 'ICT'), ('General', 'Class 6', 'Religion'),
    ('General', 'Class 7', 'Bangla'), ('General', 'Class 7', 'English'), ('General', 'Class 7', 'Mathematics'), ('General', 'Class 7', 'Science'), ('General', 'Class 7', 'BGS'), ('General', 'Class 7', 'ICT'), ('General', 'Class 7', 'Religion'),
    ('General', 'Class 8', 'Bangla'), ('General', 'Class 8', 'English'), ('General', 'Class 8', 'Mathematics'), ('General', 'Class 8', 'Science'), ('General', 'Class 8', 'BGS'), ('General', 'Class 8', 'ICT'), ('General', 'Class 8', 'Religion');

    -- General Curriculum (Class 9 & 10: Secondary)
    INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
    ('General', 'Class 9', 'Bangla'), ('General', 'Class 9', 'English'), ('General', 'Class 9', 'General Mathematics'), ('General', 'Class 9', 'Higher Mathematics'), ('General', 'Class 9', 'Physics'), ('General', 'Class 9', 'Chemistry'), ('General', 'Class 9', 'Biology'), ('General', 'Class 9', 'Accounting'), ('General', 'Class 9', 'Business Studies'), ('General', 'Class 9', 'ICT'), ('General', 'Class 9', 'BGS'),
    ('General', 'Class 10', 'Bangla'), ('General', 'Class 10', 'English'), ('General', 'Class 10', 'General Mathematics'), ('General', 'Class 10', 'Higher Mathematics'), ('General', 'Class 10', 'Physics'), ('General', 'Class 10', 'Chemistry'), ('General', 'Class 10', 'Biology'), ('General', 'Class 10', 'Accounting'), ('General', 'Class 10', 'Business Studies'), ('General', 'Class 10', 'ICT'), ('General', 'Class 10', 'BGS');

    -- English Medium (Grade 1 to Grade 10)
    INSERT INTO CurriculumSubjects (CurriculumType, ClassName, SubjectName) VALUES
    ('English Medium', 'Grade 1', 'English'), ('English Medium', 'Grade 1', 'Mathematics'), ('English Medium', 'Grade 1', 'Science'), ('English Medium', 'Grade 1', 'Social Studies'), ('English Medium', 'Grade 1', 'Bangla'),
    ('English Medium', 'Grade 2', 'English'), ('English Medium', 'Grade 2', 'Mathematics'), ('English Medium', 'Grade 2', 'Science'), ('English Medium', 'Grade 2', 'Social Studies'), ('English Medium', 'Grade 2', 'Bangla'),
    ('English Medium', 'Grade 3', 'English'), ('English Medium', 'Grade 3', 'Mathematics'), ('English Medium', 'Grade 3', 'Science'), ('English Medium', 'Grade 3', 'Social Studies'), ('English Medium', 'Grade 3', 'Bangla'),
    ('English Medium', 'Grade 4', 'English'), ('English Medium', 'Grade 4', 'Mathematics'), ('English Medium', 'Grade 4', 'Science'), ('English Medium', 'Grade 4', 'Social Studies'), ('English Medium', 'Grade 4', 'Bangla'),
    ('English Medium', 'Grade 5', 'English'), ('English Medium', 'Grade 5', 'Mathematics'), ('English Medium', 'Grade 5', 'Science'), ('English Medium', 'Grade 5', 'Social Studies'), ('English Medium', 'Grade 5', 'Bangla'),
    ('English Medium', 'Grade 6', 'English Language'), ('English Medium', 'Grade 6', 'Mathematics'), ('English Medium', 'Grade 6', 'General Science'), ('English Medium', 'Grade 6', 'ICT'), ('English Medium', 'Grade 6', 'History'), ('English Medium', 'Grade 6', 'Geography'), ('English Medium', 'Grade 6', 'Bangla'),
    ('English Medium', 'Grade 7', 'English Language'), ('English Medium', 'Grade 7', 'Mathematics'), ('English Medium', 'Grade 7', 'General Science'), ('English Medium', 'Grade 7', 'ICT'), ('English Medium', 'Grade 7', 'History'), ('English Medium', 'Grade 7', 'Geography'), ('English Medium', 'Grade 7', 'Bangla'),
    ('English Medium', 'Grade 8', 'English Language'), ('English Medium', 'Grade 8', 'Mathematics'), ('English Medium', 'Grade 8', 'General Science'), ('English Medium', 'Grade 8', 'ICT'), ('English Medium', 'Grade 8', 'History'), ('English Medium', 'Grade 8', 'Geography'), ('English Medium', 'Grade 8', 'Bangla'),
    ('English Medium', 'O-Level (Grade 9)', 'English Language'), ('English Medium', 'O-Level (Grade 9)', 'Pure Mathematics'), ('English Medium', 'O-Level (Grade 9)', 'Physics'), ('English Medium', 'O-Level (Grade 9)', 'Chemistry'), ('English Medium', 'O-Level (Grade 9)', 'Biology'), ('English Medium', 'O-Level (Grade 9)', 'Accounting'), ('English Medium', 'O-Level (Grade 9)', 'Economics'), ('English Medium', 'O-Level (Grade 9)', 'Computer Science'),
    ('English Medium', 'O-Level (Grade 10)', 'English Language'), ('English Medium', 'O-Level (Grade 10)', 'Pure Mathematics'), ('English Medium', 'O-Level (Grade 10)', 'Physics'), ('English Medium', 'O-Level (Grade 10)', 'Chemistry'), ('English Medium', 'O-Level (Grade 10)', 'Biology'), ('English Medium', 'O-Level (Grade 10)', 'Accounting'), ('English Medium', 'O-Level (Grade 10)', 'Economics'), ('English Medium', 'O-Level (Grade 10)', 'Computer Science');
END
GO

-- 3. Unified Registered Users Table (Students & Teachers)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RegisteredUsers' AND xtype='U')
BEGIN
    CREATE TABLE RegisteredUsers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(50) NOT NULL,
        Email NVARCHAR(150) NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL, -- 'Student', 'Teacher', 'Admin'
        SchoolName NVARCHAR(250) NULL,
        SchoolAddress NVARCHAR(500) NULL,
        Address NVARCHAR(500) NULL,
        Gender NVARCHAR(20) NULL,
        FatherName NVARCHAR(150) NULL,
        MotherName NVARCHAR(150) NULL,
        CurriculumType NVARCHAR(100) NULL, -- 'General', 'English Medium'
        ClassName NVARCHAR(100) NULL,
        SpecialistSubjects NVARCHAR(MAX) NULL, -- Comma-separated or JSON list
        DeviceToken NVARCHAR(MAX) NULL,
        CreatedAt DATETIME DEFAULT GETUTCDATE()
    );

    CREATE NONCLUSTERED INDEX IX_RegisteredUsers_Phone ON RegisteredUsers(Phone);
    CREATE NONCLUSTERED INDEX IX_RegisteredUsers_Email ON RegisteredUsers(Email);
END
GO

-- 4. Homework Problems Table (Posted by Students)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HomeworkProblems' AND xtype='U')
BEGIN
    CREATE TABLE HomeworkProblems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        StudentName NVARCHAR(150) NOT NULL,
        StudentPhone NVARCHAR(50) NOT NULL,
        SchoolName NVARCHAR(250) NULL,
        ClassName NVARCHAR(100) NOT NULL,
        CurriculumType NVARCHAR(100) NOT NULL,
        SubjectName NVARCHAR(100) NOT NULL,
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        ImageUrl NVARCHAR(MAX) NULL,
        CreatedAt DATETIME DEFAULT GETUTCDATE()
    );

    CREATE NONCLUSTERED INDEX IX_HomeworkProblems_Subject ON HomeworkProblems(SubjectName);
    CREATE NONCLUSTERED INDEX IX_HomeworkProblems_Class ON HomeworkProblems(ClassName);
END
GO

-- 5. Problem Replies Table (Comments / Feedback / Solutions by Teachers, Students, AI)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProblemReplies' AND xtype='U')
BEGIN
    CREATE TABLE ProblemReplies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProblemId INT NOT NULL,
        UserId INT NOT NULL,
        UserName NVARCHAR(150) NOT NULL,
        UserRole NVARCHAR(50) NOT NULL, -- 'Teacher', 'Student', 'AI'
        ReplyText NVARCHAR(MAX) NOT NULL,
        ImageUrl NVARCHAR(MAX) NULL,
        CreatedAt DATETIME DEFAULT GETUTCDATE()
    );

    CREATE NONCLUSTERED INDEX IX_ProblemReplies_ProblemId ON ProblemReplies(ProblemId);
END
GO

-- 6. Reply Likes Table (Likes given by students/users on replies)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ReplyLikes' AND xtype='U')
BEGIN
    CREATE TABLE ReplyLikes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReplyId INT NOT NULL,
        UserId INT NOT NULL,
        CreatedAt DATETIME DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Reply_User UNIQUE(ReplyId, UserId)
    );
END
GO

-- 7. User Notifications Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserNotifications' AND xtype='U')
BEGIN
    CREATE TABLE UserNotifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL, -- 'ProblemCreated', 'ReplyAdded', 'ReplyLiked'
        RelatedProblemId INT NULL,
        IsRead BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETUTCDATE()
    );

    CREATE NONCLUSTERED INDEX IX_UserNotifications_UserId ON UserNotifications(UserId);
END
GO
