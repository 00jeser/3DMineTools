using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using ThreeDMineTools.Models;

namespace ThreeDMineTools.Tools;

public static class ModelConverter
{
    [DllImport("IntersectionTool.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Intersection(MPoint voxel, MPoint Triangle1, MPoint Triangle2, MPoint Triangle3);
    public static List<List<List<Color?>>> PolygonToVoxel(MModel model)
    {
        List<List<List<Color?>>> rez = new();
        for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
        {
            rez.Add(new List<List<Color?>>());
            for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
            {
                rez.Last().Add(new List<Color?>());
                for (int z = (int)model.ZMin - 1; z <= (int)model.ZMax + 1; z++)
                {
                    Color? c = null;
                    foreach (var poly in model.Polygons)
                    {
                        if (Intersection(new MPoint(x, y, z), poly.Point1, poly.Point2, poly.Point3) == 1)
                        {
                            c = poly.AverageColor;
                            break;
                        }
                    }
                    rez.Last().Last().Add(c);
                }
            }
        }

        return rez;
    }

    public static List<List<List<Color?>>> PolygonToVoxel2(MModel model)
    {
        List<List<List<Color?>>> rez = new();
        var dX = (int)model.XMin - 1;
        var dY = (int)model.YMin - 1;
        var dZ = (int)model.ZMin - 1;
        for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
        {
            rez.Add(new List<List<Color?>>());
            for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
            {
                rez.Last().Add(new List<Color?>());
                for (int z = (int)model.ZMin - 1; z <= (int)model.ZMax + 1; z++)
                {
                    rez.Last().Last().Add(null);
                }
            }
        }
        for (int z = (int)model.ZMin; z < model.ZMax + 1; z++)
        {
            foreach (var polyz in model.Polygons)
            {
                if (betwen(polyz.Point1.Z, z, polyz.Point2.Z) ||
                    betwen(polyz.Point1.Z, z, polyz.Point3.Z) ||
                    betwen(polyz.Point3.Z, z, polyz.Point2.Z))
                {
                    for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
                    {
                        if (betwen(polyz.Point1.Y, y, polyz.Point2.Y) ||
                            betwen(polyz.Point1.Y, y, polyz.Point3.Y) ||
                            betwen(polyz.Point3.Y, y, polyz.Point2.Y))
                        {
                            for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
                            {
                                if (betwen(polyz.Point1.X, x, polyz.Point2.X) ||
                                    betwen(polyz.Point1.X, x, polyz.Point3.X) ||
                                    betwen(polyz.Point3.X, x, polyz.Point2.X))
                                {
                                    rez[x - dX][y - dY][z - dZ] = polyz.AverageColor;
                                }
                            }
                        }
                    }
                }
            }
        }
        foreach (var poly in model.Polygons)
        {
            rez[(int)poly.Point1.X - dX][(int)poly.Point1.Y - dY][(int)poly.Point1.Z - dZ] = poly.AverageColor;
            rez[(int)poly.Point2.X - dX][(int)poly.Point2.Y - dY][(int)poly.Point2.Z - dZ] = poly.AverageColor;
            rez[(int)poly.Point3.X - dX][(int)poly.Point3.Y - dY][(int)poly.Point3.Z - dZ] = poly.AverageColor;
        }
        return rez;
    }
    private static bool betwen(float first, float value, float second) => (first <= value && value <= second) || (first >= value && value >= second);


    public static List<List<List<Color?>>> PolygonToVoxel3(MModel model)
    {
        List<List<List<List<Color>>>> rezs = new(20);
        var dX = (int)model.XMin - 1;
        var dY = (int)model.YMin - 1;
        var dZ = (int)model.ZMin - 1;
        for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
        {
            rezs.Add(new List<List<List<Color>>>(20));
            for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
            {
                rezs.Last().Add(new List<List<Color>>(20));
                for (int z = (int)model.ZMin - 1; z <= (int)model.ZMax + 1; z++)
                {
                    rezs.Last().Last().Add(new List<Color>(5));
                }
            }
        }

        foreach (var poly in model.Polygons)
        {
            var xmax = (int) max(poly.Point1.X, poly.Point2.X, poly.Point3.X);
            for (int x = (int) min(poly.Point1.X, poly.Point2.X, poly.Point3.X); x <= xmax; x++)
            {
                var ymax = (int) max(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y);

                for (int y = (int) min(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y); y <= ymax; y++)
                {

                    var zmax = (int)max(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z);

                    for (int z = (int)min(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z); z <= zmax; z++)
                    {
                        rezs[x-dX][y-dY][z-dZ].Add(poly.AverageColor);
                    }
                }
            }
        }
        List<List<List<Color?>>> rez = new();
        for (int x = 0; x < rezs.Count; x++)
        {
            rez.Add(new List<List<Color?>>());
            for (int y = 0; y < rezs[x].Count; y++)
            {
                rez.Last().Add(new List<Color?>());
                for (int z = 0; z < rezs[x][y].Count; z++)
                {
                    rez.Last().Last().Add(avgColor(rezs[x][y][z]));
                }
            }
        }

        return rez;
    }
    private static float max(float f1, float f2, float f3)
    {
        if (f1 > f2 && f1 > f3)
            return f1;
        if (f2 > f3 && f2 > f1)
            return f2;
        return f3;
    }
    private static float min(float f1, float f2, float f3)
    {
        if (f1 < f2 && f1 < f3)
            return f1;
        if (f2 < f3 && f2 < f1)
            return f2;
        return f3;
    }

    private static Color? avgColor(List<Color> clrs)
    {
        if (clrs.Count == 0)
            return null;
        int r = 0;
        int g = 0;
        int b = 0;
        foreach (var clr in clrs)
        {
            r += clr.R;
            g += clr.G;
            b += clr.B;
        }

        return Color.FromRgb((byte)(r / clrs.Count), (byte)(g / clrs.Count), (byte)(b / clrs.Count));
    }
}