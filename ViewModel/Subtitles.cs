using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProStripe.ViewModel
{
    public class Subtitles: StreamWriter
    {
        private string last;
        private DateTime start;
        private int n;

        public Subtitles(string path): base(path)
        {
            last = "";
            n = 0;
        }

        private bool sameTitle(string a, string b)
        {
            if (a.Length != b.Length)
                return false;
            int afrom, ato;
            int bfrom, bto;

            afrom = a.IndexOf('A');
            ato = a.IndexOf('*');
            bfrom = b.IndexOf('A');
            bto = b.IndexOf('*');

            try
            {
                if (a.Substring(afrom, ato - afrom) == b.Substring(bfrom, bto - bfrom))
                    return true;
            }
            catch { };

            return false;
        }

        // SubRip .srt format
        public void write(DateTime when, string title)
        {
            if (sameTitle(title, last))
                return;

            if (last != "")
            {
                string times = string.Format("{0:HH:mm:ss,fff} --> {1:HH:mm:ss,fff}", start, when.AddMilliseconds(-1));
                ++n;
                WriteLine(n);
                WriteLine(times);
                WriteLine(last);
                WriteLine("");
            }
            last = title;
            start = when;
        }

        public void close(DateTime when)
        {
            write(when, "");
            Close();
            Dispose();
        }
    }
}