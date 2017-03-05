using EContract.Dssp.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_demo
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

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".pdf";
            openFileDialog.Filter = "PDF (*.pdf)|*.pdf";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath.Text = openFileDialog.FileName;
            }
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = store.Certificates;
            X509Certificate2Collection fcollection = collection.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.NonRepudiation, true);
            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Sign Certificate Select", "Select a certificate to sign with", X509SelectionFlag.SingleSelection);

            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.UT.Name = Properties.Settings.Default.user;
            dsspClient.Application.UT.Password = Properties.Settings.Default.pwd;
            dsspClient.Signer = scollection.Cast<X509Certificate2>().AsQueryable().FirstOrDefault();

            Dssp2StepSession dsspSession;
            using (Stream input = File.OpenRead(FilePath.Text)) {
                var inDoc = new Document()
                {
                    MimeType = "application/pdf",
                    Content = input
                };
                dsspSession = dsspClient.UploadDocumentFor2Step(inDoc);
            }

            dsspSession.Sign();

            var outDoc = dsspClient.DownloadDocument(dsspSession);

            using (Stream output = File.Create(FilePath.Text))
            {
                CopyStream(outDoc.Content, output);
            }

        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
