using System;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;

using Emgu.CV.Structure;


namespace MyWinFormsApp
{
    public partial class MainForm : Form
    {
        private Bitmap? originalImage;
        private Bitmap? coloredImage;
        private Button? btnBrowse;
        private PictureBox? pictureBoxOriginal;
        private PictureBox? pictureBoxColored;
        private Button? btnIdentifyAreas;
        private Button? btnColorAreas;
        private Button? btnChooseColor;
        private Button? btnSave;
        private Point? startPoint = null;
        private Point? endPoint = null;
        private Color selectedColor = Color.Red; // Default color
        private Mat grayscale;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create a panel to host controls with scrolling enabled
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;

            // Create controls and set their properties
            btnBrowse = new Button();
            btnBrowse.AutoSize = true;
            btnBrowse.Text = "Browse";
            btnBrowse.Location = new Point(20, 20);
            btnBrowse.Click += btnBrowse_Click;

            pictureBoxOriginal = new PictureBox();
            pictureBoxOriginal.Location = new Point(20, 50);
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxOriginal.MouseDown += pictureBoxOriginal_MouseDown;
            pictureBoxOriginal.MouseMove += pictureBoxOriginal_MouseMove;
            pictureBoxOriginal.MouseUp += pictureBoxOriginal_MouseUp;

            pictureBoxColored = new PictureBox();
            pictureBoxColored.Location = new Point(250, 50);
            pictureBoxColored.Size = new Size(200, 200);

            btnIdentifyAreas = new Button();
            btnIdentifyAreas.AutoSize = true;
            btnIdentifyAreas.Text = "Identify Areas";
            btnIdentifyAreas.Location = new Point(btnBrowse.Right + 20, 20);
            btnIdentifyAreas.Click += btnIdentifyAreas_Click;

            btnColorAreas = new Button();
            btnColorAreas.AutoSize = true;
            btnColorAreas.Text = "Color Areas";
            btnColorAreas.Location = new Point(btnIdentifyAreas.Right + 50, 20);
            btnColorAreas.Click += btnColorAreas_Click;

            // Button for choosing color
            btnChooseColor = new Button();
            btnChooseColor.AutoSize = true;
            btnChooseColor.Text = "Choose Color";
            btnChooseColor.Location = new Point(btnColorAreas.Right + 50, 20);
            btnChooseColor.Click += btnChooseColor_Click;

            btnSave = new Button();
            btnSave.AutoSize = true;
            btnSave.Text = "Save";
            btnSave.Location = new Point(btnChooseColor.Right + 50, 20);
            btnSave.Click += btnSave_Click;

            // Add controls to the panel
            panel.Controls.Add(btnBrowse);
            panel.Controls.Add(pictureBoxOriginal);
            panel.Controls.Add(pictureBoxColored);
            panel.Controls.Add(btnIdentifyAreas);
            panel.Controls.Add(btnColorAreas);
            panel.Controls.Add(btnChooseColor);
            panel.Controls.Add(btnSave);

            // Add the panel to the form
            Controls.Add(panel);
        }


        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the original image using Bitmap
                originalImage = new Bitmap(openFileDialog.FileName);

                // Convert the original image to grayscale using Emgu.CV
                using (Mat imagetest = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.AnyColor))
                {
                    grayscale = new Mat();
                    CvInvoke.CvtColor(imagetest, grayscale, ColorConversion.Bgr2Gray);

                    // Convert the grayscale Mat to a Bitmap for display
                    Bitmap grayscaleImage = grayscale.ToBitmap();

                    // Display the grayscale image in pictureBoxOriginal
                    pictureBoxOriginal.Image = grayscaleImage;
                }
            }
            // GetTotalPixelValue(grayscale);
        }


        private void pictureBoxOriginal_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Location;
        }

        private void pictureBoxOriginal_MouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint.HasValue)
            {
                endPoint = e.Location;
                pictureBoxOriginal.Refresh();
            }
        }

        private void pictureBoxOriginal_MouseUp(object sender, MouseEventArgs e)
        {
            if (startPoint.HasValue && endPoint.HasValue)
            {
                IdentifyAndColorArea();
                startPoint = null;
                endPoint = null;
            }
        }

        private void IdentifyAndColorArea()
        {
            if (startPoint != null && endPoint != null)
            {
                int x1 = Math.Min(startPoint.Value.X, endPoint.Value.X);
                int y1 = Math.Min(startPoint.Value.Y, endPoint.Value.Y);
                int x2 = Math.Max(startPoint.Value.X, endPoint.Value.X);
                int y2 = Math.Max(startPoint.Value.Y, endPoint.Value.Y);

                Rectangle rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);

                using (Graphics graphics = Graphics.FromImage(originalImage))
                using (Pen pen = new Pen(selectedColor, 2))
                {
                    graphics.DrawRectangle(pen, rect); // Draw unfilled rectangle
                }

                pictureBoxOriginal.Refresh();
            }
        }

        private void btnIdentifyAreas_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please select an X-ray image first.");
                return;
            }

            // Implement image processing algorithms to identify affected areas
            // For simplicity, let's assume it's already done and just mark random areas
            // Graphics graphics = Graphics.FromImage(originalImage);
            // Pen rec = new Pen(selectedColor, 2);
            // graphics.DrawRectangle(rec, new Rectangle(50, 50, 100, 100)); // Sample marking
            // pictureBoxOriginal.Refresh();
        }



        private void btnColorAreas_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            // Subscribe to the mouse move event of the original image picture box
            pictureBoxOriginal.MouseMove += pictureBoxOriginal_MouseMoveForBrush;
        }

        private void pictureBoxOriginal_MouseMoveForBrush(object sender, MouseEventArgs e)
        {
            if (originalImage == null)
                return;

            // Check if the left mouse button is pressed
            if (e.Button == MouseButtons.Left)
            {
                // Get the coordinates of the mouse pointer relative to the picture box
                int x = e.X;
                int y = e.Y;

                // Define the brush size (thickness of the line)
                int brushSize = 5; // Adjust as needed

                // Loop through the pixels around the current pixel
                for (int i = x - brushSize / 2; i <= x + brushSize / 2; i++)
                {
                    for (int j = y - brushSize / 2; j <= y + brushSize / 2; j++)
                    {
                        // Check if the pixel is within the bounds of the image
                        if (i >= 0 && i < originalImage.Width && j >= 0 && j < originalImage.Height)
                        {
                            // Color the pixel at the current position with the selected color
                            originalImage.SetPixel(i, j, selectedColor);
                        }
                    }
                }

                // Refresh the picture box to display the updated image
                pictureBoxOriginal.Refresh();
            }
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("No colored image to save.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                originalImage.Save(saveFileDialog.FileName);
                MessageBox.Show("Colored image saved successfully.");
            }
        }
        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                selectedColor = colorDialog.Color;
            }
        }

        // public long GetTotalPixelValue(Mat image)
        // {
        //     // Check if the Mat has only one channel (grayscale)
        //     if (image.Channels != 1)
        //     {
        //         throw new ArgumentException("This function expects a single-channel grayscale image.");
        //     }

        //     // Convert the Mat to a Bitmap for internal processing (optional)
        //     using (Bitmap grayscaleImage = image.ToBitmap()) // Use using block for grayscaleImage
        //     {
        //         // You can perform additional processing on the grayscaleImage here if needed
        //     }

        //     // Calculate the total number of pixels
        //     long totalPixels = image.Width * image.Height;

        //     // Get the pointer to the first pixel data
        //     byte* imageData = (byte*)image.DataPointer;

        //     // Initialize a variable to store the sum of all pixel values
        //     long totalValue = 0;

        //     // Loop through each pixel and accumulate the values
        //     for (long i = 0; i < totalPixels; i++)
        //     {
        //         totalValue += imageData[i];
        //     }

        //     return totalValue;
        // }





    }
}
