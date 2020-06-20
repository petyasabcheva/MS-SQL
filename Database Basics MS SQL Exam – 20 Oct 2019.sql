CREATE DATABASE Service

USE Service

CREATE TABLE Users(
Id INT NOT NULL PRIMARY KEY IDENTITY,
Username NVARCHAR(30) NOT NULL UNIQUE,
[Password] NVARCHAR(50) NOT NULL,
[Name] NVARCHAR(50) NOT NULL,
Birthdate DATETIME NOT NULL,
Age INT NOT NULL 
CHECK(Age>=14 AND Age<=110),
Email NVARCHAR(50) NOT NULL
)

CREATE TABLE [Status](
Id INT NOT NULL PRIMARY KEY IDENTITY,
[Label] NVARCHAR(30) NOT NULL
)

CREATE TABLE Departments(
Id INT NOT NULL PRIMARY KEY IDENTITY,
[Name] NVARCHAR(50) NOT NULL
)

CREATE TABLE Employees(
Id INT NOT NULL PRIMARY KEY IDENTITY,
FirstName NVARCHAR(25),
LastName NVARCHAR(25),
Birthdate DATETIME,
Age INT 
CHECK(Age>=18 AND Age<=110),
DepartmentId INT FOREIGN KEY 
REFERENCES Departments(Id)
)

CREATE TABLE Categories(
Id INT NOT NULL PRIMARY KEY IDENTITY,
[Name] NVARCHAR(50) NOT NULL,
DepartmentId INT NOT NULL FOREIGN KEY 
REFERENCES Departments(Id)
)

CREATE TABLE Reports(
Id INT NOT NULL PRIMARY KEY IDENTITY,
CategoryId INT NOT NULL FOREIGN KEY 
REFERENCES Categories(Id),
StatusId INT NOT NULL FOREIGN KEY 
REFERENCES [Status](Id),
OpenDate DATETIME NOT NULL,
CloseDate DATETIME,
Description NVARCHAR(200) NOT NULL,
UserId INT NOT NULL FOREIGN KEY 
REFERENCES Users(Id),
EmployeeId INT FOREIGN KEY 
REFERENCES Employees(Id)
)

INSERT INTO Employees(FirstName,LastName,Birthdate,DepartmentId)
VALUES
('Marlo','O''Malley','1958-9-21',1),
('Niki','Stanaghan','1969-11-26',4),
('Ayrton','Senna','1960-03-21',9),
('Ronnie','Peterson','1944-02-14',9),
('Giovanna','Amati','1959-07-20',5)

INSERT INTO Reports(CategoryId,StatusId,OpenDate,CloseDate,Description,UserId,EmployeeId)
VALUES
(1,1,'2017-04-13',NULL,'Stuck Road on Str.133',6,2),
(6,3,'2015-09-05','2015-12-06','Charity trail running',3,5),
(14,2,'2015-09-07',NULL,'Falling bricks on Str.58',5,2),
(4,3,'2017-07-03','2017-07-06','Cut off streetlight on Str.11',1,1)

UPDATE Reports 
SET CloseDate=GETDATE() WHERE CloseDate IS NULL;

DELETE FROM Reports WHERE StatusId=4;

SELECT Description,FORMAT(OpenDate,'dd-MM-yyyy') FROM Reports WHERE EmployeeId IS NULL
ORDER BY OpenDate,Description

SELECT Description,c.Name AS 'CategoryName' FROM Reports r
JOIN Categories c ON r.CategoryId=c.Id
WHERE CategoryId IS NOT NULL
ORDER BY Description,c.Name

SELECT TOP(5) [Name] AS 'CategoryName',COUNT(*) AS 'ReportsNumber' FROM Categories c
JOIN Reports r ON r.CategoryId=c.Id
GROUP BY [Name]
ORDER BY ReportsNumber DESC,[Name] ASC

SELECT Username,c.Name FROM Users u
JOIN Reports r ON u.Id=r.UserId
JOIN Categories c ON c.Id=r.CategoryId
WHERE DATEPART(MONTH,r.OpenDate)=DATEPART(MONTH,u.Birthdate) AND DATEPART(DAY,r.OpenDate)=DATEPART(DAY,u.Birthdate) 
ORDER BY Username,c.Name

SELECT e.FirstName+' '+e.LastName AS 'FullName',COUNT(DISTINCT UserId) AS 'UsersCount' FROM Employees e
LEFT JOIN Reports r ON r.EmployeeId=e.Id
GROUP BY e.FirstName,LastName
ORDER BY UsersCount DESC, FullName 

SELECT 
ISNULL((e.FirstName+' '+LastName),'None') AS 'Employee',
ISNULL(d.Name,'None') AS 'Department',
c.Name AS 'Category',
r.Description,FORMAT(r.OpenDate,'dd.MM.yyyy') AS 'OpenDate',
s.Label AS 'Status',
u.Name as 'User' 
FROM Reports r 
LEFT JOIN Employees e ON r.EmployeeId=e.Id
LEFT JOIN Users u ON u.Id=r.UserId
LEFT JOIN Departments d ON d.Id=e.DepartmentId
LEFT JOIN Categories c ON c.Id=r.CategoryId
LEFT JOIN [Status] s ON s.Id=r.StatusId
ORDER BY FirstName DESC,LastName DESC,Department,Category,Description,OpenDate,Status,User

CREATE OR ALTER FUNCTION udf_HoursToComplete(@StartDate DATETIME, @EndDate DATETIME)
RETURNS INT
AS
BEGIN
IF(@StartDate IS NULL OR @EndDate IS NULL)
BEGIN
RETURN 0
END
RETURN DATEDIFF(HOUR,@StartDate,@EndDate)
END

CREATE PROC usp_AssignEmployeeToReport(@EmployeeId INT, @ReportId INT)
AS
IF((SELECT DepartmentId FROM Employees WHERE Id=@EmployeeId)=
(SELECT c.DepartmentId FROM Reports r
JOIN Categories c ON r.CategoryId=c.Id
WHERE r.Id=@ReportId))
BEGIN 
UPDATE Reports
SET EmployeeId= @EmployeeId WHERE Id=@ReportId
END
ELSE 
THROW 50001,'Employee doesn''t belong to the appropriate department!',1;
GO


