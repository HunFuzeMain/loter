-- MySQL dump 10.13  Distrib 8.0.42, for Win64 (x86_64)
--
-- Host: ballast.proxy.rlwy.net    Database: railway
-- ------------------------------------------------------
-- Server version	9.3.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Appointments`
--

DROP TABLE IF EXISTS `Appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Appointments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ClientName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Phone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `InstructorId` int NOT NULL,
  `PackageId` int NOT NULL,
  `StartTime` datetime(6) NOT NULL,
  `EndTime` datetime(6) NOT NULL,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `BookingDate` datetime(6) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Appointments_InstructorId` (`InstructorId`),
  KEY `IX_Appointments_PackageId` (`PackageId`),
  CONSTRAINT `FK_Appointments_Instructors_InstructorId` FOREIGN KEY (`InstructorId`) REFERENCES `Instructors` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Appointments_Packages_PackageId` FOREIGN KEY (`PackageId`) REFERENCES `Packages` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Appointments`
--

LOCK TABLES `Appointments` WRITE;
/*!40000 ALTER TABLE `Appointments` DISABLE KEYS */;
INSERT INTO `Appointments` VALUES (1,'Molnár Ottó','molnarazotto88@gmail.com','+36 723569658',1,2,'2025-05-31 13:00:00.000000','2025-05-31 14:30:00.000000','','2025-05-04 14:13:37.642470',1),(2,'Budai Győző','envagyokagyozo@gmail.com','+36 715559789',1,6,'2025-06-05 10:30:00.000000','2025-06-05 12:00:00.000000','A helyszinen szeretnék más csomagokat is kiprobálni.','2025-05-04 14:14:47.985915',1),(3,'Orsós Gergő','orsosgeri75@gmail.com','+36 700403265',2,7,'2025-06-07 10:30:00.000000','2025-06-07 12:00:00.000000','','2025-05-04 14:15:25.940145',1),(4,'Antal Csongor','csongiantal@gmail.com','+36 728870684',2,3,'2025-06-19 13:00:00.000000','2025-06-19 14:30:00.000000','','2025-05-04 14:16:05.351582',1),(5,'Sárközi Nikoletta','sarkozinikoletta@gmail.com','+36 711130332',2,3,'2025-05-30 13:00:00.000000','2025-05-30 14:30:00.000000','','2025-05-04 14:16:52.032671',1),(6,'Somogyi Zétény','somogyi74@gmail.com','+36 769924737',2,4,'2025-05-30 13:00:00.000000','2025-05-30 14:30:00.000000','','2025-05-04 14:17:31.559838',1),(7,'Nagy Géza','geza@gmail.com','06301666997',1,4,'2025-05-31 13:00:00.000000','2025-05-31 14:30:00.000000','','2025-05-05 06:55:16.615449',1),(8,'Jozsi Kiss','kfds.lfdsa@gmail.com','36302469247',1,2,'2025-06-07 10:30:00.000000','2025-06-07 12:00:00.000000','','2025-05-05 07:28:51.590610',1);
/*!40000 ALTER TABLE `Appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `InstructorStatuses`
--

DROP TABLE IF EXISTS `InstructorStatuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `InstructorStatuses` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `InstructorStatuses`
--

LOCK TABLES `InstructorStatuses` WRITE;
/*!40000 ALTER TABLE `InstructorStatuses` DISABLE KEYS */;
INSERT INTO `InstructorStatuses` VALUES (1,'Pending'),(2,'Approved'),(3,'Hired'),(4,'Rejected'),(5,'OnLeave'),(6,'Terminated');
/*!40000 ALTER TABLE `InstructorStatuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Instructors`
--

DROP TABLE IF EXISTS `Instructors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Instructors` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Phone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Password` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `QualificationFileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdCardFileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CVFileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Status` int NOT NULL,
  `ApplicationDate` datetime(6) NOT NULL,
  `HireDate` datetime(6) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Instructors`
--

LOCK TABLES `Instructors` WRITE;
/*!40000 ALTER TABLE `Instructors` DISABLE KEYS */;
INSERT INTO `Instructors` VALUES (1,'Nagy Gergely','gergely@loter.hu','+36301234567','Budapest, Fő utca 1','test123','/uploads/qualifications/default.pdf','/uploads/idcards/default.jpg','/uploads/cvs/default.pdf',3,'2025-02-04 10:39:20.000000','2025-03-04 10:39:20.000000',1),(2,'Kovács Ákos','akos@loter.hu','+36209876543','Debrecen, Piac utca 5','test321','/uploads/qualifications/default.pdf','/uploads/idcards/default.jpg','/uploads/cvs/default.docx',3,'2025-04-04 10:39:20.000000','2025-04-19 10:39:20.000000',1);
/*!40000 ALTER TABLE `Instructors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Packages`
--

DROP TABLE IF EXISTS `Packages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Packages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Price` int NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Packages`
--

LOCK TABLES `Packages` WRITE;
/*!40000 ALTER TABLE `Packages` DISABLE KEYS */;
INSERT INTO `Packages` VALUES (1,'Különleges Alakulat',45000),(2,'Orosz',25500),(3,'Magyar',30000),(4,'9mm-es pisztoly',20000),(5,'9mm-es géppisztoly',40000),(6,'5.56 NATO és .300 blackout',75000),(7,'7.62-es',45000);
/*!40000 ALTER TABLE `Packages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Questions`
--

DROP TABLE IF EXISTS `Questions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Questions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Text` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Questions`
--

LOCK TABLES `Questions` WRITE;
/*!40000 ALTER TABLE `Questions` DISABLE KEYS */;
INSERT INTO `Questions` VALUES (1,'Van-e büfé a helyszínen?','envagyokamiki88@gmail.com','2025-05-05 08:37:32.444889'),(2,'Vihetem-e a saját fegyverem?','vargaimre75@gmail.com','2025-05-05 08:39:53.859133');
/*!40000 ALTER TABLE `Questions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
INSERT INTO `__EFMigrationsHistory` VALUES ('20250504103920_ini','6.0.33');
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-05-05 13:31:26
