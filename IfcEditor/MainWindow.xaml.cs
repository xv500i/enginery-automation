﻿using IfcLibrary.Excel;
using IfcLibrary.Ifc;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
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
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            this.Title = $"Enginery IFC Automation v{version.Major}.{version.Minor}.{version.MinorRevision}";
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            var licenseInfo = new LicenseInfo();
            licenseInfo.ShowDialog();
        }

        private void IFCButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Ficheros IFC (*.ifc)|*.ifc",
                Multiselect = false,
                ReadOnlyChecked = true,
                Title = "Selecciona el fichero IFC original",
            };
            if (dialog.ShowDialog() == true)
            {
                this.IFCFileTextBox.Text = dialog.FileName;
            }
            if (string.IsNullOrEmpty(this.OutputFileTextBox.Text))
            {
                var fileInfo = new FileInfo(dialog.FileName);
                var name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                this.OutputFileTextBox.Text = Path.Combine(fileInfo.DirectoryName, $"{name}_modificado.ifc");
            }
        }

        private void ExcelButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Ficheros excel (*.xlsx)|*.xlsx",
                Multiselect = false,
                ReadOnlyChecked = true,
                Title = "Selecciona el excel con los cambios",
            };
            if (dialog.ShowDialog() == true)
            {
                this.ExcelFileTextBox.Text = dialog.FileName;
            }
        }

        private void OutputButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Ficheros IFC (*.ifc)|*.ifc",
                AddExtension = true,
                Title = "Selecciona donde guardar el fichero IFC modificado",
            };
            if (dialog.ShowDialog() == true)
            {
                this.OutputFileTextBox.Text = dialog.FileName;
            }
        }

        private async void PatchButtonClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(this.IFCFileTextBox.Text))
            {
                MessageBox.Show("No se puede encontrar el fichero ifc original", "Alerta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(this.ExcelFileTextBox.Text))
            {
                MessageBox.Show("No se puede encontrar el fichero excel", "Alerta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.PatchButton.IsEnabled = false;

            try
            {
                await PatchAsync(this.ExcelFileTextBox.Text, this.IFCFileTextBox.Text, this.OutputFileTextBox.Text);

                MessageBox.Show($"Fichero modificado guardado en {this.OutputFileTextBox.Text}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Ha ocurrido un error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.PatchButton.IsEnabled = true;
            }
        }

        private async Task PatchAsync(string excel, string inputIfc, string outputIfc)
        {
            await Task.Run(() =>
            {
                var excelReader = new ExcelReader();
                var automatedChanges = excelReader.GetChanges(excel);

                var ifcAdapter = new IfcAdapter();
                ifcAdapter.PatchFile(inputIfc, outputIfc, automatedChanges);
            });
        }
    }
}
