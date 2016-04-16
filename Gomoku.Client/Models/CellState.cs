using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gomoku.Client.Models
{
    public enum CellState
    {
        None,       // 일반
        Point,      // 9포인트
        Top,        // 위
        Bottom,     // 아래
        Left,       // 왼쪽
        Right,      // 오른쪽
    }
}