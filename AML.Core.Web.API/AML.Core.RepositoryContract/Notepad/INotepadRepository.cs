using AML.DTO.DTO.Notepad;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.RepositoryContract.Notepad
{
    public interface INotepadRepository
    {
        Task<NotepadDTO> GetByUserIdAsync(int userId);
        Task<int> SaveAsync(NotepadDTO notepad);
        Task<int> AddAttachmentAsync(NotepadAttachmentDTO attachment);
        Task<int> DeleteAttachmentAsync(int attachmentId);
        Task<NotepadAttachmentDTO> GetAttachmentByIdAsync(int attachmentId);
    }
}
