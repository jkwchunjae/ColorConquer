using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace ColorConquerServer
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

		// 내가 가지고 있는 셀들
		Dictionary<User, HashSet<Cell>> MyCells;
		// 나와 인접해 있는 셀들
		Dictionary<User, HashSet<Cell>> EdgeCells;

		Dictionary<User, List<Color>> SelectedColor;
		Dictionary<Color, int> RemainColorCount;

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

		public bool IsFinished
		{
			get
			{
				if (RemainColorCount.Count() == 0) return true; // 남은 색이 없으면 당연히 끝!
				if (RemainColorCount.Count() >= 3) return false; // 남은 색이 3개 이상이면 절대 끝날 수 없다.
				// 남은 색의 개수가 1개 or 2개일 경우
				// Alice, Bob의 색 + 남은 색의 개수가 여전히 2개이면 게임이 끝난것으로 간주한다.
#if DEBUG
				"RemainColorCount: ".Dump();
				foreach (var color in RemainColorCount)
					"{0}: {1}".With(color.Key.ToString(), color.Value).Dump();
				"Alice's Color: {0}".With(MyCells[Alice].First().Color.ToString()).Dump();
				"Bob's Color: {0}".With(MyCells[Bob].First().Color.ToString()).Dump();
#endif
				var set = new HashSet<Color>(RemainColorCount.Select(e => e.Key));
				set.Add(MyCells[Alice].First().Color);
				set.Add(MyCells[Bob].First().Color);
				return set.Count() == 2;
			}
		}

		public User Winner
		{ get { return MyCells[Alice].Count > MyCells[Bob].Count ? Alice : Bob; } }

		public User Loser
		{ get { return MyCells[Alice].Count < MyCells[Bob].Count ? Alice : Bob; } }

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

		public int GetUserScore(User user)
		{
			if (!MyCells.ContainsKey(user)) return 0;
			// 경기가 끝났다고 판단되는 경우 내 색은 아니지만 상대방 진영에 있는 내 색도 계산한다.
			var remainCount = IsFinished && RemainColorCount.ContainsKey(MyCells[user].First().Color) ? RemainColorCount[MyCells[user].First().Color] : 0;
			return MyCells[user].Count + remainCount;
		}

		public bool StartGame()
		{
			if (Alice == null || Bob == null) return false;
			_board = new Board(_size, _countColor);
			MyCells = new Dictionary<User, HashSet<Cell>>();
			EdgeCells = new Dictionary<User, HashSet<Cell>>();
			SelectedColor = new Dictionary<User, List<Color>>();
			SelectedColor.Add(Alice, new List<Color>());
			SelectedColor.Add(Bob, new List<Color>());
			#region 남아있는 색 개수 세기
			RemainColorCount = new Dictionary<Color, int>();
			for (var row = 0; row < _size; row++)
			{
				for (var col = 0; col < _size; col++)
				{
					if (!RemainColorCount.ContainsKey(Cells[row, col].Color))
						RemainColorCount.Add(Cells[row, col].Color, 0);
					RemainColorCount[Cells[row, col].Color]++;
				}
			}
			#endregion
			_currentTurn = Alice;
			//Print();
			SetUser(Alice, 0, 0);
			SetUser(Bob, _size - 1, _size - 1);
			//Print();
			IsRunning = true;
			return true;
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

		#region Set Color
		void AddEdgeCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			EdgeCells[user].Add(cell);
		}

		void AddMyCell(User user, Cell cell)
		{
			if (MyCells[user].Contains(cell)) return;
			if (!EdgeCells[user].Contains(cell)) return; // EdgeCells 에 속해있지 않던 셀은 MyCell 이 될 수 없다.
			#region 남은 색에 대한 처리
			RemainColorCount[cell.Color]--;
			if (RemainColorCount[cell.Color] == 0)
				RemainColorCount.Remove(cell.Color);
			#endregion
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
				if (_currentTurn != user) throw new SetColorException("차례가 아닙니다."); // 상대방의 입력은 쳐낸다.
				if (!isFirst && Alice.CurrentColor == color) throw new SetColorException("본인, 상대방의 색은 선택할 수 없습니다."); // 자신, 상대방 색을 누르면 쳐낸다.
				if (!isFirst && Bob.CurrentColor == color) throw new SetColorException("본인, 상대방의 색은 선택할 수 없습니다.");
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
				SelectedColor[user].Add(color);
				#endregion

				#region Toggle Turn
				_currentTurn = _currentTurn == Alice ? Bob : Alice;
				#endregion
			}
		}
		#endregion

		bool IsValid(int row, int col)
		{
			return _board.IsValid(row, col);
		}
	}

	public class SetColorException : Exception
	{
		public SetColorException(string message) : base(message)
		{ }
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
			#region 초기화
			Cells = new Cell[size, size];
			for (var row = 0; row < size; row++)
			{
				for (var col = 0; col < size; col++)
				{
					Cells[row, col] = new Cell(row, col);
				}
			}
			#endregion

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
