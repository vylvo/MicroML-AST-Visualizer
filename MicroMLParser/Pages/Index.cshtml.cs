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

        public IndexModel(MicroMLParser.Services.MicroMLParser parser, ASTRenderer renderer)
        {
            _parser = parser;
            _renderer = renderer;
        }

        public void OnGet()
        {
            // Default to empty
            Code = string.Empty;
            SvgOutput = string.Empty;
            ErrorMessage = string.Empty;
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
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error parsing code: {ex.Message}";
                SvgOutput = string.Empty;
            }

            return Page();
        }
    }
}