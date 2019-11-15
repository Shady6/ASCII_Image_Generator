using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace formsAscii
{

    public partial class Form1 : Form
    {
        private Bitmap bmp;
        private Bitmap originalBmp;
        private AsciiSettings asciiSettings;
        private AsciiSettings twoCharsMode;
        private AsciiSettings multipleCharsMode;
        private AsciiSettings proSettings;
        private AsciiSettings randomSettings;

        public Form1()
        {
            InitializeComponent();

            twoCharsMode = new AsciiSettings(groupBox1);
            multipleCharsMode = new AsciiSettings(groupBox2, ' ', ' ');
            randomSettings = new AsciiSettings(groupBox4);
            InitRandomSettins();
            InitProSettings();

            ModeManagger.Initialize(ref asciiSettings, new List<AsciiSettings>()
            {
                twoCharsMode, multipleCharsMode, randomSettings, proSettings
            });

        }

        private void InitProSettings()
        {
            proSettings = new AsciiSettings(groupBox5);

            string letters = "MWBER@DTF84L2ui;:-,.~`";

            float part = 256f / letters.Length;

            for (int i = 1; i < letters.Length + 1; i++)
            {
                proSettings.AddArtLetter(letters[i-1], (int)Math.Round(part * i));
            }
        }

        private void InitTwoCharsModeTextFields()
        {
            trackBar1.Value = (int)asciiSettings.pixelThreshold;
            label2.Text = ((int)asciiSettings.pixelThreshold).ToString();

            textBox2.Text = asciiSettings.darkAreaChar.ToString();
            textBox3.Text = asciiSettings.brightAreaChar.ToString();

            textBox4.Text = asciiSettings.fixedSize.ToString();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (!ModeManagger.AnyControlActive())
            {
                ModeManagger.SwitchControl(ref asciiSettings);
                InitTwoCharsModeTextFields();
            }

            originalBmp = ConvertToBitmap(openFileDialog1.FileName);
            CreateAsciiArt();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Font = new Font("Courier new", 2f);
        }

        private void CreateAsciiArt()
        {
            bmp = (Bitmap)originalBmp.Clone();

            FitImageToWindow(ref bmp, asciiSettings);

            ToGrayScale(bmp);

            textBox1.Text = ToAscii(asciiSettings).ToString();

            PlaceTextBox(ref bmp, ref textBox1, this);
        }

        public void AppendSettingsLetter(AsciiSettings settings, StringBuilder sb, int colorVal)
        {
            for (int i = 0; i < settings.lettersInArt.Count; i++)
            {
                if (colorVal < settings.lettersThreshold[i])
                {
                    sb.Append(settings.lettersInArt[i]);
                    break;
                }
            }
        }

        public StringBuilder ToAscii(AsciiSettings settings)
        {
            StringBuilder res = new StringBuilder("");

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color c = bmp.GetPixel(j, i);

                    if (settings.lettersInArt.Count < 1 && settings.pixelThreshold != null)
                    {
                        if (c.R < settings.pixelThreshold)
                            res.Append(settings.darkAreaChar);
                        else
                            res.Append(settings.brightAreaChar);
                    }
                    else
                        AppendSettingsLetter(settings, res, c.R);
                    
                }

                res.Append(Environment.NewLine);
            }

            return res;
        }

        private static void FitImageToWindow(ref Bitmap bmp, AsciiSettings asciiSettings)
        {
            int fixedSize = asciiSettings.fixedSize;
            float ratio = GetPhotoRatio(bmp) >= 1 ? GetPhotoRatio(bmp) : 1 / GetPhotoRatio(bmp);
            bmp = ResizeImage(bmp, fixedSize, (int)(fixedSize / ratio));
            
        }

        private static void PlaceTextBox(ref Bitmap bmp, ref TextBox tb, Form form)
        {
            Size size = TextRenderer.MeasureText(tb.Text, tb.Font);
            tb.Width = size.Width;
            tb.Height = size.Height;

            tb.Location = new Point(form.Width - 50 - tb.Width, 10);
        }

        private static float GetPhotoRatio(Bitmap bmp)
        {
            return (float)bmp.Width / bmp.Height;
        }

        public static void ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            asciiSettings.pixelThreshold = trackBar1.Value;

            textBox1.Text = ToAscii(asciiSettings).ToString();

            label2.Text = (trackBar1.Value).ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 1 && textBox3.Text.Length == 1)
            {
                twoCharsMode.darkAreaChar = textBox2.Text[0];
                twoCharsMode.brightAreaChar = textBox3.Text[0];
                asciiSettings = twoCharsMode;

                textBox1.Text = ToAscii(asciiSettings).ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bmp != null && textBox4.Text.Length != 0)
            {
                int value;
                if (int.TryParse(textBox4.Text, out value))
                {
                    if (value > 10)
                    {
                        asciiSettings.fixedSize = value;
                        CreateAsciiArt();
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int threshold;

            if (textBox5.Text.Length != 0 && textBox6.Text.Length != 0 &&
                int.TryParse(textBox5.Text, out threshold))
            {
                multipleCharsMode.AddArtLetter(textBox6.Text[0], threshold);

                AddToCharList(textBox6.Text[0], threshold);

                textBox5.Text = "";
                textBox6.Text = "";

                textBox1.Text = ToAscii(asciiSettings).ToString();
            }
        }

        public delegate void multiCharsListHandler(char c, int threshold);

        private void AddToCharList(char c, int threshold)
        {
            Label charLabel = new Label();
            charLabel.Text = c.ToString();
            charLabel.AutoSize = true;

            Label thresholdLabel = new Label();
            thresholdLabel.Text = threshold.ToString();
            thresholdLabel.AutoSize = true;

            Button artLetterDelete = new Button();
            artLetterDelete.Tag = "ArtLetterDelete";
            artLetterDelete.Text = "Delete";
            artLetterDelete.AutoSize = true;
            artLetterDelete.Click += (s, e) =>
            {
                multipleCharsMode.RemoveWhereLetter(c);
                groupBox3.Controls.Remove(charLabel);
                groupBox3.Controls.Remove(thresholdLabel);
                groupBox3.Controls.Remove(artLetterDelete);

                textBox1.Text = ToAscii(asciiSettings).ToString();
            };

            groupBox3.Controls.AddRange(new Control[]{charLabel, thresholdLabel, artLetterDelete});

            if (groupBox3.Controls.Count == 5)
            {
                groupBox3.Controls[2].Location = new Point( 10,  30);
                groupBox3.Controls[3].Location = new Point( 30,  30);
                groupBox3.Controls[4].Location = new Point( 60,  30);
            }

            //else if (groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 50 > groupBox3.Size.Height)
            //{
            //    charLabel.Location = new Point(10,
            //        groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);

            //    thresholdLabel.Location = new Point(30,
            //        groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);

            //    artLetterDelete.Location = new Point(60,
            //        groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);
            //}
            else
            {
                charLabel.Location = new Point(10, 
                    groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);

                thresholdLabel.Location = new Point(30,
                    groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);

                artLetterDelete.Location = new Point(60,
                    groupBox3.Controls[groupBox3.Controls.Count - 6].Location.Y + 30);
            }

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
            
        }

        private void groupBox1_MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (ModeManagger.AnyControlActive())
            {
                ModeManagger.SwitchControl(ref asciiSettings);
                textBox4.Text = asciiSettings.fixedSize.ToString();

                if (groupBox2.Enabled)
                    groupBox3.Enabled = true;
                else
                    groupBox3.Enabled = false;

                CreateAsciiArt();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int val;

            if (int.TryParse(textBox7.Text, out val) && val != 0)
            {
                InitRandomSettins(val);

                textBox1.Text = ToAscii(asciiSettings).ToString();
            }
        }

        private void InitRandomSettins(int val = 20)
        {
            randomSettings.clearLetters();

            Random rnd = new Random();

            float threshold = 256f / val;

            for (int i = 1; i < val + 1; i++)
            {
                randomSettings.AddArtLetter((char)rnd.Next(33, 127), (int)Math.Round(threshold * i));
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            ExportSettings();
        }

        private void ExportSettings()
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
            {
                sw.Write(asciiSettings.ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ExportSettings();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            string txtSettings = "";

            using (StreamReader sr = new StreamReader(openFileDialog2.FileName))
            {
                txtSettings = sr.ReadToEnd();
            }

            ClearMultiCharModeLabels();

            multipleCharsMode.TxtToSettings(txtSettings, AddToCharList);

            textBox1.Text = ToAscii(asciiSettings).ToString();
        }

        private void ClearMultiCharModeLabels()
        {
            for (int i = 0; i < groupBox3.Controls.Count; i++)
            {
                if (groupBox3.Controls[i].GetType() == typeof(Label) || groupBox3.Controls[i].Tag == "ArtLetterDelete")
                {
                    groupBox3.Controls.RemoveAt(i);
                    i--;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (asciiSettings != null)
            {
                asciiSettings.Shift(-5);
                textBox1.Text = ToAscii(asciiSettings).ToString();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (asciiSettings != null)
            {
                asciiSettings.Shift(5);
                textBox1.Text = ToAscii(asciiSettings).ToString();
            }
        }
    }

    public static class ModeManagger
    {
        public static List<AsciiSettings> allSettings;
        public static int activeIndex;

        public static void Initialize(ref AsciiSettings mainSettings 
            ,List<AsciiSettings> allSettings)
        {
            activeIndex = 0;

            ModeManagger.allSettings = allSettings;

            for (int i = 0; i < allSettings.Count; i++)
            {
                ModeManagger.allSettings[i].control.Enabled = false;
            }

            //SwitchControl(ref mainSettings);
        }

        public static bool AnyControlActive()
        {
            for (int i = 0; i < allSettings.Count; i++)
            {
                if (allSettings[i].control.Enabled)
                    return true;
            }

            return false;
        }

        public static void SwitchControl(ref AsciiSettings mainSettings)
        {
            mainSettings = allSettings[activeIndex];

            for (int i = 0; i < allSettings.Count; i++)
            {
                if (allSettings[i].control.Name != allSettings[activeIndex].control.Name)
                    allSettings[i].control.Enabled = false;
                else
                    allSettings[i].control.Enabled = true;
            }
            activeIndex = (activeIndex + 1) % allSettings.Count;
        }
    }

    public class AsciiSettings
    {
        public int? pixelThreshold;
        public char darkAreaChar;
        public char brightAreaChar;
        public List<char> lettersInArt;
        public List<int> lettersThreshold;
        public int fixedSize;
        public Control control;

        public AsciiSettings(Control control, char darkAreaChar = 'M', char brightAreaChar = '`', int fixedSize = 300 ,int? pixelThreshold = 70)
        {
            this.pixelThreshold = pixelThreshold;
            lettersThreshold = new List<int>();
            lettersInArt = new List<char>();
            this.darkAreaChar = darkAreaChar;
            this.brightAreaChar = brightAreaChar;
            this.fixedSize = fixedSize;
            this.control = control;
        }

        public void TxtToSettings(string txt, Form1.multiCharsListHandler listHandler)
        {
            clearLetters();
            string[] items = txt.Split('\n');

            for (int i = 0; i < items.Length; i++)
            {
                listHandler(items[i][0], int.Parse(items[i].Remove(0, 1)));
                AddArtLetter(items[i][0], int.Parse(items[i].Remove(0, 1)));
            }
        }

        public override string ToString()
        {
            string res = "";

            for (int i = 0; i < lettersInArt.Count; i++)
            {
                res += lettersInArt[i] + lettersThreshold[i].ToString();
                if (i != lettersInArt.Count - 1)
                    res += "\n";
            }

            return res;
        }

        public void Shift(int i)
        {
            for (int j = 0; j < lettersThreshold.Count; j++)
            {
                if (lettersThreshold[j] < 254)
                lettersThreshold[j] += i;
            }
        }

        public void AddArtLetter(char letter, int threshold)
        {
            lettersInArt.Add(letter);
            lettersThreshold.Add(threshold);
            QuickSort(0, lettersThreshold.Count - 1);
        }

        public void RemoveArtLetter(int index)
        {
            lettersInArt.RemoveAt(index);
            lettersThreshold.RemoveAt(index);
        }

        public void QuickSort(int lo, int hi)
        {
            int i = lo;
            int j = hi;
            int mid = lettersThreshold[(lo + hi) / 2];

            do
            {
                while (lettersThreshold[i] < mid) i++;
                while (lettersThreshold[j] > mid) j--;
                if (i <= j)
                {
                    int t1 = lettersThreshold[i];
                    char t2 = lettersInArt[i];

                    lettersThreshold[i] = lettersThreshold[j];
                    lettersInArt[i] = lettersInArt[j];

                    lettersThreshold[j] = t1;
                    lettersInArt[j] = t2;

                    i++;
                    j--;
                }
            } while (i <= j);

            if (j > lo) QuickSort(lo, j);
            if (i < hi) QuickSort(i, hi);
        }

        public void clearLetters()
        {
            lettersThreshold.Clear();
            lettersInArt.Clear();
        }

        public void RemoveWhereLetter(char c)
        {
            int index = lettersInArt.IndexOf(c);

            RemoveArtLetter(index);
        }
    }
}
