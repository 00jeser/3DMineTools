using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public static List<List<List<Color?>>> PolygonToVoxel(MModel model, byte RoundColor = 1)
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
                            c = Color.FromRgb((byte)(poly.AverageColor.R/RoundColor*RoundColor), (byte)(poly.AverageColor.G / RoundColor * RoundColor), (byte)(poly.AverageColor.B / RoundColor * RoundColor));
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

    public static List<List<List<Color?>>> PolygonToVoxel2(MModel model, byte RoundColor = 1)
    {
        var MaxProgress = model.Polygons.Length;
        List<List<List<Color?>>> rez = new();
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
        var dX = (int)model.XMin - 1;
        var dY = (int)model.YMin - 1;
        var dZ = (int)model.ZMin - 1;
        int n = 0;
        foreach (var poly in model.Polygons)
        {
            var xmax = (int)max(poly.Point1.X, poly.Point2.X, poly.Point3.X);
            var ymax = (int)max(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y);
            var zmax = (int)max(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z);
            for (int x = (int)min(poly.Point1.X, poly.Point2.X, poly.Point3.X) - 1; x <= xmax + 1; x++)
            {
                for (int y = (int)min(poly.Point1.Y, poly.Point2.Y, poly.Point3.Y) - 1; y <= ymax + 1; y++)
                {
                    for (int z = (int)min(poly.Point1.Z, poly.Point2.Z, poly.Point3.Z) - 1; z <= zmax + 1; z++)
                    {
                        if (Intersection(new MPoint(x, y, z), poly.Point2, poly.Point1, poly.Point3) == 1)
                        {
                            rez[x - dX][y - dY][z - dZ] = poly.AverageColor;
                        }
                    }
                }
            }

            progress = n / model.Polygons.Length;
            n++;
        }

        return rez;
    }
    private static bool betwen(float first, float value, float second) => (first <= value && value <= second) || (first >= value && value >= second);

    public static List<List<List<Color?>>> PolygonToVoxel3(MModel model, bool firstColor, byte RoundColor = 1)
    {
        List<List<List<List<int>>>> rezs = new();
        var dX = (int)model.XMin - 1;
        var dY = (int)model.YMin - 1;
        var dZ = (int)model.ZMin - 1;
        for (int x = (int)model.XMin - 1; x <= (int)model.XMax + 1; x++)
        {
            rezs.Add(new List<List<List<int>>>((int)(model.YMax - model.YMin)));
            for (int y = (int)model.YMin - 1; y <= (int)model.YMax + 1; y++)
            {
                rezs.Last().Add(new List<List<int>>((int)(model.ZMax - model.ZMin)));
                for (int z = (int)model.ZMin - 1; z <= (int)model.ZMax + 1; z++)
                {
                    rezs.Last().Last().Add(new List<int>(5));
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
                        var dfwajikdwsajik = poly.AverageColor.ToSimpleColor();
                        rezs[x - dX][y - dY][z - dZ].Add(dfwajikdwsajik);
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
                    if (firstColor)
                        if (rezs[x][y][z].Count == 0)
                            rez.Last().Last().Add(null);
                        else
                            rez.Last().Last().Add(rezs[x][y][z][0].ToColor());
                    else
                        rez.Last().Last().Add(avgColor(rezs[x][y][z], RoundColor));
                }
            }
        }

        return rez;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToSimpleColor(this Color c)
    {
        return (c.A << (8 * 3)) + (c.R << (8 * 2)) + (c.G << (8 * 1)) + (c.B << (8 * 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ToColor(this int i)
    {
        return new Color()
        {
            A = (byte)((i >> 24) & 0b11111111),
            R = (byte)((i >> 16) & 0b11111111),
            G = (byte)((i >> 8)  & 0b11111111),
            B = (byte)((i >> 0)  & 0b11111111),
        };
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

    public static List<List<List<Color?>>> PolygonToVoxel4(MModel model, byte RoundColor = 1)
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
                        if (InsidePoly(poly.Point1, poly.Point2, poly.Point3, new MPoint(x, y, z)))
                        {
                            c = Color.FromRgb((byte)(poly.AverageColor.R / RoundColor * RoundColor), (byte)(poly.AverageColor.G / RoundColor * RoundColor), (byte)(poly.AverageColor.B / RoundColor * RoundColor));
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

    //https://habr.com/ru/post/204806/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InsidePoly(MPoint A, MPoint B, MPoint C, MPoint P)
    {
        float AB = MathF.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y) + (A.Z - B.Z) * (A.Z - B.Z));
        float BC = MathF.Sqrt((B.X - C.X) * (B.X - C.X) + (B.Y - C.Y) * (B.Y - C.Y) + (B.Z - C.Z) * (B.Z - C.Z));
        float CA = MathF.Sqrt((A.X - C.X) * (A.X - C.X) + (A.Y - C.Y) * (A.Y - C.Y) + (A.Z - C.Z) * (A.Z - C.Z));

        float AP = MathF.Sqrt((P.X - A.X) * (P.X - A.X) + (P.Y - A.Y) * (P.Y - A.Y) + (P.Z - A.Z) * (P.Z - A.Z));
        float BP = MathF.Sqrt((P.X - B.X) * (P.X - B.X) + (P.Y - B.Y) * (P.Y - B.Y) + (P.Z - B.Z) * (P.Z - B.Z));
        float CP = MathF.Sqrt((P.X - C.X) * (P.X - C.X) + (P.Y - C.Y) * (P.Y - C.Y) + (P.Z - C.Z) * (P.Z - C.Z));
        float diff = (triangle_square(AP, BP, AB) + triangle_square(AP, CP, CA) + triangle_square(BP, CP, BC)) - triangle_square(AB, BC, CA);
        if (MathF.Abs(diff) < 0.85) return true;
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float triangle_square(float a, float b, float c)
    {
        float p = (a + b + c) / 2;
        return MathF.Sqrt(p * (p - a) * (p - b) * (p - c));
    }


    private static Color? avgColor(IEnumerable<int> clrs, byte RoundColor = 1)
    {
        if (clrs.Count() == 0)
            return null;
        int r = 0;
        int g = 0;
        int b = 0;
        foreach (var clr in clrs)
        {
            r += (clr >> 16)  & 0b11111111;
            g += (clr >> 8) & 0b11111111;
            b += (clr >> 0) & 0b11111111;
        }

        return Color.FromRgb((byte)((r / clrs.Count()) / RoundColor * RoundColor), (byte)((g / clrs.Count()) / RoundColor * RoundColor), (byte)((b / clrs.Count()) / RoundColor * RoundColor));
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