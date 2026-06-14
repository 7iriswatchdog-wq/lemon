using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Notepad
{
    public class NotepadDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        [Column("updated_on")]
        public DateTime UpdatedOn { get; set; }

        public List<NotepadAttachmentDTO> Attachments { get; set; } = new List<NotepadAttachmentDTO>();
    }

    public class NotepadAttachmentDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("notepad_id")]
        public int NotepadId { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("file_path")]
        public string FilePath { get; set; }

        [Column("file_type")]
        public string FileType { get; set; }

        [Column("file_size")]
        public long FileSize { get; set; }

        [Column("uploaded_on")]
        public DateTime UploadedOn { get; set; }
    }
}
