-- =====================================================================
-- Activate the "Transaction Monitoring" entry in menu_master (Menu_Id=12)
-- so it shows up in the Admin Management → Module Rights checklist.
--
-- Adds nothing new to the access table - the existing
-- stm_client_sector_access table is the source of truth for which
-- sectors (Insurance / Real Estate / both) a given client can see.
-- When a client checks the "Transaction Monitoring" right the admin
-- UI will collect their sector choice and write to stm_client_sector_access.
-- =====================================================================

UPDATE menu_master SET is_active = 1 WHERE Menu_Id = 12;

-- Verify
SELECT Menu_Id, Menu_Name, is_active
FROM menu_master
WHERE Menu_Id IN (12, 1, 4, 8, 17, 18, 19, 20)
ORDER BY Menu_Id;
