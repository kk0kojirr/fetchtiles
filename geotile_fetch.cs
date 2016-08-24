using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;


namespace geotile_fetch
{
    class LonLat
    {
        public double lon { get; set; }
        public double lat { get; set; }

        public LonLat(double x, double y)
        {
            lon = x;
            lat = y;
            return;
        }
    }


    class TileArea
    {
        public int zoomlevel { get; set; }
        public Point beginning { get; set; }
        public Point terminal { get; set; }

        public TileArea(int lv, Point beg, Point ter)
        {
            zoomlevel = lv;
            beginning = beg;
            terminal = ter;
            return;
        }
    }


    public class geotile_fetch
    {
        private List<TileArea> tileareas = new List<TileArea>();


        static void Main(string[] args)
        {
            //int tileposx_st = Properties.Settings.Default.ptst.X;
            //int tileposx_en = Properties.Settings.Default.pted.X;
            //int tileposy_st = Properties.Settings.Default.ptst.Y;
            //int tileposy_en = Properties.Settings.Default.pted.Y;

            geotile_fetch g = new geotile_fetch();
            g.fetchfiles();

            return;
        }


        private void fetchfiles()
        {
            makearea();

            int cur_x, cur_y;
            // zoomlevel/xpos/yopos.format
            string uri_seed = Properties.Settings.Default.tileurl;
            string filetype = Properties.Settings.Default.filetype;

            WebClient wc = new WebClient();
            wc.Proxy = System.Net.WebRequest.GetSystemWebProxy();


            foreach (TileArea a in tileareas)
            {
                string directory_name = @"\\...\" + a.zoomlevel.ToString();
                Directory.CreateDirectory(directory_name);


                for (cur_x = a.beginning.X; cur_x < a.terminal.X + 1; cur_x++)
                {
                    DirectoryInfo di = Directory.CreateDirectory(directory_name + @"\" + cur_x.ToString());

                    for (cur_y = a.beginning.Y; cur_y < a.terminal.Y; cur_y++)
                    {
                        string uri = String.Format(uri_seed, a.zoomlevel, cur_x, cur_y, filetype);

                        Console.WriteLine(uri);

                        try
                        {
                            wc.DownloadFile(uri, di.FullName + @"\" + cur_y.ToString() + "." + filetype);
                        }
                        catch (System.Net.WebException ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }

            return;
        }


        private void makearea()
        {
            // 経度緯度を調べるには 地理院地図の白地図が便利
            LonLat point_st = new LonLat(0.00, 0.00);
            LonLat point_en = new LonLat(0.00, 0.00);

            for (int level = 2; level <= 16; level++)
            {
                Point tilepoint_st = lonlat2pix(point_st, level);
                Point tilepoint_en = lonlat2pix(point_en, level);

                tilepoint_en.X += 1;
                tilepoint_en.Y += 1;

                tileareas.Add(new TileArea(level, tilepoint_st, tilepoint_en));
            }

            return;
        }


        private Point lonlat2pix(LonLat lonlat, int zoom_level)
        {
            // Lは緯度の描画上限
            const double L = 85.05112878;
            Point pt = new Point(0, 0);


            // lon:経度 lat:緯度
            // 緯度経度からピクセル座標に ref:http://www.trail-note.net/tech/coordinate/
            // 経度：longitude
            double px = Math.Pow(2, (zoom_level + 7)) * (lonlat.lon / 180 + 1);
            // 緯度：latitiude ※ Atanhはtanhの逆数
            double py = Math.Pow(2, (zoom_level + 7)) / Math.PI *
                (-1 * Atanh(Math.Sin(Math.PI / 180 * lonlat.lat)) +
                Atanh(Math.Sin(Math.PI/180*L)));

            // ピクセル座標からタイル座標を
            pt.Y = (int)(py / 256);
            pt.X = (int)(px / 256);


            return pt;
        }


        private double Atanh(double x)
        {
            return (Math.Log((1 + x) / (1 - x)) / 2);
        }

    }
}
