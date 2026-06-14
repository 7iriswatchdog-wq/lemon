using AML.DTO.DTO.Notepad;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.RepositoryContract.Notepad
{
    public interface INotepadRepository
    {
        Task<NotepadDTO> GetByUserIdAsync(int userId);
        Task<IEnumerable<NotepadDTO>> GetAllByUserIdAsync(int userId);
        Task<int> SaveAsync(NotepadDTO notepad);
        Task<int> AddAttachmentAsync(NotepadAttachmentDTO attachment);
        Task<int> DeleteAttachmentAsync(int attachmentId);
        Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int attachmentId);
        Task<NotepadDTO> GetByIdAsync(int id);
        Task<int> DeleteAsync(int id);

        // Notepad V2
        Task<IEnumerable<Notepadv2DTO>> GetNotepadv2Async(int clientId, string search);
        Task<int> SaveNotepadv2Async(Notepadv2DTO model);
        Task<int> DeleteNotepadv2Async(int id);
    }
}
