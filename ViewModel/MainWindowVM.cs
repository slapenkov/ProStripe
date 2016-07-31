using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using ProStripe.Model;

namespace ProStripe.ViewModel
{
    public class MainWindowVM: INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            commandGo = new RelayCommand<object>(doGo, canGo);
            commandCancel = new RelayCommand<object>(doCancel, canCancel);
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(workerStripeEm);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            checkDrives = new DispatcherTimer();
            checkDrives.Interval = new TimeSpan(0, 0, 2);
            checkDrives.Tick += new EventHandler(checkDrives_Tick);
            checkDrives.Start();
        }

        void checkDrives_Tick(object sender, EventArgs e)
        {
            if (Source.FilesChanged)
            {
                Source.FilesChanged = false;
                Source.Root = Source.Root;
            }
            if (Destination.FilesChanged)
            {
                Destination.FilesChanged = false;
                Destination.Root = Destination.Root;
            }
            if (Source.Root != "" && Destination.Root != "")
                return;
            if (!FileSystem.DrivesChanged())
                return;
            if (Source.Root == "")
                Source.Root = Source.Root;
            if (Destination.Root == "")
                Destination.Root = Destination.Root;
        }

 
        private string status;
        static DispatcherTimer checkDrives;
 
        public Items Source { get; set; }
        public Items Destination { get; set; }
        public ICommand commandGo { get; set; }
        public ICommand commandCancel { get; set; }
        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged("Status"); }
        }

        private BackgroundWorker worker;

        public void doGo(object o) 
        {
            string destination = Destination.Root;
            List<object> p = new List<object>();
            p.Add(Source.Children);
            p.Add(Destination.Root);
            Status = "Beginning striping . . .";

            worker.RunWorkerAsync(p);
        }

        public bool canGo(object o)
        {
            if (!worker.IsBusy)
                ShowStatus();
            return Destination.Root != "" && Source.Selected > 0 && !worker.IsBusy;
        }

        private void ShowStatus()
        {
            string status = "Select source files";
            if (Source.Selected == 1)
                status = "One file selected";
            if (Source.Selected > 1)
                status = string.Format("{0} files selected", Source.Selected);
            if (Destination.Root == "")
                status += ": select a destination folder.";
            else
                status += " to go to " + Destination.Root + ".  Click Stripe to begin.";
            Status = status;
        }

        public void doCancel(object o)
        { 
        }

        public bool canCancel(object o)
        {
            return worker.IsBusy;
        }

        private void workerStripeEm(object o, DoWorkEventArgs e)
        {
            BackgroundWorker w = o as BackgroundWorker;
            List<object> p = e.Argument as List<object>;
            ObservableCollection<FileItem> files = p[0] as ObservableCollection<FileItem>;
            string destination = p[1] as string;
            Striper striper = new Striper();
            striper.StripeEm(files, destination, w);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string report = e.UserState as string;
            Status = report;
            Destination.Root = Destination.Root;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Destination.Root = Destination.Root;
            Status = "Completed";
            OnPropertyChanged("commandCancel");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
