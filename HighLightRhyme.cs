using System.Drawing.Drawing2D;
using System.Security.Cryptography.Xml;
using ImageEditor;

namespace RhymeHighlight
{
    public class HighLightRhyme
    {
        public static void removeBasicHighlight(Form ParentForm)
        {
            var highLightLabel = ImageEditor.MainForm.highLightLabel;
            
            highLightLabel.BackColor = Color.FromArgb(0, Color.Transparent); // Kolor nakładki
            highLightLabel.Location = new Point(0, 0); // Przesunięcie nakładki
            highLightLabel.Size = new Size(0, 0); // Rozmiar nakładki
            ParentForm.Cursor = Cursors.Default; // Kursor
            ImageEditor.MainForm.isCharHighlighted = false;
        }
        public static void createBasicHighlight(Tuple<int, int, int, int> coor, float scaleFactor, Form ParentForm, Color color, int alpha)
        {
            var initialScale = ImageEditor.MainForm.initialScale;
            var element = ImageEditor.MainForm.highLightLabel;

            if (coor != null)
            {
                // calculate x and y on window
                int x1 = (int)(coor.Item1 * scaleFactor * initialScale);
                int y1 = (int)(coor.Item2 * scaleFactor * initialScale);
                element.Location = new Point(x1, y1); // Pozycja nakładki
                int width = (int)((coor.Item3 - coor.Item1) * scaleFactor * initialScale);
                int height = (int)((coor.Item4 - coor.Item2) * scaleFactor * initialScale);
                element.Size = new Size(width, height); // Rozmiar nakładki
                element.BackColor = Color.FromArgb(alpha, color); // Kolor nakładki
                if (ImageEditor.MainForm.isRhymeMarking)
                    ParentForm.Cursor = Cursors.Cross; // Kursor
                else
                    ParentForm.Cursor = Cursors.Hand; // Kursor
                ImageEditor.MainForm.isCharHighlighted = true;
            }
        }
    }
}