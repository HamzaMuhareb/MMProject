using System;
using System.Drawing;
using System.Drawing.Imaging; // Add this using directive
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace MyWinFormsApp
{
    public partial class MainForm : Form
    {
        private Bitmap originalImage;
        private Bitmap GrayImage;
        private Bitmap modifiedGrayImage;
        private Button btnBrowse;
        private Button btnCrop;
        private Button IdentifyArea;
        private PictureBox pictureBoxOriginal;
        private PictureBox pictureBoxColored;
        private Button btnSave;
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing = false;
        private ComboBox cmbColorMap;
        private enum ColorMap
        {
            Rainbow,
            Ocean,
            Sunset,
            None,
        }

        ColorMap selectedColorMap;
        public MainForm()
        {
            this.Size = new Size(800, 600);
            InitializeComponent();
        }


        private void InitializeComponent()
        {
            // Create a panel to host controls with scrolling enabled
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Create controls and set their properties
            btnBrowse = new Button
            {
                AutoSize = true,
                Text = "Browse",
                Location = new Point(20, 20)
            };
            btnBrowse.Click += btnBrowse_Click;

            pictureBoxOriginal = new PictureBox
            {
                Location = new Point(20, 50),
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            pictureBoxOriginal.MouseDown += pictureBoxOriginal_MouseDown;
            pictureBoxOriginal.MouseMove += pictureBoxOriginal_MouseMove;
            pictureBoxOriginal.MouseUp += pictureBoxOriginal_MouseUp;
            pictureBoxOriginal.Paint += pictureBoxOriginal_Paint;

            pictureBoxColored = new PictureBox
            {
                Location = new Point(250, 50),
                Size = new Size(200, 200)
            };

            btnSave = new Button
            {
                AutoSize = true,
                Text = "Save",
                Location = new Point(btnBrowse.Right + 30, 20)
            };
            btnSave.Click += btnSave_Click;

            // Add controls to the panel
            panel.Controls.Add(btnBrowse);
            panel.Controls.Add(pictureBoxOriginal);
            panel.Controls.Add(pictureBoxColored);
            panel.Controls.Add(btnSave);

            cmbColorMap = new ComboBox
            {
                Location = new Point(btnSave.Right + 30, 20),
                Width = 120
            };
            cmbColorMap.Items.AddRange(Enum.GetNames(typeof(ColorMap)));
            cmbColorMap.SelectedIndex = 0; // Select the first color map by default
            cmbColorMap.SelectedIndexChanged += cmbColorMap_SelectedIndexChanged;

            // Add the combo box to the panel
            panel.Controls.Add(cmbColorMap);

            btnCrop = new Button
            {
                AutoSize = true,
                Text = "Crop",
                Location = new Point(cmbColorMap.Right + 130, 20) // Adjust the location as needed
            };
            btnCrop.Click += btnCrop_Click;
            panel.Controls.Add(btnCrop);

            IdentifyArea = new Button
            {
                AutoSize = true,
                Text = "Identify Area",
                Location = new Point(btnCrop.Right + 20, 20) // Adjust the location as needed
            };
            IdentifyArea.Click += btnIdentifyArea_Click; // Assign event handler here
            panel.Controls.Add(IdentifyArea);
            
            
            // Add the panel to the form
            Controls.Add(panel);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the original image using Bitmap
                originalImage = new Bitmap(openFileDialog.FileName);

                // Convert the original image to grayscale using Emgu.CV
                using (Mat imagetest = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.AnyColor))
                {
                    Mat grayscale = new Mat();
                    CvInvoke.CvtColor(imagetest, grayscale, ColorConversion.Bgr2Gray);

                    // Convert grayscale Mat to an RGB Bitmap while retaining the grayscale appearance
                    modifiedGrayImage = new Bitmap(grayscale.Width, grayscale.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(modifiedGrayImage))
                    {
                        g.DrawImage(grayscale.ToBitmap(), new Rectangle(0, 0, grayscale.Width, grayscale.Height));
                    }

                    // Display the grayscale image in pictureBoxOriginal
                    pictureBoxOriginal.Image = modifiedGrayImage;
                    if (GrayImage == null)
                    {
                        GrayImage = new Bitmap(grayscale.Width, grayscale.Height, PixelFormat.Format24bppRgb);
                        using (Graphics g = Graphics.FromImage(GrayImage))
                        {
                            g.DrawImage(grayscale.ToBitmap(), new Rectangle(0, 0, grayscale.Width, grayscale.Height));
                        }
                    }
                }
            }
        }

        private void pictureBoxOriginal_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            startPoint = e.Location;
        }

        private void pictureBoxOriginal_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                endPoint = e.Location;
                pictureBoxOriginal.Invalidate();
            }
        }

        private void pictureBoxOriginal_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            endPoint = e.Location;
        }

        private void pictureBoxOriginal_Paint(object sender, PaintEventArgs e)
        {
            if (isDrawing && startPoint != Point.Empty && endPoint != Point.Empty)
            {
                Rectangle rect = GetRectangle(startPoint, endPoint);
                Pen p = new Pen(selectedColor);
                e.Graphics.DrawRectangle(p, rect);
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        private void IdentifyAndColorArea()
        {
            if (startPoint != Point.Empty && endPoint != Point.Empty)
            {
                Rectangle rect = GetRectangle(startPoint, endPoint);
                ColorArea(rect, selectedColorMap);
                pictureBoxOriginal.Invalidate();
            }
        }
        private void cmbColorMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle the event when the user selects a color map
            // Reapply the color mapping to update the colored areas with the newly selected color map
            if (modifiedGrayImage != null)
            {
                selectedColorMap = (ColorMap)Enum.Parse(typeof(ColorMap), cmbColorMap.SelectedItem.ToString());
                pictureBoxOriginal.Image = modifiedGrayImage;
            }
        }

        private void ColorArea(Rectangle rect, ColorMap colorMap)
        {
            // Iterate over pixels and apply color mapping based on the selected color map
            for (int x = rect.Left; x < rect.Right; x++)
            {
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    if (x >= 0 && x < modifiedGrayImage.Width && y >= 0 && y < modifiedGrayImage.Height)
                    {
                        Color originalColor = GrayImage.GetPixel(x, y);
                        int brightness = originalColor.R; // Since it's grayscale, R = G = B

                        // Map brightness to a color based on the selected color map
                        Color mappedColor = MapBrightnessToColor(brightness, colorMap);

                        modifiedGrayImage.SetPixel(x, y, mappedColor);
                    }
                }
            }
        }

        private Color MapBrightnessToColor(int brightness, ColorMap colorMap)
        {
            // Implement color mapping logic based on the selected color map
            switch (colorMap)
            {
                case ColorMap.Rainbow:
                    return MapBrightnessToColor_Rainbow(brightness);
                case ColorMap.Ocean:
                    return MapBrightnessToColor_Ocean(brightness);
                case ColorMap.Sunset:
                    return MapBrightnessToColor_Sunset(brightness);
                case ColorMap.None:
                    return MapGray(brightness);
                default:
                    return Color.White; // Default color
            }
        }


        private Color MapBrightnessToColor_Rainbow(int brightness)
        {
            // Convert brightness (0-255) to a rainbow color

            if (brightness < 64)
            {
                // Interpolate between blue and green
                int blue = 255;
                int green = brightness * 4;
                return Color.FromArgb(0, green, blue);
            }
            else if (brightness < 128)
            {
                // Interpolate between green and yellow
                int green = 255;
                int red = (brightness - 64) * 4;
                return Color.FromArgb(red, green, 0);
            }
            else if (brightness < 192)
            {
                // Interpolate between yellow and red
                int red = 255;
                int green = 255 - ((brightness - 128) * 4);
                return Color.FromArgb(red, green, 0);
            }
            else
            {
                // Interpolate between red and violet
                int red = 255;
                int blue = (brightness - 192) * 4;
                return Color.FromArgb(red, 0, blue);
            }
        }
        private Color MapBrightnessToColor_Ocean(int brightness)
        {
            // Convert brightness (0-255) to an ocean color

            if (brightness < 64)
            {
                // Interpolate between dark blue and blue
                int blue = 192 + brightness;
                return Color.FromArgb(0, 0, blue);
            }
            else if (brightness < 128)
            {
                // Interpolate between blue and turquoise
                int green = 128 + (brightness - 64);
                return Color.FromArgb(0, green, 255);
            }
            else if (brightness < 192)
            {
                // Interpolate between turquoise and light green
                int green = 255 - (brightness - 128);
                return Color.FromArgb(64, green, 192);
            }
            else
            {
                // Interpolate between light green and white
                int green = 128 - (brightness - 192);
                return Color.FromArgb(127 + green, 255, 255);
            }
        }
        private Color MapBrightnessToColor_Sunset(int brightness)
        {
            // Convert brightness (0-255) to a sunset color

            if (brightness < 64)
            {
                // Interpolate between yellow and orange
                int red = brightness * 4;
                return Color.FromArgb(red, 255, 0);
            }
            else if (brightness < 128)
            {
                // Interpolate between orange and red
                int red = 255;
                int green = 255 - (brightness - 64) * 4;
                return Color.FromArgb(red, green, 0);
            }
            else if (brightness < 192)
            {
                // Interpolate between red and dark red
                int red = 255;
                int green = 64 - (brightness - 128);
                return Color.FromArgb(red, green, 0);
            }
            else
            {
                // Interpolate between dark red and black
                int red = 128 - (brightness - 192);
                return Color.FromArgb(red, 0, 0);
            }
        }

        private Color MapGray(int brightness)
        {
            return Color.FromArgb(brightness, brightness, brightness);
        }

        private void btnIdentifyArea_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (startPoint == Point.Empty || endPoint == Point.Empty)
            {
                MessageBox.Show("Please select a region to identify and color.");
                return;
            }
            IdentifyAndColorArea();
            startPoint = Point.Empty;
            endPoint = Point.Empty;
        }

        private void btnCrop_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (startPoint == Point.Empty || endPoint == Point.Empty)
            {
                MessageBox.Show("Please select a region to crop.");
                return;
            }

            // Calculate the crop rectangle
            Rectangle cropRect = GetRectangle(startPoint, endPoint);

            // Crop the original image
            GrayImage = CropImage(GrayImage, cropRect);
            modifiedGrayImage = CropImage(modifiedGrayImage, cropRect);

            // Display the cropped image
            pictureBoxOriginal.Image = modifiedGrayImage;
            
            startPoint = Point.Empty;
            endPoint = Point.Empty;
        }
        private Bitmap CropImage(Bitmap source, Rectangle cropRect)
        {
            Bitmap croppedImage = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(source, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                            cropRect, GraphicsUnit.Pixel);
            }
            return croppedImage;
        }


        private void btnIdentifyAreas_Click(object sender, EventArgs e)
        {
            if (modifiedGrayImage == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            // Add code here to identify specific areas in the image
        }

        private void btnColorAreas_Click(object sender, EventArgs e)
        {
            if (modifiedGrayImage == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            if (modifiedGrayImage == null)
            {
                MessageBox.Show("No colored image to save.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg|PNG Image|*.png"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                modifiedGrayImage.Save(saveFileDialog.FileName);
                MessageBox.Show("Colored image saved successfully.");
            }
        }

        private Color selectedColor = Color.Yellow; // Default color
    }
}
