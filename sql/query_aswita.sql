SELECT id,cust_ref_id,fname,lname,company_code,parent_id,relationship,flagtype,version FROM customermaster WHERE fname LIKE '%Aswita%' OR lname LIKE '%Aswita%';
SELECT id,cust_ref_id,fname,lname,company_code,parent_id,relationship,flagtype FROM customermaster WHERE parent_id LIKE '%NAT276%' OR FIND_IN_SET('NAT276', parent_id)>0;
SELECT * FROM tempshareholdersdata WHERE fname LIKE '%Aswita%' OR parent_id LIKE '%NAT276%';
