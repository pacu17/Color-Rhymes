using ImageEditor;

namespace Tools
{
    public class ColorTools
    {
        public static List<Color> getRhymeColors()
        {
            void OpenProjectWithColors(List<Color> colors, string fontsDirectory)
            {
                int dzielnik = 1;
                for (int i = 2; i < 6; i++){
                    if (colors.Count % i == 0)
                    {
                        dzielnik = i;
                        break;
                    }
                }
                int width = colors.Count / dzielnik;
                int height = dzielnik;
                Bitmap image = new Bitmap(width, height);
                for (int i = 0; i < colors.Count; i++)
                {
                    int x = i % width;
                    int y = i / width;
                    image.SetPixel(x, y, colors[i]);
                }
                image.Save(fontsDirectory+@"\colors.png");
            }
            
            var fontsDirectory = ImageEditor.MainForm.fontsDirectory;
            List<Color> colors = new List<Color>
            {
                Color.FromArgb(218, 96, 0),
                Color.FromArgb(0, 248, 255),
                Color.FromArgb(87, 255, 74),
                Color.FromArgb(0, 143, 255),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(255, 122, 0),
                Color.FromArgb(0, 81, 255),
                Color.FromArgb(42, 103, 0),
                Color.FromArgb(255, 35, 0),
                Color.FromArgb(0, 0, 236),
                Color.FromArgb(33, 52, 20),
                Color.FromArgb(207, 0, 0),
                Color.FromArgb(0, 4, 174),
                Color.FromArgb(0, 42, 0),
                Color.FromArgb(120, 0, 0),
                Color.FromArgb(0, 4, 65),
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(51, 0, 0),
                Color.FromArgb(0, 97, 167),
                Color.FromArgb(255, 171, 0),
                Color.FromArgb(108, 66, 0),
                Color.FromArgb(0, 131, 167),
                Color.FromArgb(0, 30, 39),
                Color.FromArgb(3, 53, 67),
                Color.FromArgb(144, 144, 144),
                Color.FromArgb(201, 59, 87),
                Color.FromArgb(255, 250, 111),
                Color.FromArgb(106, 106, 106),
                Color.FromArgb(255, 0, 239),
                Color.FromArgb(243, 255, 0),
                Color.FromArgb(77, 77, 77),
                Color.FromArgb(180, 28, 179),
                Color.FromArgb(234, 0, 103),
                Color.FromArgb(41, 41, 41),
                Color.FromArgb(147, 0, 146),
                Color.FromArgb(212, 212, 212),
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(114, 19, 68),
                Color.FromArgb(255, 140, 143)
            };

            List<Color> imageColors = new List<Color>();
            try
            {
                Bitmap image = new Bitmap(fontsDirectory+@"\colors.png");
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        Color color = image.GetPixel(i, j);
                        if (color.A == 0)
                            continue;
                        if (!imageColors.Contains(color))
                        {
                            imageColors.Add(color);
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    OpenProjectWithColors(colors, fontsDirectory);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nie udało się utworzyć obrazka z kolorami");
                    Console.WriteLine(e.Message);
                }
            }
            if (imageColors.Count > 0)
            {
                colors = imageColors;
            }
            return colors;
        }
    
        public static void colorScrollPlus()
        {
            var colorIndex = ImageEditor.MainForm.colorIndex;
            var colors = ImageEditor.MainForm.colors;

            if (colorIndex < colors.Count - 1)
                colorIndex++;
            else
                colorIndex = 0;
            ImageEditor.MainForm.highLightColor = colors[colorIndex];
            ImageEditor.MainForm.colorLabel.BackColor = ImageEditor.MainForm.highLightColor;
            ImageEditor.MainForm.colorIndex = colorIndex;
        }
        public static void colorScrollMinus()
        {
            var colorIndex = ImageEditor.MainForm.colorIndex;
            var colors = ImageEditor.MainForm.colors;

            if (colorIndex > 0)
                colorIndex--;
            else
                colorIndex = colors.Count - 1;
            ImageEditor.MainForm.highLightColor = colors[colorIndex];
            ImageEditor.MainForm.colorLabel.BackColor = ImageEditor.MainForm.highLightColor;
            ImageEditor.MainForm.colorIndex = colorIndex;
        }
    }
}
