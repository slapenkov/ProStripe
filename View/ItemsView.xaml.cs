using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using ProStripe.ViewModel;

namespace ProStripe.View
{
    /// <summary>
    /// Interaction logic for ItemsView.xaml
    /// </summary>
    public partial class ItemsView : UserControl
    {
        #region Private members
        private Items itemsViewModel;
        private string sortColumn = "Name";
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        #endregion
        
        #region Constructor
        public ItemsView()
        {
            InitializeComponent();
            itemsViewModel = new Items();
            this.DataContext = itemsViewModel;
            SetViewProperties();
        }
        #endregion

        #region View operations
        private void SetViewProperties()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(itemsViewModel.Children);
            view.GroupDescriptions.Add(new PropertyGroupDescription("IsDirectory"));

            view.Filter = new Predicate<object>(ItemFilter);

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("IsDirectory", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription(sortColumn, sortDirection));

            view.Refresh();
        }

        private bool ItemFilter(object item)
        {
            FileItem file = item as FileItem;
            if (file == null)
                return false;
            else
                return !file.IsHidden || file.IsRoot;
        }
        #endregion

        #region Events
        private void SortClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;
            string column = header.Tag as string;

            if (header == null)
                return;

            direction = ListSortDirection.Ascending;
            if (sortColumn == column &&
                sortDirection == ListSortDirection.Ascending)
                direction = ListSortDirection.Descending;   // toggle direction on same column
            sortColumn = column;
            sortDirection = direction;

            SetViewProperties();
        }

        private void Item_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            itemsViewModel.commandItemSelect.Execute(sender);
        }

        private void Item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                itemsViewModel.commandItemSelect.Execute(sender);
        }

        private void textRoot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ComboBox cb = sender as ComboBox;
                if (cb != null)
                {
                    BindingExpression be = cb.GetBindingExpression(ComboBox.TextProperty);
                    be.UpdateSource();
                }
            }
        }

        private void fileListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            SetViewProperties();
        }

        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = 0;
            foreach (object o in e.AddedItems)
            {
                FileItem f = o as FileItem;
                if (f == null)
                    continue;
                if (f.IsDirectory)
                    continue;
                f.IsSelected = true;
                selected++;
            }

            foreach (object o in e.RemovedItems)
            {
                FileItem f = o as FileItem;
                if (f == null)
                    continue;
                if (f.IsDirectory)
                    continue;
                f.IsSelected = false;
                selected--;
            }
            itemsViewModel.Selected += selected;
        }
        #endregion

    }
}
