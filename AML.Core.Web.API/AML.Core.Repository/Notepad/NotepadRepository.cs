using AML.Core.RepositoryContract.Notepad;
using AML.DTO.DTO.Notepad;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Core.Repository.Notepad
{
    public class NotepadRepository : BaseRepository, INotepadRepository
    {
        public NotepadRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public async Task<NotepadDTO> GetByUserIdAsync(int userId)
        {
            try
            {
                var sql = @"SELECT id, user_id as UserId, content, subject, created_on as CreatedOn, updated_on as UpdatedOn FROM tbl_notepad WHERE user_id = @UserId ORDER BY updated_on DESC LIMIT 1;
                          SELECT id, notepad_id as NotepadId, file_name as FileName, file_path as FilePath, file_type as FileType, file_size as FileSize, uploaded_on as UploadedOn FROM tbl_notepad_attachments WHERE notepad_id = (SELECT id FROM tbl_notepad WHERE user_id = @UserId ORDER BY updated_on DESC LIMIT 1);";

                using (var conn = GetConnections())
                {
                    using (var multi = await conn.QueryMultipleAsync(sql, new { UserId = userId }))
                    {
                        var notepad = await multi.ReadFirstOrDefaultAsync<NotepadDTO>();
                        if (notepad != null)
                        {
                            notepad.Attachments = (await multi.ReadAsync<NotepadAttachmentDTO>()).ToList();
                        }
                        return notepad;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<NotepadDTO>> GetAllByUserIdAsync(int userId)
        {
            try
            {
                var sql = "SELECT id, user_id, subject, created_on, updated_on FROM tbl_notepad WHERE user_id = @UserId ORDER BY updated_on DESC";
                using (var conn = GetConnections())
                {
                    return await conn.QueryAsync<NotepadDTO>(sql, new { UserId = userId });
                }
            }
            catch (Exception ex)
            {
                return new List<NotepadDTO>();
            }
        }

        public async Task<NotepadDTO> GetByIdAsync(int id)
        {
            try
            {
                var sql = @"SELECT id, user_id as UserId, content, subject, created_on as CreatedOn, updated_on as UpdatedOn FROM tbl_notepad WHERE id = @Id;
                          SELECT id, notepad_id as NotepadId, file_name as FileName, file_path as FilePath, file_type as FileType, file_size as FileSize, uploaded_on as UploadedOn FROM tbl_notepad_attachments WHERE notepad_id = @Id;";

                using (var conn = GetConnections())
                {
                    using (var multi = await conn.QueryMultipleAsync(sql, new { Id = id }))
                    {
                        var notepad = await multi.ReadFirstOrDefaultAsync<NotepadDTO>();
                        if (notepad != null)
                        {
                            notepad.Attachments = (await multi.ReadAsync<NotepadAttachmentDTO>()).ToList();
                        }
                        return notepad;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> SaveAsync(NotepadDTO notepad)
        {
            try
            {
                using (var conn = GetConnections())
                {
                    if (notepad.Id == 0)
                    {
                        var sql = @"INSERT INTO tbl_notepad (user_id, content, subject, created_on, updated_on) 
                                  VALUES (@UserId, @Content, @Subject, NOW(), NOW());
                                  SELECT LAST_INSERT_ID();";
                        return await conn.ExecuteScalarAsync<int>(sql, notepad);
                    }
                    else
                    {
                        var sql = @"UPDATE tbl_notepad 
                                  SET content = @Content, subject = @Subject, updated_on = NOW() 
                                  WHERE id = @Id AND user_id = @UserId;";
                        int rows = await conn.ExecuteAsync(sql, notepad);
                        if (rows == 0) return -1; // Update failed
                        return notepad.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> AddAttachmentAsync(NotepadAttachmentDTO attachment)
        {
            try
            {
                var sql = @"INSERT INTO tbl_notepad_attachments (notepad_id, file_name, file_path, file_type, file_size, uploaded_on) 
                          VALUES (@NotepadId, @FileName, @FilePath, @FileType, @FileSize, NOW());";

                using (var conn = GetConnections())
                {
                    return await conn.ExecuteAsync(sql, attachment);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return 0;
            }
        }

        public async Task<int> DeleteAttachmentAsync(int attachmentId)
        {
            try
            {
                var sql = "DELETE FROM tbl_notepad_attachments WHERE id = @Id";
                using (var conn = GetConnections())
                {
                    return await conn.ExecuteAsync(sql, new { Id = attachmentId });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int attachmentId)
        {
            try
            {
                var sql = "SELECT id, notepad_id as NotepadId, file_name as FileName, file_path as FilePath, file_type as FileType, file_size as FileSize, uploaded_on as UploadedOn FROM tbl_notepad_attachments WHERE id = @Id";
                using (var conn = GetConnections())
                {
                    return await conn.QueryFirstOrDefaultAsync<NotepadAttachmentDTO>(sql, new { Id = attachmentId });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM tbl_notepad WHERE id = @Id";
                using (var conn = GetConnections())
                {
                    return await conn.ExecuteAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
