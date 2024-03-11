using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace TraineeCampTaskReenbit.validator
{
    public class FileFormatValidationAttribute : Attribute,IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("file", out var value) && value is IFormFile file)
            {

                if (!IsFileValid(file))
                {
                    context.Result = new BadRequestObjectResult("Only .docx files are allowed.");
                }
            }
        }
        private bool IsFileValid(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var allowedExtensions = new List<string> { ".docx" };
            var fileExtension = Path.GetExtension(file.FileName);

            return allowedExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
