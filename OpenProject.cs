using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using ProjectCreate;
using ImageModify;

namespace ProjectOpen
{
    public class OpenProject
    {
        public static (Bitmap, List<Tuple<string, Tuple<int, int, int, int>>>) getTextImage(string text)
        {
            int getMaxFontSize(FontFamily fontType, string text, int imageWidth, int imageHeight)
            {
                // ilosc linii int
                int linesCount = text.Split('\n').Length - 1;
                int fontSize = 1;
                Font font = new Font(fontType, fontSize);
                SizeF textSize = new SizeF();
                using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    textSize = graphics.MeasureString(text, font);
                }
                float margin = 0.05f;
                imageWidth -= (int)(imageWidth * margin);
                imageHeight -= (int)(imageHeight * margin);
                while (textSize.Width < imageWidth && textSize.Height + linesCount * ImageEditor.MainForm.distanceBetweenLines < imageHeight)
                {
                    fontSize++;
                    font = new Font(fontType, fontSize);
                    using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        textSize = graphics.MeasureString(text, font);
                    }
                }
                return fontSize - 1;
            }

            var showRectangles = ImageEditor.MainForm.showRectangles;
            var outputPath = ImageEditor.MainForm.imagesDirectory + @"\Tekst.png";
            var textColor = ImageEditor.MainForm.textColor;

            // open txt file
            string[] lines = text.Split('\n');
            // Ustawienia obrazu
            int imageWidth = 1920;
            int imageHeight = 1080;

            // Czcionka i kolor
            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(ImageEditor.MainForm.textFont);
            // Pobierz czcionkę z kolekcji
            FontFamily fontType = fontCollection.Families[0];

            int fontSize = getMaxFontSize(fontType, text, imageWidth, imageHeight);


            Font font = new Font(fontType, fontSize, FontStyle.Bold);
            Brush brush = new SolidBrush(textColor);
            Pen rectanglePen = new Pen(Color.Red, 1); // Czerwony obrys prostokątów

            List<Tuple<string, Tuple<int, int, int, int>>> charRectangles = new List<Tuple<string, Tuple<int, int, int, int>>>();

            Bitmap textImage;
            Bitmap recImage;

            using (Bitmap bitmap = new Bitmap(imageWidth, imageHeight))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Bitmap rectangleBitmap = new Bitmap(imageWidth, imageHeight))
            using (Graphics rectangleGraphics = Graphics.FromImage(rectangleBitmap))
            {
                // Ustawienia jakości obrazu dla tekstu
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                // Ustawienia jakości obrazu dla prostokątów
                rectangleGraphics.Clear(Color.Transparent);

                // Punkt początkowy dla tekstu
                float x = 10;
                float y = 10;

                // Podział tekstu na znaki
                foreach (string line in lines)
                {
                    // Przygotowanie tekstu
                    StringFormat format = new StringFormat
                    {
                        FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                    };
                    foreach (char c in line)
                    {
                        string character = c.ToString();

                        // Pomiar prostokątu dla znaku
                        CharacterRange[] ranges = { new CharacterRange(0, 1) };
                        format.SetMeasurableCharacterRanges(ranges);

                        Region[] regions = graphics.MeasureCharacterRanges(character, font, new RectangleF(x, y, imageWidth, imageHeight), format);
                        RectangleF rect = regions[0].GetBounds(graphics);

                        // Rysowanie znaku na głównym obrazie
                        graphics.DrawString(character, font, brush, x, y, format);

                        if (showRectangles)
                        {
                            // Rysowanie prostokąta na osobnym obrazie
                            rectangleGraphics.DrawRectangle(rectanglePen, Rectangle.Round(rect));

                            int x1 = (int)rect.X;
                            int y1 = (int)rect.Y;
                            int x2 = (int)(rect.X + rect.Width);
                            int y2 = (int)(rect.Y + rect.Height);
                            charRectangles.Add(new Tuple<string, Tuple<int, int, int, int>>(character, new Tuple<int, int, int, int>(x1, y1, x2, y2)));
                        }
                        x += rect.Width;
                    }

                    // Przejście do nowej linii
                    x = 10;
                    y += font.GetHeight(graphics) + ImageEditor.MainForm.distanceBetweenLines;
                }


                string txtPath = outputPath.Replace(".png", "_rectangles.txt");
                int minx1 = charRectangles
                    .Where(t => t.Item1 != "\r")
                    .Min(t => t.Item2.Item1);

                int miny1 = charRectangles
                    .Where(t => t.Item1 != "\r")
                    .Min(t => t.Item2.Item2);

                int maxx2 = charRectangles
                    .Where(t => t.Item1 != "\r")
                    .Max(t => t.Item2.Item3) + 1;

                int maxy2 = charRectangles
                    .Where(t => t.Item1 != "\r")
                    .Max(t => t.Item2.Item4) + 1;

                using (StreamWriter sw = new StreamWriter(txtPath))
                {
                    foreach (var tuple in charRectangles)
                    {
                        // znak nowej linii
                        if (tuple.Item1 == "\r")
                        {
                            sw.WriteLine();
                            continue;
                        }
                        sw.WriteLine($"{tuple.Item1}: ({tuple.Item2.Item1}, {tuple.Item2.Item2}, {tuple.Item2.Item3}, {tuple.Item2.Item4})");
                    }
                }
                Bitmap cropImage(Bitmap image, int x1, int y1, int x2, int y2)
                {
                    int width = x2 + x1;
                    int height = y2 + y1;
                    if (width > image.Width)
                        width = image.Width;
                    if (height > image.Height)
                        height = image.Height;

                    Rectangle cropRect = new Rectangle(0, 0, width, height);
                    Bitmap croppedBitmap = image.Clone(cropRect, image.PixelFormat);
                    image.Dispose();
                    return croppedBitmap;
                }

                textImage = cropImage(bitmap, minx1, miny1, maxx2, maxy2);
                recImage = cropImage(rectangleBitmap, minx1, miny1, maxx2, maxy2);

            }

            textImage.Save(outputPath, ImageFormat.Png);

            if (showRectangles)
            {
                string rectOutputPath = outputPath.Replace(".png", "_rectangles.png");
                recImage.Save(rectOutputPath, ImageFormat.Png);
            }
            return (textImage, charRectangles);
        }

        public static List<string> ChooseProject()
        {
            var actualProjectDirectories = new List<string>();
            string startFolder = ImageEditor.MainForm.songsDirectory;
            // Sprawdzenie, czy folder istnieje, jeśli nie to utworzenie go
            if (!Directory.Exists(startFolder))
            {
                try
                {
                    Directory.CreateDirectory(startFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return null;
                }
                MessageBox.Show("Folder z piosenkami został utworzony. Dodaj do niego folder wybranej piosenki, a w nim plik 'Tekst.txt'");
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", startFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nie udało się otworzyć folderu: " + ex.Message);
                }
                return null;
            }
            else if (Directory.GetDirectories(startFolder).Length == 0)
            {
                MessageBox.Show("Folder z piosenkami jest pusty. Dodaj do niego folder wybranej piosenki, a w nim plik 'Tekst.txt'");
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", startFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nie udało się otworzyć folderu: " + ex.Message);
                }
                return null;
            }
            else
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.InitialDirectory = startFolder; // Ustawienie folderu początkowego
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result != DialogResult.OK)
                    return null;

                string selectedPath = folderBrowserDialog.SelectedPath;

                string helpPath = selectedPath.Substring(0, selectedPath.LastIndexOf("\\"));
                if (helpPath != startFolder && helpPath + "\\" != startFolder)
                {
                    MessageBox.Show("Wybierz folder z piosenką z folderu 'songs'");
                    return null;
                }

                if (Directory.GetDirectories(selectedPath).Length != 0)
                {
                    foreach (string directory in Directory.GetDirectories(selectedPath))
                    {
                        var directoryFiles = Directory.GetFiles(directory);

                        if (directoryFiles.Any(file => file.Contains("Tekst.txt")))
                        {
                            actualProjectDirectories.Add(directory);
                        }
                    }
                }
                else if (Directory.GetFiles(selectedPath).Length > 0)
                {
                    string[] possibleFileNames = { "Tekst.txt", "Text.txt", "tekst.txt", "text.txt" };

                    // Sprawdzenie, czy którykolwiek z plików istnieje
                    bool fileExists = possibleFileNames.Any(fileName => File.Exists(Path.Combine(selectedPath, fileName)));
                    if (fileExists)
                    {
                        string fileName = possibleFileNames.First(fileName => File.Exists(Path.Combine(selectedPath, fileName)));
                        string songText = System.IO.File.ReadAllText(Path.Combine(selectedPath, fileName));
                        songText = CreateProject.correctnessOfText(songText);
                        if (songText == null)
                            return null;

                        CreateProject.createSongFolders(songText, selectedPath);
                        foreach (string directory in Directory.GetDirectories(selectedPath))
                        {
                            var directoryFiles = Directory.GetFiles(directory);

                            if (directoryFiles.Any(file => file.Contains("Tekst.txt")))
                            {
                                actualProjectDirectories.Add(directory);
                            }
                        }
                    }
                    else
                        MessageBox.Show("Folder nie posiada pliku z tekstem 'Tekst.txt'");
                }
                else
                    MessageBox.Show("Folder nie posiada pliku z tekstem 'Tekst.txt'");
                return actualProjectDirectories;
            }
        }

        public static Bitmap LoadImage(EventArgs e)
        {
            var textImage = ImageEditor.MainForm.textImage;
            var imagesDirectory = ImageEditor.MainForm.imagesDirectory;

            // Tworzenie wynikowego obrazu
            Bitmap fusedImage = ModifyImage.GetFusedImage();

            return fusedImage;
        }
    }
}