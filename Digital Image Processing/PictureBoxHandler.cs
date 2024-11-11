using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace PBH
{
    internal class PictureBoxHandler
    {
        public void disposeImages(PictureBox pictureBox, Bitmap image)
        {
            if (image != null)
            {
                image.Dispose();
            }
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }
        }

        public Bitmap loadImage(PictureBox pictureBox, Bitmap image, OpenFileDialog dialog)
        {
            if (pictureBox.Image != null && image != null)
            {
                disposeImages(pictureBox, image);
            }

            image = new Bitmap(dialog.FileName);
            pictureBox.Image = image;
            return image;
        }


    }
}
