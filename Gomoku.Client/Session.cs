
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Gomoku.Client.Models;

namespace Gomoku.Client
{
    public class Session
    {
        public const int LineCount = 19;
        public const int GameSize = 5;
        private System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        private List<Models.PointModel> m_List = new List<PointModel>();
        public Session()
        {
        }

        delegate IComparable EstimateFunction(Point location, bool color);

        public Point SelectBestCell(bool color)
        {
            var candidates = FilterCellsStageThree(color);

            if (candidates.Count == 0)
                return new Point(-1, -1);

            if (candidates.Count == 1)
                return candidates[0];

            int index = new Random().Next(0, candidates.Count - 1);
            return candidates[index];
        }

        public IEnumerable<Point> EnumerateEmptyCells()
        {
            for (int _Y = 0; _Y < Session.LineCount; _Y++)
            {
                for (int _X = 0; _X < Session.LineCount; _X++)
                {
                    yield return new Point(_X, _Y);
                }
            }
        }

        internal List<Point> FilterCellsStageOne(bool color)
        {
            return this.FilterCellsCore(this.EnumerateEmptyCells(), this.EstimateForStageOne, color);
        }

        List<Point> FilterCellsStageTwo(bool color)
        {
            return this.FilterCellsCore(this.FilterCellsStageOne(color), this.EstimateForStageTwo, color);
        }

        List<Point> FilterCellsStageThree(bool color)
        {
            return this.FilterCellsCore(this.FilterCellsStageTwo(color), this.EstimateForStageThree, color);
        }

        List<Point> FilterCellsCore(IEnumerable<Point> source, EstimateFunction estimator, bool color)
        {
            var result = new List<Point>();
            IComparable bestEstimate = null;

            foreach (Point location in source)
            {
                if (this.List.Where(r => r.X == location.X && r.Y == location.Y).Count() != 0)
                {
                    continue;
                }

                var estimate = estimator(location, color);

                int compareResult = estimate.CompareTo(bestEstimate);
                if (compareResult < 0)
                    continue;
                if (compareResult > 0)
                {
                    result.Clear();
                    bestEstimate = estimate;
                }
                result.Add(location);
            }

            return result;
        }

        internal IComparable EstimateForStageOne(Point location, bool color)
        {
            int selfScore = 1 + CalcScore(location, color);
            int opponentScore = 1 + CalcScore(location, GetOpponentColor(color));

            if (selfScore >= GameSize)
                selfScore = int.MaxValue;

            return Math.Max(selfScore, opponentScore);
        }

        internal IComparable EstimateForStageOne(int x, int y, bool color)
        {
            return EstimateForStageOne(new Point(x, y), color);
        }

        internal IComparable EstimateForStageTwo(Point location, bool color)
        {
            int cx = location.X;
            int cy = location.Y;

            int selfCount = 0;
            int opponentCount = 0;

            for (int x = cx - 1; x <= cx + 1; x++)
            {
                for (int y = cy - 1; y <= cy + 1; y++)
                {
                    if (this.List.Where(r => r.X == x && r.Y == y).Count() != 0)
                    {
                        if (this.List.Where(r => r.X == x && r.Y == y).FirstOrDefault().IsBlack == color)
                        {
                            selfCount++;
                        }

                        if (this.List.Where(r => r.X == x && r.Y == y).FirstOrDefault().IsBlack == GetOpponentColor(color))
                        {
                            opponentCount++;
                        }
                    }
                }
            }

            return 2 * selfCount + opponentCount;
        }

        public bool GetOpponentColor(bool myColor)
        {
            if (myColor == true)
                return false;

            if (myColor == false)
                return true;

            throw new ArgumentException();
        }

        internal IComparable EstimateForStageTwo(int x, int y, bool color)
        {
            return EstimateForStageTwo(new Point(x, y), color);
        }

        internal IComparable EstimateForStageThree(Point location, bool color)
        {
            var dx = location.X - Session.LineCount / 2;
            var dy = location.Y - Session.LineCount / 2;
            return -Math.Sqrt(dx * dx + dy * dy);
        }

        internal IComparable EstimateForStageThree(int x, int y, bool color)
        {
            return EstimateForStageThree(new Point(x, y), color);
        }

        //public bool GetCellState(int x, int y)
        //{
        //    if (x == 0)
        //    {
        //        return CellState.Left;
        //    }
        //    else if (y == 0)
        //    {
        //        return CellState.Top;
        //    }
        //    else if (x == Session.LineCount - 1)
        //    {
        //        return CellState.Right;
        //    }

        //    if (x == 0 && y == 0)
        //    {
        //    }
        //    else if (x == 0 && y == Session.LineCount - 1)
        //    {
        //        return CellState.Top | CellState.Left;
        //    }
        //    else if (x == Session.LineCount - 1 && y == 0)
        //    {
        //    }
        //    else if (x == Session.LineCount - 1 && y == Session.LineCount - 1)
        //    {
        //    }
        //    else if (x == 0 || x == Session.LineCount - 1)
        //    {
        //        return CellState.Top | CellState.Left;
        //    }
        //    else
        //    {
        //        if ((x == 3 && y == 3)
        //            || (x == 3 && y == 9)
        //            || (x == 3 && y == 15)
        //            || (x == 9 && y == 3)
        //            || (x == 9 && y == 9)
        //            || (x == 9 && y == 15)
        //            || (x == 15 && y == 3)
        //            || (x == 15 && y == 9)
        //            || (x == 15 && y == 15)
        //            )
        //        {
        //            return CellState.Point;
        //        }
        //    }

        //    return CellState.None;
        //}

        public void Start()
        {
            this.m_List.Clear();
            this.m_Stopwatch.Start();
        }

        public void Save(string filename)
        {
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(List<PointModel>));
            TextWriter _TextWriter = new StreamWriter(filename);
            _XmlSerializer.Serialize(_TextWriter, this.m_List);
            _TextWriter.Close();
        }

        public void Load(string filename)
        {
            FileStream _FileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(List<PointModel>));
            List<PointModel> _Model = (List<PointModel>)_XmlSerializer.Deserialize(_FileStream);

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

        public void PlayTurn(PointModel model)
        {
            this.m_Stopwatch.Stop();

            this.Turn = !this.Turn;
            model.Elapsed = this.m_Stopwatch.Elapsed.ToString();
            model.Index = this.Index;
            this.m_List.Add(model);
            this.m_Stopwatch.Restart();
        }

        public bool HaveVictoryAt(PointModel model, bool color)
        {
            return model.IsBlack == color && this.CalcScore(model.Loction, color) >= 5 - 1;
        }

        private int CalcScore(Point model, bool color)
        {
            int[] counts = new int[] {
                this.CountStonesInDirection(model, -1, 0, color) + this.CountStonesInDirection(model, 1, 0, color),
                this.CountStonesInDirection(model, 0, -1, color) + this.CountStonesInDirection(model, 0, 1, color),
                this.CountStonesInDirection(model, -1, -1, color) + this.CountStonesInDirection(model, 1, 1, color),
                this.CountStonesInDirection(model, -1, 1, color) + this.CountStonesInDirection(model, 1, -1, color)
            };

            int result = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                result = Math.Max(result, counts[i]);
            }

            return result;
        }

        private int CountStonesInDirection(Point model, int x, int y, bool color)
        {
            int result = 0;
            for (int i = 1; i < 5; i++)
            {
                Point current = model + new Size(i * x, i * y);
                var _Model = this.List.Where(r => r.X == current.X && r.Y == current.Y).FirstOrDefault();
                if (_Model == null)
                {
                    break;
                }

                if (_Model.IsBlack != color)
                {
                    break;
                }

                result++;
            }

            return result;
        }

        //public Point[] GetPointList
        //{
        //    get
        //    {
        //        return this.m_PointList;
        //    }
        //}

        public int Index
        {
            get
            {
                return this.List.Count() + 1;
            }

        }

        public bool Turn { get; set; }

        public PointModel[] List
        {
            get
            {
                return this.m_List.ToArray();
            }
        }
    }
}
