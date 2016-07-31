using System.Windows;
using ProStripe.ViewModel;


namespace ProStripe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            parent = new MainWindowVM();
            this.DataContext = parent;
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }
        #endregion

        MainWindowVM parent;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            parent.Source = FileSource.DataContext as Items;
            parent.Destination = FileDestination.DataContext as Items;
        }
    }
}
