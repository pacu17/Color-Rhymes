using System.Text.RegularExpressions;
using System.Text;

namespace ProjectCreate
{
    public class CreateProject
    {
        public static void createSongFolders(string songText, string songPath)
        {
            // Wyrażenie regularne do znalezienia fragmentów w nawiasach kwadratowych
            string pattern = @"\[(.*?)\]";

            // Dopasowanie do tekstu
            MatchCollection matches = Regex.Matches(songText, pattern);

            // Słownik do przechowywania sekcji tekstu
            Dictionary<string, string> sections = new Dictionary<string, string>();

            // Początkowy indeks do podziału tekstu
            int startIndex = 0;
            // Iteracja przez wszystkie dopasowania
            foreach (Match match in matches)
            {
                // Znajdowanie nazwy sekcji (np. "Refren", "Zwrotka 1")
                string sectionName = match.Groups[1].Value;
                int sectionStartIndex = match.Index + match.Length;

                // Znalezienie końca sekcji (następne dopasowanie lub koniec tekstu)
                int sectionEndIndex = startIndex;
                if (match.NextMatch().Success)
                    sectionEndIndex = match.NextMatch().Index;
                else
                    sectionEndIndex = songText.Length;

                // Dodanie sekcji do słownika
                string sectionText = songText.Substring(sectionStartIndex, sectionEndIndex - sectionStartIndex).Trim();
                sections[sectionName] = sectionText;

                // Zaktualizowanie początkowego indeksu
                startIndex = sectionEndIndex;
            }
            bool fragments = true;
            if (sections.Count == 0)
            {
                sections["Całość"] = songText;
                fragments = false;
            }
            foreach (var section in sections)
            {
                try
                {
                    string folderName = $"[{section.Key}]";
                    string disAllowedChars = "<>:\"/\\|?*";
                    foreach (char disAllowedChar in disAllowedChars)
                        folderName = folderName.Replace(disAllowedChar.ToString(), string.Empty);

                    string folderPath = Path.Combine(songPath, folderName);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    
                    // sprawdź czy folder jest pusty
                    if (Directory.GetFiles(folderPath).Length > 0)
                    {
                        // daj możliwość usunięcia zawartości folderu
                        DialogResult dialogResult = MessageBox.Show($"Folder {folderName} nie jest pusty. Czy chcesz usunąć jego zawartość?", "Folder nie jest pusty", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            foreach (string file in Directory.GetFiles(folderPath))
                                File.Delete(file);
                        }
                    }

                    string filePath = Path.Combine(folderPath, "Tekst.txt");
                    
                    if (fragments)
                        File.WriteAllText(filePath, folderName + "\n" + section.Value);
                    else
                        File.WriteAllText(filePath, section.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd dla sekcji {section.Key}: {ex.Message}");
                }
            }
        }

        public static string correctnessOfText(string text)
        {
            string chars = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzźżAĄBCĆDEĘFGHIJKLŁMNŃOÓPQRSŚTUVWXYZŹŻ0123456789,.-?!'\":;[]()„”… \n\r";
            // ' ' - spacja
            int[] everyWhitespaceIndexes = new int[] { 9, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8239, 8287, 12288 }; // 32 właściwa spacja
            // 'e' - e
            int[] eIndexes = new int[] { 1077 };
            // '-' - myślnik
            int[] dashIndexes = new int[] { 8211 };
            
            StringBuilder finalText = new StringBuilder();
            int i = 0;

            foreach (char charInText in text)
            {
                int asciiIndex = (int)charInText;
                char correctedChar = charInText; // Nowa zmienna do przechowywania poprawionego znaku
                
                // Poprawa znaków spacji
                if (Array.Exists(everyWhitespaceIndexes, index => index == asciiIndex))
                    correctedChar = ' ';
                if (Array.Exists(eIndexes, index => index == asciiIndex))
                    correctedChar = 'e';
                if (Array.Exists(dashIndexes, index => index == asciiIndex))
                    correctedChar = '-';
                if (chars.Contains(correctedChar))
                    finalText.Append(correctedChar);

                else if (!",.-?!'\":;()„”…".Contains(correctedChar))
                {
                    // Sprawdzanie poprawności indeksu przed wyciągnięciem kontekstu
                    int start = Math.Max(0, i - 10);
                    int end = Math.Min(10, finalText.Length - i);

                    // Zabezpieczenie przed błędem indeksowania
                    string contextBefore = (finalText.Length > 0) ? finalText.ToString().Substring(start, end) : "";
                    string contextAfter = (i + 1 < text.Length) ? text.Substring(i + 1, Math.Min(10, text.Length - i - 1)) : "";

                    // Wydrukowanie informacji o błędzie
                    MessageBox.Show($"Nieznany znak: {correctedChar} na pozycji {i} - {contextBefore}#{correctedChar}#{contextAfter}. Ascii: {asciiIndex}");
                    return null;
                }
                i++;
            }
            
            return finalText.ToString();
        }

    }
}