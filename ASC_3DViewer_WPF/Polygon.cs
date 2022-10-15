using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace GemCad_Viewer
{
    class CsPolygon
    {
        public List<Point3D> m_lstPoints = new List<Point3D>();
        public string m_strLabel = "";
        public Vector3D m_NormalVector;

        public void addPoint(double px, double py, double pz)
        {
            Point3D pt = new Point3D(px, py, pz);

            for (int i = 0; i < m_lstPoints.Count(); i++)
            {
                if (CsUtils.IsSamePoint(m_lstPoints[i], pt))
                {
                    return;
                }
            }

            m_lstPoints.Add(new Point3D(px, py, pz));
        }

        public void removePoint(int nIndex)
        {
            m_lstPoints.RemoveAt(nIndex);
        }

        public void rearrange()
        {
            if (m_lstPoints.Count() < 4)
                return;

            int i, j, maxIndex = 0;
            Point3D p0 = m_lstPoints[0];
            Point3D p1, p2;

            Vector3D g0, g1;
            List<Point3D> lstPoints = new List<Point3D>();
            for (i = 0; i < m_lstPoints.Count(); i++)
                lstPoints.Add(new Point3D());
            lstPoints[0] = p0;

            double angle, maxAngle;
            List<double> dAngles = new List<double>();
            dAngles.Add(0);

            for (i = 1; i < m_lstPoints.Count(); i++)
            {
                p1 = m_lstPoints[i];
                maxAngle = 0;
                for (j = 1; j < m_lstPoints.Count(); j++)
                {
                    if (i != j)
                    {
                        p2 = m_lstPoints[j];

                        g0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
                        g1 = new Vector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);

                        angle = Vector3D.AngleBetween(g0, g1);

                        if (maxAngle < angle)
                            maxAngle = angle;
                    }
                }

                dAngles.Add(maxAngle);
            }

            maxAngle = 0;

            for (i = 1; i < dAngles.Count(); i++)
            {
                if (maxAngle < dAngles[i])
                {
                    maxAngle = dAngles[i];
                    maxIndex = i;
                }
            }

            p1 = m_lstPoints[maxIndex];
            lstPoints[1] = p1;
            dAngles.Clear();
            dAngles.Add(-1);

            for (i = 1; i < m_lstPoints.Count(); i++)
            {
                p2 = m_lstPoints[i];
                g0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
                g1 = new Vector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);

                angle = Vector3D.AngleBetween(g0, g1);
                dAngles.Add(angle);
            }

            int nLows;

            for (i = 1; i < dAngles.Count(); i++)
            {
                if (i != maxIndex)
                {
                    nLows = 0;
                    for (j = 0; j < dAngles.Count(); j++)
                    {
                        if (dAngles[j] < dAngles[i])
                            nLows++;
                    }

                    lstPoints[nLows] = m_lstPoints[i];
                }
            }

            m_lstPoints = lstPoints;
        }
    }
}
