using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace photo2tiles
{
    class Tile
    {
        private CroppedBitmap croppedBitmap;
        private double lat;
        private double lon;
        private int zoom;
        private int x;
        private int y;


        public Tile(CroppedBitmap croppedBitmap, double lat, double lon, int zoom)
        {
            this.croppedBitmap = croppedBitmap;
            this.lat = lat;
            this.lon = lon;
            this.zoom = zoom;

            calcTileX();
            calcTileY();
        }

        private double toRadians(double rad)
        {
            return Math.PI * rad / 180;
        }


        private void calcTileX()
        {
            x = (int)Math.Floor((lon + 180) / 360 * (1 << zoom));

            if (x < 0)
                x = 0;
            if (x >= (1 << zoom))
                x = ((1 << zoom) - 1);
        }

        private void calcTileY()
        {
            y = (int)Math.Floor( (1 - Math.Log(Math.Tan(toRadians(lat)) + 1 / Math.Cos(toRadians(lat))) / Math.PI) / 2 * (1<<zoom) ) ;

            if (y < 0)
                y = 0;
            if (y >= (1 << zoom))
                y = ((1 << zoom) - 1);
        }


        /*
        private void calcTileX()
        {
            x = (int)((lon + 180.0) / 360.0 * (1 << zoom));
        }

        private void calcTileY()
        {
            y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
        }
         * */

        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }

        public CroppedBitmap getCroppedBitmap()
        {
            return croppedBitmap;
        }
    }
}
