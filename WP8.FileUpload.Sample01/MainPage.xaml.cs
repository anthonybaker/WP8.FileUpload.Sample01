/// Author:         Anthony Baker
/// Date:           16/06/2013
/// Description:    Uses the PhotoChooserTask to allow the user to select
///                 an existing photo and upload it to a web service
///                 using a multipart form via HTTP post.

using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace WP8.FileUpload.Sample01
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region fields

        // Image stream variables
        private MemoryStream photoStream;
        private string fileName;

        // PhotoChooserTask definition
        PhotoChooserTask photoChooserTask;

        #endregion

        #region constructor

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // initializes the PhotoChooserTask
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(OnPhotoChooserTaskCompleted);
        }

        #endregion

        #region event handlers

        // Launches the photo chooser. 
        private void OnChoosePicture(object sender, System.Windows.Input.GestureEventArgs e)
        {
            photoChooserTask.Show();
        }

        // Called when an existing photo is chosen with the photo chooser.
        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e)
        {
            // Hide text messages
            txtError.Visibility = Visibility.Collapsed;
            txtMessage.Visibility = Visibility.Collapsed;

            // Make sure the PhotoChooserTask is resurning OK
            if (e.TaskResult == TaskResult.OK)
            {
                // initialize the result photo stream
                photoStream = new MemoryStream();

                // Save the stream result (copying the resulting stream)
                e.ChosenPhoto.CopyTo(photoStream);

                // save the original file name
                fileName = e.OriginalFileName;

                // display the chosen picture
                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.SetSource(photoStream);
                imgSelectedImage.Source = bitmapImage;

                // enable the upload button
                btnUpload.IsEnabled = true;
            }
            else
            {
                // if result is not ok, make sure user can't upload
                btnUpload.IsEnabled = false;
            }
        }

        // calls the UploadFile method
        private void OnUpload(object sender, System.Windows.Input.GestureEventArgs e)
        {
            UploadFile();
        }

        #endregion

        #region private methods

        // uploads the file
        private async void UploadFile()
        {
            try
            {
                // Make sure there is a picture selected
                if (photoStream != null)
                {
                    // initialize the client
                    // need to make sure the server accepts network IP-based requests
                    // ensure correct IP and correct port address
                    var fileUploadUrl = @"http://<yourIPaddress>:<yourport>/fileupload";
                    var client = new HttpClient();

                    // Reset the photoStream position
                    // If you don't reset the position, the content lenght sent will be 0
                    photoStream.Position = 0;

                    // This is the postdata
                    MultipartFormDataContent content = new MultipartFormDataContent();
                    content.Add(new StreamContent(photoStream), "file", fileName);

                    // upload the file sending the form info and ensure a result.
                    // it will throw an exception if the service doesn't return a valid successful status code
                    await client.PostAsync(fileUploadUrl, content)
                        .ContinueWith((postTask) =>
                        {
                            postTask.Result.EnsureSuccessStatusCode();
                        });
                }

                // Disable the Upload button
                btnUpload.IsEnabled = false;

                // reset the image control
                imgSelectedImage.Source = null;

                // Display the Uploaded message
                txtMessage.Visibility = Visibility.Visible;
            }
            catch
            {
                // Display the Uploaded message
                txtError.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}