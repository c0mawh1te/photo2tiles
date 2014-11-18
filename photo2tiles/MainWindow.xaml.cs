using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace photo2tiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        BitmapImage map;
        Tile[] tiles;

        int datum;
        int zoomLevel;

        double tl_lat, tr_lat;
        double bl_lat, br_lat;
        double tl_lon, tr_lon;
        double bl_lon, br_lon;

        string mapName;
        string mapPath;


        public MainWindow()
        {
            InitializeComponent();
        }


        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".jpg";
            ofd.Filter = "Изображение (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (ofd.ShowDialog() == true)
            {
                map = new BitmapImage(new Uri(ofd.FileName));
                mapImage.Source = map;
                loadButton.Visibility = Visibility.Collapsed;
                mapImage.Visibility = Visibility.Visible;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!checkFields())
                return;

            calcZoomLevel();
            convertCoordinates();
            cropImageForTiles();

            if (saveTiles())
            {
                saveMetainfo();

                string message = "Карта \"" + mapName + "\" была сохранена в " + mapPath;
                MessageBox.Show(message, " ", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }


        private bool checkFields()
        {
            if (loadButton.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Изображение не выбрано!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Double.TryParse(tl_lat_text.Text, out tl_lat) ||
                !Double.TryParse(tr_lat_text.Text, out tr_lat) ||
                !Double.TryParse(bl_lat_text.Text, out bl_lat) ||
                !Double.TryParse(br_lat_text.Text, out br_lat) ||
                !Double.TryParse(tl_lon_text.Text, out tl_lon) ||
                !Double.TryParse(tr_lon_text.Text, out tr_lon) ||
                !Double.TryParse(bl_lon_text.Text, out bl_lon) ||
                !Double.TryParse(br_lon_text.Text, out br_lon))
            {
                MessageBox.Show("Неверно заданы координаты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void calcNewCoordinates()
        {
            int height = map.PixelHeight;
            int width = map.PixelWidth;

            double deltaLat = Math.Abs(bl_lat - tl_lat);
            double deltaLon = Math.Abs(tr_lon - tl_lon);

            double latInPix = deltaLat / height;
            double lonInPix = deltaLon / width;

            int yTiles = height / 256;
            int xTiles = width / 256;

            int newHeight = yTiles * 256;
            int newWidth = xTiles * 256;

            if(bl_lat > tl_lat)
                bl_lat = latInPix * newHeight + tl_lat;
            else
                bl_lat = tl_lat - latInPix * newHeight;

            if (br_lon > bl_lon)
                br_lon = lonInPix * newWidth + bl_lon;
            else
                br_lon = bl_lon - lonInPix * newWidth;

            br_lat = bl_lat;
            tr_lon = br_lon;

        }

        private void convertCoordinates()
        {
            if (datum == 0)
                return;

            CoordinatesConverter cc = new CoordinatesConverter();
            cc.convert(tl_lat, tl_lon, datum);
            tl_lat = cc.getLat();
            tl_lon = cc.getLon();

            cc.convert(tr_lat, tr_lon, datum);
            tr_lat = cc.getLat();
            tr_lon = cc.getLon();

            cc.convert(bl_lat, bl_lon, datum);
            bl_lat = cc.getLat();
            bl_lon = cc.getLon();

            cc.convert(br_lat, br_lon, datum);
            br_lat = cc.getLat();
            br_lon = cc.getLon();
        }

        private void calcZoomLevel()
        {
            double distance = getDistance(tl_lat, tl_lon, tr_lat, tr_lon);
            double resolution = distance / map.PixelWidth;
            // All values are shown for equator, and you have to 
            // multiply them by cos(latitude) to adjust to a given latitude. 
            resolution = resolution / Math.Abs(Math.Cos(tl_lat));

            if (resolution > 78271.53)
                zoomLevel = 0;
            else if (resolution > 39135.76)
                zoomLevel = 1;
            else if (resolution > 19567.88)
                zoomLevel = 2;
            else if (resolution > 9783.94)
                zoomLevel = 3;
            else if (resolution > 4891.97)
                zoomLevel = 4;
            else if (resolution > 2445.98)
                zoomLevel = 5;
            else if (resolution > 1222.99)
                zoomLevel = 6;
            else if (resolution > 611.50)
                zoomLevel = 7;
            else if (resolution > 305.75)
                zoomLevel = 8;
            else if (resolution > 152.87)
                zoomLevel = 9;
            else if (resolution > 76.437)
                zoomLevel = 10;
            else if (resolution > 38.219)
                zoomLevel = 11;
            else if (resolution > 19.109)
                zoomLevel = 12;
            else if (resolution > 9.5546)
                zoomLevel = 13;
            else if (resolution > 4.7773)
                zoomLevel = 14;
            else if (resolution > 2.3887)
                zoomLevel = 15;
            else if (resolution > 1.1943)
                zoomLevel = 16;
            else if (resolution > 0.5972)
                zoomLevel = 17;
            else
                zoomLevel = 18;
        }

        private double getDistance(double lat1, double lon1, double lat2, double lon2)
        {
            //координаты в радианы
            lat1 = lat1 * Math.PI / 180;
            lon1 = lon1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            lon2 = lon2 * Math.PI / 180;

            //косинусы и синусы широт
            double cos1 = Math.Cos(lat1);
            double cos2 = Math.Cos(lat2);
            double sin1 = Math.Sin(lat1);
            double sin2 = Math.Sin(lat2);

            //разницы долгот
            double delta = lon2 - lon1;
            double cosDelta = Math.Cos(delta);
            double sinDelta = Math.Sin(delta);

            //вычисления длины большого круга
            double y = Math.Sqrt(Math.Pow(cos2 * sinDelta, 2) + Math.Pow(cos1 * sin2 - sin1 * cos2 * cosDelta, 2));
            double x = sin1 * sin2 + cos1 * cos2 * cosDelta;

            double atan = Math.Atan2(y, x);
            return atan * 6372795;

        }

        private void cropImageForTiles()
        {
            int height = map.PixelHeight;
            int width = map.PixelWidth;

            int xtiles = width / 256;
            int ytiles = height / 256;

            double delta_y = bl_lat - tl_lat;
            double delta_x = tr_lon - tl_lon;

            //double delta_y = Math.Abs(bl_lat - tl_lat);
            //double delta_x = Math.Abs(tr_lon - tl_lon);

            double[] tilesLat = new double[xtiles * ytiles];
            double[] tilesLon = new double[xtiles * ytiles];

            tiles = new Tile[xtiles * ytiles];

            int count = 0;
            for (int i = 0; i < xtiles; i++)
            {
                for (int j = 0; j < ytiles; j++)
                {
                    tilesLon[count] = (delta_x / xtiles * i + tl_lon);
                    tilesLat[count] = (delta_y / ytiles * j + tl_lat);
                    count++;
                }
            }

            count = 0;
            for (int i = 0; i < xtiles; i++)
            {
                for (int j = 0; j < ytiles; j++)
                {
                    tiles[count] = new Tile(
                        new CroppedBitmap(map, new Int32Rect(i * 256, j * 256, 256, 256)),
                        tilesLat[count],
                        tilesLon[count],
                        zoomLevel);
                    count++;
                }
            }
        }

        private bool saveTiles()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файлы карты (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                mapPath = saveFileDialog.FileName;
                mapName = saveFileDialog.SafeFileName;

                BitmapEncoder encoder;
                FileStream stream;


                foreach (Tile tile in tiles)
                {
                    encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(tile.getCroppedBitmap()));

                    if (!Directory.Exists(mapPath + "\\" + zoomLevel + "\\" + tile.getX()))
                    {
                        String path = mapPath + "\\" + zoomLevel + "\\" + tile.getX();
                        Directory.CreateDirectory(path);
                    }

                    stream = new FileStream(mapPath + "\\" + zoomLevel + "\\" + tile.getX() + "\\" + tile.getY() + ".png.tile", FileMode.Create);
                    encoder.Save(stream);
                    stream.Close();
                }
                return true;
            }
                return false;
        }

        private void saveMetainfo()
        {
            string path = mapPath + "\\" + ".metainfo";

            using (StreamWriter sw = File.CreateText(path))
            {

//                sw.WriteLine("[rule]");
//                sw.WriteLine("beanshell");

                sw.WriteLine("[name]");
                sw.WriteLine(mapName);

                sw.WriteLine("[ext]");
                sw.WriteLine(".png");

                sw.WriteLine("[min_zoom]");
                sw.WriteLine(zoomLevel);

                sw.WriteLine("[max_zoom]");
                sw.WriteLine(zoomLevel + 2);

                sw.WriteLine("[title_size]");
                sw.WriteLine("256");

                sw.WriteLine("[img_density]");
                sw.WriteLine("16");

                sw.WriteLine("[avg_img_size]");
                sw.WriteLine("18000");

                sw.WriteLine("[ellipsoid]");
                sw.WriteLine("true ");
            }

            File.SetAttributes(path, FileAttributes.Hidden);
        }
        

        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            switch (rb.Name)
            {
                case "wgs84_rb":
                    datum = 0;
                    break;

                case "pz90_rb":
                    datum = 1;
                    break;

                case "pul42_rb":
                    datum = 2;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wgs84_rb.IsChecked = true;

            //coordinates for debug only

            /*
            tl_lat_text.Text = "53,2087";
            tl_lon_text.Text = "44,9409";

            tr_lat_text.Text = "53,2087";
            tr_lon_text.Text = "45,0806";

            bl_lat_text.Text = "53,169";
            bl_lon_text.Text = "44,9409";
            br_lat_text.Text = "53,169";
            br_lon_text.Text = "45,0806";
            */

            tl_lat_text.Text = "53,210371";
            tl_lon_text.Text = "44,942558";

            tr_lat_text.Text = "53,210371";
            tr_lon_text.Text = "45,080148";

            bl_lat_text.Text = "53,169619";
            bl_lon_text.Text = "44,942558";

            br_lat_text.Text = "53,169619";
            br_lon_text.Text = "45,080148";

        }
    }
}
