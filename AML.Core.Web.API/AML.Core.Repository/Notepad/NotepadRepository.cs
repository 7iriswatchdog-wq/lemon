using AML.Core.RepositoryContract.Notepad;
using AML.DTO.DTO.Notepad;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

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
                    return await conn.ExecuteScalarAsync<int>(sql, new { UserId = model.UserId, Subject = model.Subject, Content = model.Content });
                }
                else
                {
                    var sql = "UPDATE aml_notepad SET subject = @Subject, content = @Content, updated_on = NOW() WHERE id = @Id";
                    return await conn.ExecuteAsync(sql, new { Subject = model.Subject, Content = model.Content, Id = model.Id });
                }
            }
        }

        public async Task<int> AddAttachmentAsync(NotepadAttachmentDTO model)
        {
            using (var conn = GetConnections())
            {
                var sql = "INSERT INTO aml_notepad_attachments (notepad_id, file_name, file_path, file_type, file_size, uploaded_on, is_delete) VALUES (@NotepadId, @FileName, @FilePath, @FileType, @FileSize, NOW(), 0)";
                return await conn.ExecuteAsync(sql, model);
            }
        }

        public async Task<int> DeleteAttachmentAsync(int id)
        {
            using (var conn = GetConnections())
            {
                var sql = "UPDATE aml_notepad_attachments SET is_delete = 1 WHERE id = @Id";
                return await conn.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int id)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad_attachments WHERE id = @Id AND is_delete = 0";
                return await conn.QueryFirstOrDefaultAsync<NotepadAttachmentDTO>(sql, new { Id = id });
            }
        }

        public async Task<NotepadDTO> GetByIdAsync(int id)
        {
            using (var conn = GetConnections())
            {
                var sql = "SELECT * FROM aml_notepad WHERE id = @Id AND is_delete = 0";
                var notepad = await conn.QueryFirstOrDefaultAsync<NotepadDTO>(sql, new { Id = id });
                if (notepad != null)
                {
                    var attachmentSql = "SELECT * FROM aml_notepad_attachments WHERE notepad_id = @NotepadId AND is_delete = 0";
                    notepad.Attachments = (await conn.QueryAsync<NotepadAttachmentDTO>(attachmentSql, new { NotepadId = id })).ToList();
                }
                return notepad;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            using (var conn = GetConnections())
            {
                var sql = "UPDATE aml_notepad SET is_delete = 1 WHERE id = @Id";
                return await conn.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<IEnumerable<Notepadv2DTO>> GetNotepadv2Async(int clientId, string search)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    return await conn.QueryAsync<Notepadv2DTO>("sp_GetNotepadv2", 
                        new { p_client_id = clientId, p_search = search }, 
                        commandType: CommandType.StoredProcedure);
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
                    var parameters = new DynamicParameters();
                    parameters.Add("p_id", model.id);
                    parameters.Add("p_date_of_receipt", model.date_of_receipt);
                    parameters.Add("p_cust_type", model.cust_type);
                    parameters.Add("p_wds_cust_id", model.wds_cust_id);
                    parameters.Add("p_customer_name", model.customer_name);
                    parameters.Add("p_Comment_kyc", model.comment_kyc);
                    parameters.Add("p_user_id", model.id == 0 ? model.created_by : model.updated_by);
                    parameters.Add("p_client_id", model.client_id);
                    parameters.Add("p_case_type", model.case_type);
                    parameters.Add("p_unit_ref_no", model.unit_ref_no);
                    parameters.Add("p_cust_type_realestate", model.cust_type_realestate);
                    parameters.Add("p_kyc_check", model.kyc_check);

                    return await conn.ExecuteScalarAsync<int>("sp_SaveNotepadv2", parameters, commandType: CommandType.StoredProcedure);
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
                    return await conn.ExecuteAsync("sp_DeleteNotepadv2", new { p_id = id }, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
