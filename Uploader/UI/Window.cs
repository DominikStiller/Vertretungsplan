using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace DominikStiller.VertretungsplanUploader.UI
{
    using UIStrings = Resources.UIStrings;

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
            watcher.Changed += (source, e) =>
            {
                Thread.Sleep(3000);
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
                var result = MessageBox.Show(this, UIStrings.CloseConfirmationDialog_Content, UIStrings.CloseConfirmationDialog_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                e.Cancel = result == DialogResult.No;
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, UIStrings.InfoDialog_Content, UIStrings.InfoDialog_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show(this, UIStrings.PublishSuccessDialog_Content, UIStrings.PublishSuccessDialog_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException)
            {
                MessageBox.Show(this, UIStrings.IOExceptionDialog_Content, UIStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DatabasePathChanged()
        {
            // Update GUI
            pathTextBox.Text = database;
            lastChangeLabel.Text = new FileInfo(database).LastWriteTime.ToString();
            statusLabel.Text = UIStrings.Status_Watching;
            uploadButton.Enabled = true;

            watcher.Path = Path.GetDirectoryName(database);
            watcher.Filter = Path.GetFileName(database);
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
                statusLabel.Text = UIStrings.Status_Publishing;
                statusLabel.Update();
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
                    statusLabel.Text = UIStrings.Status_Watching;
                }));
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            // Restore window on click on notification area icon
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this, UIStrings.ExitConfirmationDialog_Content, UIStrings.ExitConfirmationDialog_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
