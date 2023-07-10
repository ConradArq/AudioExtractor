namespace AudioScriptExtractor.Models
{
    public class AudioExtractorSettings
    {
        public string FFmpegPath { get; set; } = default!;
        public string SpeechKey { get; set; } = default!;
        public string SpeechRegion { get; set; } = default!;
        public string AllowedVideoExtensions { get; set; } = default!;
        public string DisallowedCharsInSubFile { get; set; } = default!;
        public string MinNumberOfWordsPerLine { get; set; } = default!;
        public string SubStartTimeOffset { get; set; } = default!;
        public string SubEndTimeOffset { get; set; } = default!;
    }
}
