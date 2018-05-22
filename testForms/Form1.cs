using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace testForms
{
    public partial class Form1 : Form
    {

        public class RGB
        {
            public int R = 0;
            public int G = 0;
            public int B = 0;

            public RGB()
            {

            }

            public RGB(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }

            public RGB(Color color)
            {
                R = color.R;
                G = color.G;
                B = color.B;
            }

            public RGB(CustomColor color)
            {
                R = (int)color.R;
                G = (int)color.G;
                B = (int)color.B;

            }
        }

        public class ColorChart
        {
            public string FamilyName { get; set; }
            public RGB Min { get; set; }
            public RGB Max { get; set; }
            public List<CustomColor> Colors = new List<CustomColor>();

            public ColorChart()
            {
                Min = new RGB(9999, 9999, 9999);
                Max = new RGB();
            }

            public void AddColor(CustomColor color)
            {
                Colors.Add(color);

                Min.R = Math.Min((int)color.R, Min.R);
                Min.G = Math.Min((int)color.G, Min.G);
                Min.B = Math.Min((int)color.B, Min.B);

                Max.R = Math.Max((int)color.R, Min.R);
                Max.G = Math.Max((int)color.G, Min.G);
                Max.B = Math.Max((int)color.B, Min.B);
            }

        }

        public class CustomColor
        {
            public string Name { get; set; }
            public string Family { get; set; }
            public Color _Color { get; set; }

            public double R { get; set; }
            public double G { get; set; }
            public double B { get; set; }

            public double Hue { get; set; }
            public double Saturation { get; set; }
            public double Brightness { get; set; }

            public CustomColor(double r, double g, double b, string name)
            {
                Name = name;
                R = r;
                G = g;
                B = b;

                _Color = Color.FromArgb((int)r, (int)g, (int)b);

                Hue = _Color.GetHue();
                Brightness = _Color.GetBrightness();
                Saturation = _Color.GetSaturation();

            }

            public CustomColor(Color color)
            {
                R = (int)color.R;
                G = (int)color.G;
                B = (int)color.B;

                _Color = Color.FromArgb((int)R, (int)G, (int)B);

                Hue = _Color.GetHue();
                Brightness = _Color.GetBrightness();
                Saturation = _Color.GetSaturation();
            }

            public CustomColor()
            {
                Name = "Black";
                R = 0;
                G = 0;
                B = 0;

                _Color = Color.Black;

                Hue = _Color.GetHue();
                Brightness = _Color.GetBrightness();
                Saturation = _Color.GetSaturation();
            }
        }

        public static List<string> ColorFamilies = new List<string> {
            "Brown",
            "Black",
            "White",
            "Yellow",
            "Orange",
            "Gray",
            "Pink",
            "Purple",
            "Cyan",
            "Red",
            "Green",
            "Blue"
        };

        public CustomColor MatchColors(Color pixel)
        {

            return new CustomColor();
        }


        private CustomColor MatchColors(CustomColor pixel)
        {


            for (int ca = 0; ca < Charts.Count; ca++)
            {
                var chart = Charts[ca];

                for (int col = 0; col < chart.Colors.Count; col++)
                {
                    var color = chart.Colors[col];

                    double diffHue = Math.Min(Math.Abs(pixel.Hue - color.Hue), 360 - Math.Abs(pixel.Hue - color.Hue));
                    double diffSat = Math.Abs(pixel.Saturation - color.Saturation);
                    double diffBright = Math.Abs(pixel.Brightness - color.Brightness);

                    if (pixel.Saturation < 0.1) //Concider grayscale
                    {
                        if (diffSat < 0.1 && diffBright < 0.1)
                        {
                            return color;
                        }
                    }
                    else
                    {
                        if (diffHue < 15)
                        {
                            return color;
                        }
                    }
                }
            }

            return null;
        }

        List<CustomColor> undefinedColors = new List<CustomColor>();
        CustomColor undefinedColor;
        int lastY = -1;
        int lastX = -1;

        public void ScanPixels()
        {

            Bitmap bmap = new Bitmap(pictureBox1.ImageLocation);
            undefinedColor = null;

            if (lastY >= bmap.Height - 2)
            {
                pictureBox2.BackColor = Color.White;
                return;
            }

            for (int y = lastY + 1; y < bmap.Height; y++)
            {

                for (int x = lastX + 1; x < bmap.Width; x++)
                {
                    Color color = bmap.GetPixel(x, y);

                    //If color does not exist, promt to create it
                    if (MatchColors(new CustomColor(color)) == null) {

                        //undefinedColors.Add(new CustomColor(color));
                        undefinedColor = new CustomColor(color);
                        lastX = x;
                        lastY = y;
                        return;
                    };
                }
            }
        }

        public static List<ColorChart> Charts = new List<ColorChart>();

        public void CreateUpdateFiles()
        {

            string[] files = Directory.GetFiles("../../ColorDefinitions");
            

            for (int cf = 0; cf < ColorFamilies.Count; cf++)
            {
                string familyName = ColorFamilies[cf];
                bool create = true;

                for (int f = 0; f < files.Length; f++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(files[f]);

                    if (fileName.ToLower() == familyName.ToLower())
                    {
                        create = false;
                        break;
                    }
                }

                if (create)
                {


                    ColorChart chart = new ColorChart();
                    chart.FamilyName = familyName;

                    using (StreamWriter file = File.CreateText("../../ColorDefinitions/" + familyName + ".json"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(file, chart);
                    }
                }
            }
        }

        public void SaveCharts()
        {
            for (int c = 0; c < Charts.Count; c++)
            {

                var chart = Charts[c];
                string familyName = chart.FamilyName;

                using (StreamWriter file = File.CreateText("../../ColorDefinitions/" + familyName + ".json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, chart);
                }
            }
        }

        public void ResetCharts()
        {
            for (int c = 0; c < ColorFamilies.Count; c++)
            {
                var familyName = ColorFamilies[c];

                var chart = new ColorChart();
                chart.FamilyName = familyName;
                Charts[c] = chart;
            }

            
        }

        public void LoadCharts()
        {
            checkedListBox1.Items.Clear();

            for (int i = 0; i < ColorFamilies.Count; i++)
            {
                string familyName = ColorFamilies[i];
                checkedListBox1.Items.Add(familyName, 0);
                using (StreamReader file = File.OpenText("../../ColorDefinitions/" + familyName + ".json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ColorChart chart = (ColorChart)serializer.Deserialize(file, typeof(ColorChart));

                    Charts.Add(chart);
                }
            }
        }

        public void UpdateSelector()
        {
            if (undefinedColor != null)
            {
                pictureBox2.BackColor = undefinedColor._Color;
            }
            
            
        }

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.ImageLocation = "../../7190d69bd90870ec2551302fbe4e205b.jpg";
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            //Create or update missing colorcharts, incase they have been changed
            CreateUpdateFiles();

            //Load charts
            LoadCharts();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanPixels();

            UpdateSelector();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (undefinedColor != null)
            {
                for (int ca = 0; ca < Charts.Count; ca++)
                {
                    if (Charts[ca].FamilyName == (string)checkedListBox1.SelectedItem)
                    {
                        undefinedColor.Family = Charts[ca].FamilyName;
                        Charts[ca].AddColor(undefinedColor);
                        ScanPixels();
                        UpdateSelector();
                        checkedListBox1.ClearSelected();
                        return;
                    }
                }
            }
            else
            {
                pictureBox1.Image = null;

            }
           
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SaveCharts();
        }

        private void RESET_Click(object sender, EventArgs e)
        {
            ResetCharts();
            SaveCharts();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = openFileDialog1)
            {
                dlg.InitialDirectory = Application.StartupPath;
                dlg.Title = "Open Image";
                dlg.Filter = "All files(*.*) | *.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PictureBox PictureBox1 = new PictureBox();

                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    pictureBox1.Image = new Bitmap(dlg.FileName);
                    pictureBox1.ImageLocation = dlg.FileName;
                    lastY = -1;
                    lastX = -1;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image != null && pictureBox1.ImageLocation.Length > 0)
            {

                Bitmap bmp = new Bitmap(pictureBox1.ImageLocation);
                Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height);


                for (int y = 0; y < bmp2.Height; y++)
                {

                    for (int x = 0; x < bmp2.Width; x++)
                    {

                        var pixel = bmp.GetPixel(x, y);
                        CustomColor color = MatchColors(new CustomColor(pixel));

                        if (color != null)
                        {
                            bmp2.SetPixel(x, y, color._Color);
                        }
                    }
                }

                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox3.Image = bmp2;

            }
        }
    }
}
