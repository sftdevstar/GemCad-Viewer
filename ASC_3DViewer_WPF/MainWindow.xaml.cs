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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.IO;
using _3DTools;

namespace GemCad_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ModelVisual3D model = null;
        List<ModelVisual3D> lstTextModel = null;
        ScreenSpaceLines3D wireFrameCube = null;
        string strPath = "";
        List<CsPolygon> m_lstPolygon = new List<CsPolygon>();
        List<CsPlane> m_lstPlane = new List<CsPlane>();
        List<CsFacetData> m_lstFacetData = new List<CsFacetData>();
        List<int> m_selectedIndexs = new List<int>();
        int m_selectedIndex = -1;
        int nGear;

        public MainWindow()
        {
            InitializeComponent();

            // set up the trackball
            var trackball = new Wpf3DTools.Trackball();
            trackball.EventSource = background;
            viewport.Camera.Transform = trackball.Transform;
            light.Transform = trackball.RotateTransform;
        }

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, Color clr)
        {
            MeshGeometry3D mymesh = new MeshGeometry3D();
            mymesh.Positions.Add(p0);
            mymesh.Positions.Add(p1);
            mymesh.Positions.Add(p2);
            mymesh.TriangleIndices.Add(0);
            mymesh.TriangleIndices.Add(1);
            mymesh.TriangleIndices.Add(2);
            Vector3D Normal = CalculateTraingleNormal(p0, p1, p2);
            mymesh.Normals.Add(Normal);
            mymesh.Normals.Add(Normal);
            mymesh.Normals.Add(Normal);

            Brush br = new SolidColorBrush(clr);
            Material Material = new DiffuseMaterial(br);
            GeometryModel3D model = new GeometryModel3D(
                mymesh, Material);

            model.BackMaterial = Material;

            Model3DGroup Group = new Model3DGroup();
            Group.Children.Add(model);
            return Group;
        }

        private Model3DGroup CreateTriangleFacet(Point3D p0, Point3D p1, Point3D p2)
        {
            Model3DGroup modelGroup;

            GeometryModel3D geoModel;

            MeshGeometry3D meshGeo;
            Material material;


            //Create MESH
            meshGeo = new MeshGeometry3D();

            // Add Position in MESH
            meshGeo.Positions.Add(p0);
            meshGeo.Positions.Add(p1);
            meshGeo.Positions.Add(p2);

            // Add triangleIndices in MESH
            meshGeo.TriangleIndices.Add(0);
            meshGeo.TriangleIndices.Add(1);
            meshGeo.TriangleIndices.Add(2);

            // Add normal in MESH
            Vector3D normal = CalculateNormal(p0, p1, p2);
            meshGeo.Normals.Add(normal);
            meshGeo.Normals.Add(normal);
            meshGeo.Normals.Add(normal);

            // Create MATERIAL
            material = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));

            // Create GEOMETRYMODEL3D wich tackes MESH adn MATERIAL as arguments
            geoModel = new GeometryModel3D(meshGeo, material);


            //Create MODEL3DGROUP
            modelGroup = new Model3DGroup();

            //Add GEOMETRYMODEL3D in MODEL3DGROUP
            modelGroup.Children.Add(geoModel);

            return modelGroup;
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private Vector3D CalculateTraingleNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        void Draw()
        {
            if (strPath == "") return;

            int i, j, k;
            CsFacetData facetData;

            m_lstPolygon.Clear();
            m_lstPlane.Clear();

            if (lstTextModel == null)
            {
                lstTextModel = new List<ModelVisual3D>();
            }
            else
            {
                for (i = 0; i < lstTextModel.Count; i++)
                {
                    viewport.Children.Remove(lstTextModel[i]);
                }

                lstTextModel.Clear();
            }
            // Get the selected file name and display in a TextBox 

            double alpha, beta, radius;
            double x, y, z;
            CsPlane pl;
            int sg = 1;

            for (i = 0; i < m_lstFacetData.Count(); i++)
            {
                facetData = m_lstFacetData[i];
                alpha = facetData.m_Angle * Math.PI / 180.0;
                radius = facetData.getRadius();
                sg = Math.Sign(alpha);
                if (sg == 0) sg = 1;

                for (j = 0; j < facetData.m_Indexs.Count(); j++)
                {
                    beta = facetData.m_Indexs[j] / nGear * Math.PI * 2;
                    x = radius * Math.Sin(alpha) * Math.Cos(beta);
                    y = radius * Math.Sin(alpha) * Math.Sin(beta);
                    z = sg * radius * Math.Cos(alpha);

                    pl = new CsPlane();
                    pl.m_NormalVector = new Vector3D(x, y, z);
                    pl.m_Point = new Point3D(x, y, z);

                    if ((facetData.m_strLabels.Count > j) && (facetData.m_strLabels[j] != ""))
                    {
                        pl.m_strLabel = facetData.m_strLabels[j];
                    }

                    if ((facetData.m_Indexs.Count == 1) && (facetData.m_strLabels.Count == 2))
                    {
                        pl.m_strLabel = facetData.m_strLabels[1];
                    }

                    m_lstPlane.Add(pl);
                }
            }

            CsPolygon pg;
            double L = 10;
            pg = new CsPolygon();
            pg.addPoint(L, L, L); pg.addPoint(L, -L, L); pg.addPoint(-L, -L, L); pg.addPoint(-L, L, L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            pg = new CsPolygon();
            pg.addPoint(L, L, L); pg.addPoint(-L, L, L); pg.addPoint(-L, L, -L); pg.addPoint(L, L, -L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            pg = new CsPolygon();
            pg.addPoint(L, L, L); pg.addPoint(L, L, -L); pg.addPoint(L, -L, -L); pg.addPoint(L, -L, L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            pg = new CsPolygon();
            pg.addPoint(-L, -L, -L); pg.addPoint(-L, L, -L); pg.addPoint(L, L, -L); pg.addPoint(L, -L, -L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            pg = new CsPolygon();
            pg.addPoint(-L, -L, -L); pg.addPoint(-L, -L, L); pg.addPoint(-L, L, L); pg.addPoint(-L, L, -L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            pg = new CsPolygon();
            pg.addPoint(-L, -L, -L); pg.addPoint(L, -L, -L); pg.addPoint(L, -L, L); pg.addPoint(-L, -L, L);
            pg.rearrange();
            m_lstPolygon.Add(pg);

            Point3D pt;

            for (i = 0; i < m_lstPlane.Count(); i++)
            {
                pl = m_lstPlane[i];

                CsCrossPoints cutPoints = new CsCrossPoints();
                CsPolygon cutPg = new CsPolygon();

                for (j = 0; j < m_lstPolygon.Count(); j++)
                {
                    pg = m_lstPolygon[j];
                    if (pg.m_lstPoints.Count() == 0)
                        continue;

                    CsCrossPoints crPoints = CsUtils.CutPolygonByPlane(pg, pl);

                    for (k = 0; k < crPoints.m_Points.Count(); k++)
                    {
                        pt = crPoints.m_Points[k];
                        cutPoints.AddPoint(pt.X, pt.Y, pt.Z);
                    }
                }

                for (j = 0; j < cutPoints.m_Points.Count(); j++)
                {
                    pt = cutPoints.m_Points[j];
                    cutPg.addPoint(pt.X, pt.Y, pt.Z);
                }

                if (cutPg.m_lstPoints.Count() > 2)
                {
                    cutPg.m_strLabel = pl.m_strLabel;
                    cutPg.m_NormalVector = pl.m_NormalVector;
                    cutPg.rearrange();
                    m_lstPolygon.Add(cutPg);
                }

                int tNum = Convert.ToInt16(txtNumber.Text);
                if (i == tNum)
                    break;
            }

            if (model != null) viewport.Children.Remove(model);
            if (wireFrameCube != null) viewport.Children.Remove(wireFrameCube);

            Model3DGroup triangle = new Model3DGroup();
            Point3D ptG = new Point3D();
            for (i = 0; i < m_lstPolygon.Count(); i++)
            {
                pg = m_lstPolygon[i];

                Color clr = Colors.White;
                if (pg.m_strLabel != "")
                {
                    ptG.X = 0; ptG.Y = 0; ptG.Z = 0;

                    for (j = 0; j < pg.m_lstPoints.Count; j++)
                    {
                        ptG.X += pg.m_lstPoints[j].X;
                        ptG.Y += pg.m_lstPoints[j].Y;
                        ptG.Z += pg.m_lstPoints[j].Z;
                    }
                    ptG.X /= pg.m_lstPoints.Count;
                    ptG.Y /= pg.m_lstPoints.Count;
                    ptG.Z /= pg.m_lstPoints.Count;

                    Vector3D ca = camera.LookDirection;

                    double angle = Vector3D.AngleBetween(ca, pg.m_NormalVector) * Math.PI / 180.0;
                    double dis = Math.Abs(Math.Sqrt(ca.X * ca.X + ca.Y * ca.Y + ca.Z * ca.Z) * Math.Cos(angle));
                    Vector3D vtNor = CsUtils.LTVector(pg.m_NormalVector, dis);
                    sg = Math.Sign(vtNor.Z);
                    Vector3D vUp = new Vector3D(- ca.X - vtNor.X * sg, - ca.Y - vtNor.Y * sg, - ca.Z - vtNor.Z * sg);
                    if ((vUp.X == 0) && (vUp.Y == 0) && (vUp.Z == 0))
                    {
                        vUp.X = -1; vUp.Y = -1;
                    }
                    Vector3D dNormalVt = CsUtils.LTVector(pg.m_NormalVector, 0.0001);
                    ptG = Vector3D.Add(dNormalVt, ptG);

                    Brush brText = new SolidColorBrush(Colors.Black);
                    Vector3D vOver = Vector3D.CrossProduct(vUp, pg.m_NormalVector);

                    vUp = CsUtils.LTVector(vUp, 2);
                    vOver = CsUtils.LTVector(vOver, 1);

                    ModelVisual3D mdlText = CsUtils.CreateTextLabel3D(pg.m_strLabel, brText, true, 0.5, ptG, vOver, vUp);
                    
                    lstTextModel.Add(mdlText);
                }
                if (IsSelected(i))
                    clr = Colors.Blue;

                for (j = 1; j < pg.m_lstPoints.Count() - 1; j++)
                {
                    triangle.Children.Add(CreateTriangleModel(pg.m_lstPoints[0], pg.m_lstPoints[j], pg.m_lstPoints[j + 1], clr));
                }
            }

            triangle.Transform = new Transform3DGroup();
            
            model = new ModelVisual3D();
            model.Content = triangle;
            
            viewport.Children.Add(model);

            wireFrameCube = new ScreenSpaceLines3D();

            int width = 2;

            wireFrameCube.Thickness = width;
            wireFrameCube.Color = Colors.OrangeRed;

            for (i = 0; i < m_lstPolygon.Count(); i++)
            {
                pg = m_lstPolygon[i];

                for (j = 0; j < pg.m_lstPoints.Count(); j++)
                {
                    k = j + 1;
                    if (k == pg.m_lstPoints.Count())
                        k = 0;
                    wireFrameCube.Points.Add(pg.m_lstPoints[j]);
                    wireFrameCube.Points.Add(pg.m_lstPoints[k]);
                }
            }

            // Store the instance of Transform3DGroup class in Transform Property of "wireFrameCube"
            // it will later use to rotate the object
            wireFrameCube.Transform = new Transform3DGroup();

            //Add SCREENSPACELINES3D in VIEWPORT3D
            viewport.Children.Add(wireFrameCube);

            for (i = 0; i < lstTextModel.Count; i++)
            {
                viewport.Children.Add(lstTextModel[i]);
            }

            //infoText.Text = string.Format("{0} tetrahedrons", triangulation.Count);
        }
        
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void btnReplay_Click(object sender, RoutedEventArgs e)
        {
            int nIndex = Convert.ToInt16(txtNumber.Text);
            if (nIndex < m_lstPlane.Count())
                nIndex++;

            txtNumber.Text = nIndex.ToString();
            Draw();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".asc";
            dlg.Filter = "asc Files (*.asc)|*.asc";

            CsFacetData facetData;
            string strLine;
            short nCount = 0;
            StreamReader file;
            int nValCount, i, j;

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            m_lstFacetData.Clear();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                strPath = dlg.FileName;

                if (strPath != "")
                {
                    file = File.OpenText(strPath);

                    while ((strLine = file.ReadLine()) != null)
                    {
                        string strTrim = strLine.Trim();
                        string[] strValues = strLine.Substring(1).Trim().Split(' ');

                        if (strTrim.IndexOf("GemCad") == 0)
                        {

                        }
                        else if (strTrim.IndexOf("g ") == 0)
                        {
                            nGear = Convert.ToInt16(strValues[0]);
                        }
                        else if (strTrim.IndexOf("y ") == 0)
                        {

                        }
                        else if (strTrim.IndexOf("I ") == 0)
                        {

                        }
                        else if (strTrim.IndexOf("H ") == 0)
                        {

                        }
                        else if (strTrim.IndexOf("a ") == 0)
                        {
                            facetData = new CsFacetData();
                            facetData.New();

                            nValCount = 0;
                            double value;
                            bool isNumeric;

                            for (i = 0; i < strValues.Length; i++)
                            {
                                isNumeric = double.TryParse(strValues[i], out value);
                                if (isNumeric)
                                {
                                    if (nValCount == 0)
                                    {
                                        facetData.m_Angle = value;
                                    }
                                    else if (nValCount == 1)
                                    {
                                        facetData.m_Radius = value;
                                    }
                                    else
                                    {

                                        facetData.m_Indexs.Add(value);
                                    }
                                    nValCount++;
                                }
                                else
                                {
                                    if (strValues[i].Trim() == "n")
                                    {
                                        string strLabel = strValues[i + 1].Trim();

                                        for (j = facetData.m_strLabels.Count; j < facetData.m_Indexs.Count; j++)
                                        {
                                            facetData.m_strLabels.Add("");
                                        }

                                        facetData.m_strLabels.Add(strLabel);
                                        i++;
                                    }
                                }
                            }
                            m_lstFacetData.Add(facetData);
                        }
                        else if (strTrim.IndexOf("G ") == 0)
                        {

                        }
                        else if (strTrim.IndexOf("F ") == 0)
                        {

                        }

                        nCount++;
                    }

                    file.Close();
                }


                Draw();
            }
        }

        private bool IsSelected(int nIndex)
        {
            bool bIsSelected = (nIndex == m_selectedIndex);

            return bIsSelected;
        }

        private void viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt2 = Mouse.GetPosition(viewport);
            Point pt;
            List<Point> lstPt = new List<Point>();
            CsPolygon pg;

            int i, j;
            m_selectedIndexs.Clear();
            
            for (i = 0; i < m_lstPolygon.Count(); i++)
            {
                lstPt.Clear();
                pg = m_lstPolygon[i];

                if (pg.m_lstPoints.Count() > 2)
                {
                    for (j = 0; j < pg.m_lstPoints.Count(); j++)
                    {
                        pt = CsUtils.Convert3DPoint(pg.m_lstPoints[j], viewport);
                        lstPt.Add(pt);
                    }

                    if (CsUtils.IsContain(lstPt, pt2))
                    {
                        m_selectedIndexs.Add(i);
                    }
                }
            }

            if (m_selectedIndexs.Count == 0) 
                return;

            CsPolygon pg0, pg1;
            pg0 = m_lstPolygon[m_selectedIndexs[0]];
            pg1 = m_lstPolygon[m_selectedIndexs[1]];
            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();

            for (i = 0; i < pg0.m_lstPoints.Count(); i++)
            {
                p0.X += (pg0.m_lstPoints[i].X / pg0.m_lstPoints.Count());
                p0.Y += (pg0.m_lstPoints[i].Y / pg0.m_lstPoints.Count());
                p0.Z += (pg0.m_lstPoints[i].Z / pg0.m_lstPoints.Count());

            }

            for (i = 0; i < pg1.m_lstPoints.Count(); i++)
            {
                p1.X += (pg1.m_lstPoints[i].X / pg1.m_lstPoints.Count());
                p1.Y += (pg1.m_lstPoints[i].Y / pg1.m_lstPoints.Count());
                p1.Z += (pg1.m_lstPoints[i].Z / pg1.m_lstPoints.Count());

            }

            bool bResult;
            Vector3D ca = camera.LookDirection; 
            Point3D ptCamera = new Point3D(ca.X, ca.Y, ca.Z);

            bResult = viewport.Camera.Transform.TryTransform(ptCamera, out ptCamera);
            ca = new Vector3D(ptCamera.X, ptCamera.Y, ptCamera.Z);

            Vector3D tmp = new Vector3D(p0.X - p1.X, p0.Y - p1.Y, p0.Z - p1.Z);
            double angle = Vector3D.AngleBetween(ca, tmp);
           

            if (angle > 90)
            {
                m_selectedIndex = m_selectedIndexs[0];
            }
            else
            {
                m_selectedIndex = m_selectedIndexs[1];
            }

            Draw();
        }
    }
}