﻿using IfcLibrary.Excel;
using IfcLibrary.Ifc;
using Microsoft.Win32;
using System;
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
        private ExcelReader excelReader;
        private IIfcAdapter ifcAdapter;

        public MainWindow()
        {
            InitializeComponent();
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            this.Title = $"Enginery IFC Automation v{version.Major}.{version.Minor}.{version.Build}";
            this.excelReader = new ExcelReader();
            this.ifcAdapter = new IfcAdapter();
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
                this.ProgressBar.Value = 0;
                this.ProgressText.Text = string.Empty;
                this.ifcAdapter.PatchProgressUpdated += IfcAdapter_PatchProgressUpdated;
                this.ProgressGroup.Visibility = Visibility.Visible;
                await PatchAsync(this.ExcelFileTextBox.Text, this.IFCFileTextBox.Text, this.OutputFileTextBox.Text);

                MessageBox.Show($"Fichero modificado guardado en {this.OutputFileTextBox.Text}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Ha ocurrido un error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.ProgressGroup.Visibility = Visibility.Collapsed;
                this.ifcAdapter.PatchProgressUpdated -= IfcAdapter_PatchProgressUpdated;
                this.PatchButton.IsEnabled = true;
            }
        }

        private void IfcAdapter_PatchProgressUpdated(object sender, PatchProgress e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ProgressBar.Value = e.PercentComplete;
                this.ProgressText.Text = e.Text;
            }));
        }

        private async Task PatchAsync(string excel, string inputIfc, string outputIfc)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.ProgressText.Text = "Leyendo excel";
                }));
                var automatedChanges = this.excelReader.GetChanges(excel);

                this.ifcAdapter.PatchFile(inputIfc, outputIfc, automatedChanges);
            });
        }
    }
}
