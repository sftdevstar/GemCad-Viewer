using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace GemCad_Viewer
{
    class CsPlane
    {
        public Point3D m_Point;
        public Vector3D m_NormalVector;
        public string m_strLabel = "";

        public void setPlane(Point3D pt, Vector3D vt)
        {
            m_Point = pt;
            m_NormalVector = vt;
        }
    }
}
