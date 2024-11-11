using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VCH;
using PBH;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;

namespace DIPForm
{
    public partial class DIPForm : Form
    {
        //Video Camera
        private VideoCamHandler videoCamHandler;
        private Boolean isCameroOn = false;
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice captureDevice;
        //Images
        private PictureBoxHandler pictureBoxHandler;
        private ImageFilters imageFilters;
        Bitmap inputImage, outputImage, backgroundImage, histogram;
        //Filters
        public enum FilterType
        {
            Copy,
            Grayscale,
            Invert,
            Sepia,
            Subtract
        }

        public DIPForm()
        {
            InitializeComponent();
            videoCamHandler = new VideoCamHandler();
            pictureBoxHandler = new PictureBoxHandler();
            imageFilters = new ImageFilters();
            this.Size = new System.Drawing.Size(800, 800);
            this.Text = "Digital Image Processor";
        }
        //
        //
        //
        private void DIPForm_Load(object sender, EventArgs e)
        {

        }
        //
        //
        // TOGGLE CAMERA ON
        private void turnOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isCameroOn) return;

            pictureBoxHandler.disposeImages(pictureBox1, inputImage);
            pictureBoxHandler.disposeImages(pictureBox2, outputImage);
            pictureBoxHandler.disposeImages(pictureBox3, histogram);
            isCameroOn = true;

            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (filterInfoCollection.Count == 0)
            {
                MessageBox.Show("No video devices found.");
                isCameroOn = false;
                videoCamHandler.toggleCam(isCameroOn, button1, button2);
                return;
            }
            videoCamHandler.toggleCam(isCameroOn, button1, button2);
            captureDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
            captureDevice.NewFrame += new NewFrameEventHandler(captureDevice_NewFrame);
            captureDevice.Start();
        }
        //
        //
        // DISPLAY FRAME TO pictureBox1
        private void captureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = frame;
        }
        //
        //
        // TOGGLE CAMERA OFF
        private void turnOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isCameroOn) return;

            pictureBoxHandler.disposeImages(pictureBox1, inputImage);
            pictureBoxHandler.disposeImages(pictureBox2, outputImage);
            pictureBoxHandler.disposeImages(pictureBox3, histogram);
            if (captureDevice != null && captureDevice.IsRunning)
            {
                captureDevice.SignalToStop();
                captureDevice.WaitForStop();
            }
            isCameroOn = false;
            videoCamHandler.toggleCam(isCameroOn, button1, button2);

        }
        //
        //
        // OPEN FILE DIALOG FOR INPUT IMAGE
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
        //
        //
        // LOAD INPUT IMAGE
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            inputImage = pictureBoxHandler.loadImage(pictureBox1, inputImage, openFileDialog1);
        }
        //
        //
        // OPEN FILE DIALOG FOR BACKGROUND IMAGE
        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }
        //
        //
        // LOAD BACKGROUND IMAGE
        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            backgroundImage = pictureBoxHandler.loadImage(pictureBox4, backgroundImage, openFileDialog2);
        }
        //
        //
        // OPEN SAVE FILE DIALOG FOR outputImage
        private void button2_Click(object sender, EventArgs e)
        {
            if (outputImage != null)
            {
                saveFileDialog1.Title = "Save Image As";
                saveFileDialog1.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                saveFileDialog1.DefaultExt = "png";
                saveFileDialog1.FileName = "output_image";

                saveFileDialog1.ShowDialog(this);
            }
            else
            {
                MessageBox.Show("There is no image to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //
        //
        // SAVE IMAGE ON OK
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    outputImage.Save(saveFileDialog1.FileName);
                    MessageBox.Show("Image saved successfully!", "Save Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the image: " + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //
        //
        // FILTER CONTROLLER
        private void ApplyFilter(FilterType filterType)
        {
            // Dispose of previous images to free memory
            pictureBoxHandler.disposeImages(pictureBox2, outputImage);
            pictureBoxHandler.disposeImages(pictureBox3, histogram);

            // Only apply filters if the camera is off
            if (!isCameroOn && inputImage != null)
            {
                switch (filterType)
                {
                    case FilterType.Copy:
                        outputImage = imageFilters.copyImage(inputImage);
                        break;
                    case FilterType.Grayscale:
                        outputImage = imageFilters.grayscale(inputImage);
                        break;
                    case FilterType.Invert:
                        outputImage = imageFilters.invert(inputImage);
                        break;
                    case FilterType.Sepia:
                        outputImage = imageFilters.sepia(inputImage);
                        break;
                    case FilterType.Subtract:
                        outputImage = imageFilters.subtract(inputImage, backgroundImage);
                        break;
                }
                pictureBox2.Image = outputImage;
                histogram = imageFilters.histogram(outputImage);
                pictureBox3.Image = histogram;
            }
        }
        //
        //
        // FILTER: COPY IMAGE
        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(FilterType.Copy);
        }
        //
        //
        // FILTER: GRAYSCALE
        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(FilterType.Grayscale);
        }
        //
        //
        // FILTER: INVERT IMAGE
        private void invertColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(FilterType.Invert);
        }
        //
        //
        // FILTER: SEPIA
        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(FilterType.Sepia);
        }
        //
        //
        // FILTER: SUBTRACT IMAGE
        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundImage != null)
            {
                ApplyFilter(FilterType.Subtract);
            }
        }
    }
}
