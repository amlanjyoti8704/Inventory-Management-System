-- MySQL dump 10.13  Distrib 8.0.42, for macos15 (x86_64)
--
-- Host: gondola.proxy.rlwy.net    Database: railway
-- ------------------------------------------------------
-- Server version	9.3.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `role` varchar(20) DEFAULT 'user',
  `ResetToken` varchar(255) DEFAULT NULL,
  `ResetTokenExpiry` datetime DEFAULT NULL,
  `PhoneNumber` varchar(15) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Username` (`Username`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Users`
--

LOCK TABLES `Users` WRITE;
/*!40000 ALTER TABLE `Users` DISABLE KEYS */;
INSERT INTO `Users` VALUES (1,'testex','test@example5.com','test@12345','2025-05-28 19:21:45','user',NULL,NULL,NULL),(2,'amlan3345','amlanjyoti3345@gmail.com','$2a$11$G1cfAH0jtCsADvQUE9YU7uhvoJXF1TJgedeb2nJdVxqvptbzA8.Ey','2025-05-30 02:44:34','admin','C53968684DB4F2F354FD5FDAB59C93BD6D8C64E1EC3C2A0CBB3F50C184C92601','2025-06-07 06:12:13',NULL),(3,'arnaav@2003','arnaavjyoti1234@gmail.com','$2a$11$2tY1XjQjgJIEVmL.C1337eLhj03rseq0ZJA2jfZyIdQR6IWNnc9U6','2025-05-30 21:29:07','user','7959C6753045F15FE5CA74558BA5AE1FC226AA04083C5DC5CC7598C9DADB1DFB','2025-06-07 06:11:47',NULL),(4,'test','test@ex.com','$2a$11$VNgC1ouLp2Ewn/KyqoiU7.OMHl8mmevARDzAJfszMkp1z1yGJEVYa','2025-06-03 23:53:35','user','7CB77CDC9B013D6440C7A49C57C64E89F93E4D1018D52050CD36ABD09562A32C','2025-06-05 21:55:45','7295883411'),(5,'Ramakanta Behera','ramakanta_behera@rediffmail.com','$2a$11$sjoJ5P4MlN0OCnsDYgbv2utEmugr6CrkHT4kau51fTVGdQSGkw1/m','2025-06-09 19:22:51','user',NULL,NULL,NULL);
/*!40000 ALTER TABLE `Users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `alert_log`
--

DROP TABLE IF EXISTS `alert_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `alert_log` (
  `log_id` int NOT NULL AUTO_INCREMENT,
  `item_id` int DEFAULT NULL,
  `current_quantity` int DEFAULT NULL,
  `alert_message` varchar(255) DEFAULT NULL,
  `alert_time` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `category_id` int DEFAULT NULL,
  `category_name` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`log_id`)
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `alert_log`
--

LOCK TABLES `alert_log` WRITE;
/*!40000 ALTER TABLE `alert_log` DISABLE KEYS */;
INSERT INTO `alert_log` VALUES (1,1022,4,'Stock below threshold for item 1022','2025-06-02 11:19:31',NULL,NULL),(2,1042,4,'Stock below threshold for item 1042','2025-06-02 15:16:43',114,'External HDD'),(3,1024,9,'Stock below threshold for item 1024','2025-06-03 10:20:31',112,'Laptop'),(4,1023,7,'Stock below threshold for item 1023','2025-06-03 10:37:26',115,'Desk'),(5,1099,2,'Stock below threshold for item 1099','2025-06-03 14:33:59',NULL,NULL),(6,1042,7,'Stock below threshold for item 1042','2025-06-03 17:31:26',114,'External HDD'),(7,1024,7,'Stock below threshold for item 1024','2025-06-04 18:45:34',112,'Laptop'),(8,1024,8,'Stock below threshold for item 1024','2025-06-06 15:55:44',112,'Laptop'),(9,1042,9,'Stock below threshold for item 1042','2025-06-07 05:41:12',114,'External HDD'),(10,1024,9,'Stock below threshold for item 1024','2025-06-07 07:29:35',112,'Laptop'),(11,1101,9,'Stock below threshold for item 1101','2025-06-07 09:48:29',118,'Test'),(12,1101,8,'Stock below threshold for item 1101','2025-06-10 13:42:02',118,'Test'),(13,1101,7,'Stock below threshold for item 1101','2025-06-10 16:01:30',118,'Test'),(14,1102,5,'Stock below threshold for item 1102','2025-06-11 05:21:12',NULL,NULL),(15,1102,8,'Stock below threshold for item 1102','2025-06-11 05:21:37',NULL,NULL),(16,1103,1,'Stock below threshold for item 1103','2025-06-11 05:22:55',102,'Mouse'),(17,1105,4,'Stock below threshold for item 1105','2025-06-11 06:15:15',104,'Keyboard'),(18,1102,9,'Stock below threshold for item 1102','2025-06-11 06:39:02',NULL,NULL),(19,NULL,0,'No items found for existing category ID: 108, Name: Scanner','2025-06-11 06:59:26',108,'Scanner'),(20,NULL,0,'No items found for existing category ID: 109, Name: Webcam','2025-06-11 06:59:26',109,'Webcam'),(21,NULL,0,'No items found for existing category ID: 111, Name: Projector','2025-06-11 06:59:26',111,'Projector'),(22,NULL,0,'No items found for existing category ID: 113, Name: Pendrive','2025-06-11 06:59:26',113,'Pendrive'),(23,NULL,0,'No items found for existing category ID: 117, Name: CPU','2025-06-11 06:59:26',117,'CPU'),(26,NULL,0,'No items found for existing category ID: 119, Name: test2','2025-06-11 08:41:13',119,'test2'),(27,1106,8,'Stock below threshold for item 1106','2025-06-11 08:44:34',NULL,NULL),(28,1106,7,'Stock below threshold for item 1106','2025-06-11 08:52:17',119,'test2'),(29,1106,9,'Stock below threshold for item 1106','2025-06-11 08:52:57',119,'test2'),(30,1023,6,'Stock below threshold for item 1023','2025-06-11 09:41:33',115,'Desk'),(31,1023,5,'Stock below threshold for item 1023','2025-06-11 09:41:34',115,'Desk'),(32,1023,4,'Stock below threshold for item 1023','2025-06-11 09:42:41',115,'Desk'),(33,1023,8,'Stock below threshold for item 1023','2025-06-11 09:47:51',115,'Desk'),(34,1023,4,'Stock below threshold for item 1023','2025-06-11 09:48:24',115,'Desk'),(35,1042,9,'Stock below threshold for item 1042','2025-06-11 09:55:47',114,'External HDD'),(36,1042,5,'Stock below threshold for item 1042','2025-06-11 09:57:06',114,'External HDD'),(37,NULL,0,'No items found for existing category ID: 120, Name: test3','2025-06-11 11:31:46',120,'test3');
/*!40000 ALTER TABLE `alert_log` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `category_id` int NOT NULL AUTO_INCREMENT,
  `category_name` varchar(100) DEFAULT NULL,
  `threshold` int DEFAULT NULL,
  PRIMARY KEY (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=121 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES (102,'Mouse',10),(103,'Monitor',10),(104,'Keyboard',10),(107,'Printer',5),(108,'Scanner',5),(109,'Webcam',10),(110,'Router',5),(111,'Projector',2),(112,'Laptop',10),(113,'Pendrive',20),(114,'External HDD',11),(115,'Desk',10),(116,'Cables',10),(117,'CPU',5),(118,'Test',10),(119,'test2',10),(120,'test3',0);
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`%`*/ /*!50003 TRIGGER `after_category_insert` AFTER INSERT ON `category` FOR EACH ROW BEGIN
  DECLARE itemCount INT;

  SELECT COUNT(*) INTO itemCount 
  FROM consumableItems 
  WHERE category_id = NEW.category_id;

  IF itemCount = 0 THEN
    INSERT INTO alert_log (item_id, current_quantity, alert_message, category_id, category_name)
    VALUES (
      NULL,
      0,
      CONCAT('No items found for existing category ID: ', NEW.category_id, ', Name: ', NEW.category_name),
      NEW.category_id,
      NEW.category_name
    );
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `consumableItems`
--

DROP TABLE IF EXISTS `consumableItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `consumableItems` (
  `item_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  `category_id` int DEFAULT NULL,
  `model_no` varchar(50) DEFAULT NULL,
  `brand` varchar(50) DEFAULT NULL,
  `quantity` int DEFAULT '0',
  `storage_loc_l1` varchar(150) DEFAULT NULL,
  `storage_loc_l2` varchar(150) DEFAULT NULL,
  `warranty_expiration` date DEFAULT NULL,
  PRIMARY KEY (`item_id`),
  KEY `category_id` (`category_id`),
  CONSTRAINT `consumableitems_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1107 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `consumableItems`
--

LOCK TABLES `consumableItems` WRITE;
/*!40000 ALTER TABLE `consumableItems` DISABLE KEYS */;
INSERT INTO `consumableItems` VALUES (1023,'wooden table',115,'UC-238','Godrej',4,'VN','Warehouse',NULL),(1024,'Gaming Laptop',112,'DELL-1UH23HHI','DELL',10,'Shaktinagar','warehouse',NULL),(1042,'Hard Disk',114,'SS98SDO','Seagate',5,'vindhyanagar','warehouse',NULL),(1095,'Monitor',103,'Hp-100','HP',19,'Vindhyanagar','warehouse',NULL),(1096,'JIO-fiber',110,'JIO-22938','JIO',24,'N/A','N/A',NULL),(1098,'USB Cable',116,'UC-100','HP',50,'Rack 2','Box B','0001-01-01'),(1101,'TestItem',118,'Test-100','test',10,'n/a','n/a','0001-01-01'),(1103,'Dell mouse',102,'DELL_100','DELL',1,'xyz','xyz','0001-01-01'),(1104,'Printer Ink',107,'HP-903XL','HP',10,'Cabinet A','Shelf 2','0001-01-01'),(1105,'HP keyboard',104,'HP_237','HP',4,'na','na','0001-01-01'),(1106,'example',119,'TH-i32','BAHS',9,'n/a','n/a','0001-01-01');
/*!40000 ALTER TABLE `consumableItems` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`%`*/ /*!50003 TRIGGER `check_threshold_after_update` AFTER UPDATE ON `consumableItems` FOR EACH ROW BEGIN
  DECLARE threshold_value INT;
  DECLARE cat_name VARCHAR(100);

  SELECT threshold, category_name INTO threshold_value, cat_name
  FROM category
  WHERE category_id = NEW.category_id;

  IF NEW.quantity < threshold_value THEN
    INSERT INTO alert_log (
      item_id, 
      category_id, 
      category_name, 
      current_quantity, 
      alert_message
    )
    VALUES (
      NEW.item_id,
      NEW.category_id,
      cat_name,
      NEW.quantity,
      CONCAT('Stock below threshold for item ', NEW.item_id)
    );
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `issue_records`
--

DROP TABLE IF EXISTS `issue_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `issue_records` (
  `issue_id` int NOT NULL AUTO_INCREMENT,
  `issued_to` varchar(50) DEFAULT NULL,
  `department` varchar(50) DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  `status` varchar(20) DEFAULT 'pending',
  `requested_by` varchar(50) DEFAULT NULL,
  `item_id` int DEFAULT NULL,
  `issue_date` datetime DEFAULT NULL,
  `return_status` enum('none','requested','approved','rejected') DEFAULT 'none',
  PRIMARY KEY (`issue_id`)
) ENGINE=InnoDB AUTO_INCREMENT=341 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `issue_records`
--

LOCK TABLES `issue_records` WRITE;
/*!40000 ALTER TABLE `issue_records` DISABLE KEYS */;
INSERT INTO `issue_records` VALUES (325,'Mayank','CSE',2,'declined','arnaavjyoti1234@gmail.com',1096,'2025-06-03 18:49:40','none'),(329,'Mohan','CSE',3,'returned','arnaavjyoti1234@gmail.com',1042,'2025-06-03 23:01:26','approved'),(330,'Rehan','AIML',2,'approved','test@ex.com',1095,'2025-06-04 13:27:44','none'),(331,'Naruto','Production',4,'approved','arnaavjyoti1234@gmail.com',1024,'2025-06-05 00:15:34','approved'),(332,'Vikash','IT',5,'declined','arnaavjyoti1234@gmail.com',1095,'2025-06-07 10:31:41','none'),(333,'Tarush','IT',10,'pending','arnaavjyoti1234@gmail.com',1098,'2025-06-07 10:34:28','none'),(334,'Amlan','CSE',1,'approved','arnaavjyoti1234@gmail.com',1042,'2025-06-07 11:11:12','requested'),(335,'Ramakanta Behera','HR',1,'approved','ramakanta_behera@rediffmail.com',1095,'2025-06-09 13:54:47','none'),(336,'Shekhar','XYZ',2,'approved','arnaavjyoti1234@gmail.com',1101,'2025-06-10 13:41:05','none'),(337,'Farhan','IT',3,'approved','arnaavjyoti1234@gmail.com',1101,'2025-06-10 16:00:56','none'),(338,'Rama','HR',1,'pending','ramakanta_behera@rediffmail.com',1042,'2025-06-10 16:16:17','none'),(339,'ME','IT',3,'approved','arnaavjyoti1234@gmail.com',1106,'2025-06-11 08:43:59','none'),(340,'Raju','IT',3,'approved','arnaavjyoti1234@gmail.com',1106,'2025-06-11 08:51:46','none');
/*!40000 ALTER TABLE `issue_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `issued`
--

DROP TABLE IF EXISTS `issued`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `issued` (
  `issue_id` int DEFAULT NULL,
  `item_id` int DEFAULT NULL,
  `issue_date` date DEFAULT NULL,
  KEY `item_id` (`item_id`),
  KEY `issued_ibfk_2` (`issue_id`),
  CONSTRAINT `issued_ibfk_1` FOREIGN KEY (`item_id`) REFERENCES `consumableItems` (`item_id`),
  CONSTRAINT `issued_ibfk_2` FOREIGN KEY (`issue_id`) REFERENCES `issue_records` (`issue_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `issued`
--

LOCK TABLES `issued` WRITE;
/*!40000 ALTER TABLE `issued` DISABLE KEYS */;
/*!40000 ALTER TABLE `issued` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `after_issue_insert` AFTER INSERT ON `issued` FOR EACH ROW BEGIN
  DECLARE qty INT;
  SELECT quantity INTO qty FROM issue_records WHERE issue_id = NEW.issue_id;
  UPDATE consumableItems
  SET quantity = quantity - qty
  WHERE item_id = NEW.item_id;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `purchase_details`
--

DROP TABLE IF EXISTS `purchase_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_details` (
  `item_id` int NOT NULL,
  `order_id` int NOT NULL AUTO_INCREMENT,
  `quantity` int DEFAULT NULL,
  `price` decimal(9,2) DEFAULT NULL,
  `purchase_date` date DEFAULT NULL,
  PRIMARY KEY (`order_id`),
  KEY `item_id` (`item_id`),
  CONSTRAINT `purchase_details_ibfk_1` FOREIGN KEY (`item_id`) REFERENCES `consumableItems` (`item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2097 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `purchase_details`
--

LOCK TABLES `purchase_details` WRITE;
/*!40000 ALTER TABLE `purchase_details` DISABLE KEYS */;
INSERT INTO `purchase_details` VALUES (1024,2023,5,80999.00,'2025-05-30'),(1042,2041,8,4999.00,'2025-06-02'),(1095,2066,11,59999.00,'2025-06-02'),(1096,2067,12,2000.00,'2025-06-02'),(1024,2070,6,79999.00,'2025-06-03'),(1023,2072,4,39999.99,'2025-06-03'),(1098,2075,25,120.50,'2025-05-29'),(1024,2081,1,123099.00,'2025-06-07'),(1101,2082,9,100.00,'2025-06-07'),(1101,2083,1,200.00,'2025-06-07'),(1101,2084,2,100.00,'2025-06-10'),(1103,2088,1,2000.00,'2025-06-11'),(1101,2089,3,312.00,'2025-06-11'),(1104,2090,10,399.99,'2025-05-28'),(1105,2091,4,3999.00,'2025-06-11'),(1106,2093,11,242.00,'2025-06-11'),(1106,2094,2,241.00,'2025-06-11'),(1106,2095,2,231.00,'2025-06-11'),(1023,2096,4,39999.00,'2025-06-11');
/*!40000 ALTER TABLE `purchase_details` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `after_purchase_insert` AFTER INSERT ON `purchase_details` FOR EACH ROW BEGIN
  UPDATE consumableItems
  SET quantity = quantity + NEW.quantity
  WHERE item_id = NEW.item_id;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Temporary view structure for view `xyz`
--

DROP TABLE IF EXISTS `xyz`;
/*!50001 DROP VIEW IF EXISTS `xyz`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `xyz` AS SELECT 
 1 AS `total_quantity`,
 1 AS `category_name`,
 1 AS `threshold`*/;
SET character_set_client = @saved_cs_client;

--
-- Final view structure for view `xyz`
--

/*!50001 DROP VIEW IF EXISTS `xyz`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`%` SQL SECURITY DEFINER */
/*!50001 VIEW `xyz` AS select coalesce(sum(`ci`.`quantity`),0) AS `total_quantity`,`c`.`category_name` AS `category_name`,`c`.`threshold` AS `threshold` from (`category` `c` left join `consumableItems` `ci` on((`ci`.`category_id` = `c`.`category_id`))) group by `ci`.`category_id`,`c`.`category_name`,`c`.`threshold` having (`total_quantity` < `c`.`threshold`) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-06-11 17:06:44
