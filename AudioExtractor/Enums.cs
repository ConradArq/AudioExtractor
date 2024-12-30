using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AudioScriptExtractor
{
    public enum OutputComposition
    {
        [Display(Name = "Original audio")]
        ORIGINAL = 0,

        [Display(Name = "Speech sinthesized text")]
        TTS = 1,

        [Display(Name = "Original audio + Speech sinthesized text")]
        ORIGINAL_TTS = 2,

        [Display(Name = "Speech sinthesized text + Original audio")]
        TTS_ORIGINAL = 3,

        [Display(Name = "Original audio + Speech sinthesized text + Original audio")]
        ORIGINAL_TTS_ORIGINAL = 4,

        [Display(Name = "Custom number of lines (default 1+1)")]
        BULK = 5,
    }
}
