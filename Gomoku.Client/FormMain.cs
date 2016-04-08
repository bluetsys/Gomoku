using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace Gomoku.Client
{
    public partial class FormMain : Form
    {
        private Session m_Session = new Session();
        private int m_Margin = 30;
        private Color lineColor = Color.FromArgb(0, 0, 0);
        private int m_LineCount = 19;

        public FormMain()
        {
            this.InitializeComponent();
            this.m_Margin = this.pictureBoxBoard.Width / this.m_LineCount;
            this.RePaintGoBoard();
            this.m_Session.Start();
        }

        private void NewRePaintGoBoard()
        {
            this.pictureBoxBoard.Refresh();
        }

        private void RePaintGoBoard()
        {
            Bitmap _Bitmap = new Bitmap(this.pictureBoxBoard.Width, this.pictureBoxBoard.Height);
            using (Graphics _Graphics = Graphics.FromImage(_Bitmap))
            {
                Pen _Pen = new Pen(lineColor, 3);
                Console.WriteLine("{0}, {1}, {2}, {3}", this.m_Margin / 2, this.m_Margin / 2, this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2);
                _Graphics.DrawLine(_Pen, this.m_Margin / 2, this.m_Margin / 2, this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2);
                _Graphics.DrawLine(_Pen, this.m_Margin / 2, this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2, this.m_Margin / 2);
                _Graphics.DrawLine(_Pen, this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2);
                _Graphics.DrawLine(_Pen, this.m_Margin * this.m_LineCount - this.m_Margin / 2, this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2, this.m_Margin * this.m_LineCount - this.m_Margin / 2);

                _Pen = new Pen(lineColor, 1);
                for (int i = this.m_Margin + this.m_Margin / 2; i < this.m_Margin * m_LineCount - this.m_Margin / 2; i += this.m_Margin)
                {
                    _Graphics.DrawLine(_Pen, this.m_Margin / 2, i, this.m_Margin * m_LineCount - this.m_Margin / 2, i);
                    _Graphics.DrawLine(_Pen, i, this.m_Margin / 2, i, this.m_Margin * m_LineCount - this.m_Margin / 2);
                }

                int _n = 10;//점크기
                            // 9군데 점
                int _p1 = 4 * this.m_Margin - 20;
                int _p2 = (this.m_LineCount + 1) * this.m_Margin / 2 - 20;
                int _p3 = (this.m_LineCount - 3) * this.m_Margin - 20;
                // this.pictureBoxBoard.BackColor = Color.Yellow;

                // 9군데 점찍기
                _Graphics.FillEllipse(Brushes.Black, _p1, _p1, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p2, _p1, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p3, _p1, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p1, _p2, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p2, _p2, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p3, _p2, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p1, _p3, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p2, _p3, _n, _n);
                _Graphics.FillEllipse(Brushes.Black, _p3, _p3, _n, _n);

                this.pictureBoxBoard.Image = _Bitmap;
            }
        }

        private void pictureBoxBoard_MouseMove(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Default;

            int _X = (e.Location.X / this.m_Margin) + 1;
            int _Y = (e.Location.Y / this.m_Margin) + 1;

            if (this.m_Session.List.Where(r => r.X == _X && r.Y == _Y).Count() == 0)
            {
                base.Cursor = Cursors.Hand;
            }
        }

        private void NewMethod(Graphics graphics, Model model, bool undo = false, int size = 26)
        {
            int _XPoint = (this.m_Margin) * model.X - this.m_Margin + (this.m_Margin - size) / 2;
            int _YPoint = (this.m_Margin) * model.Y - this.m_Margin + (this.m_Margin - size) / 2;

            Bitmap _Bitmap = new Bitmap(size, size);
            Graphics _Graphics = Graphics.FromImage(_Bitmap);

            if (!undo)
            {
                var _Turn1 = new System.Drawing.SolidBrush(Color.FromArgb(0, 0, 0));
                var _Turn2 = new System.Drawing.SolidBrush(Color.FromArgb(255, 255, 255));

                if (model.IsBlack)
                {
                    _Turn1 = new System.Drawing.SolidBrush(Color.FromArgb(255, 255, 255));
                    _Turn2 = new System.Drawing.SolidBrush(Color.FromArgb(0, 0, 0));
                }

                _Graphics.FillEllipse(_Turn1, 0, 0, size, size);
                _Graphics.DrawString(model.Index.ToString("000"), new Font("Arial", 10), _Turn2, 1, 6);
            }
            else
            {
                var _Pen = new Pen(lineColor, 1);
                _Graphics.FillEllipse(new System.Drawing.SolidBrush(this.pictureBoxBoard.BackColor), 0, 0, size, size);
                _Graphics.DrawLine(_Pen, size / 2, 0, size / 2, size);
                _Graphics.DrawLine(_Pen, 0, size / 2, size, size / 2);
            }

            graphics.DrawImage(_Bitmap, _XPoint, _YPoint);
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Default;
        }

        private void pictureBoxBoard_MouseUp(object sender, MouseEventArgs e)
        {
            int _X = (e.Location.X / this.m_Margin) + 1;
            int _Y = (e.Location.Y / this.m_Margin) + 1;

            if (this.m_Session.List.Where(r => r.X == _X && r.Y == _Y).Count() != 0)
            {
                return;
            }

            Model _Model = new Model();

            _Model.X = _X;
            _Model.Y = _Y;

            _Model.IsBlack = this.m_Session.Turn;

            this.m_Session.PlayTurn(_Model);

            this.NewMethod(pictureBoxBoard.CreateGraphics(), _Model);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var _FileDialog = new SaveFileDialog();

            _FileDialog.InitialDirectory = "c:\\";
            _FileDialog.Filter = "gomoku play file (*.gdp)|*.gdp|All files (*.*)|*.*";
            _FileDialog.FilterIndex = 1;
            _FileDialog.RestoreDirectory = true;

            if (_FileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.m_Session.Save(_FileDialog.FileName);
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            var _FileDialog = new OpenFileDialog();

            _FileDialog.InitialDirectory = "c:\\";
            _FileDialog.Filter = "gomoku play file (*.gdp)|*.gdp|All files (*.*)|*.*";
            _FileDialog.FilterIndex = 1;
            _FileDialog.RestoreDirectory = true;

            if (_FileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.m_Session.Load(_FileDialog.FileName);

            this.NewRePaintGoBoard();
            foreach (var item in this.m_Session.List)
            {
                this.NewMethod(this.pictureBoxBoard.CreateGraphics(), item);
            }
        }

        private void buttonReStart_Click(object sender, EventArgs e)
        {
            this.RePaintGoBoard();
            this.m_Session.Start();
        }

        private void buttonUnDo_Click(object sender, EventArgs e)
        {
            // this.NewRePaintGoBoard();
            this.NewMethod(this.pictureBoxBoard.CreateGraphics(), this.m_Session.List.LastOrDefault(), true);
            this.m_Session.PlayUnDo();
            //foreach (var item in this.m_Session.List)
            //{
            //    this.NewMethod(this.pictureBoxBoard.CreateGraphics(), item);
            //}
        }
    }

    [Serializable()]
    public class Session
    {
        private System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        private List<Model> m_List = null;

        public Session()
        {
            this.m_List = new List<Model>();
        }

        public void Start()
        {
            this.m_List.Clear();
            this.m_Stopwatch.Start();
        }

        public void Save(string filename)
        {
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(List<Model>));
            TextWriter _TextWriter = new StreamWriter(filename);
            _XmlSerializer.Serialize(_TextWriter, this.m_List);
            _TextWriter.Close();
        }

        public void Load(string filename)
        {
            FileStream _FileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(List<Model>));
            List<Model> _Model = (List<Model>)_XmlSerializer.Deserialize(_FileStream);

            this.m_List = _Model;

            _FileStream.Close();
        }

        public void PlayUnDo()
        {
            if (this.m_List.Count == 0)
            {
                return;
            }

            this.Turn = !this.Turn;
            this.m_List.Remove(this.m_List.Last());
        }

        public void PlayTurn(Model model)
        {
            this.m_Stopwatch.Stop();

            this.Turn = !this.Turn;
            model.Elapsed = this.m_Stopwatch.Elapsed.ToString();
            model.Index = this.Index;
            this.m_List.Add(model);
            this.m_Stopwatch.Restart();
        }

        public int Index
        {
            get
            {
                return this.List.Count() + 1;
            }

        }

        public bool Turn { get; set; }

        public Model[] List
        {
            get
            {
                return this.m_List.ToArray();
            }
        }
    }

    public class Model
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
    }
}