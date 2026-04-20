using AML.DTO.DTO.Notepad;
using System.Threading.Tasks;

namespace AML.Core.ServiceContract.Notepad
{
    public interface INotepadService
    {
        Task<NotepadDTO> GetNotepadAsync(int userId);
        Task<bool> SaveNotepadAsync(NotepadDTO notepad);
        Task<bool> AddAttachmentAsync(int userId, NotepadAttachmentDTO attachment);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<NotepadAttachmentDTO> GetAttachmentAsync(int attachmentId);
    }
}
