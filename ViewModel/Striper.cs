using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace ProStripe.ViewModel
{
    public class Striper
    {
        public string Timecode { get; set; }
        public string Date { get; set; }

        public FileInfo Source { get; set; }
        public FileInfo Destination { get; set; }
        public FileInfo AudioTrack { get; set; }
        public FileInfo SubtitleTrack { get; set; }
        public int ExitCode { get; set; }
        public BackgroundWorker worker;

        public Striper() 
        {
        }

        public void StripeEm(ObservableCollection<FileItem> files, string destination, BackgroundWorker w)
        {
            worker = w;
            Destination = new FileInfo(destination);
            foreach (FileItem f in files)
            {
                if (f.IsSelected)
                {
                    Source = new FileInfo(f.FullName);
                    if (Extract())
                        Convert(destination, ".mov");
                    else
                        Copy(destination);
                }
            }
        }

        public void Report(string format, string source, string destination, string timecode = "")
        {
            string report = string.Format(format, Path.GetFileName(source), Path.GetFileName(destination), timecode);
            worker.ReportProgress(0, report);
        }

        private void Copy(string destination)
        {
            Report("Copying {0} to {1}", Source.FullName, destination);
            Destination = new FileInfo(Path.Combine(destination, Source.Name));
            if (Source.FullName == Destination.FullName)
                return;
            Source.CopyTo(Destination.FullName);
        }

        private void Convert(string destination, string extension)
        {
            string file = Path.GetFileNameWithoutExtension(Source.Name) + extension;
            string full = Path.Combine(destination, file);
            Destination = MakeUnique(full);
            Report("Striping {0} to {1} with timecode {2}", Source.FullName, Destination.FullName, Timecode);
            string command =
                // string.Format("-i \"{0}\" -timecode {1} -acodec copy -vcodec copy \"{2}\"", Source.FullName, Timecode, Destination.FullName);
                string.Format("-i \"{0}\" -timecode {1} -acodec copy -vcodec dnxhd -b 45M \"{2}\"", Source.FullName, Timecode, Destination.FullName);
            if (ProcessCommand(command))
            {
                AudioTrack.Delete();
            }
        }

        private bool Extract()
        {
            Timecode = "";
            ExtractAudio();
            if (AudioTrack != null)
                DecodeAudio();

            return Timecode != "";
        }

        private void ExtractAudio()
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
            }
        }

        private void DecodeAudio()
        {
            Timecode = "";

            string file = Path.GetFileNameWithoutExtension(Source.Name) + ".csv";
            string full = Path.Combine(Destination.FullName, file);
            //string subtitlePath = tempPath(Source.Name, ".srt");
            SubtitleTrack = new FileInfo(full);
            if (SubtitleTrack.Exists)
                SubtitleTrack.Delete();

            using (WavReader wav = new WavReader(AudioTrack.FullName))
            {
                using (Subtitles subtitles = new Subtitles(SubtitleTrack.FullName))
                {
                    Decode decoder = new Decode((short)wav.ByteRate);
                    NMEA gprmc = new NMEA();
                    DataProcessor processor = new DataProcessor();
                    byte[] sound;
                    subtitles.WriteLine("# RaceRender Data");
                    subtitles.WriteLine("# Created by ProSync");
                    subtitles.WriteLine(DataProcessor.csvHead);
                    sound = wav.ReadChannel(22100, 1); //0 is left channel, 1 is right
                    while (sound.Length > 0)
                    {
                        decoder.write(sound);
                        while (decoder.available() > 0)
                        {
                            Timecode t = decoder.read();
                            if (gprmc.Decode(t))
                            {
                                string s = gprmc.csv;
                                //subtitles.WriteLine(s);
                                //add data point to processor for collection and smoothing
                                processor.AddData(gprmc);

                            }
                            if (Timecode == "" && t.user8 == 0)
                            {
                                Timecode = tc(t);
                                Date = date(t);
                            }
                        }
                        // TODO: report progress
                        sound = wav.ReadChannel(22100, 1); //0 is left channel, 1 is right
                    }
                    processor.Smooth();
                    processor.WriteData(subtitles);
                    //subtitles.close(gprmc.timestamp);
                }
            }
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
            //p.Exited += new EventHandler(FFmbc_HasExited);
            p.Start();
            p.WaitForExit();

            ExitCode = p.ExitCode;
            p.Close();

            // TODO: analyze output
            return ExitCode == 0;
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

        private string date(Timecode t)
        {
            string result;
            result = string.Format("{0:00}/{1:00}/{2:00}", t.month, t.day, t.year);
            return result;
        }
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
            byte[] result = new byte[sound.Length];
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
