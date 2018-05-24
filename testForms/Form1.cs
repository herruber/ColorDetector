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
        public static int Hues = 30;
        public static int Brightnesses = 3;
        public double grayLimit = 0.2;
        public double brightLimit = 0.2;

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
                Name = color.Name;
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
        
        public static Dictionary<string, CustomColor> NameColorPairs = new Dictionary<string, CustomColor>() {
            {"Brown", new CustomColor(166, 99, 76, "Brown") },
            {"Yellow", new CustomColor(Color.Yellow) },
            {"Orange", new CustomColor(Color.Orange) },
            {"Pink", new CustomColor(Color.Pink) },
            {"Purple", new CustomColor(Color.Purple) },
            {"Cyan", new CustomColor(Color.Cyan) },
            {"Red", new CustomColor(Color.Red) },
            {"Green", new CustomColor(Color.Green) },
            {"Blue", new CustomColor(Color.Blue) },
            {"Beige", new CustomColor(Color.Beige) },
            {"LightBlue", new CustomColor(Color.LightBlue) },
            {"LightGreen", new CustomColor(Color.LightGreen) },
            {"LightCoral", new CustomColor(Color.LightCoral) }

        };

        public class Family
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public List<CustomColor> colors = new List<CustomColor>();

            public Family(int min, int max)
            {
                Min = min;
                Max = max;
            }
        }

        //public static CustomColor[,,] HSBList = new CustomColor[Hues, Saturations, Brightnesses];
        public static CustomColor[] HList = new CustomColor[Hues];
        public static Family[] Families =
        {
            new Family(0, 50),
            new Family(50, 70),
            new Family(70, 170),
            new Family(170, 190),
            new Family(190, 290),
            new Family(290, 310),
            new Family(310, 360)
        };



        public static CustomColor[] GrayList = {
            new CustomColor(Color.Black),
            new CustomColor(Color.Gray),
            new CustomColor(Color.LightGray),
            new CustomColor(Color.White)
        };
        
        public List<Panel> panels = new List<Panel>();

        private CustomColor EvaluateSaturation(Color color)
        {
            var sat = color.GetSaturation();
            var bright = color.GetBrightness();

            if (bright < brightLimit)
            {
                return new CustomColor(Color.Black);
            }

            if (sat > grayLimit)
            {
                return null;
            }
            else
            {
                //Get the closest brightness index
                int id = (int)Math.Round(bright * (GrayList.Length - 1));

                return new CustomColor(Color.Gray); // GrayList[id];
            }
            
            
        }

        private CustomColor EvaluateColor(Color color)
        {

            var Hue = color.GetHue();
            CustomColor closestColor = new CustomColor();
            double closestDistance = 9999;

            int familyId = ColorToFamily(new CustomColor(color));

            if (familyId > -1)
            {
                var family = Families[familyId];

                foreach (var item in family.colors)
                {
                    var predefinedHue = item.Hue;
                    //Get the smallest hue difference
                    double distance = Math.Min(Math.Abs(predefinedHue - Hue), 360 - Math.Abs(predefinedHue - Hue));
                    double distance2 = Math.Abs(color.GetBrightness() * color.GetSaturation() - item.Saturation * item.Brightness) * 360;

                    if (distance < closestDistance)
                    {
                        closestColor = item;
                        closestColor.Name = item.Name;
                        closestDistance = distance;

                    }
                }
            }

            return closestColor;
        }

        private CustomColor ClosestColor(Color pixel)
        {
            CustomColor saturation = EvaluateSaturation(pixel);
            CustomColor color = null;

            if (saturation == null)
            {
                color = EvaluateColor(pixel);
                return color;
            }
            else
            {
                return saturation;
            }
        }

        private int ColorToFamily(CustomColor color)
        {

            for (int i = 0; i < Families.Length; i++)
            {

                if (Families[i].Min <= color.Hue && Families[i].Max >= color.Hue)
                {
                    return i;
                }

            }

            return -1;
        }

        private void PopulateFamilyList()
        {
            int counter = 0;

            foreach (var color in NameColorPairs)
            {
                var pb = new PictureBox();
                pb.Width = 80;
                pb.Height = 20;
                pb.BackColor = color.Value._Color;
                int id = ColorToFamily(color.Value);

                if (id > -1)
                {
                    Families[id].colors.Add(color.Value);
                }

                pb.Location = new Point(0, counter * pb.Height);
                pb.Click += panel_Click;
                listBox1.Controls.Add(pb);
                counter++;
            }

        }

        List<CustomColor> undefinedColors = new List<CustomColor>();

        static double MaxSize = 400 * 400;

        public void ScanPixels()
        {
            InfoBox.Clear();
            Bitmap bmap = new Bitmap(pictureBox1.Image, 400, 400);
            Bitmap bmap2 = new Bitmap(400, 400);

            Dictionary<string, int> counters = new Dictionary<string, int>();
            
            foreach (var item in NameColorPairs)
            {
                counters.Add(item.Key, 0);
            }

            foreach (var item in GrayList)
            {
                counters.Add(item.Name, 0);
            }

            for (int y = 0; y < bmap.Height; y++)
            {

                for (int x = 0; x < bmap.Width; x++)
                {

                    
                    Color color = bmap.GetPixel(x, y);

                    CustomColor bestMatch = ClosestColor(color);

                    counters[bestMatch.Name] += 1;
                    bmap2.SetPixel(x, y, bestMatch._Color);

                }
            }

            pictureBox3.Image = bmap2;
            List<KeyValuePair<string, int>> sorted = (from kv in counters orderby kv.Value descending select kv).ToList();

            //InfoBox.AppendText("\nImage contains no unknown colors");
            InfoBox.AppendText("Done scanning image");

            foreach (var pair in sorted)
            {
                InfoBox.AppendText("\n" + 100 * pair.Value / MaxSize + "% " + pair.Key);
            }
        }

        public static List<ColorChart> Charts = new List<ColorChart>();

        public void UpdateSelector()
        {

        }

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            PopulateFamilyList();

            //PopulateHSBList();

            
        }

        private void panel_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanPixels();

            UpdateSelector();
        }

        private void SaveHSBList()
        {

                //using (StreamWriter file = File.CreateText("../../ColorDefinitions/hsbList.json"))
                //{
                //    JsonSerializer serializer = new JsonSerializer();
                //    serializer.Formatting = Formatting.Indented;
                //    serializer.Serialize(file, HSBList);
                //InfoBox.AppendText("\n Saving HSB List... DONE!");
                //}
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SaveHSBList();
        }

        private void RESET_Click(object sender, EventArgs e)
        {
            //HSBList = new CustomColor[Hues, Saturations, Brightnesses];

            //for (int h = 0; h < HSBList.GetLength(0); h++)
            //{

            //    for (int s = 0; s < HSBList.GetLength(1); s++)
            //    {

            //        for (int b = 0; b < HSBList.GetLength(2); b++)
            //        {

            //        }
            //    }

            //}

            SaveHSBList();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = openFileDialog1)
            {
                
                dlg.InitialDirectory = Path.GetFullPath(Path.Combine(Application.StartupPath, "..\\..\\Images"));
                dlg.Title = "Open Image";
                dlg.Filter = "All files(*.*) | *.*";



                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PictureBox PictureBox1 = new PictureBox();
                    InfoBox.Clear();
                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    pictureBox1.Image = new Bitmap(dlg.FileName);
                    pictureBox1.ImageLocation = dlg.FileName;
                    ScanPixels();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            //if (pictureBox1.Image != null && pictureBox1.ImageLocation.Length > 0)
            //{

            //    Bitmap bmp = new Bitmap(pictureBox1.ImageLocation);
            //    Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height);

            //    for (int y = 0; y < bmp2.Height; y++)
            //    {

            //        for (int x = 0; x < bmp2.Width; x++)
            //        {

            //            var pixel = bmp.GetPixel(x, y);

            //            //CustomColor color = ClosestListColor(new CustomColor(pixel));

            //            if (color != null)
            //            {
            //                bmp2.SetPixel(x, y, NameColorPairs[color.Family]._Color);
            //            }
            //        }
            //    }

            //    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            //    pictureBox3.Image = bmp2;
            //}
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {

            var x = e.X;
            var y = e.Y;
            var map = new Bitmap(pictureBox1.Image, 400, 400);

            double pixx = (double)x / (double)pictureBox1.Width;
            double pixy = (double)y / (double)pictureBox1.Height;

            pixx *= 400;
            pixy *= 400;

            InfoBox.AppendText("\n" +" " +(int)pixx + " , " + (int)pixy);
            InfoBox.AppendText("\n" + map.GetPixel((int)pixx, (int)pixy));

            //if (pictureBox1.Image != null)
            //{
            //    var map = new Bitmap(pictureBox1.Image, pictureBox1.Size);

            //    Color color = map.GetPixel(x, y);
            //    pixResult = PixelToListValue(color);
            //    pictureBox2.BackColor = color;

            //    if (pixResult.color == null)
            //    {
            //        pixResult.color = new CustomColor(color);
            //    }
            //}

        }

        bool handpick = false;

        private void button5_Click(object sender, EventArgs e)
        {

            handpick = !handpick;

            if (handpick)
            {
                button5.Text = "ON";
            }
            else
            {
                button5.Text = "OFF";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var updown = (NumericUpDown)sender;
            grayLimit = (double)updown.Value;
            ScanPixels();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            var updown = (NumericUpDown)sender;
            brightLimit = (double)updown.Value;
            ScanPixels();
        }
    }

}
