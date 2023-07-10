using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;

namespace AudioScriptExtractor.Models
{
    public class AudioExtractor
    {
        [Required(ErrorMessage = "Please enter the working directory route")]
        [Display(Name = "Full path of directory where video, .srt and .pbf files are located", Prompt = "Example: C:\\Users\\MyUser\\Videos")]
        public string SourceDirectory { get; set; } = default!;

        [Display(Name = "Use PBF file?")]
        public bool UseBookmarks { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please select a type of composition for the output file")]
        [Display(Name = "Select type of audio composition")]
        public OutputComposition OutputCompositionCode { get; set; } = default!;

        [RequiredIf(nameof(OutputCompositionCode), "BULK", ErrorMessage = "Please enter a number")]
        [Display(Name = "Number of lines of original and synthesized audio to alternate")]
        public byte? BulkMaxLines { get; set; }
    }
}
