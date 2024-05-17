using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.IO;

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
        private Button btnSave;
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing = false;
        private ComboBox cmbColorMap;
        private TextBox txtInput;
        private Button btnSaveText;
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
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Image Editor";
            this.MinimumSize = new Size(1280, 720);

            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            btnBrowse = new Button
            {
                AutoSize = true,
                Text = "Browse",
                Margin = new Padding(10)
            };
            btnBrowse.Click += btnBrowse_Click;

            btnSave = new Button
            {
                AutoSize = true,
                Text = "Save Image",
                Margin = new Padding(10)
            };
            btnSave.Click += btnSave_Click;

            cmbColorMap = new ComboBox
            {
                Width = 120,
                Margin = new Padding(10)
            };
            cmbColorMap.Items.AddRange(Enum.GetNames(typeof(ColorMap)));
            cmbColorMap.SelectedIndex = 0;
            cmbColorMap.SelectedIndexChanged += cmbColorMap_SelectedIndexChanged;

            btnCrop = new Button
            {
                AutoSize = true,
                Text = "Crop",
                Margin = new Padding(10)
            };
            btnCrop.Click += btnCrop_Click;

            IdentifyArea = new Button
            {
                AutoSize = true,
                Text = "Identify Area",
                Margin = new Padding(10)
            };
            IdentifyArea.Click += btnIdentifyArea_Click;

            flowPanel.Controls.Add(btnBrowse);
            flowPanel.Controls.Add(btnSave);
            flowPanel.Controls.Add(cmbColorMap);
            flowPanel.Controls.Add(btnCrop);
            flowPanel.Controls.Add(IdentifyArea);

            Panel imagePanel = new Panel
            {
                Height = 480,
                Dock = DockStyle.Top,
                AutoScroll = true,
                Padding = new Padding(10),
            };

            pictureBoxOriginal = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize,
                Margin = new Padding(10)
            };
            pictureBoxOriginal.MouseDown += pictureBoxOriginal_MouseDown;
            pictureBoxOriginal.MouseMove += pictureBoxOriginal_MouseMove;
            pictureBoxOriginal.MouseUp += pictureBoxOriginal_MouseUp;
            pictureBoxOriginal.Paint += pictureBoxOriginal_Paint;

            imagePanel.Controls.Add(pictureBoxOriginal);

            FlowLayoutPanel textPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };

            txtInput = new TextBox
            {
                Width = 300,
                Height = 100,
                Multiline = true,
                Margin = new Padding(10)
            };

            btnSaveText = new Button
            {
                AutoSize = true,
                Text = "Save Text",
                Margin = new Padding(10)
            };
            btnSaveText.Click += btnSaveText_Click;

            textPanel.Controls.Add(txtInput);
            textPanel.Controls.Add(btnSaveText);

            this.Controls.Add(textPanel);
            this.Controls.Add(imagePanel);
            this.Controls.Add(flowPanel);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(openFileDialog.FileName);

                using (Mat imagetest = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.AnyColor))
                {
                    Mat grayscale = new Mat();
                    CvInvoke.CvtColor(imagetest, grayscale, ColorConversion.Bgr2Gray);

                    modifiedGrayImage = new Bitmap(grayscale.Width, grayscale.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(modifiedGrayImage))
                    {
                        g.DrawImage(grayscale.ToBitmap(), new Rectangle(0, 0, grayscale.Width, grayscale.Height));
                    }

                    pictureBoxOriginal.Image = modifiedGrayImage;
                    GrayImage = new Bitmap(grayscale.Width, grayscale.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(GrayImage))
                    {
                        g.DrawImage(grayscale.ToBitmap(), new Rectangle(0, 0, grayscale.Width, grayscale.Height));
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
            if (modifiedGrayImage != null)
            {
                selectedColorMap = (ColorMap)Enum.Parse(typeof(ColorMap), cmbColorMap.SelectedItem.ToString());
                pictureBoxOriginal.Image = modifiedGrayImage;
            }
        }

        private void ColorArea(Rectangle rect, ColorMap colorMap)
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    if (x >= 0 && x < modifiedGrayImage.Width && y >= 0 && y < modifiedGrayImage.Height)
                    {
                        Color originalColor = GrayImage.GetPixel(x, y);
                        int brightness = originalColor.R;

                        Color mappedColor = MapBrightnessToColor(brightness, colorMap);

                        modifiedGrayImage.SetPixel(x, y, mappedColor);
                    }
                }
            }
        }

        private Color MapBrightnessToColor(int brightness, ColorMap colorMap)
        {
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
                    return Color.White;
            }
        }

        private Color MapBrightnessToColor_Rainbow(int brightness)
        {
            if (brightness < 64)
            {
                int red = brightness * 4;
                return Color.FromArgb(red, 0, 255);
            }
            else if (brightness < 128)
            {
                int green = (brightness - 64) * 4;
                return Color.FromArgb(255, green, 255);
            }
            else if (brightness < 192)
            {
                int green = 255 - ((brightness - 128) * 4);
                return Color.FromArgb(255, green, 0);
            }
            else
            {
                int red = 255;
                int blue = (brightness - 192) * 4;
                return Color.FromArgb(red, 0, blue);
            }
        }

        private Color MapBrightnessToColor_Ocean(int brightness)
        {
            if (brightness < 64)
            {
                int blue = 192 + brightness;
                return Color.FromArgb(0, 0, blue);
            }
            else if (brightness < 128)
            {
                int green = 128 + (brightness - 64);
                return Color.FromArgb(0, green, 255);
            }
            else if (brightness < 192)
            {
                int green = 255 - (brightness - 128);
                return Color.FromArgb(64, green, 192);
            }
            else
            {
                int green = 128 - (brightness - 192);
                return Color.FromArgb(127 + green, 255, 255);
            }
        }

        private Color MapBrightnessToColor_Sunset(int brightness)
        {
            if (brightness < 64)
            {
                int red = brightness * 4;
                return Color.FromArgb(red, 255, 0);
            }
            else if (brightness < 128)
            {
                int red = 255;
                int green = 255 - (brightness - 64) * 4;
                return Color.FromArgb(red, green, 0);
            }
            else if (brightness < 192)
            {
                int red = 255;
                int green = 64 - (brightness - 128);
                return Color.FromArgb(red, green, 0);
            }
            else
            {
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

            Rectangle cropRect = GetRectangle(startPoint, endPoint);

            GrayImage = CropImage(GrayImage, cropRect);
            modifiedGrayImage = CropImage(modifiedGrayImage, cropRect);

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

        private void btnSaveText_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Please enter some text to save.");
                return;
            }

            // SaveFileDialog saveFileDialog = new SaveFileDialog
            // {
            //     Filter = "Text Files|*.txt"
            // };
            // if (saveFileDialog.ShowDialog() == DialogResult.OK)
            // {
            //     File.WriteAllText(saveFileDialog.FileName, text);
            //     MessageBox.Show("Text saved successfully.");
            // }

            using (Graphics g = Graphics.FromImage(modifiedGrayImage))
            {
                using (Font font = new Font("Arial", 16))
                {
                    g.DrawString(txtInput.Text, font, Brushes.Yellow, startPoint);
                }
            }

            pictureBoxOriginal.Image = modifiedGrayImage;
        }

        private Color selectedColor = Color.Yellow;
    }
}
