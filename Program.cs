using System.Drawing.Imaging;
using ImageModify;
using ProjectOpen;
using RhymeHighlight;
using Tools;
using InfoSave;

namespace ImageEditor
{
    // Główna klasa formularza aplikacji
    public class MainForm : Form
    {
        // DEBUG
        public static bool showRectangles = true;    // Flaga, która informuje, czy pokazywać prostokąty

        public static PictureBox pictureBox;          // Obiekt do wyświetlania obrazu
        public static Panel containerPanel;           // Panel, w którym znajduje się PictureBox
        public static Bitmap textImage;
        public static Bitmap rhymeImage;
        public static Bitmap fusedImage;
        public static bool isHighLightOnTheTop = true;
        // Zmienne do przechowywania obrazów i współrzędnych znaków
        public static List<Tuple<string, Tuple<int, int, int, int>>> charCoords = null;
        public static int distanceBetweenLines = 5;
        public static float initialScale = 1.0f;      // Współczynnik skali obrazu

        public static string mainDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @""));
        public static string filesDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\files"));
        public static string fontsDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\files\fonts"));
        public static string imagesDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\files\images"));
        public static string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\files\project"));
        public static string songsDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\songs\"));
        public static string actualProjectDirectory = "";
        public static Label highLightLabel;
        public static bool isCharHighlighted = false;
        public static bool isRhymeMarking = false;    // Flaga, która informuje, czy zaznaczanie jest włączone
        public static Color highLightColor;
        public static int highLightAlpha = 95;
        public static float scaleFactor = 1.0f;       // Współczynnik skali obrazu
        public static Label colorLabel;
        public static int colorIndex = 0;  // Indeks koloru z listy kolorów
        public static List<Color> colors = null;
        public static Color textColor = Color.FromArgb(255, 255, 255);
        public static String textFont = filesDirectory + @"\fonts\font.ttf";

        public static List<Highlights> highLightList = new List<Highlights>();
        // Default project settings
        public static Color backgroundColor = Color.FromArgb(60, 60, 60);
        
        
        

        private int songPartIndex = 0;


        private OpenFileDialog openFileDialog;  // Dialog do otwierania pliku obrazu
        private Label imageXY;
        
        private Label centerLabel;
        private Button hiddenButton;
 
        private float PanelWidth = 0.12f;  // Szerokość panelu na przyciski
        private float containerWidth = 0.7f;   // Szerokość panelu konteneroweg
        private float containerHeight = 0.8f;   // Wysokość panelu kontenerowego



        private Point dragStart;                // Punkt, w którym rozpoczęto przeciąganie obrazu
        private bool isDragging = false;        // Flaga, która informuje, czy obraz jest przeciągany
        private bool isCtrlPressed = false;     // Flaga, która informuje, czy klawisz Ctrl jest wciśnięty
        private bool isWindowActive = true;     // Flaga, która informuje, czy okno jest aktywne


        private Panel leftPanel = new Panel();  // Panel na przyciski
        private Panel rightPanel = new Panel(); // Panel na przyciski
        

        // Zmienne do przechowywania kolorów
        Tuple<int, int, int, int> lastCoor = null;
        Tuple<int, int, int, int> lastExtendedCoor = null;

        List<string> actualProjectDirectories = new List<string>();
        private float brighterHighlight = 1.75f;  // Przezroczystość podświetlenia

        public MainForm()
        {
            // {
            //     new Highlights { Range = new int[] { 1, 2 }, Color = new int[] { 255, 0, 0 } },
            //     new Highlights { Range = new int[] { 3, 4 }, Color = new int[] { 0, 255, 0 } },
            //     new Highlights { Range = new int[] { 5, 6 }, Color = new int[] { 0, 0, 255 } }
            // };
            highLightList.Add(new Highlights { Range = new int[] { 15, 24 }, Color = new int[] { 255, 255, 255 } });
            // Istniejąca konfiguracja okna
            this.Width = 1200;
            this.Height = 400;
            this.MinimumSize = new Size(750, 450);
            this.Text = $"Image Viewer - {this.ClientSize.Width}x{this.ClientSize.Height}";

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyPress;
            this.KeyUp += MainForm_KeyRelease;

            // Pobierz kolory dla rymów
            colors = ColorTools.getRhymeColors();
            
            // Panel na przyciski po lewej stronie
            leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;  // Przyczepienie panelu do lewej strony
            leftPanel.Width = (int)(PanelWidth * this.ClientSize.Width);       // Ustawienie szerokości panelu
            leftPanel.BackColor = Color.LightGray;  // Kolor tła panelu
            this.Controls.Add(leftPanel);     // Dodanie panelu do okna

            // Panel na przyciski po prawej stronie
            rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Right;  // Przyczepienie panelu do prawej strony
            rightPanel.Width = (int)(PanelWidth * this.ClientSize.Width);       // Ustawienie szerokości panelu
            rightPanel.BackColor = Color.LightGray;  // Kolor tła panelu
            this.Controls.Add(rightPanel);     // Dodanie panelu do okna

            // Panel kontenerowy dla obrazu
            containerPanel = new Panel();
            containerPanel.BackColor = Color.Gray;
            containerPanel.Size = new Size((int)(this.ClientSize.Width * containerWidth), (int)(this.ClientSize.Height * containerHeight));
            containerPanel.Location = new Point((this.ClientSize.Width - containerPanel.Width) / 2, (this.ClientSize.Height - containerPanel.Height) / 2);
            containerPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(containerPanel);

            // PictureBox
            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BackColor = Color.Transparent;
            // Dodaj obsługę zdarzeń myszy dla PictureBox
            pictureBox.MouseDown += PictureBoxMousePress;
            pictureBox.MouseClick += PictureBoxMouseClick;
            pictureBox.MouseUp += PictureBoxMouseRelease;
            pictureBox.MouseMove += PictureBoxMouseMove;
            pictureBox.MouseLeave += PictureBoxMouseOut;
            containerPanel.Controls.Add(pictureBox);

            
            // Przycisk do dopasowania obrazu do ramki
            var initPosButton = new Button();
            initPosButton.Text = "Init Position";
            initPosButton.Dock = DockStyle.Top;
            initPosButton.Height = 40;
            initPosButton.Click += InitPosButton_Click;
            leftPanel.Controls.Add(initPosButton);

            // Przycisk do centrowania obrazu
            var centerButton = new Button();
            centerButton.Text = "Center Image";
            centerButton.Dock = DockStyle.Top;
            centerButton.Height = 40;
            centerButton.Click += CenterButton_Click;
            leftPanel.Controls.Add(centerButton);

            // Przycisk do tworzenia projektu
            var openMainFolderButton = new Button();
            openMainFolderButton.Text = "Open Main Folder";
            openMainFolderButton.Dock = DockStyle.Top;
            openMainFolderButton.Height = 40;
            openMainFolderButton.Click += OpenMainFolderButton_Click;
            leftPanel.Controls.Add(openMainFolderButton);

            // Przycisk do otworzenia projektu
            var openProjectButton = new Button();
            openProjectButton.Text = "Open Project";
            openProjectButton.Dock = DockStyle.Top;
            openProjectButton.Height = 40;
            openProjectButton.Click += OpenProjectButton_Click;
            leftPanel.Controls.Add(openProjectButton);



            // Label z pozycja x i y myszki na obrazie
            imageXY = new Label();
            imageXY.Text = "x: 0, y: 0";
            imageXY.Dock = DockStyle.Bottom;
            imageXY.Height = 40;
            leftPanel.Controls.Add(imageXY);
            
            

            highLightColor = colors[0];
            // Aktualnie wybrany kolor
            colorLabel = new Label();
            colorLabel.Dock = DockStyle.Bottom;
            colorLabel.Height = 40;
            colorLabel.BackColor = highLightColor;
            leftPanel.Controls.Add(colorLabel);


            // Tworzymy mały element (np. Label)
            highLightLabel = new Label();
            highLightLabel.MouseMove += highLightLabelMouseMove;
            highLightLabel.MouseClick += highLightLabelMouseClick;
            pictureBox.Controls.Add(highLightLabel); // Dodanie nakładki do PictureBox

            // Konfiguracja OpenFileDialog
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            this.MouseWheel += MainForm_MouseWheel;
            this.Resize += MainForm_Resize;
            this.Deactivate += new EventHandler(MainForm_Deactivated);
            this.Activated += new EventHandler(MainForm_Activated);


            // Tworzenie labela
            centerLabel = new Label();
            centerLabel.Text = "";
            centerLabel.Font = new Font("Arial", 15, FontStyle.Bold);
            centerLabel.Dock = DockStyle.Top;
            centerLabel.Height = 40;
            centerLabel.Width = rightPanel.Width;
            centerLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Tworzenie lewego przycisku
            Button leftButton = new Button();
            leftButton.Text = "<";
            leftButton.Font = new Font("Arial", 15, FontStyle.Bold);
            leftButton.Size = new Size(rightPanel.Width / 2, 30); // Szerokość 50% panelu
            leftButton.Location = new Point(0, centerLabel.Height); // Pozycja poniżej etykiety
            leftButton.Click += new EventHandler(PrevSongPart_Click);

            // Tworzenie prawego przycisku
            Button rightButton = new Button();
            rightButton.Text = ">";
            rightButton.Font = new Font("Arial", 15, FontStyle.Bold);
            rightButton.Size = new Size(rightPanel.Width / 2, 30); // Szerokość 50% panelu
            rightButton.Location = new Point(rightPanel.Width / 2, centerLabel.Height); // Pozycja poniżej etykiety
            rightButton.Click += new EventHandler(NextSongPart_Click);

            // Dodanie kontrolek do panelu
            rightPanel.Controls.Add(centerLabel);
            rightPanel.Controls.Add(leftButton);
            rightPanel.Controls.Add(rightButton);

            // Obsługa zmiany rozmiaru panelu
            rightPanel.Resize += (sender, e) =>
            {
                // Aktualizacja rozmiaru i pozycji przycisków
                leftButton.Size = new Size(rightPanel.Width / 2, 30);
                rightButton.Size = new Size(rightPanel.Width / 2, 30);
                leftButton.Location = new Point(0, centerLabel.Height);
                rightButton.Location = new Point(rightPanel.Width / 2, centerLabel.Height);
                centerLabel.Width = rightPanel.Width;
            };
            
            
            
            






            hiddenButton = new Button();
            hiddenButton.Size = new Size(0, 0); // Ukryty rozmiar
            hiddenButton.Location = new Point(-10, -10); // Poza widocznym obszarem
            hiddenButton.TabStop = false; // Wyłączenie przechodzenia do niego przez Tab
            rightPanel.Controls.Add(hiddenButton);

            // Ustawienie fokusu na ukrytym przycisku
            this.Load += (sender, e) =>
            {
                hiddenButton.Focus();
            };
        }

        // Metoda obsługująca kliknięcie przycisku centrowania obrazu
        private void CenterButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
                return;

            // Centrowanie obrazu w panelu kontenerowym
            pictureBox.Location = new Point(
                (containerPanel.ClientSize.Width - pictureBox.Width) / 2,
                (containerPanel.ClientSize.Height - pictureBox.Height) / 2
            );
        } 

        private void InitPosButton_Click(object sender, EventArgs e)
        {
            ModifyImage.GetInitialScalePos();
        }

        
        private void OpenMainFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", mainDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się otworzyć folderu: " + ex.Message);
            }
        }
        
        private void OpenProjectPart(EventArgs e)
        {
            string getLastFolder(string path)
            {
                // Znajdź ostatni indeks "\" w ścieżce
                int lastBackslashIndex = path.LastIndexOf('\\');

                if (lastBackslashIndex != -1)
                {
                    // Wyciągnij część ścieżki po ostatnim "\" (czyli ostatni folder)
                    string lastFolder = path.Substring(lastBackslashIndex + 1);
                    return lastFolder;
                }
                else
                    return null;
            }
            
            if (actualProjectDirectories.Count == 0)
                return;
            
            actualProjectDirectory = actualProjectDirectories[songPartIndex];

            centerLabel.Text = $"Część: {songPartIndex + 1}/{actualProjectDirectories.Count}";
            string songPart = getLastFolder(actualProjectDirectory);
            if (songPart == null)
            {
                MessageBox.Show("Błędna ścieżka do pliku: " + actualProjectDirectory);
                return;
            }
            textImage = ModifyImage.CreateTextImage();
            // make red
            rhymeImage = new Bitmap(textImage.Width, textImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(rhymeImage))
            {
                g.Clear(Color.Transparent);
            }

            fusedImage = OpenProject.LoadImage(e);
            ModifyImage.GetInitialScalePos();
        }
        
        private void OpenProjectButton_Click(object sender, EventArgs e)
        {
            if (actualProjectDirectories.Count != 0)
            {
                DialogResult dialogResult = MessageBox.Show("Czy na pewno chcesz otworzyć nowy projekt? Wszystkie zmiany zostaną utracone.", "Ostrzeżenie", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes)
                    return;
            }
            actualProjectDirectories = new List<string>();
            actualProjectDirectories = OpenProject.ChooseProject();
            OpenProjectPart(e);
        }
        
        private void NextSongPart_Click(object sender, EventArgs e)
        {
            if (songPartIndex < actualProjectDirectories.Count - 1)
                songPartIndex += 1;
            OpenProjectPart(e);
        }
        
        
        private void PrevSongPart_Click(object sender, EventArgs e)
        {
            if (songPartIndex > 0)
                songPartIndex -= 1;
            OpenProjectPart(e);
        }
        // Metoda rejestruje wciśnięcie przycisku myszy na PictureBox
        private void PictureBoxMousePress(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isCtrlPressed)  // Jeśli kliknięto lewy przycisk myszy
            {
                isDragging = true;  // Ustawienie flagi wskazującej, że obraz jest przeciągany
                dragStart = new Point(e.X, e.Y);  // Zapamiętanie punktu, w którym rozpoczęto przeciąganie
            }
        }

        // Metoda rejestruje kliknięcie myszy na PictureBox
        private void PictureBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (isCharHighlighted && isRhymeMarking && !isCtrlPressed)  // Jeśli kliknięto lewy przycisk myszy
                {
                    var (x1, y1, x2, y2) = lastExtendedCoor;
                    rhymeImage = ModifyImage.AddRhyme(x1, y1, x2, y2);
                    fusedImage = ModifyImage.GetFusedImage();
                    pictureBox.Image = new Bitmap(fusedImage, new Size(pictureBox.Width, pictureBox.Height));

                    lastExtendedCoor = null;
                    lastCoor = null;
                    isRhymeMarking = false;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (isCharHighlighted && !isCtrlPressed && isRhymeMarking)  // Jeśli kliknięto prawy przycisk myszy
                {
                    HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                    lastExtendedCoor = null;
                    lastCoor = null;
                    isRhymeMarking = false;
                }
            }
        }

        // Metoda rejestruje przesunięcie myszy na PictureBox
        private void PictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (!isWindowActive)
                return;
            // Jeśli Ctrl jest wciśnięty i obraz jest przeciągany
            if (isCtrlPressed && isDragging && pictureBox.Image != null)
                ModifyImage.MoveImage(e, dragStart);
            if (fusedImage != null && !isCtrlPressed)  // Jeśli obraz istnieje i Ctrl nie jest wciśnięty
            {
                // Obliczanie współrzędnych obrazu
                int imageX = (int)((e.X) / scaleFactor / initialScale);
                int imageY = (int)((e.Y) / scaleFactor / initialScale);
                if (isRhymeMarking)
                {   
                    Tuple<int, int, int, int> coor = ModifyImage.GetCharCoords(imageX, lastCoor.Item2);
                    if (coor != null && coor != lastCoor && coor != lastExtendedCoor)
                    {
                        if (coor.Item1 >= lastCoor.Item1)
                        {
                            var (x1, y1, _, _) = lastCoor;
                            var (_, _, x2, y2) = coor;
                            coor = new Tuple<int, int, int, int>(x1, y1, x2, y2);
                        }
                        else
                        {
                            var (_, _, x2, y2) = lastCoor;
                            var (x1, y1, _, _) = coor;
                            coor = new Tuple<int, int, int, int>(x1, y1, x2, y2);
                        }
                        HighLightRhyme.createBasicHighlight(coor, scaleFactor, this, highLightColor, highLightAlpha);
                        lastExtendedCoor = coor;
                    }
                }
                else
                {
                    Tuple<int, int, int, int> coor = ModifyImage.GetCharCoords(imageX, imageY);
                    if (coor == null)
                    {
                        if (isCharHighlighted)
                            HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                        return;
                    }
                    int alpha = (int)(highLightAlpha * brighterHighlight);
                    if (coor != lastCoor)
                        HighLightRhyme.createBasicHighlight(coor, scaleFactor, this, highLightColor, alpha);
                    lastCoor = coor;
                }
            }
        }

        // Obsługuje poruszanie myszką na oknie
        private void PictureBoxMouseOut(object sender, EventArgs e)
        {
            // jesli kursor nie jest na highlight
            Point cursorPosition = pictureBox.PointToClient(Cursor.Position);
            if (!highLightLabel.Bounds.Contains(cursorPosition) && isCharHighlighted)
            {
                HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                lastExtendedCoor = null;
                lastCoor = null;
                isRhymeMarking = false;
            }
        }

        // Metoda obsługująca zwolnienie przycisku myszy na PictureBox
        private void PictureBoxMouseRelease(object sender, MouseEventArgs e)
        {
            if (fusedImage == null)
                return;
            if (e.Button == MouseButtons.Left && isCtrlPressed)  // Jeśli zwolniono lewy przycisk myszy
                isDragging = false;  // Ustawienie flagi na false, co oznacza zakończenie przeciągania
            else if (e.Button == MouseButtons.Left && !isCtrlPressed)  // Jeśli nie jest wciśnięty klawisz Ctrl
            {
                // WROC WRÓĆ
                ModifyImage.GetFusedImage();
                pictureBox.Image = new Bitmap(fusedImage, new Size(pictureBox.Width, pictureBox.Height));
            }
        }
        
        // Metoda obsługująca poruszanie myszką na podświetleniu
        private void highLightLabelMouseMove(object sender, MouseEventArgs e)
        {
            if (!isWindowActive)
                return;
            if (fusedImage != null && !isCtrlPressed)  // Jeśli obraz istnieje i Ctrl nie jest wciśnięty
            {
                Point posHighLight = highLightLabel.Location;
                Point posMouse = highLightLabel.PointToClient(Cursor.Position);
                int x = posHighLight.X + posMouse.X;
                int y = posHighLight.Y + posMouse.Y;

                // Obliczanie współrzędnych obrazu
                int imageX = (int)((x) / scaleFactor / initialScale);
                int imageY = (int)((y) / scaleFactor / initialScale);
                if (isRhymeMarking)
                {
                    Tuple<int, int, int, int> coor = ModifyImage.GetCharCoords(imageX, lastCoor.Item2);
                    if (coor != null && coor != lastCoor && coor != lastExtendedCoor)
                    {
                        if (coor.Item1 >= lastCoor.Item1)
                        {
                            var (x1, y1, _, _) = lastCoor;
                            var (_, _, x2, y2) = coor;
                            coor = new Tuple<int, int, int, int>(x1, y1, x2, y2);
                        }
                        else
                        {
                            var (_, _, x2, y2) = lastCoor;
                            var (x1, y1, _, _) = coor;
                            coor = new Tuple<int, int, int, int>(x1, y1, x2, y2);
                        }
                        HighLightRhyme.createBasicHighlight(coor, scaleFactor, this, highLightColor, highLightAlpha);
                        lastExtendedCoor = coor;
                    }
                }
                else
                {
                    Tuple<int, int, int, int> coor = ModifyImage.GetCharCoords(imageX, imageY);
                    int alpha = (int)(highLightAlpha * brighterHighlight);
                    HighLightRhyme.createBasicHighlight(coor, scaleFactor, this, highLightColor, alpha);
                }
            }
        }
        
        // Metoda obsługująca kliknięcie myszą na podświetleniu
        private void highLightLabelMouseClick(object sender, MouseEventArgs e)
        {
            if (!isWindowActive || !isCharHighlighted)
                return;
            if (e.Button == MouseButtons.Left)
            {
                if (isCharHighlighted && !isCtrlPressed && !isRhymeMarking)  // Jeśli kliknięto lewy przycisk myszy
                {
                    isRhymeMarking = true;
                }
                else if (isCharHighlighted && isRhymeMarking && !isCtrlPressed)  // Jeśli kliknięto lewy przycisk myszy
                {    
                    var (x1, y1, x2, y2) = lastExtendedCoor;
                    rhymeImage = ModifyImage.AddRhyme(x1, y1, x2, y2);
                    fusedImage = ModifyImage.GetFusedImage();
                    pictureBox.Image = new Bitmap(fusedImage, new Size(pictureBox.Width, pictureBox.Height));

                    isRhymeMarking = false;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (isCharHighlighted && !isCtrlPressed && isRhymeMarking)  // Jeśli kliknięto prawy przycisk myszy
                {
                    HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                    lastExtendedCoor = null;
                    lastCoor = null;
                    isRhymeMarking = false;
                }
            }
        }
  

        // Metoda obsługująca wciśnięcie klawisza
        private void MainForm_KeyPress(object sender, KeyEventArgs e)
        {
            if (!isWindowActive)
                return;
            if (e.KeyCode == Keys.ControlKey) {
                if (isCharHighlighted)
                    HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                isCtrlPressed = true;
                this.Cursor = Cursors.SizeAll;
            }
        }

        // Metoda obsługująca zwolnienie klawisza
        private void MainForm_KeyRelease(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey) {
                isCtrlPressed = false;
                this.Cursor = Cursors.Default;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (isCharHighlighted)
                    HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
                isRhymeMarking = false;
                lastExtendedCoor = null;
                lastCoor = null;
            }
        }
        
        // Metoda obsługująca przewijanie kółkiem myszy w głównym oknie
        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!isWindowActive)
                return;
            if (isCtrlPressed) // Skalowanie działa tylko z Ctrl
                ModifyImage.ScaleImage(e);
            else if (e.Delta > 0) // Jeśli przewijanie w górę 
            {
                ColorTools.colorScrollPlus();
                if (isCharHighlighted)
                    HighLightRhyme.createBasicHighlight(lastCoor, scaleFactor, this, highLightColor, highLightAlpha);
            }
            else if (e.Delta < 0) // Jeśli przewijanie w dół
            {
                ColorTools.colorScrollMinus();
                if (isCharHighlighted)
                    HighLightRhyme.createBasicHighlight(lastCoor, scaleFactor, this, highLightColor, highLightAlpha);
            }
 
            imageXY.Text = $"{colorIndex}";
        }

        // Obsługuje zmianę rozmiaru okna
        private void MainForm_Resize(object sender, EventArgs e)
        {
            containerPanel.Size = new Size((int)(this.ClientSize.Width * containerWidth), (int)(this.ClientSize.Height * containerHeight));
            containerPanel.Location = new Point((this.ClientSize.Width - containerPanel.Width) / 2, (this.ClientSize.Height - containerPanel.Height) / 2);
            leftPanel.Width = (int)(PanelWidth * this.ClientSize.Width);
            rightPanel.Width = (int)(PanelWidth * this.ClientSize.Width);
            

            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0) // minimalizowanie okna
            {
                return;
            }
            
            if (pictureBox.Image != null)
            {
                ModifyImage.GetInitialScalePos();
            }

            // Dodanie aktualnego rozmiaru okna do tytułu
            this.Text = $"Image Viewer - {this.ClientSize.Width}x{this.ClientSize.Height}";
        }

        // Obsługuje aktywację okna
        private void MainForm_Activated(object sender, EventArgs e)
        {
            isWindowActive = true;
        }
        
        // Obsługuje deaktywację okna
        private void MainForm_Deactivated(object sender, EventArgs e)
        {
            if (isCharHighlighted)
                HighLightRhyme.removeBasicHighlight(this); // Usunięcie podświetlenia
            isCtrlPressed = false;
            this.Cursor = Cursors.Default;
            isWindowActive = false;
        }   
    }

    // Klasa uruchamiająca aplikację
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
