using TraineeCampTaskReenbit.validator;

namespace TraineeCampTaskReenbit.DTO
{
    [FileFormatValidationAttribute]
    public class BlobRequestDTO
    {
        public string Name {  get; set; }
        public string URL {  get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
