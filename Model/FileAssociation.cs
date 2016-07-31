using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;

//  ProStripe timecode striper
//  Get associated file type and icon
//
//  Copyright 2013, Bill Price

namespace ProStripe.Model
{
    public class FileAssociation
    {
        public ImageSource Icon { get; set; }
        public string FileType { get; set; }

        static ImageSource DefaultIcon = null;
        static string DefaultFileType = "";

        public FileAssociation(string path, bool isDirectory)
        {
            if (DefaultIcon == null)
            {
                GetFileInfo("xxxxx", false);
                DefaultIcon = Icon;
                DefaultFileType = FileType;
            }
            Icon = DefaultIcon;
            FileType = DefaultFileType;
            GetFileInfo(path, isDirectory); 
        }

        private void GetFileInfo(string path, bool isDirectory)
        {
            SHFILEINFO info = new SHFILEINFO();
            uint attributes = isDirectory ? 
                Shell32.FILE_ATTRIBUTE_DIRECTORY : 
                Shell32.FILE_ATTRIBUTE_FILE;
            uint flags = 
                Shell32.SHFGI_TYPENAME | 
                Shell32.SHGFI_ICON | 
                Shell32.SHGFI_SMALLICON | 
                Shell32.SHGFI_USEFILEATTRIBUTES;
            IntPtr result =
                Shell32.SHGetFileInfo(path, attributes, ref info, (uint)Marshal.SizeOf(info), flags);
            IntPtr hIcon = info.hIcon;
            try
            {
                ImageSource img = Imaging.CreateBitmapSourceFromHIcon(
                            hIcon,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                Icon = img;
                FileType = info.szTypeName;
            }
            finally { Shell32.DestroyIcon(hIcon); }
        }
    }

    #region Shell32

    public static class Shell32
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHFGI_TYPENAME = 0x400;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint FILE_ATTRIBUTE_FILE = 0x00000100;
    }



    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

}

#endregion
