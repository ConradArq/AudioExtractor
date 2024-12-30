# Audio Script Extractor

## Overview
**Audio Script Extractor** is a .NET application built with **Razor Pages** that extracts audio dialogue from video files using their corresponding `.srt` subtitle files, with audio processing powered by **FFmpeg** and text-to-speech synthesis using **Microsoft Cognitive Services Speech SDK**. It also allows users to refine the extraction by leveraging `.pbf` bookmark files to select specific lines of interest from the subtitles.

The app generates a single `.mp3` file per given directory, containing all processed audio segments extracted based on the specified criteria.

---

## Features
- Extract audio dialogue from video files using `.srt` subtitle files.
- Refine extractions using `.pbf` bookmark files to filter subtitle lines.
- Multiple audio composition options:
  - Original audio.
  - Text-to-Speech (TTS) synthesized audio.
  - Alternating combinations of original and synthesized audio.
- Fully customizable through user-defined settings, including:
  - Start and end time offsets for subtitles.
  - Minimum word count filtering for subtitle lines.
- Generates output logs and script files for detailed tracking of the process.
- Supports batch processing of multiple files in a single directory.

---

## Technologies Used
- **.NET Core** (Backend Logic)
- **Razor Pages** (Frontend for user interaction)
- **FFmpeg** (Audio extraction from video files)
- **Microsoft Cognitive Services Speech SDK** (Text-to-Speech synthesis)
- **HTML, JavaScript, and CSS** (Frontend design and interactivity)

---

## Getting Started

### Prerequisites
1. **FFmpeg**: Ensure `ffmpeg.exe` is available and its path is configured in the application settings.
2. **Microsoft Cognitive Services**:
   - Set up a Speech API key and region.
   - Update the application settings with these credentials.
3. A directory containing:
   - Video files (supported formats configured in settings).
   - Corresponding `.srt` subtitle files.
   - Optional `.pbf` bookmark files for refined audio extraction.

---

### How to Use
1. **Setup**:
   - Clone this repository.
   - Configure the necessary settings, such as the path to `ffmpeg.exe`, Speech API credentials, and other parameters in the app.
2. **Run the Application**:
   - Start the Razor Pages app.
   - Enter the directory containing your video files and corresponding `.srt` files.
   - Choose your desired output composition and other settings.
   - Click **Extract**.
3. **Output**:
   - The extracted audio files will be saved in the `AudioExtractor` directory under your user profile.

---

## Project Structure
- **Backend**:
  - `AudioExtractorService`: Core logic for audio extraction and processing.
  - `Models`: Contains data models for managing settings and inputs.
- **Frontend**:
  - Razor Pages for user input forms and interactivity.
- **Dependencies**:
  - `ffmpeg.exe`: Used for cutting audio streams from video files.
  - Speech SDK for TTS synthesis.

---

## Example Use Case
Imagine you have a directory containing several videos with subtitles and you need the dialogue for transcription or analysis. This app simplifies the process by:
1. Extracting only the dialogue sections from the videos.
2. Optionally synthesizing the dialogue into speech using TTS.
3. Producing an audio file ready for playback or further processing.

---

## License
This project is licensed under the [MIT License](LICENSE).
