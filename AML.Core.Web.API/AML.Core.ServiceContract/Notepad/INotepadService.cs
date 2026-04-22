using AML.DTO.DTO.Notepad;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.ServiceContract.Notepad
{
    public interface INotepadService
    {
        Task<NotepadDTO> GetNotepadAsync(int userId);
        Task<IEnumerable<NotepadDTO>> GetAllNotesAsync(int userId);
        Task<NotepadDTO> GetNoteByIdAsync(int id);
        Task<bool> SaveNotepadAsync(NotepadDTO notepad);
        Task<bool> AddAttachmentAsync(int userId, NotepadAttachmentDTO attachment);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<NotepadAttachmentDTO> GetAttachmentAsync(int attachmentId);
        Task<bool> DeleteNoteAsync(int id);
    }
}
