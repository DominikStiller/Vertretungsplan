using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace DominikStiller.VertretungsplanUploader.UI
{
    public partial class Window : Form
    {
        string database;
        string s3UploadBucket, s3UploadKey;
        FileSystemWatcher watcher;
        AmazonS3Client s3;

        public Window()
        {
            InitializeComponent();

            var s3UploadSection = ConfigurationManager.GetSection("s3Upload") as NameValueCollection;
            s3UploadBucket = s3UploadSection["Bucket"];
            s3UploadKey = s3UploadSection["Key"];

            var awsCredentialsSection = ConfigurationManager.GetSection("awsCredentials") as NameValueCollection;
            var awsCredentials = new BasicAWSCredentials(awsCredentialsSection["AccessKey"], awsCredentialsSection["SecretKey"]);
            s3 = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.EUCentral1);

            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "TS-Internet.mdb";
            watcher.Changed += (source, e) =>
            {
                // Do not show messages if the upload was triggered automatically
                UploadToS3Silent();
            };

            database = Properties.Settings.Default.Database;
            if (database != "" && File.Exists(database))
                DatabasePathChanged();
        }

        private void Window_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
                Hide();
            }
            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon.Visible = false;
            }
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                var result = MessageBox.Show(this, "Wollen Sie das Programm wirklich beenden?\nWenn sie es minimieren werden Änderungen weiter im Hintergrund veröffentlicht.", "Programm beenden", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                e.Cancel = result == DialogResult.No;
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Dies ist ein Helfer-Programm, um den Online-Vertretungsplan zu aktualisieren. Es arbeitet selbstständig im Hintergrund und lädt die neue Version automatisch hoch, das Hochladen kann aber auch manuell ausgelöst werden.\n\nBei Fragen wenden Sie sich bitte an Dominik Stiller (domi.stiller@gmail.com).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chooseFileButton_Click(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                database = openFileDialog.FileName;
                Properties.Settings.Default.Database = database;
                Properties.Settings.Default.Save();
                DatabasePathChanged();
                UploadToS3Silent();
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                UploadToS3();
            }
            catch (IOException)
            {
                MessageBox.Show(this, "Es ist ein Dateifehler aufgetreten. Möglicherweise wird die Datei gerade von einem anderen Programm benutzt. Der Vertretungsplan wurde nicht veröffentlicht.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(this, "Der Vertretungsplan wurde erfolgreich veröffentlicht. Die Änderungen werden bald online angezeigt.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DatabasePathChanged()
        {
            // Update GUI
            pathTextBox.Text = database;
            lastChangeLabel.Text = new FileInfo(database).LastWriteTime.ToString();
            stateLabel.Text = "Überwache auf Änderungen...";
            uploadButton.Enabled = true;

            watcher.Path = Path.GetDirectoryName(database);
            watcher.EnableRaisingEvents = true;
        }

        private void UploadToS3Silent()
        {
            try
            {
                UploadToS3();
            }
            // Can occur if file is locked
            catch (IOException) { }
        }

        private void UploadToS3()
        {
            Invoke(new Action(() =>
            {
                stateLabel.Text = "Veröffentliche...";
                stateLabel.Update();
            }));

            try
            {
                s3.PutObject(new PutObjectRequest()
                {
                    BucketName = s3UploadBucket,
                    Key = s3UploadKey,
                    FilePath = database
                });
            }
            finally
            {
                Invoke(new Action(() =>
                {
                    stateLabel.Text = "Überwache auf Änderungen...";
                }));
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this, "Wollen Sie das Programm wirklich beenden? Änderungen werden dann nicht mehr veröffentlicht.", "Programm beenden", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
