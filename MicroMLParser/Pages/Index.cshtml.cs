using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroMLParser.Services;
using System;

namespace MicroMLParser.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MicroMLParser.Services.MicroMLParser _parser;
        private readonly ASTRenderer _renderer;

        [BindProperty]
        public string Code { get; set; }

        public string SvgOutput { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorLine { get; set; }
        public string ErrorContext { get; set; }

        public IndexModel(MicroMLParser.Services.MicroMLParser parser, ASTRenderer renderer)
        {
            _parser = parser;
            _renderer = renderer;
        }

        public void OnGet()
        {
            // Default to an empty string
            Code = string.Empty;
            SvgOutput = string.Empty;
            ErrorMessage = string.Empty;
            ErrorLine = 0;
            ErrorContext = string.Empty;
        }

        public IActionResult OnPost()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    ErrorMessage = "Please enter some MicroML code.";
                    SvgOutput = string.Empty;
                    return Page();
                }

                var ast = _parser.Parse(Code);
                SvgOutput = _renderer.RenderToSvg(ast);
                ErrorMessage = string.Empty;
                ErrorLine = 0;
                ErrorContext = string.Empty;
            }
            catch (MicroMLParser.Services.MicroMLParser.ParseException ex)
            {
                ErrorMessage = ex.Message;
                ErrorLine = ex.LineNumber;
                ErrorContext = ex.Context;
                SvgOutput = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                ErrorLine = 0;
                ErrorContext = string.Empty;
                SvgOutput = string.Empty;
            }

            return Page();
        }
    }
}