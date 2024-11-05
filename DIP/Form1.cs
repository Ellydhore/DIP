using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Additional Libs
using HNUDIP;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;

namespace DIP
{
    public partial class Form1 : Form
    {
        Bitmap loaded, loaded_bg, processed_part1, processed_part2;
        private FilterInfoCollection videoDevices; // List of all available video devices
        private VideoCaptureDevice videoSource; // The selected video device (camera)
        int part = 1;
        public Form1()
        {
            InitializeComponent();
            button1.Enabled = false;
            pictureBox3.Visible = false;
            subtractToolStripMenuItem.Enabled = false;
            loadBackgroundToolStripMenuItem.Enabled=false;
        }

        private void cameraOnToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cameraOffToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dIPToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

// >> OPEN FILE LOGIC >>
        private void openFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void loadBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            if(part == 2)
            {
                loaded_bg = new Bitmap(openFileDialog2.FileName);
                pictureBox2.Image = loaded_bg;
            }
        }

        // >> SAVE FILE LOGIC >>
        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            if (processed_part1 == null)
            {
                MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            saveFileDialog1.ShowDialog(this);
            
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if(part == 1)
            {
                if (processed_part1 == null)
                {
                    MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string fileName = saveFileDialog1.FileName;

                if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".jpg";
                }

                processed_part1.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            } else if(part == 2)
            {
                if (processed_part2 == null)
                {
                    MessageBox.Show("There is no image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string fileName = saveFileDialog1.FileName;

                if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".jpg";
                }

                processed_part2.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }


// >> DIP LOGIC >>
        private void pixelCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed_part1 = ImageProcess.CopyImage(loaded);
                pictureBox2.Image = processed_part1;
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed_part1 = ImageProcess.Inverted(loaded);
                pictureBox2.Image = processed_part1;
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed_part1 = ImageProcess.Histogram(loaded);
                pictureBox2.Image = processed_part1;
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed_part1 = ImageProcess.Sepia(loaded);
                pictureBox2.Image = processed_part1;
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (part == 2)
            {
                int greenThreshold = 100; 
                processed_part2 = ImageProcess.Subtract(loaded, loaded_bg, greenThreshold);

                pictureBox3.Image = processed_part2;
            }
        }



        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                processed_part1 = ImageProcess.Greyscale(loaded);
                pictureBox2.Image = processed_part1;
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void turnOnCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice); // Get all video devices

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No camera found.");
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString); // Use the first available camera
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame); // Attach the NewFrame event
            videoSource.Start(); // Start the camera
        }

        private void turnOffCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop(); // Signal the camera to stop
                videoSource.WaitForStop(); // Wait for it to stop completely
                pictureBox1.Image = null;
            }
        }

        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone(); // Clone the frame into a bitmap
            pictureBox1.Image = bitmap; // Display the frame in the PictureBox
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }

            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // >> SWITCH PARTS
        private void button1_Click(object sender, EventArgs e)
        {
            if (part != 1)
            {
                part = 1;
                button1.Enabled = false;
                button2.Enabled = true;
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                pictureBox3.Image = null;
                pictureBox3.Visible = false;
                subtractToolStripMenuItem.Enabled = false;
                label1.Text = "Part 1";
                pixelCopyToolStripMenuItem.Enabled = true;
                colorInversionToolStripMenuItem.Enabled = true;
                histogramToolStripMenuItem.Enabled = true;
                sepiaToolStripMenuItem.Enabled = true;
                greyscaleToolStripMenuItem.Enabled = true;
                loadBackgroundToolStripMenuItem.Enabled = false;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (part != 2)
            {
                part = 2; 
                button2.Enabled = false;
                button1.Enabled = true;
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                pictureBox3.Image = null;
                pictureBox3.Visible = true;
                subtractToolStripMenuItem.Enabled = true;
                label1.Text = "Part 2";
                pixelCopyToolStripMenuItem.Enabled = false;
                colorInversionToolStripMenuItem.Enabled = false;
                histogramToolStripMenuItem.Enabled = false;
                sepiaToolStripMenuItem.Enabled = false;
                greyscaleToolStripMenuItem.Enabled = false;
                loadBackgroundToolStripMenuItem.Enabled = true;
            }
        }

        private void camSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
