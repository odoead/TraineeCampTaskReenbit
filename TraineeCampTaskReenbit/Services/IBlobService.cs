using TraineeCampTaskReenbit.DTO;

namespace TraineeCampTaskReenbit.Services
{
    public interface IBlobService
    {
        Task<List<BlobRequestDTO>> GetAllAsync();
        Task<BlobResponseDTO> UploadAsync(IFormFile file, string Email);
        Task<BlobResponseDTO> DeleteAsync(string FileName);
        Task<BlobRequestDTO> DownloadAsync(string FileName);
    }
}
