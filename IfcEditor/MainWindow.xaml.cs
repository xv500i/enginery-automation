using System.Windows;

namespace IfcEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            var licenseInfo = new LicenseInfo();
            licenseInfo.ShowDialog();
        }
    }
}
