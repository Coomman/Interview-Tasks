using System;
using System.Windows;

using Technostar.ViewModels;

namespace Technostar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel();
        }

        private void ComboBox_OnSelectionChanged(object? sender, EventArgs e)
        {
            var vm = (ViewModel) DataContext;

            vm.OnSelectionChanged();
        }
    }
}
