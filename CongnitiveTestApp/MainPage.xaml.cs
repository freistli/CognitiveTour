using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CongnitiveTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("f8564c504dff432688c85fe6abcc520e");
        public MainPage()
        {
            this.InitializeComponent();

        }
        private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
                }
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            // Users expect to have a filtered view of their folders 
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            // Open the picker for the user to pick a file
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap 
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.DecodePixelWidth = 600; //match the target Image.Width, not shown
                    await bitmapImage.SetSourceAsync(fileStream);
                    FacePhoto.Source = bitmapImage;
                }
            }

            Status.Text = "Detecting...";
            
           
            
                var property = await file.Properties.GetImagePropertiesAsync();
                //Create bitmap from image size
                var writeableBmp = BitmapFactory.New((int)property.Width, (int)property.Height);
                using (writeableBmp.GetBitmapContext())
                {
                    //Load bitmap from image file
                    using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        writeableBmp = await BitmapFactory.New(1, 1).FromStream(fileStream, BitmapPixelFormat.Bgra8);
                    }
                }

                //find face that DetectAsync Face API
                using (var imageFileStream = await file.OpenStreamForReadAsync())
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    if (faces == null) return;

                Status.Text = String.Format("Detection Finished. {0} face(s) detected", faces.Length);

                //display rect
                foreach (var face in faces)
                    {
                        writeableBmp.DrawRectangle(face.FaceRectangle.Left, face.FaceRectangle.Top,
                        face.FaceRectangle.Left + face.FaceRectangle.Width,
                        face.FaceRectangle.Top + face.FaceRectangle.Height, Colors.Blue);
                    }
             

                FacePhoto.Source = writeableBmp;
            }
        }
    }
}
