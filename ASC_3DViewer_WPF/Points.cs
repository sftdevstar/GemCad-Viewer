using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace GemCad_Viewer
{
    class CsCrossPoints
    {
        public List<Point3D> m_Points = new List<Point3D>();

        public void AddPoint(double px, double py, double pz)
        {
            Point3D pt = new Point3D(px, py, pz);

            for (int i = 0; i < m_Points.Count(); i++)
            {
                if (CsUtils.IsSamePoint(m_Points[i], pt))
                {
                    return;
                }
            }

            m_Points.Add(pt);
        }
    }
}
