USE master
GO

DROP DATABASE IF EXISTS MedicalCertificatesDb
GO
CREATE DATABASE MedicalCertificatesDb
GO
USE MedicalCertificatesDb

ALTER DATABASE MedicalCertificatesDb
SET RECOVERY FULL;
GO
SET DATEFORMAT dmy;  


DROP TABLE IF EXISTS StudentsGroupArchive_table
GO
DROP TABLE IF EXISTS Certificates_table
GO
DROP TABLE IF EXISTS HealthGroup_table
GO
DROP TABLE IF EXISTS PEGroup_table
GO
DROP TABLE IF EXISTS Students_table
GO
DROP TABLE IF EXISTS Groups_table
GO
DROP TABLE IF EXISTS Courses_table
GO
DROP TABLE IF EXISTS Departments_table
GO


CREATE TABLE Departments_table (
	DepartmentId INT PRIMARY KEY IDENTITY NOT NULL,
	Name NVARCHAR(30) UNIQUE NOT NULL,
	MaxCourse INT DEFAULT '3' NOT NULL
)
GO

CREATE TABLE Courses_table (
	CourseId INT PRIMARY KEY IDENTITY NOT NULL,
	DepartmentId INT NOT NULL,
	Number INT DEFAULT '1' NOT NULL,
	Year NVARCHAR(5) NOT NULL,
	CONSTRAINT FK_Department FOREIGN KEY (DepartmentId) REFERENCES Departments_table(DepartmentId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
)
GO

CREATE TABLE Groups_table (
	GroupId INT PRIMARY KEY IDENTITY NOT NULL,
	CourseId INT NOT NULL,
	Name NVARCHAR(5) NOT NULL,
	CONSTRAINT FK_Course FOREIGN KEY (CourseId) REFERENCES Courses_table(CourseId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
)
GO

CREATE TABLE Students_table (
	StudentId INT PRIMARY KEY IDENTITY NOT NULL,
	GroupId INT NOT NULL,
	FirstName NVARCHAR(20) NOT NULL,
	SecondName NVARCHAR(20) NOT NULL,
	ThirdName NVARCHAR(20) NOT NULL,
	BirthDate DATE NOT NULL,
	CONSTRAINT FK_Group FOREIGN KEY (GroupId) REFERENCES Groups_table(GroupId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
)
GO

CREATE TABLE HealthGroup_table (
	HealthGroupId INT PRIMARY KEY IDENTITY NOT NULL,
	HealthGroup NVARCHAR(20) NOT NULL
)
GO

CREATE TABLE PEGroup_table (
	PEGroupId INT PRIMARY KEY IDENTITY NOT NULL,
	PEGroup NVARCHAR(20) NOT NULL
)
GO

INSERT INTO HealthGroup_table (HealthGroup) VALUES (N'1-группа'), (N'2-группа'), (N'3-группа'), (N'4-группа')
INSERT INTO PEGroup_table(PEGroup) VALUES (N'Основная'), (N'Подготовительная'), (N'СМГ'), (N'ЛФК')

CREATE TABLE Certificates_table (
	CertificateId INT PRIMARY KEY IDENTITY NOT NULL,
	StudentId INT NOT NULL,
	HealthGroupId INT NOT NULL,
	PEGroupId INT NOT NULL,
	IssueDate DATE NOT NULL,
	ValidDate DATE NOT NULL,
	Note NVARCHAR(MAX) NULL,
	CONSTRAINT FK_Student FOREIGN KEY (StudentId) REFERENCES Students_table(StudentId)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	CONSTRAINT FK_HealthGroup FOREIGN KEY (HealthGroupId) REFERENCES HealthGroup_table(HealthGroupId)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	CONSTRAINT FK_PEGroup FOREIGN KEY (PEGroupId) REFERENCES PEGroup_table(PEGroupId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
)
GO

CREATE TABLE StudentsGroupArchive_table(
	NoteId INT PRIMARY KEY IDENTITY NOT NULL,
	StudentId INT NOT NULL,
	CourseId INT NOT NULL,
	OldGroupId INT NOT NULL,
	NewGroupId INT NOT NULL,
	Year NVARCHAR(5) NOT NULL,
	AlterDate DATE DEFAULT GETDATE() NOT NULL
)
GO


--Триггеры

DROP TRIGGER IF EXISTS UpperCaseName_trigger
GO
CREATE TRIGGER UpperCaseName_trigger
ON Students_table AFTER INSERT
AS
BEGIN
	DECLARE @NameFirst NVARCHAR(1) = (SELECT TOP 1 FirstName FROM inserted)
	DECLARE @SurnameFirst NVARCHAR(1) = (SELECT TOP 1 SecondName FROM inserted)
	DECLARE @ThirdNameFirst NVARCHAR(1) = (SELECT TOP 1 ThirdName FROM inserted)
	IF @NameFirst COLLATE Cyrillic_General_CS_AS != UPPER(@NameFirst) COLLATE Cyrillic_General_CS_AS
		UPDATE Students_table SET FirstName = UPPER(@NameFirst) + SUBSTRING(FirstName, 2, LEN(FirstName)) WHERE StudentId = (SELECT StudentId FROM inserted)
	IF @SurnameFirst COLLATE Cyrillic_General_CS_AS != UPPER(@SurnameFirst) COLLATE Cyrillic_General_CS_AS
		UPDATE Students_table SET SecondName = UPPER(@SurnameFirst) + SUBSTRING(SecondName, 2, LEN(SecondName)) WHERE StudentId = (SELECT StudentId FROM inserted)
	IF @ThirdNameFirst COLLATE Cyrillic_General_CS_AS != UPPER(@ThirdNameFirst) COLLATE Cyrillic_General_CS_AS
		UPDATE Students_table SET ThirdName = UPPER(@ThirdNameFirst) + SUBSTRING(ThirdName, 2, LEN(ThirdName)) WHERE StudentId = (SELECT StudentId FROM inserted)
END
GO

DROP TRIGGER IF EXISTS StudentsAlter_trigger
GO
CREATE TRIGGER StudentsAlter_trigger
ON Students_table AFTER UPDATE
AS
BEGIN
	DECLARE @OldGroup INT = (SELECT GroupId FROM deleted)
	DECLARE @NewGroup INT = (SELECT GroupId FROM inserted)
	DECLARE @Year NVARCHAR(5) = (SELECT Year FROM inserted 
			JOIN Groups_table ON inserted.GroupId = Groups_table.GroupId
			JOIN Courses_table ON Courses_table.CourseId = Groups_table.CourseId)
	IF @OldGroup != @NewGroup
		BEGIN
			INSERT StudentsGroupArchive_table(StudentId, CourseId, OldGroupId, NewGroupId, Year)
			VALUES (
				(SELECT StudentId FROM inserted),
				(SELECT Courses_table.CourseId FROM inserted
					JOIN Groups_table ON inserted.GroupId = Groups_table.GroupId
					JOIN Courses_table ON Courses_table.CourseId = Groups_table.CourseId),
				@OldGroup,
				@NewGroup,
				@Year
			)
		END
END
GO

INSERT INTO Departments_table(Name, MaxCourse) VALUES (N'Инф. тех.', 4), (N'Тест', 3)
INSERT INTO Courses_table(Number, DepartmentId, Year) VALUES (1, 1, '23/24')
INSERT INTO Groups_table(Name, CourseId) VALUES (N'T-341', 1)
INSERT INTO Groups_table(Name, CourseId) VALUES (N'T-342', 1)
INSERT INTO Students_table(GroupId, FirstName, SecondName, ThirdName, BirthDate) VALUES (1, 'aboba', N'Аваы', N'аваыв', '31-12-2004')
INSERT INTO Students_table(GroupId, FirstName, SecondName, ThirdName, BirthDate) VALUES (1, N'виктор', N'ав', N'ав', '31-12-2004')
INSERT INTO Students_table(GroupId, FirstName, SecondName, ThirdName, BirthDate) VALUES (1, N'гор', N'Аваы', N'аваыв', '12-12-2004')
UPDATE Students_table SET GroupId = 2 WHERE StudentId = 2
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate) VALUES (1, 3, 3, '12-12-2022', '12-12-2023')
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate) VALUES (1, 3, 3, '12-12-2021', '12-12-2022')
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate) VALUES (1, 3, 3, '12-07-2023', '12-07-2024')
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate) VALUES (2, 3, 3, '28-02-2023', '28-02-2024')
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate) VALUES (3, 2, 2, '02-09-2022', '02-09-2023')

SELECT * FROM StudentsGroupArchive_table
SELECT * FROM Students_table


--Представления

DROP VIEW IF EXISTS StudentsCertificates_view
GO
CREATE VIEW StudentsCertificates_view
AS
SELECT s.StudentId, s.FirstName, s.SecondName, s.ThirdName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note FROM Students_table AS s
	JOIN Certificates_table as c ON c.StudentId = s.StudentId
	JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
	JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
GO

SELECT * FROM StudentsCertificates_view WHERE StudentId = 1


--Процедуры

DROP PROCEDURE IF EXISTS ReceiveStudentsGroup_procedure
GO
CREATE PROCEDURE ReceiveStudentsGroup_procedure @Year NVARCHAR(5), @GroupId INT
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.FirstName, s.SecondName, s.ThirdName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
			WHERE ValidDate > '30-08-' + '20' + SUBSTRING(@Year, 1, 2) AND IssueDate < '01-08-' + '20' + SUBSTRING(@Year, 4, 2) AND s.GroupId = @GroupId
	UNION
		SELECT s.FirstName, s.SecondName, s.ThirdName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN StudentsGroupArchive_table AS sa ON sa.StudentId = s.StudentId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
			WHERE sa.Year = @Year AND ValidDate > '30-08-' + '20' + SUBSTRING(@Year, 1, 2) AND IssueDate < '01-08-' + '20' + SUBSTRING(@Year, 4, 2) AND sa.OldGroupId = @GroupId
		--TODO: Если он был в 2х разных группах или просто глянуть инфу за 2+ года назад то оно не приравняет Year
	
	)
	SELECT FirstName, SecondName, ThirdName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
	FROM RankedCertificates
	WHERE rn = 1
END
GO

EXEC ReceiveStudentsGroup_procedure '23/24', 1

DROP PROCEDURE IF EXISTS UpdateCourseYear_procedure
GO
CREATE PROCEDURE UpdateCourseYear_procedure
AS
BEGIN
	UPDATE Courses_table SET Number = Number + 1, Year = CAST((CAST(SUBSTRING(Year, 1, 2) AS INT) + 1) AS VARCHAR) + '/' + CAST((CAST(SUBSTRING(Year, 4, 2) AS INT) + 1) AS VARCHAR)
		WHERE Number + 1 <= (SELECT MaxCourse FROM Departments_table WHERE Departments_table.DepartmentId = CourseId)
END






--Создание, изменение, удаление

--Отделение

DROP PROCEDURE IF EXISTS CreateDepartment_procedure
GO
CREATE PROCEDURE CreateDepartment_procedure @Name NVARCHAR(30), @MaxCourse INT
AS
BEGIN
	INSERT Departments_table(Name, MaxCourse) VALUES (@Name, @MaxCourse)
END
GO

DROP PROCEDURE IF EXISTS DeleteDepartment_procedure
GO
CREATE PROCEDURE DeleteDepartment_procedure @Id INT
AS
BEGIN
	DELETE Departments_table WHERE DepartmentId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateDepartment_procedure
GO
CREATE PROCEDURE UpdateDepartment_procedure @Id INT, @Name NVARCHAR(30), @MaxCourse INT
AS
BEGIN
	UPDATE Departments_table SET Name = @Name, MaxCourse = @MaxCourse WHERE DepartmentId = @Id
END
GO


--Курс

DROP PROCEDURE IF EXISTS CreateCourse_procedure
GO
CREATE PROCEDURE CreateCourse_procedure @DepId INT, @Number INT, @Year NVARCHAR(5)
AS
BEGIN
	INSERT Courses_table(DepartmentId, Number, Year) VALUES (@DepId, @Number, @Year)
END
GO

DROP PROCEDURE IF EXISTS DeleteCourse_procedure
GO
CREATE PROCEDURE DeleteCourse_procedure @Id INT
AS
BEGIN
	DELETE Courses_table WHERE CourseId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateCourse_procedure
GO
CREATE PROCEDURE UpdateCourse_procedure @Id INT, @DepId INT, @Number INT, @Year NVARCHAR(5)
AS
BEGIN
	UPDATE Courses_table SET DepartmentId = @DepId, Number = @Number, Year = @Year WHERE CourseId = @Id
END
GO


--Группа

DROP PROCEDURE IF EXISTS CreateGroup_procedure
GO
CREATE PROCEDURE CreateGroup_procedure @CourseId INT, @Name NVARCHAR(5)
AS
BEGIN
	INSERT Groups_table(CourseId, Name) VALUES (@CourseId, @Name)
END
GO

DROP PROCEDURE IF EXISTS DeleteGroup_procedure
GO
CREATE PROCEDURE DeleteGroup_procedure @Id INT
AS
BEGIN
	DELETE Groups_table WHERE GroupId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateGroup_procedure
GO
CREATE PROCEDURE UpdateGroup_procedure @Id INT, @CourseId INT, @Name NVARCHAR(5)
AS
BEGIN
	UPDATE Groups_table SET CourseId = @CourseId, Name = @Name WHERE GroupId = @Id
END
GO


--Студент

DROP PROCEDURE IF EXISTS CreateStudent_procedure
GO
CREATE PROCEDURE CreateStudent_procedure @GroupId INT, @FirstName NVARCHAR(20), @SecondName NVARCHAR(20), @ThirdName NVARCHAR(20), @BirthDate DATE
AS
BEGIN
	INSERT Students_table(FirstName, SecondName, ThirdName, GroupId, BirthDate) VALUES (@FirstName, @SecondName, @ThirdName, @GroupId, @BirthDate)
END
GO

DROP PROCEDURE IF EXISTS DeleteStudent_procedure
GO
CREATE PROCEDURE DeleteStudent_procedure @Id INT
AS
BEGIN
	DELETE Students_table WHERE StudentId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateStudent_procedure
GO
CREATE PROCEDURE UpdateStudent_procedure @Id INT, @GroupId INT, @FirstName NVARCHAR(20), @SecondName NVARCHAR(20), @ThirdName NVARCHAR(20), @BirthDate DATE
AS
BEGIN
	UPDATE Students_table SET @GroupId = @GroupId, FirstName = @FirstName, SecondName = @SecondName, ThirdName = @ThirdName, BirthDate = @BirthDate WHERE StudentId = @Id
END
GO



--Справка

DROP PROCEDURE IF EXISTS CreateCertificate_procedure
GO
CREATE PROCEDURE CreateCertificate_procedure @StudentId INT, @HealthGroupId INT, @PEGroupId INT, @IssueDate DATE, @ValidDate DATE, @Note NVARCHAR(MAX)
AS
BEGIN
	INSERT Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate, Note) VALUES (@StudentId, @HealthGroupId, @PEGroupId, @IssueDate, @ValidDate, @Note)
END
GO

DROP PROCEDURE IF EXISTS DeleteCertificate_procedure
GO
CREATE PROCEDURE DeleteCertificate_procedure @Id INT
AS
BEGIN
	DELETE Certificates_table WHERE CertificateId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateCertificate_procedure
GO
CREATE PROCEDURE UpdateCertificate_procedure @Id INT, @StudentId INT, @HealthGroupId INT, @PEGroupId INT, @IssueDate DATE, @ValidDate DATE, @Note NVARCHAR(MAX)
AS
BEGIN
	UPDATE Certificates_table SET StudentId = @StudentId, HealthGroupId = @HealthGroupId, PEGroupId = @PEGroupId, IssueDate = @IssueDate, @ValidDate = @ValidDate, Note = @Note WHERE CertificateId = @Id
END
GO