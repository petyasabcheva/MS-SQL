CREATE DATABASE Airport

USE Airport

CREATE TABLE Planes(
Id INT PRIMARY KEY IDENTITY,
[Name] NVARCHAR(30) NOT NULL,
Seats INT NOT NULL,
[Range] INT NOT NULL
)

CREATE TABLE Flights(
Id INT PRIMARY KEY IDENTITY,
DepartureTime DATETIME,
ArrivalTime DATETIME,
Origin NVARCHAR(50) NOT NULL,
Destination NVARCHAR(50) NOT NULL,
PlaneId INT NOT NULL FOREIGN KEY 
REFERENCES Planes(Id)
)

CREATE TABLE Passengers(
Id INT PRIMARY KEY IDENTITY,
FirstName NVARCHAR(30) NOT NULL,
LastName NVARCHAR(30) NOT NULL,
Age INT NOT NULL,
[Address] NVARCHAR(30) NOT NULL,
PassportId NVARCHAR(11) NOT NULL
)

CREATE TABLE LuggageTypes(
Id INT PRIMARY KEY IDENTITY,
[Type] NVARCHAR(30) NOT NULL,
)

CREATE TABLE Luggages(
Id INT PRIMARY KEY IDENTITY,
LuggageTypeId INT NOT NULL FOREIGN KEY 
REFERENCES LuggageTypes(Id),
PassengerId INT NOT NULL FOREIGN KEY 
REFERENCES Passengers(Id)
)

CREATE TABLE Tickets(
Id INT PRIMARY KEY IDENTITY,
PassengerId INT NOT NULL FOREIGN KEY 
REFERENCES Passengers(Id),
FlightId INT NOT NULL FOREIGN KEY 
REFERENCES Flights(Id),
LuggageId INT NOT NULL FOREIGN KEY 
REFERENCES Luggages(Id),
Price DECIMAL(10,2) NOT NULL
)

INSERT INTO Planes(Name,Seats,Range)
VALUES 
('Airbus 336',112,5132),
('Airbus 330',432,5325),
('Boeing 369',231,2355),
('Stelt 297',254,2143),
('Boeing 338',165,5111),
('Airbus 558',387,1342),
('Boeing 128',345,5541)

INSERT INTO LuggageTypes([Type])
VALUES 
('Crossbody Bag'),
('School Backpack'),
('Shoulder Bag')


UPDATE Tickets  
SET Price*= 1.13
WHERE FlightID=(SELECT TOP(1) Id FROM Flights WHERE Destination='Carlsbad');

DELETE FROM Tickets WHERE FlightId=(SELECT TOP(1) Id FROM Flights WHERE Destination='Ayn Halagim');
DELETE FROM Flights WHERE Destination='Ayn Halagim';

SELECT Origin,Destination FROM Flights
ORDER BY Origin,Destination;

SELECT Id,Name,Seats,Range FROM Planes 
WHERE Name LIKE '%tr%'
ORDER BY Id,Name,Seats,Range

SELECT FlightId,SUM(Price) AS Price FROM Tickets
GROUP BY FlightId
ORDER BY Price DESC,FlightId

SELECT TOP(10) FirstName,LastName,t.Price FROM Passengers p
JOIN Tickets t ON p.Id=t.PassengerId
ORDER BY Price DESC,FirstName,LastName


SELECT Type,COUNT(*) AS 'MostUsedLuggage' FROM Luggages l
JOIN LuggageTypes lt ON l.LuggageTypeId=lt.Id
GROUP BY Type
ORDER BY MostUsedLuggage DESC,Type

SELECT FirstName+' '+LastName AS 'FullName',Origin,Destination FROM Passengers p
JOIN Tickets t ON p.Id=t.PassengerId
JOIN Flights f ON f.Id=t.FlightId
ORDER BY FullName,Origin,Destination

SELECT FirstName,LastName,Age FROM Passengers p
LEFT JOIN Tickets t ON p.Id=t.PassengerId
WHERE t.Id IS NULL
ORDER BY Age DESC,FirstName,LastName

SELECT p.PassportId,p.Address FROM Passengers p
LEFT JOIN Luggages l ON l.PassengerId=p.Id
WHERE l.Id IS NULL
ORDER BY PassportId ASC,Address

SELECT FirstName,LastName,COUNT(DISTINCT t.Id) AS 'Total Trips' FROM Tickets t 
RIGHT JOIN Passengers p ON t.PassengerId=p.Id
GROUP BY FirstName,LastName
ORDER BY [Total Trips] DESC,FirstName,LastName

SELECT
p.FirstName+' '+LastName  AS 'Full Name',
pl.Name as 'Plane Name',
f.Origin+' - '+Destination as 'Trip',
lt.Type as 'Luggage Type'
FROM Passengers p
JOIN Tickets t ON p.Id=t.PassengerId
JOIN Flights f ON f.Id=t.FlightId
JOIN Planes pl ON pl.Id=f.PlaneId
JOIN Luggages l ON l.Id=t.LuggageId
JOIN LuggageTypes lt ON l.LuggageTypeId=lt.Id
ORDER BY [Full Name],[Plane Name],Origin,Destination,[Luggage Type]

SELECT FirstName,LastName,Destination,MostExpensiveTrips.Price FROM(
SELECT FirstName,LastName,MAX(Price) AS Price FROM Passengers p 
JOIN Tickets t ON p.Id=t.PassengerId
GROUP BY FirstName,LastName) AS [MostExpensiveTrips] 
JOIN Tickets t ON MostExpensiveTrips.Price=t.Price
JOIN Flights f ON t.FlightId=f.Id
ORDER BY MostExpensiveTrips.Price DESC,FirstName,LastName

SELECT Destination,COUNT(DISTINCT t.Id) AS 'FilesCount' FROM Flights f
LEFT JOIN Tickets t ON t.FlightId=f.Id
GROUP BY Destination
ORDER BY FilesCount DESC,Destination ASC

SELECT p.Name,p.Seats,COUNT(DISTINCT t.Id) AS 'Passengers Count' FROM Planes p
LEFT JOIN Flights f ON f.PlaneId=p.Id
LEFT JOIN Tickets t ON t.FlightId=f.Id
GROUP BY p.Name,p.Seats
ORDER BY [Passengers Count] DESC,p.Name,p.Seats


CREATE OR ALTER FUNCTION udf_CalculateTickets(@origin NVARCHAR(50), @destination NVARCHAR(50), @peopleCount INT)
RETURNS NVARCHAR(100)
AS 
BEGIN
IF(@peopleCount<=0)
BEGIN
RETURN 'Invalid people count!'
END
DECLARE @flightId int=(SELECT TOP(1) Id FROM Flights WHERE Origin=@origin AND Destination=@destination)
IF(@flightId IS NULL)
BEGIN
RETURN 'Invalid flight!'
END
DECLARE @totalPrice DECIMAL(10,2)
SET @totalPrice=@peopleCount* (SELECT TOP(1) Price FROM Tickets WHERE FlightId=@flightId)
RETURN 'Total price '+CAST(@totalPrice AS varchar(30))
END

CREATE PROC usp_CancelFlights
AS
UPDATE Flights
SET DepartureTime=NULL,ArrivalTime=NULL
WHERE ArrivalTime>DepartureTime
GO