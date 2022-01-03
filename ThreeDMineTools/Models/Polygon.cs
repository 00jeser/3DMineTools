using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ThreeDMineTools.Models
{
    public record MPolygon
    {
        public MPoint Point1;
        public MPoint Point2;
        public MPoint Point3;

        public Color AverageColor;
    }
    [StructLayout(LayoutKind.Sequential)]
    public record struct MPoint
    {
        [MarshalAs(UnmanagedType.R4)]
        public float X;
        [MarshalAs(UnmanagedType.R4)]
        public float Y;
        [MarshalAs(UnmanagedType.R4)]
        public float Z;

        public MPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public MPoint(Point3D point)
        {
            X = (float)point.X;
            Y = (float)point.Y;
            Z = (float)point.Z;
        }
    }
}
