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
                var sql = @"SELECT * FROM tbl_notepad WHERE user_id = @UserId;
                          SELECT * FROM tbl_notepad_attachments WHERE notepad_id = (SELECT id FROM tbl_notepad WHERE user_id = @UserId);";

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
                // Log exception
                return null;
            }
        }

        public async Task<int> SaveAsync(NotepadDTO notepad)
        {
            try
            {
                var sql = @"INSERT INTO tbl_notepad (user_id, content, subject, created_on, updated_on) 
                          VALUES (@UserId, @Content, @Subject, NOW(), NOW()) 
                          ON DUPLICATE KEY UPDATE content = @Content, subject = @Subject, updated_on = NOW();";

                using (var conn = GetConnections())
                {
                    return await conn.ExecuteAsync(sql, notepad);
                }
            }
            catch (Exception ex)
            {
                // Log exception
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
                // Log exception
                return 0;
            }
        }

        public async Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int attachmentId)
        {
            try
            {
                var sql = "SELECT * FROM tbl_notepad_attachments WHERE id = @Id";
                using (var conn = GetConnections())
                {
                    return await conn.QueryFirstOrDefaultAsync<NotepadAttachmentDTO>(sql, new { Id = attachmentId });
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }
    }
}
