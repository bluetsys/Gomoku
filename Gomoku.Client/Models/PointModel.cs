using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Gomoku.Client.Models
{
    public class PointModel
    {
        [XmlAttribute]
        public string Elapsed { get; set; }

        [XmlAttribute]
        public int Index { get; set; }

        [XmlAttribute]
        public int X { get; set; }

        [XmlAttribute]
        public int Y { get; set; }

        [XmlAttribute]
        public bool IsBlack { get; set; }

        [XmlIgnore]
        public Point Loction
        {
            get
            {
                return new Point(this.X, this.Y);
            }
        }

        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}, color:{2}", this.X, this.Y, this.IsBlack);
        }
    }
}