using ImageEditor;
using ImageModify;
using System.Text.Json;

namespace InfoSave
{
    public class SaveInfo
    {
        public static Tuple<int, int, int> ColorToIntList(Color color)
        {
            int red = color.R;
            int green = color.G;
            int blue = color.B;
            return Tuple.Create(red, green, blue);
        }

        public static Color IntListToColor(Tuple<int, int, int> color)
        {
            int red = color.Item1;
            int green = color.Item2;
            int blue = color.Item3;
            return Color.FromArgb(red, green, blue);
        }

        public static void editHighLightList(int x1, int y1, int x2, int y2, Color color)
        {
            void addHighlightIfNeeded(((int, int), Tuple<int, int, int>) rectangle, ((int, int), Tuple<int, int, int>) defaultRectangle)
            {
                if (rectangle != defaultRectangle)
                    MainForm.highLightList.Add(new Highlights { Range = new[] { rectangle.Item1.Item1, rectangle.Item1.Item2 }, Color = new[] { rectangle.Item2.Item1, rectangle.Item2.Item2, rectangle.Item2.Item3 } });
            }
            
            var colorTuple = ColorToIntList(color);

            var (start, end) = ModifyImage.GetCharRange(x1, y1, x2, y2);
            if (start == -1 || end == -1) 
            {
                MessageBox.Show("Błędne zaznaczenie");
                return;
            }

            var highlights = MainForm.highLightList;
            int len = highlights.Count;
            var allRanges = highlights.Select(h => h.Range).ToList();

            int i = 0;
            ((int, int), Tuple<int, int, int>) defaultRange = ((-1, -1), Tuple.Create(-1, -1, -1));
            var newRectangle = defaultRange;
            var oldRectangle_1 = defaultRange;
            var oldRectangle_2 = defaultRange;

            foreach (var range in allRanges)
            {
                var prevStart = range[0];
                var prevEnd = range[1];
                Tuple<int, int, int> prevColor = Tuple.Create(highlights[i].Color[0], highlights[i].Color[1], highlights[i].Color[2]);

                // Optymalizacja ilości prostokątów
                if (start < prevStart)
                {
                    if (end < prevStart)
                    {
                        newRectangle = ((start, end), colorTuple);
                    }
                    else if (end == prevStart)
                    {
                        newRectangle = ((start, end), colorTuple);
                        oldRectangle_1 = ((end + 1, prevEnd), prevColor);
                        MainForm.highLightList.RemoveAt(i);
                    }
                    else if (end > prevStart)
                    {
                        if (end < prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                        }
                        else if (end == prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end > prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                    }
                    
                }
                else if (start > prevEnd)
                {
                    newRectangle = ((start, end), colorTuple);
                }
                else if (start == prevStart)
                {
                    if (end < prevStart)
                    {
                        MessageBox.Show("BŁĄD 1");
                        return;
                    }
                    else if (end == prevStart)
                    {
                        newRectangle = ((start, end), colorTuple);
                        oldRectangle_1 = ((end + 1, prevEnd), prevColor);
                        MainForm.highLightList.RemoveAt(i);
                    }
                    else if (end > prevStart)
                    {
                        if (end < prevEnd) {
                            newRectangle = ((start, end), colorTuple);
                            oldRectangle_1 = ((end + 1, prevEnd), prevColor);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end == prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end > prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                    }
                }
                else if (start > prevStart)
                {
                    if (end < prevStart)
                    {
                        MessageBox.Show("BŁĄD 2");
                        return;
                    }
                    else if (end == prevStart)
                    {
                        MessageBox.Show("BŁĄD 3");
                        return;
                    }
                    else if (end > prevStart)
                    {
                        if (end < prevEnd)
                        {
                            oldRectangle_1 = ((prevStart, start - 1), prevColor);
                            newRectangle = ((start, end), colorTuple);
                            oldRectangle_2 = ((end + 1, prevEnd), prevColor);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end == prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            oldRectangle_1 = ((prevStart, start - 1), prevColor);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end > prevEnd)
                        {
                            oldRectangle_1 = ((prevStart, start - 1), prevColor);
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                    }
                }
                else if (start < prevEnd)
                {
                    if (end < prevStart)
                    {
                        newRectangle = ((start, end), colorTuple);
                    }
                    else if (end == prevStart)
                    {
                        newRectangle = ((start, end), colorTuple);
                        oldRectangle_1 = ((end + 1, prevEnd), prevColor);
                        MainForm.highLightList.RemoveAt(i);
                    }
                    else if (end > prevStart)
                    {
                        if (end < prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            oldRectangle_1 = ((end + 1, prevEnd), prevColor);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end == prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                        else if (end > prevEnd)
                        {
                            newRectangle = ((start, end), colorTuple);
                            MainForm.highLightList.RemoveAt(i);
                        }
                    }
                }
                else if (start == prevEnd)
                {
                    oldRectangle_1 = ((prevStart, start - 1), prevColor);
                    newRectangle = ((start, end), colorTuple);
                    MainForm.highLightList.RemoveAt(i);
                }
                
                if (newRectangle != defaultRange || oldRectangle_1 != defaultRange || oldRectangle_2 != defaultRange)
                    break;
                i++;
            }

            // sprawdź czy jest błąd zapisu prostokąta
            if (newRectangle.Item1.Item2 < newRectangle.Item1.Item1)
            {
                MessageBox.Show("Błędne zaznaczenie");
                return;
            }
            
            
            // edit highLightList
            if (color.A != 0)
                addHighlightIfNeeded(newRectangle, defaultRange);
            addHighlightIfNeeded(oldRectangle_1, defaultRange);
            addHighlightIfNeeded(oldRectangle_2, defaultRange);
            saveProjectInfo();
        }
    
        public static void saveProjectInfo()
        {
            // Pobieranie ścieżki projektu
            var actualProjectDirectory = MainForm.actualProjectDirectory;
            string destinationPath = Path.Combine(actualProjectDirectory, "projectInfo.json");

            // Pobieranie właściwości projektu
            var textFont = MainForm.textFont.Substring(MainForm.textFont.LastIndexOf('\\') + 1);
            var textColor = new List<dynamic> { ColorToIntList(MainForm.textColor).Item1, ColorToIntList(MainForm.textColor).Item2, ColorToIntList(MainForm.textColor).Item3 };
            var backgroundColor = new List<dynamic> { ColorToIntList(MainForm.backgroundColor).Item1, ColorToIntList(MainForm.backgroundColor).Item2, ColorToIntList(MainForm.backgroundColor).Item3 };
            var highLightAlpha = MainForm.highLightAlpha;
            var isHighLightOnTheTop = MainForm.isHighLightOnTheTop;
            var highLightListJson = MainForm.highLightList;

            // Przygotowanie listy do serializacji
            var highLightList = new List<dynamic>();
            foreach (var highlight in highLightListJson)
            {
                var range = highlight.Range;
                var color = highlight.Color;
                highLightList.Add(new { range, color });
            }

            // Tworzenie obiektu projektu
            var projectInfo = new
            {
                textFont,
                textColor,
                backgroundColor,
                highLightAlpha,
                isHighLightOnTheTop,
                highLightList
            };

            // Serializacja obiektu do JSON
            var json = JsonSerializer.Serialize(projectInfo, new JsonSerializerOptions { WriteIndented = true });

            // Zapis do pliku
            File.WriteAllText(destinationPath, json);
        }
    }

    public class Highlights
    {
        public int[] Range { get; set; }
        public int[] Color { get; set; }
    }
}