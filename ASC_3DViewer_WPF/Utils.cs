using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Windows.Media;
using System.Windows.Controls;

namespace GemCad_Viewer
{
    class CsUtils
    {
        public static double bias = 0.0000000001;

        public static CsCrossPoints CutPolygonByPlane(CsPolygon pg, CsPlane pl)
        {
            CsCrossPoints crossPt = new CsCrossPoints();
            Point3D g0, g1, p;
            double cx, cy, cz;
            p = pl.m_Point;
            Vector3D n = pl.m_NormalVector;
            double delta, d;
            int i = 0;

            while (true)
            {
                g0 = pg.m_lstPoints[i];
                if (i == pg.m_lstPoints.Count - 1)
                {
                    g1 = pg.m_lstPoints[0];
                }
                else
                {
                    g1 = pg.m_lstPoints[i + 1];
                }

                d = n.X * (g1.X - g0.X) + n.Y * (g1.Y - g0.Y) + n.Z * (g1.Z - g0.Z);

                if (Math.Abs(d) > bias)
                {
                    delta = (n.X * (p.X - g0.X) + n.Y * (p.Y - g0.Y) + n.Z * (p.Z - g0.Z)) / d;
                    if (Math.Abs(delta) < bias)
                        delta = 0;

                    cx = g0.X + (g1.X - g0.X) * delta;
                    cy = g0.Y + (g1.Y - g0.Y) * delta;
                    cz = g0.Z + (g1.Z - g0.Z) * delta;

                    if (delta >= 0 && delta <= 1)
                        crossPt.AddPoint(cx, cy, cz);
                }

                i++;
                if (i == pg.m_lstPoints.Count)
                {
                    break;
                }
            }

            i = pg.m_lstPoints.Count - 1;

            while (true)
            {
                g1 = pg.m_lstPoints[i];

                d = n.X * (g1.X - p.X) + n.Y * (g1.Y - p.Y) + n.Z * (g1.Z - p.Z);

                if (d > 0)
                {
                    pg.removePoint(i);
                }

                i--;
                if (i == -1)
                    break;
            }

            for (i = 0; i < crossPt.m_Points.Count(); i++)
            {
                p = crossPt.m_Points[i];
                pg.addPoint(p.X, p.Y, p.Z);
            }

            if (pg.m_lstPoints.Count() > 2)
                pg.rearrange();
            return crossPt;
        }

        public static bool IsSamePoint(Point3D pt1, Point3D pt2)
        {
            bool bIsSame = false;
            double dis = Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2.0) +
                                   Math.Pow(pt1.Y - pt2.Y, 2.0) +
                                   Math.Pow(pt1.Z - pt2.Z, 2.0));
            if (dis < bias)
                bIsSame = true;

            return bIsSame;
        }

        public static bool IsContain(List<Point> lstPt, Point pt)
        {
            bool bIsContain = false;
            int i, k, nCou = 0;
            Point pt1, pt2;
            double dx, dy, a, b, c;

            for (i = 0; i < lstPt.Count(); i++)
            {
                k = i + 1;
                if (i == lstPt.Count() - 1) k = 0;
                pt1 = lstPt[i];
                pt2 = lstPt[k];

                dx = pt2.X - pt1.X;
                dy = pt2.Y - pt1.Y;

                if (Math.Abs(dx) > bias)
                {
                    a = dy / dx;
                    b = pt1.Y - a * pt1.X;
                    c = pt.Y - a * pt.X - b;

                    nCou += Math.Sign(c * dx);
                }
            }

            if (nCou * Math.Sign(nCou) == lstPt.Count())
                bIsContain = true;
            
            return bIsContain;
        }

        public static Point3D Convert2DPoint(Point pointToConvert, Visual3D sphere, TranslateTransform3D cameraPosition)        // transform world matrix
        {
            bool success = true;
            Viewport3DVisual viewport;
            Matrix3D screenTransform = MathUtils.TryTransformTo2DAncestor(sphere, out viewport, out success);
            Point3D pointInWorld = new Point3D();
            if (screenTransform.HasInverse)
            {
                Matrix3D reverseTransform = screenTransform;
                reverseTransform.Invert();
                Point3D pointOnScreen = new Point3D(pointToConvert.X, pointToConvert.Y, 1);
                pointInWorld = reverseTransform.Transform(pointOnScreen);
                pointInWorld = new Point3D(((pointInWorld.X + cameraPosition.OffsetX) / 2),
                                            ((pointInWorld.Y + cameraPosition.OffsetY) / 2),
                                            ((pointInWorld.Z + cameraPosition.OffsetZ) / 2));
            }
            return pointInWorld;
        }

        public static Point Convert3DPoint(Point3D p3d, Viewport3D vp)
        {
            bool TransformationResultOK;
            Viewport3DVisual vp3Dv = VisualTreeHelper.GetParent(vp.Children[0]) as Viewport3DVisual;
            Matrix3D m = MathUtils.TryWorldToViewportTransform(vp3Dv, out TransformationResultOK);
            if (!TransformationResultOK) return new Point(0, 0);
            Point3D pb = m.Transform(p3d);
            Point p2d = new Point(pb.X, pb.Y);
            return p2d;
        }

        public static ModelVisual3D CreateTextLabel3D(string text,
                                                      Brush textColor,
                                                      bool bDoubleSided,
                                                      double height,
                                                      Point3D center,
                                                      Vector3D over,
                                                      Vector3D up)
        {
            Run runText = new Run(text);
            TextBlock tb = new TextBlock(runText);
            
            tb.Foreground = textColor;
            tb.Background = Brushes.Transparent; //new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            tb.FontFamily = new FontFamily("Arial");
            DiffuseMaterial mat = new DiffuseMaterial();
            mat.Brush = new VisualBrush(tb);
            
            double width = text.Length * height;

            Point3D p0 = center - width / 2 * over - height / 2 * up;
            Point3D p1 = p0 + up * 1 * height;
            Point3D p2 = p0 + over * width;
            Point3D p3 = p0 + up * 1 * height + over * width;

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);   
            mg.Positions.Add(p1);   
            mg.Positions.Add(p2);   
            mg.Positions.Add(p3);   

            if (bDoubleSided)
            {
                mg.Positions.Add(p0);   
                mg.Positions.Add(p1);   
                mg.Positions.Add(p2);   
                mg.Positions.Add(p3);  
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (bDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            if (bDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(1, 1));
                mg.TextureCoordinates.Add(new Point(1, 0));
                mg.TextureCoordinates.Add(new Point(0, 1));
                mg.TextureCoordinates.Add(new Point(0, 0));
            }

            ModelVisual3D mv3d = new ModelVisual3D();
            mv3d.Content = new GeometryModel3D(mg, mat); ;
            return mv3d;
        }

        public static Vector3D LTVector(Vector3D vt, double rate)
        {
            double length = Math.Sqrt(vt.X * vt.X + vt.Y * vt.Y + vt.Z * vt.Z);
            double x, y, z;
            x = vt.X * rate / length;
            y = vt.Y * rate / length;
            z = vt.Z * rate / length;

            return new Vector3D(x, y, z);
        }

    }
}
