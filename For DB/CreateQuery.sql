USE master
GO

IF EXISTS (SELECT * FROM sys.syslogins WHERE name = 'Nurse')
DROP LOGIN Nurse
GO
CREATE LOGIN Nurse WITH PASSWORD = 'med321';

DROP DATABASE IF EXISTS MedCertificatesDb
GO
CREATE DATABASE MedCertificatesDb
GO
USE MedCertificatesDb

USE MedCertificatesDb;
CREATE USER Nurse FOR LOGIN Nurse;

EXEC sp_addrolemember 'db_datareader', 'Nurse';
EXEC sp_addrolemember 'db_datawriter', 'Nurse';

ALTER DATABASE MedCertificatesDb
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
DROP TABLE IF EXISTS HealthGroupChanges_table
GO


CREATE TABLE Departments_table (
	DepartmentId INT PRIMARY KEY IDENTITY NOT NULL,
	Name NVARCHAR(100) UNIQUE NOT NULL,
	MaxCourse INT DEFAULT '3' NOT NULL
)
GO

CREATE TABLE Courses_table (
	CourseId INT PRIMARY KEY IDENTITY NOT NULL,
	DepartmentId INT NOT NULL,
	Number INT DEFAULT '1' NOT NULL,
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

INSERT INTO Departments_table(Name, MaxCourse) VALUES (N'Неопределенные', 999)
INSERT INTO Courses_table(DepartmentId, Number) VALUES (1, 99)
INSERT INTO Groups_table(CourseId, Name) VALUES (1, '####')

CREATE TABLE Students_table (
	StudentId INT PRIMARY KEY IDENTITY NOT NULL,
	GroupId INT NOT NULL,
	FirstName NVARCHAR(20) NOT NULL,
	SecondName NVARCHAR(20) NOT NULL,
	ThirdName NVARCHAR(20) NULL,
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

INSERT INTO HealthGroup_table (HealthGroup) VALUES (N'1-группа'), (N'2-группа'), (N'3-группа'), (N'4-группа'), (N'Не указана')
INSERT INTO PEGroup_table(PEGroup) VALUES (N'Основная'), (N'Подготовительная'), (N'СМГ'), (N'ЛФК'), (N'Освобождение'), (N'Не указана')

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

CREATE TABLE HealthGroupChanges_table (
	StudentId INT NOT NULL,
	CurrHealth NVARCHAR(20) NOT NULL,
	CurrPE NVARCHAR(20) NOT NULL,
	PrevHealth NVARCHAR(20) NOT NULL,
	PrevPE NVARCHAR(20) NOT NULL,
	Date DATE DEFAULT GETDATE() NOT NULL,
	CONSTRAINT FK_Changes_Student FOREIGN KEY (StudentId) REFERENCES Students_table(StudentId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
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

DROP TRIGGER IF EXISTS CertificateChange_trigger
GO
CREATE TRIGGER CertificateChange_trigger
ON Certificates_table AFTER UPDATE
AS
BEGIN
	DECLARE @CurrHealth NVARCHAR(20) = (SELECT TOP 1 HealthGroup FROM inserted JOIN HealthGroup_table ON HealthGroup_table.HealthGroupId = inserted.HealthGroupId)
	DECLARE @PrevHealth NVARCHAR(20) = (SELECT TOP 1 HealthGroup FROM deleted JOIN HealthGroup_table ON HealthGroup_table.HealthGroupId = deleted.HealthGroupId)
	DECLARE @CurrPE NVARCHAR(20) = (SELECT TOP 1 PEGroup FROM inserted JOIN PEGroup_table ON PEGroup_table.PEGroupId = inserted.PEGroupId)
	DECLARE @PrevPE NVARCHAR(20) = (SELECT TOP 1 PEGroup FROM deleted JOIN PEGroup_table ON PEGroup_table.PEGroupId = deleted.PEGroupId)
	
	IF @CurrHealth != @PrevHealth OR @CurrPE != @PrevPE
		INSERT HealthGroupChanges_table(StudentId, CurrHealth, CurrPE, PrevHealth, PrevPE)
			VALUES ((SELECT TOP 1 StudentId FROM inserted), @CurrHealth, @CurrPE, @PrevHealth, @PrevPE)
END
GO


--Представления

DROP VIEW IF EXISTS StudentsCertificates_view
GO
CREATE VIEW StudentsCertificates_view
AS
SELECT DISTINCT s.StudentId, s.FirstName, s.SecondName, s.ThirdName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note FROM Students_table AS s
	JOIN Certificates_table as c ON c.StudentId = s.StudentId
	JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
	JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
GO

DROP VIEW IF EXISTS DataGrid_view
GO
CREATE VIEW DataGrid_view
AS
SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, s.StudentId, c.CertificateId, FirstName, SecondName, ThirdName, g.Name AS GroupName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
GO

DROP VIEW IF EXISTS HealthGroupChanges_view
GO
CREATE VIEW HealthGroupChanges_view
AS
SELECT SecondName, FirstName, ThirdName, g.Name AS GroupName, CurrHealth, CurrPE, PrevHealth, PrevPE, h.Date AS 'Update date' FROM Students_table
	JOIN HealthGroupChanges_table AS h ON h.StudentId = Students_table.StudentId
	JOIN Groups_table AS g ON g.GroupId = Students_table.GroupId
GO

DROP VIEW IF EXISTS TotalReport_view
GO
CREATE VIEW TotalReport_view
AS
SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, s.FirstName, s.SecondName, s.ThirdName, g.Name AS GroupName, pe.PEGroup, h.HealthGroup, c.IssueDate, c.ValidDate, cr.Number AS Course, d.Name AS Department
FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
		JOIN Courses_table AS cr ON cr.CourseId = g.CourseId
		JOIN Departments_table AS d ON d.DepartmentId = cr.CourseId
GO



--Процедуры

DROP PROCEDURE IF EXISTS AutoCreateCourses_procedure 
GO
CREATE PROCEDURE AutoCreateCourses_procedure @DepId INT
AS
BEGIN
	DECLARE @Num INT = (SELECT TOP 1 MaxCourse FROM Departments_table WHERE DepartmentId = @DepId)
	WHILE @Num > 0
	BEGIN
		IF NOT EXISTS (SELECT * FROM Courses_table WHERE Number = @Num AND DepartmentId = @DepId)
			INSERT Courses_table(DepartmentId, Number) VALUES (@DepId, @Num)
		SET @Num = @Num - 1
	END
END
GO

DROP PROCEDURE IF EXISTS ReceiveStudentsGroup_procedure
GO
CREATE PROCEDURE ReceiveStudentsGroup_procedure @GroupId INT
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.StudentId, c.CertificateId, s.FirstName, s.SecondName, s.ThirdName, g.Name AS GroupName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
			WHERE s.GroupId = @GroupId
	)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, StudentId, CertificateId, FirstName, SecondName, ThirdName, GroupName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
	FROM RankedCertificates
	WHERE rn = 1
	ORDER BY SecondName ASC, FirstName ASC
END
GO

DROP PROCEDURE IF EXISTS ReceiveStudentsCourse_procedure
GO
CREATE PROCEDURE ReceiveStudentsCourse_procedure @CourseId INT
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.StudentId, c.CertificateId, s.FirstName, s.SecondName, s.ThirdName, s.BirthDate, g.Name AS GroupName, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
			WHERE g.CourseId = @CourseId
	)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, StudentId, CertificateId, FirstName, SecondName, ThirdName, GroupName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
	FROM RankedCertificates
	WHERE rn = 1
	ORDER BY SecondName ASC, FirstName ASC
END
GO

DROP PROCEDURE IF EXISTS ReceiveStudentsDepartment_procedure
GO
CREATE PROCEDURE ReceiveStudentsDepartment_procedure @DepartmentId INT
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.StudentId, c.CertificateId, s.FirstName, s.SecondName, s.ThirdName, g.Name AS GroupName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
		JOIN Courses_table as cr ON cr.CourseId = g.CourseId
			WHERE cr.DepartmentId = @DepartmentId
	)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, StudentId, CertificateId, FirstName, SecondName, ThirdName, GroupName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
	FROM RankedCertificates
	WHERE rn = 1
	ORDER BY SecondName ASC, FirstName ASC
END
GO

DROP PROCEDURE IF EXISTS ReceiveStudents_procedure
GO
CREATE PROCEDURE ReceiveStudents_procedure
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.StudentId, c.CertificateId, s.FirstName, s.SecondName, s.ThirdName, g.Name AS GroupName, s.BirthDate, h.HealthGroup, pe.PEGroup, c.ValidDate, c.IssueDate, c.Note,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
	)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, StudentId, CertificateId, FirstName, SecondName, ThirdName, GroupName, BirthDate, HealthGroup, PEGroup, ValidDate, IssueDate, Note
	FROM RankedCertificates
	WHERE rn = 1
	ORDER BY SecondName ASC, FirstName ASC
END
GO

DROP PROCEDURE IF EXISTS ReceiveStudentsForReport_procedure
GO
CREATE PROCEDURE ReceiveStudentsForReport_procedure
AS
BEGIN
	WITH RankedCertificates AS (
		SELECT s.SecondName, s.FirstName, s.ThirdName, g.Name AS GroupName, pe.PEGroup, h.HealthGroup, c.IssueDate, c.ValidDate, cr.Number AS Course, d.Name AS Department,
			   ROW_NUMBER() OVER(PARTITION BY s.StudentId ORDER BY c.ValidDate DESC) as rn
		FROM Students_table AS s
		JOIN Certificates_table as c ON c.StudentId = s.StudentId
		JOIN HealthGroup_table AS h ON h.HealthGroupId = c.HealthGroupId
		JOIN PEGroup_table AS pe ON pe.PEGroupId = c.PEGroupId
		JOIN Groups_table AS g ON g.GroupId = s.GroupId
		JOIN Courses_table AS cr ON cr.CourseId = g.CourseId
		JOIN Departments_table AS d ON d.DepartmentId = cr.CourseId 
	)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum, SecondName, FirstName, ThirdName, GroupName, PEGroup, HealthGroup, IssueDate, ValidDate, Course, Department
	FROM RankedCertificates
	WHERE rn = 1
	GROUP BY PEGroup, HealthGroup, Department, Course, GroupName, SecondName, FirstName, ThirdName, IssueDate, ValidDate
END
GO

DROP PROCEDURE IF EXISTS UpdateCourseYear_procedure
GO
CREATE PROCEDURE UpdateCourseYear_procedure
AS
BEGIN
	UPDATE Courses_table SET Number = Number + 1
	DELETE Courses_table
		WHERE Number > (SELECT TOP 1 MaxCourse FROM Departments_table WHERE Departments_table.DepartmentId = Courses_table.DepartmentId)
END



--Создание, изменение, удаление

--Отделение

DROP PROCEDURE IF EXISTS CreateDepartment_procedure
GO
CREATE PROCEDURE CreateDepartment_procedure @Name NVARCHAR(100), @MaxCourse INT
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
CREATE PROCEDURE UpdateDepartment_procedure @Id INT, @Name NVARCHAR(100), @MaxCourse INT
AS
BEGIN
	UPDATE Departments_table SET Name = @Name, MaxCourse = @MaxCourse WHERE DepartmentId = @Id
END
GO


--Курс

DROP PROCEDURE IF EXISTS CreateCourse_procedure
GO
CREATE PROCEDURE CreateCourse_procedure @DepId INT, @Number INT
AS
BEGIN
	INSERT Courses_table(DepartmentId, Number) VALUES (@DepId, @Number)
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
CREATE PROCEDURE UpdateCourse_procedure @Id INT, @Number INT
AS
BEGIN
	UPDATE Courses_table SET Number = @Number WHERE CourseId = @Id
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
	UPDATE Students_table SET GroupId = @GroupId, FirstName = @FirstName, SecondName = @SecondName, ThirdName = @ThirdName, BirthDate = @BirthDate WHERE StudentId = @Id
END
GO

DROP PROCEDURE IF EXISTS UpdateStudentGroup_procedure
GO
CREATE PROCEDURE UpdateStudentGroup_procedure @Id INT, @GroupId INT
AS
BEGIN
	UPDATE Students_table SET GroupId = @GroupId WHERE StudentId = @Id
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
CREATE PROCEDURE UpdateCertificate_procedure @Id INT, @HealthGroupId INT, @PEGroupId INT, @IssueDate DATE, @ValidDate DATE, @Note NVARCHAR(MAX)
AS
BEGIN
	UPDATE Certificates_table SET HealthGroupId = @HealthGroupId, PEGroupId = @PEGroupId, IssueDate = @IssueDate, ValidDate = @ValidDate, Note = @Note WHERE CertificateId = @Id
END
GO


INSERT INTO Departments_table(Name, MaxCourse)
	VALUES (N'ПОИТ', 4), (N'Бух. учет, анализ и контроль', 3), (N'Планово-экономическая и аналитическая деятельность', 3), (N'Банковская деятельность', 3),
	(N'Правоведение', 3), (N'Торговая деятельность', 3), (N'Операционная деятельность в логистике', 3)

INSERT INTO Courses_table(DepartmentId, Number)
	VALUES (2, 1), (2, 2), (2, 3), (2, 4),
			(3, 1), (3, 2), (3, 3),
			(4, 1), (4, 2), (4, 3),
			(5, 1), (5, 2), (5, 3),
			(6, 1), (6, 2), (6, 3),
			(7, 1), (7, 2), (7, 3),
			(8, 1), (8, 2), (8, 3)

			SELECT * FROM Courses_table
INSERT INTO Groups_table(CourseId, Name) VALUES (4, N'Т-095'), (4, N'Т-094'), (2, N'Т-232'), (2, N'Т-233')
INSERT INTO Students_table(GroupId, SecondName, FirstName, ThirdName, BirthDate) 
VALUES 
(2, N'Волынцев', N'Николай', N'Андреевич', '12-12-2004'),
(2, N'Петров', N'Иван', N'Алексеевич', '01-01-2005'),
(2, N'Смирнов', N'Алексей', N'Владимирович', '02-02-2005'),
(2, N'Кузнецов', N'Сергей', N'Петрович', '03-03-2005'),
(2, N'Попов', N'Дмитрий', N'Иванович', '04-04-2005'),
(2, N'Васильев', N'Андрей', N'Сергеевич', '05-05-2005'),
(2, N'Павлов', N'Александр', N'Андреевич', '06-06-2005'),
(2, N'Семенов', N'Владимир', N'Алексеевич', '07-07-2005'),
(2, N'Голубев', N'Степан', N'Владимирович', '08-08-2005'),
(2, N'Новиков', N'Ярослав', N'Михайлович', '09-09-2005'),
(2, N'Морозов', N'Артем', N'Александрович', '10-10-2005'),
(2, N'Волков', N'Никита', N'Андреевич', '11-11-2005'),
(2, N'Соловьев', N'Михаил', N'Владимирович', '12-12-2005'),
(2, N'Осипов', N'Даниил', N'Александрович', '01-01-2006'),
(2, N'Белов', N'Егор', N'Сергеевич', '02-02-2006'),
(2, N'Комаров', N'Максим', N'Андреевич', '03-03-2006'),
(2, N'Киселев', N'Илья', N'Владимирович', '04-04-2006'),
(2, N'Ильин', N'Тимофей', N'Алексеевич', '05-05-2006'),
(2, N'Гусев', N'Роман', N'Сергеевич', '06-06-2006'),
(2, N'Титов', N'Павел', N'Андреевич', '07-07-2006')

SET DATEFORMAT dmy; 
INSERT INTO Certificates_table(StudentId, HealthGroupId, PEGroupId, IssueDate, ValidDate)
VALUES
(1, 1, 1, '12-12-2023', '12-12-2024'),
(2, 1, 1, '13-12-2023', '13-12-2024'),
(3, 1, 1, '14-06-2023', '14-06-2024'),
(4, 1, 1, '15-03-2023', '15-03-2024'),
(5, 1, 1, '16-12-2023', '16-12-2024'),
(6, 1, 1, '17-12-2023', '17-12-2024'),
(7, 1, 1, '18-10-2023', '18-10-2024'),
(8, 1, 1, '19-11-2023', '19-11-2024'),
(9, 1, 1, '20-12-2023', '20-12-2024'),
(10, 1, 1, '21-12-2023', '21-12-2024'),
(11, 1, 1, '22-12-2023', '22-12-2024'),
(12, 1, 1, '23-12-2023', '23-12-2024'),
(13, 1, 1, '24-05-2023', '24-05-2024'),
(14, 1, 1, '25-10-2023', '25-10-2024'),
(15, 1, 1, '26-11-2023', '26-11-2024'),
(16, 1, 1, '27-08-2023', '27-08-2024'),
(17, 1, 1, '28-06-2023', '28-06-2024'),
(18, 1, 1, '29-07-2023', '29-07-2024'),
(19, 1, 1, '30-12-2023', '30-12-2024'),
(20, 1, 1, '31-03-2023', '31-03-2024');

