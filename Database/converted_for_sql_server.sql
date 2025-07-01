-- MySQL dump 10.13  Distrib 8.0.42, for macos15 (x86_64)
--
-- Host: gondola.proxy.rlwy.net    Database: railway
-- ------------------------------------------------------
-- Server version	9.3.0












--
-- Table structure for table [Users[
--

DROP TABLE IF EXISTS [Users];


CREATE TABLE [Users] (
  [Id] int NOT NULL IDENTITY(1,1),
  [Username] varchar(50) NOT NULL,
  [Email] varchar(100) NOT NULL,
  [Password] varchar(255) NOT NULL,
  [CreatedAt] datetime DEFAULT GETDATE(),
  [role] varchar(20) DEFAULT 'user',
  [ResetToken] varchar(255) DEFAULT NULL,
  [ResetTokenExpiry] datetime DEFAULT NULL,
  [PhoneNumber] varchar(15) DEFAULT NULL,
  PRIMARY KEY ([Id]),
  UNIQUE KEY [Username] ([Username]),
  UNIQUE KEY [Email] ([Email])
) IDENTITY(1,1)=6  ;


--
-- Dumping data for table [Users[
--



INSERT INTO [Users] VALUES (1,'testex','test@example5.com','test@12345','2025-05-28 19:21:45','user',NULL,NULL,NULL),(2,'amlan3345','amlanjyoti3345@gmail.com','$2a$11$G1cfAH0jtCsADvQUE9YU7uhvoJXF1TJgedeb2nJdVxqvptbzA8.Ey','2025-05-30 02:44:34','admin','C53968684DB4F2F354FD5FDAB59C93BD6D8C64E1EC3C2A0CBB3F50C184C92601','2025-06-07 06:12:13',NULL),(3,'arnaav@2003','arnaavjyoti1234@gmail.com','$2a$11$2tY1XjQjgJIEVmL.C1337eLhj03rseq0ZJA2jfZyIdQR6IWNnc9U6','2025-05-30 21:29:07','user','7959C6753045F15FE5CA74558BA5AE1FC226AA04083C5DC5CC7598C9DADB1DFB','2025-06-07 06:11:47',NULL),(4,'test','test@ex.com','$2a$11$VNgC1ouLp2Ewn/KyqoiU7.OMHl8mmevARDzAJfszMkp1z1yGJEVYa','2025-06-03 23:53:35','user','7CB77CDC9B013D6440C7A49C57C64E89F93E4D1018D52050CD36ABD09562A32C','2025-06-05 21:55:45','7295883411'),(5,'Ramakanta Behera','ramakanta_behera@rediffmail.com','$2a$11$sjoJ5P4MlN0OCnsDYgbv2utEmugr6CrkHT4kau51fTVGdQSGkw1/m','2025-06-09 19:22:51','user',NULL,NULL,NULL);

UN

--
-- Table structure for table [alert_log[
--

DROP TABLE IF EXISTS [alert_log];


CREATE TABLE [alert_log] (
  [log_id] int NOT NULL IDENTITY(1,1),
  [item_id] int DEFAULT NULL,
  [current_quantity] int DEFAULT NULL,
  [alert_message] varchar(255) DEFAULT NULL,
  [alert_time] timestamp NULL DEFAULT GETDATE(),
  [category_id] int DEFAULT NULL,
  [category_name] varchar(100) DEFAULT NULL,
  PRIMARY KEY ([log_id])
) IDENTITY(1,1)=38  ;


--
-- Dumping data for table [alert_log[
--



INSERT INTO [alert_log] VALUES (1,1022,4,'Stock below threshold for item 1022','2025-06-02 11:19:31',NULL,NULL),(2,1042,4,'Stock below threshold for item 1042','2025-06-02 15:16:43',114,'External HDD'),(3,1024,9,'Stock below threshold for item 1024','2025-06-03 10:20:31',112,'Laptop'),(4,1023,7,'Stock below threshold for item 1023','2025-06-03 10:37:26',115,'Desk'),(5,1099,2,'Stock below threshold for item 1099','2025-06-03 14:33:59',NULL,NULL),(6,1042,7,'Stock below threshold for item 1042','2025-06-03 17:31:26',114,'External HDD'),(7,1024,7,'Stock below threshold for item 1024','2025-06-04 18:45:34',112,'Laptop'),(8,1024,8,'Stock below threshold for item 1024','2025-06-06 15:55:44',112,'Laptop'),(9,1042,9,'Stock below threshold for item 1042','2025-06-07 05:41:12',114,'External HDD'),(10,1024,9,'Stock below threshold for item 1024','2025-06-07 07:29:35',112,'Laptop'),(11,1101,9,'Stock below threshold for item 1101','2025-06-07 09:48:29',118,'Test'),(12,1101,8,'Stock below threshold for item 1101','2025-06-10 13:42:02',118,'Test'),(13,1101,7,'Stock below threshold for item 1101','2025-06-10 16:01:30',118,'Test'),(14,1102,5,'Stock below threshold for item 1102','2025-06-11 05:21:12',NULL,NULL),(15,1102,8,'Stock below threshold for item 1102','2025-06-11 05:21:37',NULL,NULL),(16,1103,1,'Stock below threshold for item 1103','2025-06-11 05:22:55',102,'Mouse'),(17,1105,4,'Stock below threshold for item 1105','2025-06-11 06:15:15',104,'Keyboard'),(18,1102,9,'Stock below threshold for item 1102','2025-06-11 06:39:02',NULL,NULL),(19,NULL,0,'No items found for existing category ID: 108, Name: Scanner','2025-06-11 06:59:26',108,'Scanner'),(20,NULL,0,'No items found for existing category ID: 109, Name: Webcam','2025-06-11 06:59:26',109,'Webcam'),(21,NULL,0,'No items found for existing category ID: 111, Name: Projector','2025-06-11 06:59:26',111,'Projector'),(22,NULL,0,'No items found for existing category ID: 113, Name: Pendrive','2025-06-11 06:59:26',113,'Pendrive'),(23,NULL,0,'No items found for existing category ID: 117, Name: CPU','2025-06-11 06:59:26',117,'CPU'),(26,NULL,0,'No items found for existing category ID: 119, Name: test2','2025-06-11 08:41:13',119,'test2'),(27,1106,8,'Stock below threshold for item 1106','2025-06-11 08:44:34',NULL,NULL),(28,1106,7,'Stock below threshold for item 1106','2025-06-11 08:52:17',119,'test2'),(29,1106,9,'Stock below threshold for item 1106','2025-06-11 08:52:57',119,'test2'),(30,1023,6,'Stock below threshold for item 1023','2025-06-11 09:41:33',115,'Desk'),(31,1023,5,'Stock below threshold for item 1023','2025-06-11 09:41:34',115,'Desk'),(32,1023,4,'Stock below threshold for item 1023','2025-06-11 09:42:41',115,'Desk'),(33,1023,8,'Stock below threshold for item 1023','2025-06-11 09:47:51',115,'Desk'),(34,1023,4,'Stock below threshold for item 1023','2025-06-11 09:48:24',115,'Desk'),(35,1042,9,'Stock below threshold for item 1042','2025-06-11 09:55:47',114,'External HDD'),(36,1042,5,'Stock below threshold for item 1042','2025-06-11 09:57:06',114,'External HDD'),(37,NULL,0,'No items found for existing category ID: 120, Name: test3','2025-06-11 11:31:46',120,'test3');

UN

--
-- Table structure for table [category[
--

DROP TABLE IF EXISTS [category];


CREATE TABLE [category] (
  [category_id] int NOT NULL IDENTITY(1,1),
  [category_name] varchar(100) DEFAULT NULL,
  [threshold] int DEFAULT NULL,
  PRIMARY KEY ([category_id])
) IDENTITY(1,1)=121  ;


--
-- Dumping data for table [category[
--



INSERT INTO [category] VALUES (102,'Mouse',10),(103,'Monitor',10),(104,'Keyboard',10),(107,'Printer',5),(108,'Scanner',5),(109,'Webcam',10),(110,'Router',5),(111,'Projector',2),(112,'Laptop',10),(113,'Pendrive',20),(114,'External HDD',11),(115,'Desk',10),(116,'Cables',10),(117,'CPU',5),(118,'Test',10),(119,'test2',10),(120,'test3',0);

UN
;



CREATE TABLE [consumableItems] (
  [item_id] int NOT NULL IDENTITY(1,1),
  [name] varchar(50) DEFAULT NULL,
  [category_id] int DEFAULT NULL,
  [model_no] varchar(50) DEFAULT NULL,
  [brand] varchar(50) DEFAULT NULL,
  [quantity] int DEFAULT '0',
  [storage_loc_l1] varchar(150) DEFAULT NULL,
  [storage_loc_l2] varchar(150) DEFAULT NULL,
  [warranty_expiration] date DEFAULT NULL,
  PRIMARY KEY ([item_id]),
  KEY [category_id] ([category_id]),
  CONSTRAINT [consumableitems_ibfk_1] FOREIGN KEY ([category_id]) REFERENCES [category] ([category_id])
) IDENTITY(1,1)=1107  ;


--
-- Dumping data for table [consumableItems[
--



INSERT INTO [consumableItems] VALUES (1023,'wooden table',115,'UC-238','Godrej',4,'VN','Warehouse',NULL),(1024,'Gaming Laptop',112,'DELL-1UH23HHI','DELL',10,'Shaktinagar','warehouse',NULL),(1042,'Hard Disk',114,'SS98SDO','Seagate',5,'vindhyanagar','warehouse',NULL),(1095,'Monitor',103,'Hp-100','HP',19,'Vindhyanagar','warehouse',NULL),(1096,'JIO-fiber',110,'JIO-22938','JIO',24,'N/A','N/A',NULL),(1098,'USB Cable',116,'UC-100','HP',50,'Rack 2','Box B','0001-01-01'),(1101,'TestItem',118,'Test-100','test',10,'n/a','n/a','0001-01-01'),(1103,'Dell mouse',102,'DELL_100','DELL',1,'xyz','xyz','0001-01-01'),(1104,'Printer Ink',107,'HP-903XL','HP',10,'Cabinet A','Shelf 2','0001-01-01'),(1105,'HP keyboard',104,'HP_237','HP',4,'na','na','0001-01-01'),(1106,'example',119,'TH-i32','BAHS',9,'n/a','n/a','0001-01-01');

UN
;



CREATE TABLE [issue_records] (
  [issue_id] int NOT NULL IDENTITY(1,1),
  [issued_to] varchar(50) DEFAULT NULL,
  [department] varchar(50) DEFAULT NULL,
  [quantity] int DEFAULT NULL,
  [status] varchar(20) DEFAULT 'pending',
  [requested_by] varchar(50) DEFAULT NULL,
  [item_id] int DEFAULT NULL,
  [issue_date] datetime DEFAULT NULL,
  [return_status] enum('none','requested','approved','rejected') DEFAULT 'none',
  PRIMARY KEY ([issue_id])
) IDENTITY(1,1)=341  ;


--
-- Dumping data for table [issue_records[
--



INSERT INTO [issue_records] VALUES (325,'Mayank','CSE',2,'declined','arnaavjyoti1234@gmail.com',1096,'2025-06-03 18:49:40','none'),(329,'Mohan','CSE',3,'returned','arnaavjyoti1234@gmail.com',1042,'2025-06-03 23:01:26','approved'),(330,'Rehan','AIML',2,'approved','test@ex.com',1095,'2025-06-04 13:27:44','none'),(331,'Naruto','Production',4,'approved','arnaavjyoti1234@gmail.com',1024,'2025-06-05 00:15:34','approved'),(332,'Vikash','IT',5,'declined','arnaavjyoti1234@gmail.com',1095,'2025-06-07 10:31:41','none'),(333,'Tarush','IT',10,'pending','arnaavjyoti1234@gmail.com',1098,'2025-06-07 10:34:28','none'),(334,'Amlan','CSE',1,'approved','arnaavjyoti1234@gmail.com',1042,'2025-06-07 11:11:12','requested'),(335,'Ramakanta Behera','HR',1,'approved','ramakanta_behera@rediffmail.com',1095,'2025-06-09 13:54:47','none'),(336,'Shekhar','XYZ',2,'approved','arnaavjyoti1234@gmail.com',1101,'2025-06-10 13:41:05','none'),(337,'Farhan','IT',3,'approved','arnaavjyoti1234@gmail.com',1101,'2025-06-10 16:00:56','none'),(338,'Rama','HR',1,'pending','ramakanta_behera@rediffmail.com',1042,'2025-06-10 16:16:17','none'),(339,'ME','IT',3,'approved','arnaavjyoti1234@gmail.com',1106,'2025-06-11 08:43:59','none'),(340,'Raju','IT',3,'approved','arnaavjyoti1234@gmail.com',1106,'2025-06-11 08:51:46','none');

UN

--
-- Table structure for table [issued[
--

DROP TABLE IF EXISTS [issued];


CREATE TABLE [issued] (
  [issue_id] int DEFAULT NULL,
  [item_id] int DEFAULT NULL,
  [issue_date] date DEFAULT NULL,
  KEY [item_id] ([item_id]),
  KEY [issued_ibfk_2] ([issue_id]),
  CONSTRAINT [issued_ibfk_1] FOREIGN KEY ([item_id]) REFERENCES [consumableItems] ([item_id]),
  CONSTRAINT [issued_ibfk_2] FOREIGN KEY ([issue_id]) REFERENCES [issue_records] ([issue_id]) ON DELETE CASCADE
)  ;


--
-- Dumping data for table [issued[
--




UN
;



CREATE TABLE [purchase_details] (
  [item_id] int NOT NULL,
  [order_id] int NOT NULL IDENTITY(1,1),
  [quantity] int DEFAULT NULL,
  [price] decimal(9,2) DEFAULT NULL,
  [purchase_date] date DEFAULT NULL,
  PRIMARY KEY ([order_id]),
  KEY [item_id] ([item_id]),
  CONSTRAINT [purchase_details_ibfk_1] FOREIGN KEY ([item_id]) REFERENCES [consumableItems] ([item_id])
) IDENTITY(1,1)=2097  ;


--
-- Dumping data for table [purchase_details[
--



INSERT INTO [purchase_details] VALUES (1024,2023,5,80999.00,'2025-05-30'),(1042,2041,8,4999.00,'2025-06-02'),(1095,2066,11,59999.00,'2025-06-02'),(1096,2067,12,2000.00,'2025-06-02'),(1024,2070,6,79999.00,'2025-06-03'),(1023,2072,4,39999.99,'2025-06-03'),(1098,2075,25,120.50,'2025-05-29'),(1024,2081,1,123099.00,'2025-06-07'),(1101,2082,9,100.00,'2025-06-07'),(1101,2083,1,200.00,'2025-06-07'),(1101,2084,2,100.00,'2025-06-10'),(1103,2088,1,2000.00,'2025-06-11'),(1101,2089,3,312.00,'2025-06-11'),(1104,2090,10,399.99,'2025-05-28'),(1105,2091,4,3999.00,'2025-06-11'),(1106,2093,11,242.00,'2025-06-11'),(1106,2094,2,241.00,'2025-06-11'),(1106,2095,2,231.00,'2025-06-11'),(1023,2096,4,39999.00,'2025-06-11');

UN
;


SET @saved_cs_client     = @@character_set_client;


SET character_set_client = @saved_cs_client;

--
-- Final view structure for view [xyz[
--






















-- Dump completed on 2025-06-11 17:06:44
