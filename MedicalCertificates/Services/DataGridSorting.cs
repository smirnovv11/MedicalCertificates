using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace MedicalCertificates.Services
{
    class DataGridSorting
    {
        private static ListSortDirection direction;
        public static void HandleDataGridSorting(DataGrid dataGrid, DataGridSortingEventArgs e)
        {
            if (e.Column != null)
            {
                string sortBy = e.Column.SortMemberPath;

                if (direction == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }
                ListSortDirection newSortDirection = direction;

                e.Column.SortDirection = newSortDirection;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (dataGrid.ItemsSource != null)
                {
                    dataView.SortDescriptions.Clear();
                    dataView.SortDescriptions.Add(new SortDescription(sortBy, newSortDirection));
                }
            }
        }
    }
}
