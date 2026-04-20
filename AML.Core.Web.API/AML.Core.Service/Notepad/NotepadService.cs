using AML.Core.RepositoryContract.Notepad;
using AML.Core.ServiceContract.Notepad;
using AML.DTO.DTO.Notepad;
using System;
using System.IO;
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

        public async Task<bool> SaveNotepadAsync(NotepadDTO notepad)
        {
            return await _notepadRepository.SaveAsync(notepad) > 0;
        }

        public async Task<bool> AddAttachmentAsync(int userId, NotepadAttachmentDTO attachment)
        {
            var notepad = await _notepadRepository.GetByUserIdAsync(userId);
            if (notepad == null)
            {
                // Create notepad if it doesn't exist
                notepad = new NotepadDTO { UserId = userId, Content = "" };
                await _notepadRepository.SaveAsync(notepad);
                notepad = await _notepadRepository.GetByUserIdAsync(userId);
            }

            attachment.NotepadId = notepad.Id;
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
    }
}
