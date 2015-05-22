using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorConquer;
using Extensions;

namespace Server
{
	class ColorConquerGame
	{
		User Alice, Bob;
		User _currentTurn;
		Board _board;
		int _size;

		Dictionary<User, HashSet<Cell>> MyCells;
		Dictionary<User, HashSet<Cell>> EdgeCells;

		int[] dx = new int[] { 1, 0, -1, 0 };
		int[] dy = new int[] { 0, 1, 0, -1 };

		public ColorConquerGame(User alice, User bob)
		{
			Alice = alice;
			Bob = bob;
		}

		public bool StartGame()
		{
			_size = 19;
			if (Alice == null || Bob == null) return false;
			_board = new Board(_size);
			MyCells = new Dictionary<User, HashSet<Cell>>();
			EdgeCells = new Dictionary<User, HashSet<Cell>>();
			_currentTurn = Alice;
			SetUser(Alice, 0, 0);
			SetUser(Bob, _size - 1, _size - 1);
			return true;
		}

		public void SetUser(User user, int row, int col)
		{
			if (!IsValid(row, col)) return;

			var cells = _board.Cells;

			MyCells.Add(user, new HashSet<Cell>());
			EdgeCells.Add(user, new HashSet<Cell>());

			AddEdgeCell(user, cells[row, col]);
			SetColor(user, cells[row, col].Color, true);
		}

		public void AddEdgeCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			EdgeCells[user].Add(cell);
		}

		public void AddMyCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			if (!EdgeCells[user].Contains(cell)) return; // EdgeCells 에 속해있지 않던 셀은 MyCell 이 될 수 없다.
			MyCells[user].Add(cell);
			EdgeCells[user].Remove(cell);
		}

		public void SetColor(User user, Color color, bool isFirst = false)
		{
			lock (_currentTurn)
			{
				if (_currentTurn != user) return; // 상대방의 입력은 쳐낸다.
				if (!isFirst && Alice.CurrentColor == color) return; // 자신, 상대방 색을 누르면 쳐낸다.
				if (!isFirst && Bob.CurrentColor == color) return;

				// DFS, BFS 뭐든 둘중 하나 써야함
				var que = new Queue<Cell>();
				foreach (var matched in EdgeCells[user].Where(e => e.Color == color))
					que.Enqueue(matched);

				var reserved = new HashSet<Cell>();

				while (que.Count > 0)
				{
					var currCell = que.Dequeue();
					if (reserved.Contains(currCell)) continue;
					reserved.Add(currCell);
					
					var matchedList = _board.GetEdges(currCell)
						.Where(e => e.Color == color)
						.Where(e => !reserved.Contains(e))
						.Where(e => !MyCells[user].Contains(e));
					
					foreach (var matched in matchedList)
						que.Enqueue(matched);
				}

				_currentTurn = _currentTurn == Alice ? Bob : Alice;
			}
		}
		
		public bool IsValid(int row, int col)
		{
			return _board.IsValid(row, col);
		}
	}

	class Board
	{
		int _size;
		public Cell[,] Cells;

		static int[] dx = new int[] { 1, 0, -1, 0 };
		static int[] dy = new int[] { 0, 1, 0, -1 };

		public Board(int size, int countColor = 6)
		{
			_size = size;
			Cells = new Cell[size, size];
			for (var row = 0; row < size; row++)
			{
				for (var col = 0; col < size; col++)
				{
					Cells[row, col] = new Cell(row, col);
				}
			}

			#region 대칭으로 색칠
			var Colors = Enum.GetValues(typeof(Color));
			for (var row = 0; row < size; row++)
			{
				for (var col = 0; col < size - row; col++)
				{
					var index = StaticRandom.Next(countColor);
					Cells[row, col].Color = (Color)Colors.GetValue(index);
					Cells[size - row - 1, size - col - 1].Color = (Color)Colors.GetValue(countColor - index);
				}
			}
			#endregion
		}

		public IEnumerable<Cell> GetEdges(Cell cell)
		{
			return dx.Zip(dy, (dxx, dyy) => new {dx = dxx, dy = dyy})
				.Where(e => IsValid(cell.Row + e.dx, cell.Column + e.dy))
				.Select(e => Cells[cell.Row + e.dx, cell.Column + e.dy]);

		}
		
		public bool IsValid(int row, int col)
		{
			return (row >= 0 && row < _size && col >= 0 && col < _size);
		}

	}

	class Cell
	{
		public User Owner;
		public int Row, Column;
		public Color Color;

		public Cell(int row, int col)
		{
			Row = row;
			Column = col;
			Owner = null;
			Color = Color.A;
		}
	}
}
