using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCH
{
    internal class VideoCamHandler
    {
        public void toggleCam(Boolean isOn, Button loadImage, Button saveImage)
        {
            if(isOn)
            {
                loadImage.Enabled = false;
                saveImage.Enabled = false;
            } else
            {
                loadImage.Enabled = true;
                saveImage.Enabled = true;
            }
        }
    }
}
