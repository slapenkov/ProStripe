using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace ProStripe.ViewModel
{
    public class Striper
    {
        public string Timecode { get; set; }
        public string Begin { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public bool HasAudio { get; set; }

        public FileInfo Source { get; set; }
        public FileInfo Destination { get; set; }
        public FileInfo AudioTrack { get; set; }
        public FileInfo SubtitleTrack { get; set; }
        public int ExitCode { get; set; }

        public Striper() 
        {
        }

        public void Convert(string destination, string extension)
        {
            if (!Extract(true))
                return;
            string file = Path.GetFileNameWithoutExtension(Source.Name) + extension;
            string full = Path.Combine(destination, file);
            Destination = MakeUnique(full);
            string command =
                string.Format("-i \"{0}\" -timecode {1} -acodec copy -vcodec copy \"{2}\"", Source.FullName, Timecode, Destination.FullName);
            if (ProcessCommand(command))
            {
                AudioTrack.Delete();
                if (SubtitleTrack != null)
                    SubtitleTrack.Delete();
            }
        }

        private bool Extract(bool complete = false)
        {
            ExtractAudio(complete);
            if (AudioTrack != null)
                DecodeAudio();

            return HasAudio;
        }

        private void ExtractAudio(bool complete)
        {
            string audioPath = tempPath(Source.Name, ".wav");
            FileInfo audio = new FileInfo(audioPath);
            if (audio.Exists)
                audio.Delete();
            string command;
            command = String.Format("-i \"{0}\" -acodec pcm_u8 -sample_fmt u8 -ar 22100 \"{1}\"", Source.FullName, audio.FullName);
            if (ProcessCommand(command))
            {
                AudioTrack = audio;
                HasAudio = true;
            }
        }

        private void DecodeAudio()
        { 
            List<Timecode> timecodes = new List<Timecode>();

            using (WavReader wav = new WavReader(AudioTrack.FullName))
            {
                Decode decoder = new Decode((short)wav.ByteRate);
                byte[] sound;
                sound = wav.ReadChannel(16000, 0);
                while (sound.Length > 0)
                {
                    decoder.write(sound);
                    while (decoder.available() > 0)
                        timecodes.Add(decoder.read());
                    // TODO: decode date, GPS, other sensor, write subtitle file
                    // TODO: report progress
                    sound = wav.ReadChannel(16000, 0);
                }
            }

            if (timecodes.Count > 0)
                base.Timecode = tc(timecodes[0]);
            // TODO: round timecode to nearest frame
            // TODO: grab the date if present
        }


        private bool ProcessCommand(string command)
        {
            Process p = new Process();
            p.StartInfo.FileName = "FFmbc.exe";
            p.StartInfo.Arguments = command;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += new DataReceivedEventHandler(FFmbc_Output);
            p.Exited += new EventHandler(FFmbc_HasExited);
            p.Start();
            //p.WaitForExit();

            //ExitCode = p.ExitCode;
            //p.Close();

            // TODO: analyze output
            // TODO: run it as a BackgroundWorker
            //return ExitCode == 0;
            return true;
        }

        private void FFmbc_Output(object sender, DataReceivedEventArgs errLine)
        {
            // TODO: report progress
        }

        private void FFmbc_HasExited(object sender, System.EventArgs e)
        { 
            // TODO: report completion
        }
                    
        #region Helpers
        private string tempPath(string file, string extension)
        {
            string temp = Path.GetTempPath();
            string name = Path.GetFileNameWithoutExtension(file) + extension;
            return Path.Combine(temp, name);
        }

        private FileInfo MakeUnique(string fullname)
        {
                        
            string path = Path.GetDirectoryName(fullname);
            string name = Path.GetFileNameWithoutExtension(fullname);
            string extension = Path.GetExtension(fullname);

            for (int i = 1; i < 1000; i++) 
            {
                if (!File.Exists(fullname))
                    break;
                fullname = Path.Combine(path, name + "(" + i + ")" + extension);
            }
            return new FileInfo(fullname);
        }

        private string tc(Timecode t)
        {
            string result =
                t.hour.ToString("D2") + ":" +
                t.minute.ToString("D2") + ":" +
                t.second.ToString("D2") + (t.drop ? ";" : ":") +
                t.frame.ToString("D2");
            return result;
        }
        #endregion
    }

    public class VideoAttributes
    {
        public VideoAttributes(FileInfo source)
        {
            Source = source;
        }
        public VideoAttributes(string file)
        {
            FileInfo f = new FileInfo(file);
            Source = f;
        }

        #region Properties
        public string Timecode { get; set; }
        public string Begin { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public bool   HasAudio { get; set; }
        public bool HasSubtitles { get; set; }
        #endregion
    }

    public class WavReader: BinaryReader
    {
        // WAV file header
        public byte[]   RIFF { get; private set; }
        public long     ChunkSize { get; private set; }
        public byte[]   WAVE { get; private set; }
        public byte[]   fmt { get; private set; }
        public long     FmtSize { get; private set; }
        public short    AudioFormat { get; private set; }
        public short    Channels { get; private set; }
        public long     SampleRate { get; private set; }
        public long     ByteRate { get; private set; }
        public short    BlockAlign { get; private set; }
        public short    BitsPerSample { get; private set; }
        public byte[]   data { get; private set; }
        public long     DataSize { get; private set; }
        // followed by DataSize audio samples

        public WavReader(string file) :
            base (File.OpenRead(file))
        {
            ReadHeader();
        }

        public byte[] ReadChannel(int count, short channel)
        {
            byte[] sound;
            sound = ReadBytes(count * Channels);
            if (Channels == 1)
                return sound;
            byte[] result = new byte[count];
            for (int c = 0, i = 0; c < sound.Length; i++, c += Channels)
                 result[i] = sound[c + channel];
            return result;
        }

        private void ReadHeader()
        {
            RIFF = ReadBytes(4);
            ChunkSize = ReadInt32();
            WAVE = ReadBytes(4);
            fmt = ReadBytes(4);
            FmtSize  = ReadInt32();
            AudioFormat = ReadInt16();
            Channels = ReadInt16();
            SampleRate = ReadInt32();
            ByteRate = ReadInt32();
            BlockAlign = ReadInt16();
            BitsPerSample = ReadInt16();
            data = ReadBytes(4);
            DataSize = ReadInt32();
        }
    }
}
