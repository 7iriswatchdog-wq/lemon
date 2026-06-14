-- =====================================================================
-- STM - Per-client sector access (multi-tenant SaaS)
-- A client (= tenant) sees only sectors they have an access row for.
-- The "master" client (National Compliance) is granted access to ALL sectors.
-- =====================================================================

CREATE TABLE IF NOT EXISTS stm_client_sector_access (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    client_id   INT NOT NULL,
    sector_id   INT NOT NULL,
    is_master   TINYINT(1) NOT NULL DEFAULT 0,   -- 1 if this client is the platform master/admin
    created_on  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_stm_csa (client_id, sector_id),
    INDEX idx_stm_csa_client (client_id),
    CONSTRAINT fk_stm_csa_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Grant client 1 (National) access to both sectors as MASTER
INSERT INTO stm_client_sector_access (client_id, sector_id, is_master)
SELECT 1, id, 1 FROM stm_sector
ON DUPLICATE KEY UPDATE is_master = 1;
