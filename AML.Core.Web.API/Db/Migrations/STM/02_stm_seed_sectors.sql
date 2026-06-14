-- =====================================================================
-- STM - Seed sectors (Insurance, Real Estate)
-- =====================================================================

INSERT INTO stm_sector (sector_code, sector_name, description, is_active, client_id)
VALUES
  ('INS', 'Insurance',   'Life, General, Health, Motor and Property Insurance transactions', 1, 0),
  ('RE',  'Real Estate', 'Property purchase, sale, lease, brokerage and developer transactions', 1, 0)
ON DUPLICATE KEY UPDATE sector_name = VALUES(sector_name), description = VALUES(description);
