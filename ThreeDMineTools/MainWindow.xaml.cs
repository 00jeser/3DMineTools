using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using ObjLoader.Loader.Common;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using Substrate;
using Substrate.Core;
using Substrate.Nbt;
using ThreeDMineTools.Models;
using ThreeDMineTools.Tools;
using Color = System.Windows.Media.Color;
using Image = System.Drawing.Image;
using Polygon = System.Windows.Shapes.Polygon;


namespace ThreeDMineTools
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        private float max(float f1, float f2, float f3)
        {
            if (f1 > f2 && f1 > f3)
                return f1;
            if (f2 > f3 && f2 > f1)
                return f2;
            return f3;
        }
        private float min(float f1, float f2, float f3)
        {
            if (f1 < f2 && f1 < f3)
                return f1;
            if (f2 < f3 && f2 < f1)
                return f2;
            return f3;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;
        }

        public static PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            PixelColor[,] pixels = new PixelColor[source.PixelWidth, source.PixelHeight];
            int stride = source.PixelWidth * ((source.Format.BitsPerPixel + 7) / 8);
            GCHandle pinnedPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            source.CopyPixels(
                new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                pinnedPixels.AddrOfPinnedObject(),
                pixels.GetLength(0) * pixels.GetLength(1) * 4,
                stride);
            pinnedPixels.Free();
            return pixels;
        }

        private float XMax = float.MinValue, YMax = float.MinValue, ZMax = float.MinValue, XMin = float.MaxValue, YMin = float.MaxValue, ZMin = float.MaxValue;

        private async void OpenModel(object sender, RoutedEventArgs e)
        {
            writeButton.IsEnabled = false;
            StatusTB.Text = "opening...";
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "all|*.obj;*.stl|obj|*.obj|stl|*.stl";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName.IsNullOrEmpty())
                return;


            progress.Value = 0.1;
            await Task.Delay(1);

            XMax = float.MinValue;
            YMax = float.MinValue;
            ZMax = float.MinValue;
            XMin = float.MaxValue;
            YMin = float.MaxValue;
            ZMin = float.MaxValue;

            ModelReader CurrentHelixObjReader = openFileDialog.FileName.EndsWith(".obj") ? new ObjReader() : new StLReader();
            Model3DGroup MyModel3DGroup = CurrentHelixObjReader.Read(openFileDialog.FileName);

            VoxelViewport.Children.Clear();
            VoxelViewport.Children.Add(new ModelVisual3D()
            {
                Content = new AmbientLight()
                {
                    Color = Color.FromRgb(255, 255, 255),
                }
            });
            VoxelViewport.Children.Add(new ModelVisual3D()
            {
                Content = new DirectionalLight()
                {
                    Color = Color.FromRgb(255, 255, 255),
                    Direction = new Vector3D(-1, -1, -2),
                }
            });
            VoxelViewport.Children.Add(new ModelVisual3D()
            {
                Content = MyModel3DGroup
            });

            foreach (var model3d in MyModel3DGroup.Children)
            {
                XMax = MathF.Max(XMax, (float)(model3d.Bounds.X + model3d.Bounds.SizeX));
                YMax = MathF.Max(YMax, (float)(model3d.Bounds.Y + model3d.Bounds.SizeY));
                ZMax = MathF.Max(ZMax, (float)(model3d.Bounds.Z + model3d.Bounds.SizeZ));
                XMin = MathF.Min(XMin, (float)model3d.Bounds.X);
                YMin = MathF.Min(YMin, (float)model3d.Bounds.Y);
                ZMin = MathF.Min(ZMin, (float)model3d.Bounds.Z);
            }

            progress.Value = 0.2;
            await Task.Delay(1);

            var mats = new List<Material>();
            var polygons = new List<MPolygon>(1000);
            foreach (var model3d in MyModel3DGroup.Children)
            {
                if (model3d is GeometryModel3D gmodel3d)
                {
                    DiffuseMaterial mat2 = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                    if (gmodel3d.Material is MaterialGroup mat)
                    {
                        mats.AddRange(mat.Children);
                        mat2 = mat.Children[0] as DiffuseMaterial;
                    }
                    else if (gmodel3d.Material is DiffuseMaterial dmat)
                    {
                        mats.Add(dmat);
                        mat2 = dmat;
                    }

                    PixelColor[,] pixels = new PixelColor[0, 0];
                    double width = 0;
                    double height = 0;
                    if (mat2.Brush is ImageBrush)
                    {
                        pixels = GetPixels((mat2.Brush as ImageBrush).ImageSource as BitmapSource);
                        width = pixels.GetLength(0);
                        height = pixels.GetLength(1);
                    }

                    if (gmodel3d.Geometry is MeshGeometry3D mesh)
                    {
                        for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                        {
                            var poly = new MPolygon()
                            {
                                Point1 = new MPoint(mesh.Positions[mesh.TriangleIndices[i]]),
                                Point2 = new MPoint(mesh.Positions[mesh.TriangleIndices[i + 1]]),
                                Point3 = new MPoint(mesh.Positions[mesh.TriangleIndices[i + 2]])
                            };
                            if (mat2.Brush is SolidColorBrush)
                                poly.AverageColor = (mat2.Brush as SolidColorBrush).Color;
                            else if (mat2.Brush is ImageBrush)
                            {
                                int r = 0;
                                int g = 0;
                                int b = 0;
                                var x = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].X - (int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].X) * (height - 1);
                                var y = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y -(int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y) * (width - 1);
                                var clr = pixels[(int)(y), (int)(x)];
                                r += clr.R;
                                g += clr.G;
                                b += clr.B;
                                x = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].X - (int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].X) * (height - 1);
                                y = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y - (int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y) * (width - 1);
                                clr = pixels[(int)(y), (int)(x)];
                                r += clr.R;
                                g += clr.G;
                                b += clr.B;
                                x = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].X - (int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].X) * (height - 1);
                                y = (mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y - (int)mesh.TextureCoordinates[mesh.TriangleIndices[i]].Y) * (width - 1);
                                clr = pixels[(int)(y), (int)(x)];
                                r += clr.R;
                                g += clr.G;
                                b += clr.B;
                                r /= 3;
                                g /= 3;
                                b /= 3;
                                poly.AverageColor = Color.FromRgb((byte)r, (byte)g, (byte)b);
                            }

                            polygons.Add(poly);

                        }
                    }
                }
            }

            progress.Value = 0.8;
            await Task.Delay(1);

            model = new MModel()
            {
                Polygons = polygons.ToArray(),
                YMin = YMin,
                ZMin = ZMin,
                XMin = XMin,
                XMax = XMax,
                YMax = YMax,
                ZMax = ZMax
            };
            convertButton.IsEnabled = true;
            File.Delete(openFileDialog.FileName.Split("\\").Last().Replace(".obj", ".mtl"));

            scale.Minimum = 10 / (YMax - YMin);
            scale.Maximum = 250 / (YMax - YMin);
            scale.Value = 1;
            scale.IsEnabled = true;

            radius = max(XMax - XMin, YMax - YMin, ZMax - ZMin) * 2;
            centerX = (int)((XMax + XMin) / 2);
            centerZ = (int)((ZMax + ZMin) / 2);
            VoxelsPreviewCamera.Position = new Point3D(0, (YMax + YMin) / 2, radius);
            VoxelsPreviewCamera.LookDirection = new Vector3D((XMax + XMin) / 2 - VoxelsPreviewCamera.Position.X, (YMax + YMin) / 2 - VoxelsPreviewCamera.Position.Y, (ZMax + ZMin) / 2 - VoxelsPreviewCamera.Position.Z);

            progress.Value = 1;
            StatusTB.Text = "opened";
        }

        private MModel model;
        private List<List<List<Color?>>> vertex;
        private void WriteSchematics(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Schematic|*.schematic";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName.IsNullOrEmpty())
                return;


            List<byte> bytes = new List<byte>(10000);
            List<byte> data = new List<byte>(10000);

            for (int y = 0; y < vertex[0].Count; y++)
            {
                for (int z = 0; z < vertex[0][0].Count; z++)
                {
                    for (int x = 0; x < vertex.Count; x++)
                    {
                        if (vertex[x][y][z] == null)
                        {
                            bytes.Add(0);
                            data.Add(0);
                        }
                        else
                        {
                            (byte, byte) r = BlockPicker.GetBlockFroColor((Color)vertex[x][y][z]);
                            bytes.Add(r.Item1);
                            data.Add(r.Item2);
                        }
                    }
                }
            }


            FileStream file = new(saveFileDialog.FileName, FileMode.OpenOrCreate);
            NbtTree tree = new NbtTree();
            tree.Name = "dwa";
            tree.Root.Add("Width", new TagNodeShort((short)vertex.Count));
            tree.Root.Add("Height", new TagNodeShort((short)vertex[0].Count));
            tree.Root.Add("Length", new TagNodeShort((short)vertex[0][0].Count));
            tree.Root.Add("Blocks", new TagNodeByteArray(bytes.ToArray()));
            tree.Root.Add("Data", new TagNodeByteArray(data.ToArray()));
            tree.Root.Add("Entities", new TagNodeList(TagType.TAG_BYTE));
            tree.Root.Add("TileEntities", new TagNodeList(TagType.TAG_BYTE));
            tree.Root.Add("Materials", new TagNodeString("Classic"));
            tree.WriteTo(file);
            file.Close();

            StatusTB.Text = "Written";

        }

        private async void ConvertModel(object sender, RoutedEventArgs e)
        {
            XMax = float.MinValue;
            YMax = float.MinValue;
            ZMax = float.MinValue;
            XMin = float.MaxValue;
            YMin = float.MaxValue;
            ZMin = float.MaxValue;
            MModel newModel = new();
            newModel.YMin = model.YMin * (float)scale.Value;
            newModel.XMin = model.XMin * (float)scale.Value;
            newModel.ZMin = model.ZMin * (float)scale.Value;
            newModel.YMax = model.YMax * (float)scale.Value;
            newModel.XMax = model.XMax * (float)scale.Value;
            newModel.ZMax = model.ZMax * (float)scale.Value;
            newModel.Polygons = new MPolygon[model.Polygons.Length];


            StatusTB.Text = "converting...";
            for (int i = 0; i < model.Polygons.Length; i++)
            {
                newModel.Polygons[i] = new MPolygon()
                {
                    Point1 = new MPoint(model.Polygons[i].Point1.X * (float)scale.Value, model.Polygons[i].Point1.Y * (float)scale.Value, model.Polygons[i].Point1.Z * (float)scale.Value),
                    Point2 = new MPoint(model.Polygons[i].Point2.X * (float)scale.Value, model.Polygons[i].Point2.Y * (float)scale.Value, model.Polygons[i].Point2.Z * (float)scale.Value),
                    Point3 = new MPoint(model.Polygons[i].Point3.X * (float)scale.Value, model.Polygons[i].Point3.Y * (float)scale.Value, model.Polygons[i].Point3.Z * (float)scale.Value),
                    AverageColor = model.Polygons[i].AverageColor
                };
            }

            convertButton.IsEnabled = false;
            writeButton.IsEnabled = false;
            var t = new Task(AlghoType.SelectedIndex switch
            {
                0 => () => vertex = ModelConverter.PolygonToVoxel(newModel),
                1 => () => vertex = ModelConverter.PolygonToVoxel2(newModel),
                2 => () => vertex = ModelConverter.PolygonToVoxel3(newModel)
            }
            );
            t.Start();
            while (!t.IsCompleted)
            {
                await Task.Delay(100);
                progress.Value = ModelConverter.progress;
                StatusTB.Text = $"converting...({(ModelConverter.progress * 100):F1}%)";
            }
            writeButton.IsEnabled = true;
            convertButton.IsEnabled = true;
            StatusTB.Text = "converted";


            //---------------------preview--------------------------------------------
            VoxelViewport.Children.Clear();
            VoxelViewport.Children.Add(new ModelVisual3D()
            {
                Content = new AmbientLight()
                {
                    Color = Color.FromRgb(255, 255, 255)
                }
            });
            VoxelViewport.Children.Add(new ModelVisual3D()
            {
                Content = new DirectionalLight()
                {
                    Color = Color.FromRgb(255, 255, 255),
                    Direction = new Vector3D(-1, -1, -2)
                }
            });
            foreach (var model in ModelConverter.VoxelToPolygon(vertex))
            {
                await Task.Delay(0);
                VoxelViewport.Children.Add(new ModelVisual3D()
                {
                    Content = new GeometryModel3D()
                    {
                        Geometry = new MeshGeometry3D()
                        {
                            Positions = model.Item1,
                            TriangleIndices = model.Item2
                        },
                        Material = new DiffuseMaterial()
                        {
                            Brush = new SolidColorBrush(model.Item3),
                            AmbientColor = Color.FromRgb(255, 255, 255)
                        },
                        BackMaterial = new DiffuseMaterial()
                        {
                            Brush = new SolidColorBrush(model.Item3),
                            AmbientColor = Color.FromRgb(255, 255, 255)
                        }
                    }
                });
            }

            //for (int x = 0; x < vertex.Count; x++)
            //{
            //    progress.Value = x / vertex.Count;
            //    await Task.Delay(1);
            //    for (int y = 0; y < vertex[x].Count; y++)
            //    {
            //        for (int z = 0; z < vertex[x][y].Count; z++)
            //        {
            //            if (vertex[x][y][z] != null)
            //            {
            //                VoxelViewport.Children.Add(new CubeVisual3D()
            //                {
            //                    Center = new Point3D(x, y, z),
            //                    Material = new DiffuseMaterial(new SolidColorBrush(vertex[x][y][z] ?? Color.FromRgb(239, 84, 252))),
            //                    SideLength = 1,
            //                });
            //            }
            //        }
            //    }
            //}
            //---------------------preview--------------------------------------------


            VoxelsPreviewCamera.Position = new Point3D(
                vertex.Count / 2,
                vertex[0].Count / 2,
                max(vertex.Count, vertex[0].Count, vertex[0][0].Count) * 2
            );
            centerX = vertex.Count / 2;
            centerZ = vertex[0][0].Count / 2;
            rotation = 0;
            radius = max(vertex.Count, vertex[0].Count, vertex[0][0].Count) * 2;
        }

        private void scaleChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            heightTB.Text = ((YMax - YMin) * scale.Value).ToString();
        }


        private float rotation = 0;
        int centerX = 0, centerZ = 0;
        private float radius = 0;
        private double lastX = 0;
        private async void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                await Task.Delay(1);
                rotation += ((float)lastX - (float)e.GetPosition(sender as Viewport3D).X) / 100;
                rotation %= (float)(2 * Math.PI);
                VoxelsPreviewCamera.Position = new Point3D(MathF.Cos(rotation) * radius + centerX, VoxelsPreviewCamera.Position.Y, MathF.Sin(rotation) * radius + centerZ);
                VoxelsPreviewCamera.LookDirection = new Vector3D(
                    centerX - VoxelsPreviewCamera.Position.X,
                    0,
                    centerZ - VoxelsPreviewCamera.Position.Z
                    );
                //((VoxelViewport.Children.First() as ModelVisual3D).Content as DirectionalLight).Direction =
                //    VoxelsPreviewCamera.LookDirection;
            }
            lastX = e.GetPosition(sender as Viewport3D).X;
        }

        private void GridToRotate_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            radius -= (float)e.Delta / 10;
            VoxelsPreviewCamera.Position = new Point3D(MathF.Cos(rotation) * radius + centerX, VoxelsPreviewCamera.Position.Y, MathF.Sin(rotation) * radius + centerZ);
        }
    }
}
