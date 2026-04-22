using AML.Core.RepositoryContract.Notepad;
using AML.Core.ServiceContract.Notepad;
using AML.DTO.DTO.Notepad;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.Service.Notepad
{
    public class NotepadService : INotepadService
    {
        private readonly INotepadRepository _notepadRepository;

        public NotepadService(INotepadRepository notepadRepository)
        {
            _notepadRepository = notepadRepository;
        }

        public async Task<NotepadDTO> GetNotepadAsync(int userId)
        {
            return await _notepadRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<NotepadDTO>> GetAllNotesAsync(int userId)
        {
            return await _notepadRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<NotepadDTO> GetNoteByIdAsync(int id)
        {
            return await _notepadRepository.GetByIdAsync(id);
        }

        public async Task<bool> SaveNotepadAsync(NotepadDTO notepad)
        {
            // If it's a new note, ensure the subject is unique for this user
            if (notepad.Id == 0)
            {
                var allNotes = await _notepadRepository.GetAllByUserIdAsync(notepad.UserId);
                var existingSubjects = allNotes.Select(n => n.Subject).ToList();
                
                if (existingSubjects.Contains(notepad.Subject))
                {
                    string baseSubject = notepad.Subject;
                    int counter = 1;
                    while (existingSubjects.Contains($"{baseSubject} ({counter})"))
                    {
                        counter++;
                    }
                    notepad.Subject = $"{baseSubject} ({counter})";
                }
            }
            var id = await _notepadRepository.SaveAsync(notepad);
            if (id <= 0 && notepad.Id != 0) return false; // Update failed
            if (notepad.Id == 0) notepad.Id = id;
            return id > 0;
        }

        public async Task<bool> AddAttachmentAsync(int userId, NotepadAttachmentDTO attachment)
        {
            var notepadId = attachment.NotepadId;
            if (notepadId == 0)
            {
                var notepad = await _notepadRepository.GetByUserIdAsync(userId);
                if (notepad == null)
                {
                    notepad = new NotepadDTO { UserId = userId, Content = "", Subject = "New Note" };
                    notepadId = await _notepadRepository.SaveAsync(notepad);
                }
                else
                {
                    notepadId = notepad.Id;
                }
            }

            attachment.NotepadId = notepadId;
            return await _notepadRepository.AddAttachmentAsync(attachment) > 0;
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _notepadRepository.GetAttachmentByIdAsync(attachmentId);
            if (attachment != null)
            {
                if (File.Exists(attachment.FilePath))
                {
                    File.Delete(attachment.FilePath);
                }
                return await _notepadRepository.DeleteAttachmentAsync(attachmentId) > 0;
            }
            return false;
        }

        public async Task<NotepadAttachmentDTO> GetAttachmentAsync(int attachmentId)
        {
            return await _notepadRepository.GetAttachmentByIdAsync(attachmentId);
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            var note = await _notepadRepository.GetByIdAsync(id);
            if (note != null && note.Attachments != null)
            {
                foreach (var att in note.Attachments)
                {
                    if (File.Exists(att.FilePath))
                    {
                        File.Delete(att.FilePath);
                    }
                }
            }
            return await _notepadRepository.DeleteAsync(id) > 0;
        }
    }
}
