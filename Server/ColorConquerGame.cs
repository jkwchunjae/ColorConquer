using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Extensions;

namespace Server
{
	public class ColorConquerGame
	{
		public User Alice, Bob;
		User _currentTurn;
		Board _board;
		int _size;
		int _countColor;

		Cell[,] Cells { get { return _board.Cells; } }
		public bool IsRunning { get; private set; }
		public User CurrentTurn { get { return _currentTurn; } }
		public int Size { get { return _size; } }
		public int CountColor { get { return _countColor; } }
		public List<string> CellsColor
		{
			get
			{
				var cellsColor = new List<string>();
				for (var row = 0; row < _size; row++)
				{
					var colors = string.Empty;
					for (var col = 0; col < _size; col++)
						colors += Cells[row, col].Color.ToString();
					cellsColor.Add(colors);
				}
				return cellsColor;
			}
		}

		Dictionary<User, HashSet<Cell>> MyCells;
		Dictionary<User, HashSet<Cell>> EdgeCells;

		static int[] dx = new int[] { 1, 0, -1, 0 };
		static int[] dy = new int[] { 0, 1, 0, -1 };

		public ColorConquerGame(User alice, User bob, int size = 15, int countColor = 6)
		{
			Alice = alice;
			Bob = bob;
			_size = size;
			_countColor = countColor;
		}

		#region public void Print()
		public void Print()
		{
#if DEBUG
			string tmp = "";
			for (var row = 0; row < _size; row++)
			{
				tmp = "";
				for (var col = 0; col < _size; col++)
				{
					tmp += Cells[row, col].Color;
				}
				tmp.Dump();
			}

			"Current Turn: {0}".With(_currentTurn == Alice ? "Alice" : "Bob").Dump();
			"Alice's Color: {0}".With(Alice.CurrentColor).Dump();
			"Bob's  Color: {0}".With(Bob.CurrentColor).Dump();
			try
			{
				tmp = MyCells[Alice].Select(e => "({0}, {1})".With(e.Row, e.Column)).StringJoin(" ");
				"Alice's MyCells ({1}): {0}".With(tmp, tmp.AsEnumerable().Where(e => e == ',').Count()).Dump();
				tmp = EdgeCells[Alice].Select(e => "({0}, {1})".With(e.Row, e.Column)).StringJoin(" ");
				"Alice's EdgeCells ({1}): {0}".With(tmp, tmp.AsEnumerable().Where(e => e == ',').Count()).Dump();
				tmp = MyCells[Bob].Select(e => "({0}, {1})".With(e.Row, e.Column)).StringJoin(" ");
				"Bob's MyCells ({1}): {0}".With(tmp, tmp.AsEnumerable().Where(e => e == ',').Count()).Dump();
				tmp = EdgeCells[Bob].Select(e => "({0}, {1})".With(e.Row, e.Column)).StringJoin(" ");
				"Bob's EdgeCells ({1}): {0}".With(tmp, tmp.AsEnumerable().Where(e => e == ',').Count()).Dump();
			}
			catch { }
			"".Dump();
#endif
		}
		#endregion

		public bool StartGame()
		{
			if (Alice == null || Bob == null) return false;
			_board = new Board(_size, _countColor);
			MyCells = new Dictionary<User, HashSet<Cell>>();
			EdgeCells = new Dictionary<User, HashSet<Cell>>();
			_currentTurn = Alice;
			Print();
			SetUser(Alice, 0, 0);
			SetUser(Bob, _size - 1, _size - 1);
			Print();
			IsRunning = true;
			return true;
		}

		public string GetStatus()
		{
			return "";
		}

		void SetUser(User user, int row, int col)
		{
			if (!IsValid(row, col)) return;

			var cells = _board.Cells;

			MyCells.Add(user, new HashSet<Cell>());
			EdgeCells.Add(user, new HashSet<Cell>());

			user.CurrentColor = cells[row, col].Color;
			AddEdgeCell(user, cells[row, col]);
			SetColor(user, user.CurrentColor, true);
		}

		void AddEdgeCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			EdgeCells[user].Add(cell);
		}

		void AddMyCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			if (!EdgeCells[user].Contains(cell)) return; // EdgeCells 에 속해있지 않던 셀은 MyCell 이 될 수 없다.
			MyCells[user].Add(cell);
			EdgeCells[user].Remove(cell);
			foreach (var edge in _board.GetEdges(cell).Where(e => !MyCells[user].Contains(e)))
				EdgeCells[user].Add(edge);
		}

		public void SetColor(User user, Color color, bool isFirst = false)
		{
			lock (_currentTurn)
			{
				#region Cutting
				if (_currentTurn != user) return; // 상대방의 입력은 쳐낸다.
				if (!isFirst && Alice.CurrentColor == color) return; // 자신, 상대방 색을 누르면 쳐낸다.
				if (!isFirst && Bob.CurrentColor == color) return;
				#endregion

				#region BFS
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
				#endregion

				#region SetMyCell
				foreach (var reserve in reserved)
					this.AddMyCell(user, reserve);

				foreach (var mycell in MyCells[user])
					mycell.Color = color;

				user.CurrentColor = color;
				#endregion

				#region Toggle Turn
				_currentTurn = _currentTurn == Alice ? Bob : Alice;
				#endregion
			}
		}

		bool IsValid(int row, int col)
		{
			return _board.IsValid(row, col);
		}
	}

	#region Class Board
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
					Cells[size - row - 1, size - col - 1].Color = (Color)Colors.GetValue(countColor - index - 1);
				}
			}
			#endregion
		}

		public IEnumerable<Cell> GetEdges(Cell cell)
		{
			return dx.Zip(dy, (dxx, dyy) => new { Row = cell.Row + dxx, Column = cell.Column + dyy })
				.Where(e => IsValid(e.Row, e.Column))
				.Select(e => Cells[e.Row, e.Column]);
		}
		
		public bool IsValid(int row, int col)
		{
			return (row >= 0 && row < _size && col >= 0 && col < _size);
		}
	}
	#endregion

	#region Class Cell
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
#endregion
}
