SELECT cust_ref_id,fname,lname,company_code,parent_id,relationship,flagtype FROM customermaster WHERE client_id=1 AND (company_code='NAT276' OR FIND_IN_SET('NAT276', company_code)>0);
SELECT cust_ref_id,fname,lname,company_code,parent_id,relationship,flagtype FROM customermaster WHERE parent_id='NAT276' OR FIND_IN_SET('NAT276', parent_id)>0;
SELECT id,companycode,fullname,relationship,type,flagtype,mainpartycode FROM tempshareholdersdata WHERE companycode='NAT276' OR FIND_IN_SET('NAT276', companycode)>0;
