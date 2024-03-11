namespace TraineeCampTaskReenbit.DTO
{
    public class BlobResponseDTO
    {
        public bool? isSuccess { get; set; }
        public string Message {  get; set; }
        public BlobRequestDTO Blob { get; set; }
    }
}
