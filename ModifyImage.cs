using System.Security.Cryptography.Xml;
using ProjectOpen;

namespace ImageModify
{
    public class ModifyImage
    {
        public static Bitmap GetFusedImage(Bitmap existingResultImage = null)
        {
            var textImage = ImageEditor.MainForm.textImage;
            var rhymeImage = ImageEditor.MainForm.rhymeImage;

            if (existingResultImage == null || 
                existingResultImage.Width != textImage.Width || 
                existingResultImage.Height != textImage.Height)
            {
                existingResultImage?.Dispose();
                existingResultImage = new Bitmap(textImage.Width, textImage.Height);
            }
            
            if (ImageEditor.MainForm.isHighLightOnTheTop)
            {
                using (Graphics g = Graphics.FromImage(existingResultImage))
                {
                    g.Clear(Color.Transparent);

                    g.DrawImage(textImage, 0, 0);
                    g.DrawImage(rhymeImage, 0, 0);
                }
            }
            else
            {
                using (Graphics g = Graphics.FromImage(existingResultImage))
                {
                    g.Clear(Color.Transparent);

                    g.DrawImage(rhymeImage, 0, 0);
                    g.DrawImage(textImage, 0, 0);
                }
            }
            return existingResultImage;
        }
        
        public static Bitmap CreateTextImage()
        {
            var filesDirectory = ImageEditor.MainForm.filesDirectory;
            var actualProjectDirectory = ImageEditor.MainForm.actualProjectDirectory;

            
            var textPath = Path.Combine(actualProjectDirectory, "Tekst.txt");
            string text = System.IO.File.ReadAllText(textPath);
            (var textImage, ImageEditor.MainForm.charCoords) = OpenProject.getTextImage(text);
            return textImage;
        }
        
        public static Tuple<int, int, int, int> GetCharCoords(int mouseX, int mouseY)
        {
            bool skip = false;
            bool bracketFound = false;
            foreach (var (character, (x1, y1, x2, y2)) in ImageEditor.MainForm.charCoords)
            {
                if (!bracketFound && character == "]")
                {
                    skip = false;
                    bracketFound = true;
                    continue;
                }
                if (!bracketFound && character == "[")
                {
                    skip = true;
                    continue;
                }

                List<string> disAllowedChars = new List<string> { " ", "\r", "\n", ",", ".", "-", "?", "!", "'", ":", ";", "(", ")", "„", "”", "-", "…", "[", "]", "\""};
                if (skip || disAllowedChars.Contains(character))
                    continue;
                if (mouseX >= x1 && mouseX <= x2 && mouseY >= y1 && mouseY <= y2)
                    return Tuple.Create(x1, y1, x2, y2);
            }
            return null;
        }

        public static (int, int) GetCharRange(int x1, int y1, int x2, int y2)
        {
            var charCoords = ImageEditor.MainForm.charCoords;
            
            int start = -1;
            int end = -1;
            for (int i = 0; i < charCoords.Count; i++)
            {
                var (_, (cx1, cy1, cx2, cy2)) = charCoords[i];
                if (cx1 == x1 && cy1 == y1)
                {
                    start = i;
                }
                if (cx2 == x2 && cy2 == y2)
                {
                    end = i;
                    break;
                }
            }
            return (start, end);
        }
        
        public static void GetInitialScalePos()
        {
            var pictureBox = ImageEditor.MainForm.pictureBox;
            var containerPanel = ImageEditor.MainForm.containerPanel;
            var fusedImage = ImageEditor.MainForm.fusedImage;

            if (fusedImage == null)
                return;

            int targetWidth = containerPanel.ClientSize.Width; // Szerokość ramki
            int targetHeight = containerPanel.ClientSize.Height; // Wysokość ramki
            int scaledWidth = 0;
            int scaledHeight = 0;
            float initialScale = (float)targetWidth / fusedImage.Width; // Współczynnik skalowania
            if ((int)(fusedImage.Height * initialScale) > targetHeight) { // Jeśli obraz jest za wysoki
                initialScale = (float)targetHeight / fusedImage.Height; // Współczynnik skalowania
                scaledWidth = (int)(fusedImage.Width * initialScale); // Nowa szerokość
                scaledHeight = (int)(fusedImage.Height * initialScale); // Nowa wysokość
            } else {
                scaledWidth = (int)(fusedImage.Width * initialScale); // Nowa szerokość
                scaledHeight = (int)(fusedImage.Height * initialScale); // Nowa wysokość
            }

            Bitmap resizedImage = new Bitmap(fusedImage, new Size(scaledWidth, scaledHeight)); // Zmiana rozmiaru obrazu

            // Wyświetlenie obrazu w PictureBox
            pictureBox.Image = resizedImage;
            pictureBox.Size = new Size(resizedImage.Width, resizedImage.Height);
            pictureBox.Location = new Point((containerPanel.ClientSize.Width - pictureBox.Width) / 2, (containerPanel.ClientSize.Height - pictureBox.Height) / 2);
            ImageEditor.MainForm.scaleFactor = 1.0f; // Zresetowanie współczynnika skali        
            ImageEditor.MainForm.initialScale = initialScale; // Zapisanie współczynnika skali
        }    
    
        public static (int, int) AdaptImageToPanel(int newLeft, int newTop)
        {
            var pictureBox = ImageEditor.MainForm.pictureBox;
            var containerPanel = ImageEditor.MainForm.containerPanel;
            
            // Ograniczenie do obszaru panelu kontenerowego, jeśli obraz jest mniejszy lub równy od panelu
            if (pictureBox.Width <= containerPanel.ClientSize.Width)
            {
                newLeft = Math.Max(containerPanel.ClientRectangle.Left, Math.Min(containerPanel.ClientRectangle.Right - pictureBox.Width, newLeft));
            }
            if (pictureBox.Height <= containerPanel.ClientSize.Height)
            {
                newTop = Math.Max(containerPanel.ClientRectangle.Top, Math.Min(containerPanel.ClientRectangle.Bottom - pictureBox.Height, newTop));
            }

            // Ograniczenie do obszaru obrazu, jeśli obraz jest większy od tablicy
            if (pictureBox.Width > containerPanel.ClientSize.Width)
            {
                newLeft = Math.Min(0, Math.Max(containerPanel.ClientSize.Width - pictureBox.Width, newLeft));
            }
            if (pictureBox.Height > containerPanel.ClientSize.Height)
            {
                newTop = Math.Min(0, Math.Max(containerPanel.ClientSize.Height - pictureBox.Height, newTop));
            }

            return (newLeft, newTop);
        }
        
        public static void  MoveImage(MouseEventArgs e, Point dragStart)
        {
            
            var pictureBox = ImageEditor.MainForm.pictureBox;
            int newLeft = pictureBox.Left + e.X - dragStart.X;  // Obliczenie nowej pozycji poziomej
            int newTop = pictureBox.Top + e.Y - dragStart.Y;  // Obliczenie nowej pozycji pionowej

            (newLeft, newTop) = AdaptImageToPanel(newLeft, newTop);

            pictureBox.Location = new Point(newLeft, newTop);  // Ustawienie nowej pozycji obrazu
        }

        public static void ScaleImage(MouseEventArgs e)
        {
            var pictureBox = ImageEditor.MainForm.pictureBox;
            var fusedImage = ImageEditor.MainForm.fusedImage;
            var initialScale = ImageEditor.MainForm.initialScale;
            var scaleFactor = ImageEditor.MainForm.scaleFactor;


            if (fusedImage == null) // Skalowanie działa tylko z jeśli obraz istnieje
                return;
            // Ustawienie współczynnika skalowania
            float zoomStep = 0.1f; // O ile powiększać/pomniejszać na każdy "scroll"
            scaleFactor = scaleFactor + (e.Delta > 0 ? zoomStep : -zoomStep);

            // Ograniczenie zakresu skalowania
            if (scaleFactor < 0.5f || scaleFactor > 2.0f) // Min i Max skalowanie
                return;

            // Obliczenie nowych wymiarów obrazu
            int newWidth = (int)(fusedImage.Width * scaleFactor * initialScale);
            int newHeight = (int)(fusedImage.Height * scaleFactor * initialScale);


            pictureBox.Image = new Bitmap(fusedImage, new Size(newWidth, newHeight));
            pictureBox.Size = new Size(newWidth, newHeight);

            // Obliczenie przesunięcia, aby środek zoomu był zgodny z pozycją myszy
            int mouseOffsetX = e.X - pictureBox.Left;
            int mouseOffsetY = e.Y - pictureBox.Top;
            int newLeft = pictureBox.Left - (int)((mouseOffsetX * (scaleFactor / scaleFactor)) - mouseOffsetX);
            int newTop = pictureBox.Top - (int)((mouseOffsetY * (scaleFactor / scaleFactor)) - mouseOffsetY);

            (newLeft, newTop) = AdaptImageToPanel(newLeft, newTop);

            pictureBox.Location = new Point(newLeft, newTop);  // Ustawienie nowej pozycji obrazu

            ImageEditor.MainForm.scaleFactor = scaleFactor;
        }

        public static Bitmap AddRhyme(int x1, int y1, int x2, int y2, bool erase = false)
        {
            var rhymeImage = ImageEditor.MainForm.rhymeImage;
            var color = ImageEditor.MainForm.highLightColor;
            var alpha = ImageEditor.MainForm.highLightAlpha;

    
            if (rhymeImage == null)
            {
                MessageBox.Show("Brak obrazu z rymami", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // wyczyszczenie tego obszaru
            for (int i = x1; i < x2 && i < rhymeImage.Width; i++)
            {
                for (int j = y1; j < y2 && j < rhymeImage.Height; j++)
                {
                    // Ustawienie koloru piksela na przezroczysty
                    Color transparentColor = Color.FromArgb(0, 0, 0, 0); // Alpha = 0 oznacza pełną przezroczystość
                    rhymeImage.SetPixel(i, j, transparentColor);
                }
            }


            if (erase)
            {
                InfoSave.SaveInfo.editHighLightList(x1, y1, x2, y2, Color.Transparent);
                return rhymeImage;
            }

            
            InfoSave.SaveInfo.editHighLightList(x1, y1, x2, y2, color);
            // draw rectangle on rhymeImage
            using (Graphics g = Graphics.FromImage(rhymeImage))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(alpha, color)), x1, y1, x2 - x1, y2 - y1);
            }
            return rhymeImage;
        }
    
    }
}
