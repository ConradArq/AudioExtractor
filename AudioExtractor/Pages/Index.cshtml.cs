using AudioScriptExtractor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioScriptExtractor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AudioExtractorSettings _audioExtractorSettings;

        public IndexModel(ILogger<IndexModel> logger, IOptions<AudioExtractorSettings> audioExtractorSettings)
        {
            _logger = logger;
            _audioExtractorSettings = audioExtractorSettings.Value;
        }

        public void OnGet()
        {
        }


        [BindProperty]
        public AudioExtractor AudioExtractor { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await AudioExtractorService.ExtractAudio(AudioExtractor, _audioExtractorSettings, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return RedirectToPage("./Index");
        }
    }
}