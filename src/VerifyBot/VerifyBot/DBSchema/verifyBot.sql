SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `verifyBot`
--

-- --------------------------------------------------------

--
-- Table structure for table `guild`
--

CREATE TABLE `guild` (
  `id` bigint UNSIGNED NOT NULL,
  `verified_role_id` bigint UNSIGNED DEFAULT NULL,
  `creation_time` bigint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `guild_ban`
--

CREATE TABLE `guild_ban` (
  `id` int NOT NULL,
  `guild_id` bigint UNSIGNED NOT NULL,
  `username_record_id` int NOT NULL,
  `creation_time` bigint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `pending_verification`
--

CREATE TABLE `pending_verification` (
  `id` int NOT NULL,
  `user_id` bigint UNSIGNED NOT NULL,
  `username_record_id` int NOT NULL,
  `token` varchar(32) COLLATE utf8mb4_general_ci NOT NULL,
  `creation_time` bigint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `user`
--

CREATE TABLE `user` (
  `id` bigint UNSIGNED NOT NULL,
  `username_record_id` int DEFAULT NULL,
  `creation_time` bigint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `username_record`
--

CREATE TABLE `username_record` (
  `id` int NOT NULL,
  `encrypted_username` varbinary(512) NOT NULL,
  `username_salt` varbinary(64) NOT NULL,
  `username_hash` varbinary(64) NOT NULL,
  `creation_time` bigint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `guild`
--
ALTER TABLE `guild`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `guild_ban`
--
ALTER TABLE `guild_ban`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk__guild__guild_ban` (`guild_id`),
  ADD KEY `fk__username_record__guild_ban` (`username_record_id`);

--
-- Indexes for table `pending_verification`
--
ALTER TABLE `pending_verification`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk__username_record__pending_verification` (`username_record_id`),
  ADD KEY `index__user_id__token` (`user_id`,`token`);

--
-- Indexes for table `user`
--
ALTER TABLE `user`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_username_record_id` (`username_record_id`) USING BTREE;

--
-- Indexes for table `username_record`
--
ALTER TABLE `username_record`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `guild_ban`
--
ALTER TABLE `guild_ban`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `pending_verification`
--
ALTER TABLE `pending_verification`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `username_record`
--
ALTER TABLE `username_record`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `guild_ban`
--
ALTER TABLE `guild_ban`
  ADD CONSTRAINT `fk__guild__guild_ban` FOREIGN KEY (`guild_id`) REFERENCES `guild` (`id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  ADD CONSTRAINT `fk__username_record__guild_ban` FOREIGN KEY (`username_record_id`) REFERENCES `username_record` (`id`) ON DELETE CASCADE ON UPDATE RESTRICT;

--
-- Constraints for table `pending_verification`
--
ALTER TABLE `pending_verification`
  ADD CONSTRAINT `fk__user__pending_verification` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  ADD CONSTRAINT `fk__username_record__pending_verification` FOREIGN KEY (`username_record_id`) REFERENCES `username_record` (`id`) ON DELETE CASCADE ON UPDATE RESTRICT;

--
-- Constraints for table `user`
--
ALTER TABLE `user`
  ADD CONSTRAINT `fk__username_record__user` FOREIGN KEY (`username_record_id`) REFERENCES `username_record` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
