using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Gomoku.Client.Models;

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

                int _n = this.m_Margin / 3;//점크기
                                           // 9군데 점
                int _p1 = 4 * this.m_Margin - (this.m_Margin * 2) / 3;
                int _p2 = (this.m_LineCount + 1) * this.m_Margin / 2 - (this.m_Margin * 2) / 3;
                int _p3 = (this.m_LineCount - 3) * this.m_Margin - (this.m_Margin * 2) / 3;
                this.pictureBoxBoard.BackColor = Color.Yellow;

                if (this.m_LineCount == 19)
                {
                    Point[] _Point = new Point[] {
                            new Point(3,3),
                            new Point(3,9),
                            new Point(3,15),
                            new Point(9,3),
                            new Point(9,9),
                            new Point(9,15),
                            new Point(15,3),
                            new Point(15,9),
                            new Point(15,15),
                    };

                    foreach (var item in _Point)
                    {
                        int _XPoint = (this.m_Margin) * item.X + 10;
                        int _YPoint = (this.m_Margin) * item.Y + 10;
                        _Graphics.FillEllipse(Brushes.Black, _XPoint, _YPoint, 10, 10);
                    }
                }

                this.pictureBoxBoard.Image = _Bitmap;
            }
        }

        private void pictureBoxBoard_MouseMove(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Default;

            int _X = (e.Location.X / this.m_Margin);
            int _Y = (e.Location.Y / this.m_Margin);

            if (this.m_Session.List.Where(r => r.X == _X && r.Y == _Y).Count() == 0)
            {
                base.Cursor = Cursors.Hand;
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Default;
        }

        private void pictureBoxBoard_MouseUp(object sender, MouseEventArgs e)
        {
            int _X = (e.Location.X / this.m_Margin);
            int _Y = (e.Location.Y / this.m_Margin);

            if (this.m_Session.List.Where(r => r.X == _X && r.Y == _Y).Count() != 0)
            {
                return;
            }

            PointModel _Model = new PointModel()
            {
                X = _X,
                Y = _Y,
                IsBlack = this.m_Session.Turn,
            };

            this.m_Session.PlayTurn(_Model);
            this.NewMethod(pictureBoxBoard.CreateGraphics(), _Model);
            this.MakeMove(_Model, _Model.IsBlack);

            bool opponentColor = this.m_Session.GetOpponentColor(_Model.IsBlack);
            Point opponentLocation = this.m_Session.SelectBestCell(opponentColor);

            PointModel _Model1 = new PointModel()
            {
                X = opponentLocation.X,
                Y = opponentLocation.Y,
                IsBlack = this.m_Session.Turn,
            };

            this.m_Session.PlayTurn(_Model1);
            this.NewMethod(pictureBoxBoard.CreateGraphics(), _Model1);
            this.MakeMove(_Model1, _Model1.IsBlack);
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
            this.NewMethod(this.pictureBoxBoard.CreateGraphics(), this.m_Session.List.LastOrDefault(), true);
            this.m_Session.PlayUnDo();
        }

        private void MakeMove(PointModel model, bool color)
        {
            if (this.m_Session.HaveVictoryAt(model, color))
            {
                MessageBox.Show(string.Format("{0} 승", color ? "백" : "흑"));
            }
        }

        private void NewMethod(Graphics graphics, PointModel model, bool undo = false, int size = 26)
        {
            if (model == null)
            {
                return;
            }

            if (size > this.m_Margin)
            {
                size = this.m_Margin;
            }
            int _XPoint = (this.m_Margin) * model.X + (this.m_Margin - size) / 2;
            int _YPoint = (this.m_Margin) * model.Y + (this.m_Margin - size) / 2;

            Bitmap _Bitmap = new Bitmap(size, size);
            Graphics _Graphics = Graphics.FromImage(_Bitmap);

            if (!undo)
            {
                var _Turn1 = Brushes.Black;
                var _Turn2 = Brushes.White;

                if (model.IsBlack)
                {
                    _Turn1 = Brushes.White;
                    _Turn2 = Brushes.Black;
                }

                _Graphics.FillEllipse(_Turn1, 0, 0, size, size);
                _Graphics.DrawString(model.Index.ToString("000"), new Font("Arial", m_Margin / 3), _Turn2, 1, 6);
            }
            else
            {
                var _Pen = new Pen(lineColor, 3);
                _Graphics.FillEllipse(new System.Drawing.SolidBrush(this.pictureBoxBoard.BackColor), 0, 0, size, size);

                if (model.X == 0 && model.Y == 0)
                {
                    _Graphics.DrawLine(_Pen, size / 2, size / 2, size, size / 2); // 세로선
                    _Graphics.DrawLine(_Pen, size / 2, size / 2, size / 2, size);
                }
                else if (model.X == 0 && model.Y == this.m_LineCount - 1)
                {
                    _Graphics.DrawLine(_Pen, size / 2, size / 2, size, size / 2); // 세로선
                    _Graphics.DrawLine(_Pen, size / 2, 0, size / 2, size / 2);
                }
                else if (model.X == this.m_LineCount - 1 && model.Y == 0)
                {
                    _Graphics.DrawLine(_Pen, 0, size / 2, size / 2, size / 2); // 세로선
                    _Graphics.DrawLine(_Pen, size / 2, size / 2, size / 2, size);
                }
                else if (model.X == this.m_LineCount - 1 && model.Y == this.m_LineCount - 1)
                {
                    _Graphics.DrawLine(_Pen, size / 2, size / 2, size / 2, 0); // 세로선
                    _Graphics.DrawLine(_Pen, 0, size / 2, size / 2, size / 2);
                }
                else if (model.X == 0 || model.X == this.m_LineCount - 1)
                {
                    _Graphics.DrawLine(_Pen, size / 2, 0, size / 2, size); // 세로선
                    if (model.X == 0)
                    {
                        _Graphics.DrawLine(new Pen(lineColor, 1), size / 2, size / 2, size, size / 2);
                    }
                    else
                    {
                        _Graphics.DrawLine(new Pen(lineColor, 1), 0, size / 2, size / 2, size / 2);
                    }
                }
                else if (model.Y == 0 || model.Y == this.m_LineCount - 1)
                {
                    _Graphics.DrawLine(_Pen, 0, size / 2, size, size / 2); // 가로선
                    if (model.Y == 0)
                    {
                        _Graphics.DrawLine(new Pen(lineColor, 1), size / 2, size / 2, size / 2, size);
                    }
                    else
                    {
                        _Graphics.DrawLine(new Pen(lineColor, 1), size / 2, 0, size / 2, size / 2);
                    }
                }
                else
                {
                    _Pen = new Pen(lineColor, 1);
                    _Graphics.DrawLine(_Pen, size / 2, 0, size / 2, size);
                    _Graphics.DrawLine(_Pen, 0, size / 2, size, size / 2);

                    if ((model.X == 3 && model.Y == 3)
                        || (model.X == 3 && model.Y == 9)
                        || (model.X == 3 && model.Y == 15)
                        || (model.X == 9 && model.Y == 3)
                        || (model.X == 9 && model.Y == 9)
                        || (model.X == 9 && model.Y == 15)
                        || (model.X == 15 && model.Y == 3)
                        || (model.X == 15 && model.Y == 9)
                        || (model.X == 15 && model.Y == 15)
                        )
                    {
                        int _n = this.m_Margin / 3;//점크기
                        _Graphics.FillEllipse(Brushes.Black, (size - _n) / 2, (size - _n) / 2, _n, _n);
                    }
                }

            }

            graphics.DrawImage(_Bitmap, _XPoint, _YPoint);
        }
    }
}