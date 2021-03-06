﻿CREATE TABLE servers (
  id BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
  server_name VARCHAR(255) NOT NULL,
  server_ip VARCHAR(255) NOT NULL,
  server_port INT(11) NOT NULL,
  server_password VARCHAR(255) DEFAULT NULL,
  guild_id BIGINT(20) UNSIGNED NOT NULL,
  PRIMARY KEY (id)
);

ALTER TABLE servers 
  ADD UNIQUE INDEX UK_servers(server_ip, server_port, guild_id);