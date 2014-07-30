using System;
using System.Collections.Generic;
using System.Text;

namespace photo2tiles
{
    class CoordinatesConverter
    {
        const int MODE_WGS_84 = 0;
        const int MODE_PZ_90 = 1;
        const int MODE_PULKOVO_42 = 2;

        const double Pi = Math.PI;

        const double ro = 206264.8062;
        const float aP = 6378245;
        const double alP = 1 / 298.3;

        const float aW = 6378136;
        const double alW = 1 / 298.257839;
        double a = (aP + aW) / 2;
        const double da = aW - aP;

        double e2P;
        double e2W;
        double e2;
        double de2;

        const double dx = 23.92;
        const double dy = -141.27;
        const double dz = -80.9;

        const int wx = 0;
        const int wy = 0;
        const int wz = 0;
        const int ms = 0;

        double lat;
        double lon;

        public CoordinatesConverter()
        {
            e2P = 2 * alP - Math.Pow(alP, 2);
            e2W = 2 * alW - Math.Pow(alW, 2);
            e2 = (e2P + e2W) / 2;
            de2 = e2W - e2P;

        }

        double SK42_WGS84_Lat(double a, double c, double d)
        {
            return a + dB(a, c, d) / 3600;
        }

        double SK42_WGS84_Long(double a, double c, double d)
        {
            return c + dLnew(a, c, d) / 3600;
        }


        double dB(double a, double c, double d)
        {
            double B = a * Pi / 180;
            double L = c * Pi / 180;
            double M = this.a * (1 - e2) / Math.Pow(1 - e2 * Math.Pow(Math.Sin(B), 2), 1.5);
            double N = this.a * Math.Pow(1 - e2 * Math.Pow(Math.Sin(B), 2), -0.5);
            return ro / (M + d) * (N / this.a * e2 * Math.Sin(B) * Math.Cos(B) * da + (Math.Pow(N, 2) / Math.Pow(this.a, 2) + 1) *
                    N * Math.Sin(B) * Math.Cos(B) * de2 / 2 - (dx * Math.Cos(L) + dy * Math.Sin(L)) * Math.Sin(B) + dz *
                    Math.Cos(B)) - wx * Math.Sin(L) * (1 + e2 * Math.Cos(2 * B)) + wy * Math.Cos(L) *
                    (1 + e2 * Math.Cos(2 * B)) - ro * ms * e2 * Math.Sin(B) * Math.Cos(B);
        }

        double dLnew(double a, double c, double d)
        {
            double B = a * Pi / 180;
            double L = c * Pi / 180;
            double N = this.a * Math.Pow(1 - e2 * Math.Pow(Math.Sin(B), 2), -0.5);
            return ro / ((N + d) * Math.Cos(B)) * (-dx * Math.Sin(L) + dy * Math.Cos(L)) + Math.Tan(B) * (1 - e2) *
                    (wx * Math.Cos(L) + wy * Math.Sin(L)) - wz;
        }


        double gradtomx(double a, double c)
        {
            int n = (int)(6 + c) / 6;
            double l0 = (c - (3 + 6 * (n - 1))) / 57.29577915;
            a = a * Pi / 180;
            double x1 = Math.Pow(l0, 2) * (109500 - 574700 * Math.Sin(a) * Math.Sin(a) + 863700 * Math.Pow(Math.Sin(a), 4) - 398600 * Math.Pow(Math.Sin(a), 6));
            double x2 = l0 * l0 * (278194 - 830174 * Math.Pow(Math.Sin(a), 2) + 572434 * Math.Pow(Math.Sin(a), 4) - 16010 * Math.Pow(Math.Sin(a), 6) + x1);
            double x3 = l0 * l0 * (672483.4 - 811219.9 * Math.Pow(Math.Sin(a), 2) + 5420 * Math.Pow(Math.Sin(a), 4) - 10.6 * Math.Pow(Math.Sin(a), 6) + x2);
            double x4 = l0 * l0 * (1594561.25 + 5336.535 * Math.Pow(Math.Sin(a), 2) + 26.79 * Math.Pow(Math.Sin(a),
                    4) + 0.149 * Math.Pow(Math.Sin(a), 6) + x3);
            return 6367558.4968 * a - Math.Sin(2 * a) * (16002.89 + 66.9607 * Math.Pow(Math.Sin(a), 2) + 0.3515 * Math.Pow(Math.Sin(a), 4) - x4);
        }

        double gradtomy(double a, double c)
        {
            a = a * Pi / 180;
            int n = (int)(6 + c) / 6;
            double l = (c - (3 + 6 * (n - 1))) / 57.29577915;
            return 1E5 * (5 + 10 * n) + l * Math.Cos(a) * (6378245 + 21346.1415 * Math.Pow(Math.Sin(a), 2) + 107.159 * Math.Pow(Math.Sin(a), 4) + 0.5977 *
                    Math.Pow(Math.Sin(a), 6) + l * l * (1070204.16 - 2136826.66 * Math.Pow(Math.Sin(a), 2) + 17.98 * Math.Pow(Math.Sin(a), 4) - 11.99 *
                    Math.Pow(Math.Sin(a), 6) + l * l * (270806 - 1523417 * Math.Pow(Math.Sin(a), 2) + 1327645 * Math.Pow(Math.Sin(a), 4) - 21701 * Math.Pow(Math.Sin(a), 6)
                    + l * l * (79690 - 866190 * Math.Pow(Math.Sin(a), 2) + 1730360 * Math.Pow(Math.Sin(a), 4) - 945460 * Math.Pow(Math.Sin(a), 6)))));
        }


        double mtogradb(double a, double c)
        {
            int n = (int)(c * Math.Pow(10, -6));
            double beta = a / 6367558.4968;
            double b0 = beta + Math.Sin(2 * beta) * (0.00252588685 - 1.49186E-5 * Math.Pow(Math.Sin(beta), 2) + 1.1904E-7 * Math.Pow(Math.Sin(beta), 4));
            double z0 = (c - 1E5 * (10 * n + 5)) / (6378245 * Math.Cos(b0));
            double db = -z0 * z0 * Math.Sin(2 * b0) * (0.251684631 - 0.003369263 * Math.Pow(Math.Sin(b0), 2) + 1.1276E-5 *
                    Math.Pow(Math.Sin(b0), 4) - z0 * z0 * (0.10500614 - 0.04559916 * Math.Pow(Math.Sin(b0), 2) + 0.00228901 *
                    Math.Pow(Math.Sin(b0), 4) - 2.987E-5 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 * (0.042858 - 0.025318 * Math.Pow(Math.Sin(b0),
                    2) + 0.014346 * Math.Pow(Math.Sin(b0), 4) - 0.001264 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 *
                    (0.01672 - 0.0063 * Math.Pow(Math.Sin(b0), 2) + 0.01188 * Math.Pow(Math.Sin(b0), 4) - 0.00328 * Math.Pow(Math.Sin(b0), 6)))));
            return 180 * (b0 + db) / Pi;
        }

        double mtogradl(double a, double c)
        {
            int n = (int)(c * Math.Pow(10, -6));
            double beta = a / 6367558.4968;
            double b0 = beta + Math.Sin(2 * beta) * (0.00252588685 - 1.49186E-5 * Math.Pow(Math.Sin(beta), 2) + 1.1904E-7 * Math.Pow(Math.Sin(beta), 4));
            double z0 = (c - 1E5 * (10 * n + 5)) / (6378245 * Math.Cos(b0));
            double l = z0 * (1 - 0.0033467108 * Math.Pow(Math.Sin(b0), 2) - 5.6002E-6 * Math.Pow(Math.Sin(b0), 4) - 0 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 * (0.16778975 + 0.16273586 * Math.Pow(Math.Sin(b0), 2) - 5.249E-4 * Math.Pow(Math.Sin(b0), 4) - 8.46E-6 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 * (0.0420025 + 0.1487407 * Math.Pow(Math.Sin(b0),
                    2) + 0.005942 * Math.Pow(Math.Sin(b0), 4) - 1.5E-5 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 * (0.01225 + 0.09477 * Math.Pow(Math.Sin(b0), 2) + 0.03282 * Math.Pow(Math.Sin(b0), 4) - 3.4E-4 * Math.Pow(Math.Sin(b0), 6) - z0 * z0 * (0.0038 + 0.0524 * Math.Pow(Math.Sin(b0), 2) + 0.0482 * Math.Pow(Math.Sin(b0), 4) + 0.0032 * Math.Pow(Math.Sin(b0), 6))))));
            return 180 * (6 * (n - 0.5) / 57.29577951 + l) / Pi;
        }


        public void convert(double lat, double lon, int mode)
        {
            switch (mode)
            {
                case MODE_WGS_84:
                    break;

                case MODE_PZ_90:
                    double x90 = gradtomx(lat,lon);
                    double y90 = gradtomy(lat, lon);
                    double x42 = x90 + 3.3 * 1E-6 * y90 - 25;
                    double y42 = -3.3 * 1E-6 * x90 + y90 + 141;//!&!&!
                    this.lat = SK42_WGS84_Lat(mtogradb(x42, y42), mtogradl(x42, y42), 0);
                    this.lon = SK42_WGS84_Long(mtogradb(x42, y42), mtogradl(x42, y42), 0);
                    break;
                
                case MODE_PULKOVO_42:
                    this.lat = SK42_WGS84_Lat(lat, lon, 0);
                    this.lon = SK42_WGS84_Long(lat, lon, 0);
                    break;
            }
        }


        public double getLat()
        {
            return lat;
        }

        public double getLon()
        {
            return lon;
        }
    }
}
