using System.Diagnostics;
using System.Formats.Tar;
using System.Text.RegularExpressions;
using AudioScriptExtractor.Models;
using AudioScriptExtractor.Pages;
using Microsoft.CognitiveServices.Speech;

namespace AudioScriptExtractor
{
    public class AudioExtractorService
    {
        public static async Task ExtractAudio(AudioExtractor audioExtractor, AudioExtractorSettings audioExtractorSettings, ILogger<IndexModel> logger)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = audioExtractorSettings.FFmpegPath;
            myProcess.StartInfo.CreateNoWindow = true;

            string outputFileDirectory = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "\\AudioExtractor");
            Directory.CreateDirectory(outputFileDirectory);

            string outputFilePath = string.Concat(outputFileDirectory, "\\", Path.GetFileName(audioExtractor.SourceDirectory), '.', audioExtractor.OutputCompositionCode, ".mp3");

            string[] subtitleFilesPaths = Directory.GetFiles(audioExtractor.SourceDirectory, "*.srt", SearchOption.AllDirectories);

            int iterationCounter, intVal, lineNumber; iterationCounter = lineNumber = 0;
            Regex rgTime = new Regex("([0-9]{2}):([0-9]{2}):([0-9]{2}),[0-9]{3} --> ([0-9]{2}):([0-9]{2}):([0-9]{2}),[0-9]{3}");
            string startTime;
            string endTime;
            string text = string.Empty;

            MemoryStream originalAudioMSMp3 = new MemoryStream();
            List<MemoryStream> originalAudioBulkMSList = new List<MemoryStream>();

            using (StreamWriter logFile = File.AppendText(string.Concat(outputFileDirectory, "\\", Path.GetFileName(audioExtractor.SourceDirectory), ".log")))
            using (StreamWriter scriptFile = File.AppendText(string.Concat(outputFileDirectory, "\\", Path.GetFileName(audioExtractor.SourceDirectory), ".script")))
            using (FileStream outputFS = File.OpenWrite(outputFilePath))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Byte numLinesInChunk = 0;

                foreach (string subtitleFilePath in subtitleFilesPaths)
                {

                    string fileWOExtPath = string.Concat(Path.GetDirectoryName(subtitleFilePath), "\\", Path.GetFileNameWithoutExtension(subtitleFilePath));
                    string? videoFilePath = null;

                    //Checks if a corresponding video file of the same name as the subtitle file can be found. If not, the event is registered and it moves on to the next subtitle file.
                    if (audioExtractor.OutputCompositionCode != OutputComposition.TTS)
                    {
                        foreach (string ext in audioExtractorSettings.AllowedVideoExtensions.Split(","))
                        {
                            string videoFilePathTmp = string.Concat(fileWOExtPath, ".", ext);
                            if (File.Exists(videoFilePathTmp))
                            {
                                videoFilePath = videoFilePathTmp;
                                break;
                            }
                        }

                        if (videoFilePath == null)
                        {
                            logger.LogWarning($"{fileWOExtPath} video file could not be found");
                            continue;
                        }
                    }

                    IEnumerable<double>? bookmarks = null;

                    //If bookmarks option is selected, checks if a corresponding pbf file of the same name as the subtitle file can be found. If not, the event is registered and it moves on to the next subtitle file.
                    if (audioExtractor.UseBookmarks)
                    {
                        string bookmarksFilePath = string.Concat(fileWOExtPath, ".pbf");

                        if (File.Exists(bookmarksFilePath))
                        {
                            bookmarks = System.IO.File.ReadAllLines(bookmarksFilePath).Where(s => s.Contains("*")).Select(s => double.Parse(s.Substring(s.IndexOf("=") + 1, s.IndexOf("*") - s.IndexOf("=") - 1)));
                        }
                        else
                        {
                            logger.LogWarning($"{bookmarksFilePath} bookmarks file could not be found");
                            continue;
                        }
                    }

                    string[] linesParsed = ParseSubtitleFile(File.ReadAllLines(subtitleFilePath), bookmarks, audioExtractorSettings);

                    foreach (string line in linesParsed)
                    {

                        //1. NUMBER -> Parsing of first line of chunk which is a positive integer (E.g. "1")
                        if (int.TryParse(line, out intVal))
                        {
                            lineNumber = intVal;
                            continue;
                        }

                        //2. TIMESTAMPS parsing (00:00:01,631 --> 00:00:05,631) -> Cutting-out of the audio stream via ffmpeg -> Addition of audio cutout to List
                        if (rgTime.IsMatch(line))
                        {
                            //Skip this step if output is composed only of synthesized audio
                            if (audioExtractor.OutputCompositionCode == OutputComposition.TTS) continue;

                            startTime = DateTime.Parse(line.Substring(0, 12).Replace(',', '.')).AddMilliseconds(Double.Parse(audioExtractorSettings.SubStartTimeOffset)).ToString("HH:mm:ss.fff");
                            endTime = DateTime.Parse(line.Substring(17, 12).Replace(',', '.')).AddMilliseconds(Double.Parse(audioExtractorSettings.SubEndTimeOffset)).ToString("HH:mm:ss.fff");

                            myProcess.StartInfo.Arguments = $"-i \"{videoFilePath}\" -ss {startTime} -to {endTime} -map 0:a -f mp3 pipe:";
                            myProcess.StartInfo.RedirectStandardOutput = true;

                            myProcess.Start();

                            originalAudioMSMp3.SetLength(0);

                            myProcess.StandardOutput.BaseStream.CopyTo(originalAudioMSMp3);

                            if (originalAudioMSMp3.Length == 0) throw new Exception($"Cannot copy audio stream from video file: {videoFilePath}");

                            logFile.WriteLine("#" + ++iterationCounter + " Line number: " + lineNumber + " - Audio Stream Length: " + originalAudioMSMp3.Length + " - Elapsed Milliseconds: " + stopwatch.ElapsedMilliseconds + " - File Name: " + Path.GetFileNameWithoutExtension(subtitleFilePath));

                            stopwatch.Restart();

                            continue;
                        }

                        //4. EMPTY LINE (End of subtitle chunk) -> Making up of the output through different combinations of TTS and Original Audio
                        if (String.IsNullOrEmpty(line))
                        {

                            logFile.Write("\n");
                            logFile.WriteLine(text); scriptFile.WriteLine(text);
                            logFile.Write("\n"); scriptFile.Write("\n"); 


                            //Get synthesized audio from subtitles unless only original sound is needed
                            MemoryStream msTTS = audioExtractor.OutputCompositionCode != OutputComposition.ORIGINAL? await GetAudibleMemoryStreamFrom(text, audioExtractorSettings) : null!;

                            switch (audioExtractor.OutputCompositionCode)
                            {
                                //Output file is composed only of original sound from source file
                                case OutputComposition.ORIGINAL:
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(outputFS);
                                    break;

                                //Output file is composed only of synthesized audio from subtitles
                                case OutputComposition.TTS:
                                    msTTS.Position = 0;
                                    msTTS.CopyTo(outputFS);
                                    break;

                                //Output file is composed of original sound + synthesized audio from subtitles alternating 1 by 1 lines
                                case OutputComposition.ORIGINAL_TTS:
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(outputFS);
                                    msTTS.Position = 0;
                                    msTTS.CopyTo(outputFS);
                                    break;

                                //Output file is composed of synthesized audio from subtitles + original sound alternating 1 by 1 lines
                                case OutputComposition.TTS_ORIGINAL:
                                    msTTS.Position = 0;
                                    msTTS.CopyTo(outputFS);
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(outputFS);
                                    break;

                                //Output file is composed of original sound + synthesized audio from subtitles + repeated original sound alternating 1 by 1 lines
                                case OutputComposition.ORIGINAL_TTS_ORIGINAL:
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(outputFS);
                                    msTTS.Position = 0;
                                    msTTS.CopyTo(outputFS);
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(outputFS);
                                    break;

                                //Output file is composed of synthesized audio from subtitles + original sound alternating N by N lines
                                case OutputComposition.BULK:
                                    msTTS.Position = 0;
                                    msTTS.CopyTo(outputFS);

                                    MemoryStream originalOutputMSBulk = new MemoryStream();
                                    originalAudioMSMp3.Position = 0;
                                    originalAudioMSMp3.CopyTo(originalOutputMSBulk);
                                    originalAudioBulkMSList.Add(originalOutputMSBulk);

                                    if (numLinesInChunk + originalAudioBulkMSList.Count() > audioExtractor.BulkMaxLines)
                                    {
                                        foreach (MemoryStream ob in originalAudioBulkMSList)
                                        {
                                            ob.Position = 0;
                                            ob.CopyTo(outputFS);
                                            ob.Dispose();
                                        }
                                        originalAudioBulkMSList.Clear();
                                    }

                                    break;
                            }


                            text = string.Empty;
                            numLinesInChunk = 0;
                            continue;
                        }


                        //3. TEXT LINES
                        else
                        {
                            text = String.IsNullOrEmpty(text) ? line : String.Concat(text, " ", line);
                            numLinesInChunk++;
                        }
                    }

                }

                myProcess.Dispose();
                stopwatch.Stop();
            }
        }

        public static async Task<MemoryStream> GetAudibleMemoryStreamFrom(string text, AudioExtractorSettings audioExtractorSettings)
        {

            var speechConfig = SpeechConfig.FromSubscription(audioExtractorSettings.SpeechKey, audioExtractorSettings.SpeechRegion);
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz96KBitRateMonoMp3);

            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, null);

            var ssmlTmp = File.ReadAllText("ssml.xml");
            var ssml = ssmlTmp.Replace("{text}", text);
            var result = await speechSynthesizer.SpeakSsmlAsync(ssml);


            if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                string exceptionMsg = cancellation.Reason.ToString();

                if (cancellation.Reason == CancellationReason.Error)
                {
                    exceptionMsg += cancellation.ErrorCode + cancellation.ErrorDetails;
                }

                throw new Exception("Error while trying to speech synthesize text", new Exception(exceptionMsg));
            }

            using var stream = AudioDataStream.FromResult(result);

            return new MemoryStream(result.AudioData);
        }

        public static string[] ParseSubtitleFile(string[] lines, IEnumerable<double>? bookmarks, AudioExtractorSettings audioExtractorSettings)
        {

            /*
                lineList contains chunks, e.g:
                    50
                    00:07:06,968 --> 00:07:08,677
                    - Where's your team?
                    - Waiting.

                chunkList contains a the list of all chunks through to the end of file
            */


            List<List<string>> chunkList = new List<List<string>>();
            List<string> lineList = new List<string>();
            foreach (string l in lines)
            {
                lineList.Add(l.Replace('&',' '));

                if (String.IsNullOrEmpty(l))
                {

                    if (bookmarks != null)
                    {
                        var startTimeMilliseconds = (new TimeSpan(0, Int32.Parse(lineList[1].Substring(0, 2)), Int32.Parse(lineList[1].Substring(3, 2)), Int32.Parse(lineList[1].Substring(6, 2)), Int32.Parse(lineList[1].Substring(9, 3)))).TotalMilliseconds;
                        var endTimeMilliseconds = (new TimeSpan(0, Int32.Parse(lineList[1].Substring(17, 2)), Int32.Parse(lineList[1].Substring(20, 2)), Int32.Parse(lineList[1].Substring(23, 2)), Int32.Parse(lineList[1].Substring(26, 3)))).TotalMilliseconds;
                        if (!bookmarks.Any(b => b > startTimeMilliseconds && b < endTimeMilliseconds))
                        {
                            lineList = new List<string>();
                            continue;
                        }
                    }

                    chunkList.Add(lineList);
                    lineList = new List<string>();
                }
            }

            List<List<string>> parseChunkList = chunkList.Where(c => c.All(l => !l.ContainsAny(audioExtractorSettings.DisallowedCharsInSubFile.Split(','))) && c.Count() > 1 && c[2].Split(' ').Length >= Int32.Parse(audioExtractorSettings.MinNumberOfWordsPerLine)).ToList();

            string[] parsedLines = parseChunkList.SelectMany(x => x).ToArray();

            return parsedLines;
        }
    }
}

