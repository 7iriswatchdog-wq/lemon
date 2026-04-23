using AML.Core.RepositoryContract.Notepad;
using AML.DTO.DTO.Notepad;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AML.Core.Repository.Notepad
{
    public class NotepadRepository : INotepadRepository
    {
        private readonly IConfiguration _configuration;

        public NotepadRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection GetConnections()
        {
            return new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<NotepadDTO>> GetAllByUserIdAsync(int userId)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad WHERE user_id = @UserId AND is_delete = 0 ORDER BY updated_on DESC";
                return await conn.QueryAsync<NotepadDTO>(sql, new { UserId = userId });
            }
        }

        public async Task<NotepadDTO> GetByUserIdAsync(int userId)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad WHERE user_id = @UserId AND is_delete = 0 LIMIT 1";
                return await conn.QueryFirstOrDefaultAsync<NotepadDTO>(sql, new { UserId = userId });
            }
        }

        public async Task<int> SaveAsync(NotepadDTO model)
        {
            using (var conn = GetConnections())
            {
                if (model.Id == 0)
                {
                    var sql = "INSERT INTO aml_notepad (user_id, subject, content, created_on, updated_on, is_delete) VALUES (@UserId, @Subject, @Content, NOW(), NOW(), 0); SELECT LAST_INSERT_ID();";
                    model.Id = await conn.ExecuteScalarAsync<int>(sql, model);
                    return model.Id;
                }
                else
                {
                    var sql = "UPDATE aml_notepad SET subject = @Subject, content = @Content, updated_on = NOW() WHERE id = @Id";
                    return await conn.ExecuteAsync(sql, model);
                }
            }
        }

        public async Task<int> AddAttachmentAsync(NotepadAttachmentDTO attachment)
        {
            using (var conn = GetConnections())
            {
                var sql = "INSERT INTO aml_notepad_attachments (notepad_id, file_name, file_path, file_type, file_size, uploaded_on, is_delete) VALUES (@NotepadId, @FileName, @FilePath, @FileType, @FileSize, NOW(), 0)";
                return await conn.ExecuteAsync(sql, attachment);
            }
        }

        public async Task<int> DeleteAttachmentAsync(int attachmentId)
        {
            using (var conn = GetConnections())
            {
                var sql = "UPDATE aml_notepad_attachments SET is_delete = 1 WHERE id = @Id";
                return await conn.ExecuteAsync(sql, new { Id = attachmentId });
            }
        }

        public async Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int attachmentId)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad_attachments WHERE id = @Id AND is_delete = 0";
                return await conn.QueryFirstOrDefaultAsync<NotepadAttachmentDTO>(sql, new { Id = attachmentId });
            }
        }

        public async Task<NotepadDTO> GetByIdAsync(int id)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad WHERE id = @Id AND is_delete = 0";
                return await conn.QueryFirstOrDefaultAsync<NotepadDTO>(sql, new { Id = id });
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    var sql = "UPDATE aml_notepad SET is_delete = 1 WHERE id = @Id";
                    return await conn.ExecuteAsync(sql, new { Id = id });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Notepadv2DTO>> GetNotepadv2Async(int clientId, string search)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    var sql = "SELECT * FROM notepadv2 WHERE client_id = @ClientId AND is_delete = 0";
                    if (!string.IsNullOrEmpty(search))
                    {
                        sql += " AND (customer_name LIKE @Search OR wds_cust_id LIKE @Search OR unit_ref_no LIKE @Search)";
                    }
                    sql += " ORDER BY id DESC";
                    
                    return await conn.QueryAsync<Notepadv2DTO>(sql, new { ClientId = clientId, Search = $"%{search}%" });
                }
            }
            catch (Exception)
            {
                return new List<Notepadv2DTO>();
            }
        }

        public async Task<int> SaveNotepadv2Async(Notepadv2DTO model)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    if (model.id == 0)
                    {
                        var sql = @"INSERT INTO notepadv2 (date_of_receipt, cust_type, wds_cust_id, customer_name, Comment_kyc, created_by, created_on, client_id, case_type, unit_ref_no, cust_type_realestate, kyc_check) 
                                   VALUES (@date_of_receipt, @cust_type, @wds_cust_id, @customer_name, @Comment_kyc, @created_by, NOW(), @client_id, @case_type, @unit_ref_no, @cust_type_realestate, @kyc_check)";
                        return await conn.ExecuteAsync(sql, model);
                    }
                    else
                    {
                        var sql = @"UPDATE notepadv2 SET date_of_receipt = @date_of_receipt, cust_type = @cust_type, wds_cust_id = @wds_cust_id, 
                                   customer_name = @customer_name, Comment_kyc = @Comment_kyc, updated_by = @updated_by, updated_on = NOW(), 
                                   case_type = @case_type, unit_ref_no = @unit_ref_no, cust_type_realestate = @cust_type_realestate, kyc_check = @kyc_check
                                   WHERE id = @id";
                        return await conn.ExecuteAsync(sql, model);
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> DeleteNotepadv2Async(int id)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    var sql = "UPDATE notepadv2 SET is_delete = 1 WHERE id = @Id";
                    return await conn.ExecuteAsync(sql, new { Id = id });
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
