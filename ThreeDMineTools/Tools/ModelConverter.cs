using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using ThreeDMineTools.Models;

namespace ThreeDMineTools.Tools;

public static class ModelConverter
{
    [DllImport("IntersectionTool.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Intersection(MPoint voxel, MPoint Triangle1, MPoint Triangle2, MPoint Triangle3);

    public static double progress = 0;
    public static List<List<List<Color?>>> PolygonToVoxel(MModel model)
    {
        var MaxProgress = ((int)model.XMax + 1) - ((int)model.XMin - 1);
        var CurentProgress = 0d;
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

            CurentProgress++;
            progress = CurentProgress / MaxProgress;
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
        List<List<List<List<Color>>>> rezs = new((int)(model.XMax - model.XMin));
        var dX = (int)model.XMin - 1;
        var dY = (int)model.YMin - 1;
        var dZ = (int)model.ZMin - 1;
        for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
        {
            rezs.Add(new List<List<List<Color>>>((int)(model.YMax - model.YMin)));
            for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
            {
                rezs.Last().Add(new List<List<Color>>((int)(model.ZMax - model.ZMin)));
                for (int z = (int)model.ZMin - 1; z <= (int)model.ZMax + 1; z++)
                {
                    rezs.Last().Last().Add(new List<Color>(5));
                }
            }

            progress = 1 + (x + model.XMin - 1) / (model.XMax - model.XMin + 1);
        }

        int n = 0;
        foreach (var poly in model.Polygons)
        {
            var xmax = (int)max(poly.Point1.X, poly.Point2.X, poly.Point3.X);
            for (int x = (int)min(poly.Point1.X, poly.Point2.X, poly.Point3.X); x <= xmax; x++)
            {
                var ymax = (int)max(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y);

                for (int y = (int)min(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y); y <= ymax; y++)
                {

                    var zmax = (int)max(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z);

                    for (int z = (int)min(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z); z <= zmax; z++)
                    {
                        rezs[x - dX][y - dY][z - dZ].Add(poly.AverageColor);
                    }
                }
            }

            progress = n / model.Polygons.Length;
            n++;
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

        return Color.FromRgb((byte)((r / clrs.Count)/50*50), (byte)((g / clrs.Count)/50*50), (byte)((b / clrs.Count)/50*50));
    }

    public static List<(Point3DCollection, Int32Collection, Color)> VoxelToPolygon(List<List<List<Color?>>> vertex)
    {
        Dictionary<Color, List<Point3D>> vertexes = new Dictionary<Color, List<Point3D>>();
        Dictionary<Color, List<int>> triangles = new Dictionary<Color, List<int>>();
        for (int x = 0; x < vertex.Count; x++)
        {
            for (int y = 0; y < vertex[0].Count; y++)
            {
                for (int z = 0; z < vertex[0][0].Count; z++)
                {
                    if (vertex[x][y][z] == null)
                        continue;
                    var NNcolor = vertex[x][y][z] ?? new Color();
                    if (!vertexes.Keys.Contains(NNcolor))
                    {
                        vertexes[NNcolor] = new List<Point3D>(300);
                        triangles[NNcolor] = new List<int>(300);
                    }
                    vertexes[NNcolor].Add(new Point3D(x - 0.5, y - 0.5, z - 0.5));
                    vertexes[NNcolor].Add(new Point3D(x + 0.5, y - 0.5, z - 0.5));
                    vertexes[NNcolor].Add(new Point3D(x - 0.5, y - 0.5, z + 0.5));
                    vertexes[NNcolor].Add(new Point3D(x + 0.5, y - 0.5, z + 0.5));
                    vertexes[NNcolor].Add(new Point3D(x - 0.5, y + 0.5, z - 0.5));
                    vertexes[NNcolor].Add(new Point3D(x + 0.5, y + 0.5, z - 0.5));
                    vertexes[NNcolor].Add(new Point3D(x - 0.5, y + 0.5, z + 0.5));
                    vertexes[NNcolor].Add(new Point3D(x + 0.5, y + 0.5, z + 0.5));


                    var vertexCount = vertexes[NNcolor].Count;
                    triangles[NNcolor].Add(vertexCount - 7 - 1);//0
                    triangles[NNcolor].Add(vertexCount - 5 - 1);//2
                    triangles[NNcolor].Add(vertexCount - 6 - 1);//1
                                                   
                    triangles[NNcolor].Add(vertexCount - 4 - 1);//3
                    triangles[NNcolor].Add(vertexCount - 6 - 1);//1
                    triangles[NNcolor].Add(vertexCount - 5 - 1);//2
                                                   
                    triangles[NNcolor].Add(vertexCount - 3 - 1);//4
                    triangles[NNcolor].Add(vertexCount - 1 - 1);//6
                    triangles[NNcolor].Add(vertexCount - 2 - 1);//5
                                                   
                    triangles[NNcolor].Add(vertexCount - 1 - 1);//6
                    triangles[NNcolor].Add(vertexCount - 0 - 1);//7
                    triangles[NNcolor].Add(vertexCount - 2 - 1);//5
                                                   
                    triangles[NNcolor].Add(vertexCount - 7 - 1);//0
                    triangles[NNcolor].Add(vertexCount - 6 - 1);//1
                    triangles[NNcolor].Add(vertexCount - 3 - 1);//4
                                                   
                    triangles[NNcolor].Add(vertexCount - 2 - 1);//5
                    triangles[NNcolor].Add(vertexCount - 3 - 1);//4
                    triangles[NNcolor].Add(vertexCount - 6 - 1);//1
                                                   
                    triangles[NNcolor].Add(vertexCount - 1 - 1);
                    triangles[NNcolor].Add(vertexCount - 5 - 1);
                    triangles[NNcolor].Add(vertexCount - 4 - 1);
                                                   
                    triangles[NNcolor].Add(vertexCount - 1 - 1);
                    triangles[NNcolor].Add(vertexCount - 4 - 1);
                    triangles[NNcolor].Add(vertexCount - 0 - 1);
                                                   
                    triangles[NNcolor].Add(vertexCount - 7 - 1);
                    triangles[NNcolor].Add(vertexCount - 5 - 1);
                    triangles[NNcolor].Add(vertexCount - 3 - 1);
                                                   
                    triangles[NNcolor].Add(vertexCount - 1 - 1);
                    triangles[NNcolor].Add(vertexCount - 3 - 1);
                    triangles[NNcolor].Add(vertexCount - 5 - 1);
                                                   
                    triangles[NNcolor].Add(vertexCount - 6 - 1);
                    triangles[NNcolor].Add(vertexCount - 4 - 1);
                    triangles[NNcolor].Add(vertexCount - 2 - 1);
                                                   
                    triangles[NNcolor].Add(vertexCount - 0 - 1);
                    triangles[NNcolor].Add(vertexCount - 2 - 1);
                    triangles[NNcolor].Add(vertexCount - 4 - 1);
                }
            }
        }

        List<(Point3DCollection, Int32Collection, Color)> rez = new List<(Point3DCollection, Int32Collection, Color)>();
        foreach (var color in vertexes.Keys)
        {
            rez.Add((
                new Point3DCollection(vertexes[color]),
                new Int32Collection(triangles[color]),
                color
                ));
        }
        return rez;
    }

}