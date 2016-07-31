using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using ProStripe.Model;

// ProStripe timecode striper
// View model: list of folders, files

// Copyright 2013, Bill Price


namespace ProStripe.ViewModel
{
    /// <summary>
    /// File system items
    /// </summary>
    public class Items: INotifyPropertyChanged
    {
        #region Private members
        private string root;
        private ObservableCollection<FileItem> children;
        private List<string> history;
        private int index = 0;
        private FileSystemWatcher watcher;
        #endregion

        #region Properties
        /// <summary> Path to parent folder
        /// </summary>
        public string Root
        {
            get { return root; }
            set 
            {   
                root = value;
                OnPropertyChanged("Root");
                IEnumerable<FileSystemInfo> files = FileSystem.GetChildren(value);
                IEnumerable<FileItem> items = 
                    from x in files select new FileItem(x);
                Children = new ObservableCollection<FileItem>(items);

                if (history[index] != value)
                {
                    if (canForward(value))
                        history.RemoveRange(index + 1, history.Count - index - 1);
                    index = history.Count;
                    history.Add(value);
                }
                bool canGo = value != "";
                try
                {
                    if (canGo)
                        watcher.Path = value;
                    watcher.EnableRaisingEvents = canGo;
                }
                catch { };
            }
        }

        /// <summary> Children of Root
        /// </summary>
        public ObservableCollection<FileItem> Children
        {
            get { return children; }
            set
            {   
                children = value;
                OnPropertyChanged("Children");
                // TODO: move SetViewProperties here from ItemsView.
            }
        }

        public int Selected { get; set; }
        public bool FilesChanged { get; set; }

        public ICommand commandUp { get; set; }
        public ICommand commandBack { get; set; }
        public ICommand commandForward { get; set; }
        public ICommand commandItemSelect { get; set; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var item = Children.FirstOrDefault(i => i.Name == e.Name);
            if (item == null)
                return;
            FilesChanged = true;
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            var item = Children.FirstOrDefault(i => i.Name == e.OldName);
            if (item == null)
                return;
            FilesChanged = true;
        }

        #endregion

        #region Commands
        private void doUp(object o)
        {
            DirectoryInfo dir = new DirectoryInfo(Root);
            if (dir.Parent == null)
                Root = "";
            else
                Root = dir.Parent.FullName;
        }
        private bool canUp(object o)
        {
            return Root != "";
        }

        private void doBack(object o)
        {
            Root = history[--index];
        }
        private bool canBack(object o)
        { 
            return index > 0; 
        }

        private void doForward(object o)
        {
            Root = history[++index];
        }
        private bool canForward(object o)
        { 
            return index + 1 < history.Count;
        }

        private void doItemSelect(ListViewItem selected)
        {
            FileItem item = selected.Content as FileItem;
            if (item.IsDirectory)
                Root = item.FullName;
            else
                Process.Start(item.FullName);
        }
        #endregion

        #region Constructor
        public Items()
        {
            watcher = new FileSystemWatcher();
            // watcher.SynchronizingObject = this;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);

            history = new List<string>();
            history.Add("");
            Root = "";
            commandUp = new RelayCommand<object>(doUp, canUp);
            commandBack = new RelayCommand<object>(doBack, canBack);
            commandForward = new RelayCommand<object>(doForward, canForward);
            commandItemSelect = new RelayCommand<ListViewItem>(doItemSelect);
        }
        #endregion
     }

    /// <summary>
    /// File system item
    /// </summary>
    public class FileItem
    {
        #region Private members
        FileSystemInfo f;
        FileAssociation a;
        #endregion

        #region Constructor
        public FileItem(FileSystemInfo info)
        {
            f = info;
            a = new FileAssociation(Name, IsDirectory);
        }
        #endregion

        #region Properties
        public string   FullName        { get { return f.FullName; } }
        public string   Name            { get { return f.Name; } }
        public string   Extension       { get { return f.Extension; } }
        public DateTime CreationTime    { get { return f.CreationTime; } }
        public DateTime LastAccessTime  { get { return f.LastAccessTime; } }
        public DateTime LastWriteTime   { get { return f.LastWriteTime; } }
        public FileAttributes Attributes{ get { return f.Attributes; } }
        public bool Exists      { get { return f.Exists; } }
        public bool IsDirectory { get { return HasFlag(FileAttributes.Directory); } }
        public bool IsArchive   { get { return HasFlag(FileAttributes.Archive); } }
        public bool IsHidden    { get { return HasFlag(FileAttributes.Hidden); } }
        public bool IsOffline   { get { return HasFlag(FileAttributes.Offline); } }
        public bool IsReadOnly  { get { return HasFlag(FileAttributes.ReadOnly); } }
        public bool IsSystem    { get { return HasFlag(FileAttributes.System); } }
        public bool IsRoot      { get { return Name == FullName; } }
        public bool IsSelected  { get; set; }
        public void Refresh() { f.Refresh(); }

        public string FileType { get { return a.FileType; } }
        public ImageSource Icon{ get { return a.Icon; } }

        private bool HasFlag(FileAttributes flag)
        {
            try
            {
                return (f.Attributes & flag) == flag;
            }
            catch { return false; }
        }
        #endregion
    }

}
