using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GemCad_Viewer
{
    class CsFacetData
    {
        public double m_Angle;
        public double m_Radius;
        public List<double> m_Indexs;
        public List<string> m_strLabels;
        public double m_Zoom = 9;

        public void New()
        {
            m_Angle = 0;
            m_Radius = 0;
            m_Indexs = new List<double>();
            m_strLabels = new List<string>();
        }

        public double getRadius()
        {
            return m_Radius * m_Zoom;
        }
    }
}
