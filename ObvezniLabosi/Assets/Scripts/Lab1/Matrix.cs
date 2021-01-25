using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizaProjektiranjeRacunalom
{
	public class Matrix
	{
		const double ZeroTreshold = 1e-10;

		private IDictionary<(int i, int j), double> _values = new Dictionary<(int, int), double>();

		private int[] _pivotIndex;
		private int _pivotIndexChanges = 0;

		public int Width { get; private set; }
		public int Height { get; private set; }

		#region Constructors

		public Matrix(int width, int height)
		{
			Width = width;
			Height = height;
			_pivotIndex = Enumerable.Range(0, Width).ToArray();
		}

		public Matrix(string inputFile)
		{
			var values = File.ReadAllLines(inputFile)
				.Select(s => s.Split(' ').Select(double.Parse).ToArray()).ToArray();
			Height = values.Length;
			Width = values[0].Length;
			_pivotIndex = Enumerable.Range(0, Width).ToArray();
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					this[i, j] = values[j][i];
				}
			}
		}

		#endregion

		#region Operators

		public double this[(int i, int j) c]
		{
			get => this[c.i, c.j];
			set => this[c.i, c.j] = value;
		}

		public double this[int i, int j]
		{
			get
			{
				if (!AssertBounds(i, j)) throw new IndexOutOfRangeException();
				return _values.TryGetValue((_pivotIndex[i], j), out var v) ? v : 0;
			}
			set
			{
				if (!AssertBounds(i, j)) throw new IndexOutOfRangeException();
				_values[(_pivotIndex[i], j)] = value;
			}
		}

		public static Matrix operator ~(Matrix a)
		{
			var m = new Matrix(a.Height, a.Width);
			foreach (var entity in a._values)
			{
				m[entity.Key.Item2, entity.Key.Item1] = entity.Value;
			}
			return m;
		}

		public static Matrix operator -(Matrix a)
		{
			return a * -1;
		}

		public static Matrix operator -(Matrix a, Matrix b)
		{
			return a + -b;
		}

		public static Matrix operator *(Matrix a, Matrix b)
		{
			if (a.Width != b.Height) throw new IndexOutOfRangeException("Unable to multiply matrices due to mismatched dimensions");

			var m = new Matrix(b.Width, a.Height);
			
			for (int i = 0; i < a.Height; i++)
			{
				for (int j = 0; j < b.Width; j++)
				{
					for (int k = 0; k < a.Width; k++)  //B height
					{
						m[j, i] += a[k, i] * b[j, k];
					}
				}
			}

			return m;
		}

		public static Matrix operator +(Matrix a, Matrix b)
		{
			if (!a.AssertBounds(b)) { throw new IndexOutOfRangeException("Matrix dimensions must match"); }
			var m = a.Clone();
			foreach (var entity in b._values)
			{
				m[entity.Key] += b[entity.Key];
			}

			return m;
		}

		public static Matrix operator *(Matrix a, double v)
		{
			var m = a.Clone();
			foreach (var entity in a._values)
			{
				m[entity.Key] *= v;
			}

			return m;
		}

		public static bool operator ==(Matrix left, Matrix right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Matrix left, Matrix right)
		{
			return !Equals(left, right);
		}

		#endregion

		#region Overrides

		protected bool Equals(Matrix other)
		{

			if (other.Height != Height) return false;
			if (other.Width != Width) return false;
			if (other._values.Count != _values.Count) return false;
			for (int i = 0; i < Width; i++)
			{
				if (_pivotIndex[i] != other._pivotIndex[i]) return false;
			}
			return _values.All(entity => Math.Abs(other[entity.Key] - entity.Value) < ZeroTreshold);

		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Matrix)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (_values != null ? _values.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_pivotIndex != null ? _pivotIndex.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Width;
				hashCode = (hashCode * 397) ^ Height;
				return hashCode;
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					sb.AppendFormat("{0:F2} \t", this[i, j]);
				}
				sb.Append(Environment.NewLine);
			}

			return sb.ToString();
		}

		#endregion

		public Matrix ForwardSubstitution(Matrix b)
		{
			if (b.Width == 1) b = ~b;
			if (b.Height != 1 || b.Width != this.Width) throw new InvalidDataException("B must be a vector with a dimension matching this matrix");
			var y = new Matrix(b.Width, 1);
			
			for (int i = 0; i < b.Width; i++)
			{
				y[i, 0] = b[_pivotIndex[i], 0];
				for (int j = 0; j < i; j++)
				{
					y[i, 0] -= y[j, 0] * (i == j ? 1 : this[i, j]);
				}
			}
			return y;
		}

		public Matrix BackSubstitution(Matrix y)
		{
			if (y.Width == 1) y = ~y;
			if (y.Height != 1 || y.Width != this.Width) throw new InvalidDataException("Y must be a vector with a dimension matching this matrix");
			var x = new Matrix(y.Width, 1);

			for (int i = y.Width -1 ; i >= 0 ; i--)
			{
				x[i, 0] = y[i, 0];
				for (int j = i+1; j < y.Width; j++)
				{
					x[i, 0] -= this[i, j] * x[j, 0];
				}
				x[i, 0] /= this[i, i];
			}

			return x;
		}

		public void LowerUpper(bool doPivot = false)
		{
			var backup = this.Clone();

			for (int i = 0; i < Width - 1; i++)
			{
				int pivot = i;
				if (doPivot)
				{
					for (int j = i + 1; j < Width; j++)
					{
						if (Math.Abs(this[j, i]) > Math.Abs(this[pivot, i]))
						{
							pivot = j;
						}
					}

					if (pivot != i)
					{
						_pivotIndexChanges++;
						_pivotIndex[i] ^= _pivotIndex[pivot];
						_pivotIndex[pivot] ^= _pivotIndex[i];
						_pivotIndex[i] ^= _pivotIndex[pivot];
					}
				}
				for (int j = i + 1; j < Width; j++)
				{
					if (this[i, i] == 0)
					{
						_values = backup._values;
						throw new InvalidOperationException($"Pivot value [i = {i}, pivot[i] = {_pivotIndex[i]}] is 0");
					}
					this[j, i] /= this[i, i];
					for (int k = i + 1; k < Width; k++)
					{
						this[j, k] -= this[j, i] * this[i, k];
					}
				}

			}
		}

		public Matrix Inverse()
		{
			if (Math.Abs(Determinant()) < ZeroTreshold) throw new InvalidOperationException("Matrix determinant is zero");

			var backup = this.Clone();

			LowerUpper(true);

			var m = new Matrix(Width, Height);
			var b = Identity(3);
			b._pivotIndex = _pivotIndex;

			for (int i = 0; i < Width; i++)
			{
				var y = ForwardSubstitution(b.GetLine(i));
				var x = BackSubstitution(y);
				for (int j = 0; j < Height; j++)
				{
					m[j, i] = x[j, 0];
				}
			}

			Restore(backup);
			return m;
		}

		public double Determinant()
		{
			var backup = Clone();
			LowerUpper(true);
			var det = Math.Pow(-1, _pivotIndexChanges) * Enumerable.Range(0, Width).Select(i => this[i, i]).Aggregate((a, b) => a * b);
			Restore(backup);
			return det;
		}

		private Matrix GetLine(int i)
		{
			var m = new Matrix(Width, 1);
			for (int j = 0; j < Width; j++)
			{
				m[j, 0] = this[j, i];
			}
			return m;
		}
		
		private Matrix Clone()
		{
			var m = new Matrix(Width, Height);
			foreach (var entity in _values)
			{
				m[entity.Key] = entity.Value;
			}
			m._pivotIndex = (int[])_pivotIndex.Clone();
			m._pivotIndexChanges = _pivotIndexChanges;
			return m;
		}

		private void Restore(Matrix backup)
		{
			backup = backup.Clone();
			_values = backup._values;
			_pivotIndex = backup._pivotIndex;
			_pivotIndexChanges = backup._pivotIndexChanges;
		}

		private bool AssertBounds(Matrix other)
		{
			if (Width != other.Width) return false;
			return Height == other.Height;
		}

		private bool AssertBounds(int i, int j)
		{
			if (i < 0) return false;
			if (i >= Width) return false;
			if (j < 0) return false;
			return j < Height;
		}

		public static Matrix Identity(int i)
		{
			var m = new Matrix(i, i);
			for (int j = 0; j < i; j++)
			{
				m[j, j] = 1;
			}

			return m;
		}

		static void Main(string[] args)
		{
			Console.WriteLine(ZeroTreshold);
			Task2();
			Task3();
			Task4();
			Task5();
			Task6();
			Task7();
			Task8();
			Task9();
			Task10();
			Console.ReadLine();
		}

		static void Task2()
		{
			Console.WriteLine("Task 2");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 3,
				[0, 1] = 9,
				[0, 2] = 6,
				[1, 0] = 4,
				[1, 1] = 12,
				[1, 2] = 12,
				[2, 0] = 1,
				[2, 1] = -1,
				[2, 2] = 1,
			};

			try
			{
				m.LowerUpper();
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine($"Unable to separate using LU: {e.Message}");
			}
			m.LowerUpper(true);
			var b = new Matrix(3, 1)
			{
				[0, 0] = 12,
				[1, 0] = 12,
				[2, 0] = 1
			};
			var y = m.ForwardSubstitution(b);
			Console.WriteLine($"x = {Environment.NewLine}{m.BackSubstitution(y)}");
		}

		static void Task3()
		{
			Console.WriteLine("Task 3");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 1,
				[0, 1] = 2,
				[0, 2] = 3,
				[1, 0] = 4,
				[1, 1] = 5,
				[1, 2] = 6,
				[2, 0] = 7,
				[2, 1] = 8,
				[2, 2] = 9,
			};

			var b = new Matrix(3, 1)
			{
				[0, 0] = 1,
				[1, 0] = 5,
				[2, 0] = 3
			};

			try
			{
				var c = m.Clone();
				c.LowerUpper();
				Console.WriteLine(c.BackSubstitution(c.ForwardSubstitution(b)));
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine($"Unable to separate using LU: {e.Message}");
			}
			
			m.LowerUpper(true);
			var y = m.ForwardSubstitution(b);
			Console.WriteLine(m.BackSubstitution(y));

		}

		static void Task4()
		{

			Console.WriteLine("Task 4");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 0.000001,
				[0, 1] = 3000000,
				[0, 2] = 2000000,
				[1, 0] = 1000000,
				[1, 1] = 2000000,
				[1, 2] = 3000000,
				[2, 0] = 2000000,
				[2, 1] = 1000000,
				[2, 2] = 2000000,
			};

			var b = new Matrix(3, 1)
			{
				[0, 0] = 12000000.000001,
				[1, 0] = 14000000,
				[2, 0] = 10000000
			};

			var c = m.Clone();
			c.LowerUpper();
			var y = c.ForwardSubstitution(b);
			Console.WriteLine($"LU = {Environment.NewLine}{c}; X = {Environment.NewLine}{c.BackSubstitution(y)}");
			m.LowerUpper(true);
			y = m.ForwardSubstitution(b);
			Console.WriteLine($"LUP = {Environment.NewLine}{m}; X = {Environment.NewLine}{m.BackSubstitution(y)}");
		}

		static void Task5()
		{
			Console.WriteLine("Task 5");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 0,
				[0, 1] = 1,
				[0, 2] = 2,
				[1, 0] = 2,
				[1, 1] = 0,
				[1, 2] = 3,
				[2, 0] = 3,
				[2, 1] = 5,
				[2, 2] = 1,
			};
			m.LowerUpper(true);
			var b = new Matrix(3, 1)
			{
				[0, 0] = 6,
				[1, 0] = 9,
				[2, 0] = 3
			};
			var y = m.ForwardSubstitution(b);
			Console.WriteLine($"x = {Environment.NewLine}{m.BackSubstitution(y)}");
		}

		static void Task6()
		{
			Console.WriteLine("Task 6");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 4000000000,
				[0, 1] = 1000000000,
				[0, 2] = 3000000000,
				[1, 0] = 4,
				[1, 1] = 2,
				[1, 2] = 7,
				[2, 0] = 0.0000000003,
				[2, 1] = 0.0000000005,
				[2, 2] = 0.0000000002,
			};

			var b = new Matrix(3, 1)
			{
				[0, 0] = 9000000000,
				[1, 0] = 15,
				[2, 0] = 0.0000000015
			};

			m.LowerUpper(true);
			var y = m.ForwardSubstitution(b);
			Console.WriteLine(m.BackSubstitution(y));
		}

		static void Task7()
		{
			Console.WriteLine("Task 7");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 1,
				[0, 1] = 2,
				[0, 2] = 3,
				[1, 0] = 4,
				[1, 1] = 5,
				[1, 2] = 6,
				[2, 0] = 7,
				[2, 1] = 8,
				[2, 2] = 9
			};

			try
			{
				Console.WriteLine(m.Inverse());
			}
			catch (InvalidOperationException ioe)
			{
				Console.WriteLine($"Invalid Operation Exception: {ioe.Message}");
			}
			
		}
		static void Task8()
		{
			Console.WriteLine("Task 8");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 4,
				[0, 1] = -5,
				[0, 2] = -2,
				[1, 0] = 5,
				[1, 1] = -6,
				[1, 2] = -2,
				[2, 0] = -8,
				[2, 1] = 9,
				[2, 2] = 3
			};
			Console.WriteLine(m.Inverse());
		}
		static void Task9()
		{

			Console.WriteLine("Task 9");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 4,
				[0, 1] = -5,
				[0, 2] = -2,
				[1, 0] = 5,
				[1, 1] = -6,
				[1, 2] = -2,
				[2, 0] = -8,
				[2, 1] = 9,
				[2, 2] = 3
			};
			Console.WriteLine(m.Determinant());
		}

		static void Task10()
		{
			Console.WriteLine("Task 10");
			var m = new Matrix(3, 3)
			{
				[0, 0] = 3,
				[0, 1] = 9,
				[0, 2] = 6,
				[1, 0] = 4,
				[1, 1] = 12,
				[1, 2] = 12,
				[2, 0] = 1,
				[2, 1] = -1,
				[2, 2] = 1
			};
			Console.WriteLine(m.Determinant());
		}

		static void Debug(string[] args)
		{
			#region Task_LU
			var m11 = new Matrix(4, 4)
			{
				[0, 0] = 4,
				[0, 1] = 3,
				[0, 2] = 2,
				[0, 3] = 1,
				[1, 0] = 4,
				[1, 1] = 6,
				[1, 2] = 1,
				[1, 3] = -1,
				[2, 0] = -8,
				[2, 1] = 3,
				[2, 2] = -5,
				[2, 3] = -6,
				[3, 0] = 12,
				[3, 1] = 12,
				[3, 2] = 7,
				[3, 3] = 4
			};
			m11.LowerUpper();
			Console.WriteLine(m11);
			#endregion

			#region Task_Substitution
			var m12 = new Matrix(3, 3)
			{
				[0, 0] = 6,
				[0, 1] = 2,
				[0, 2] = 10,
				[1, 0] = 2,
				[1, 1] = 3,
				[1, 2] = 0,
				[2, 0] = 0,
				[2, 1] = 4,
				[2, 2] = 2
			};
			m12.LowerUpper();
			Console.WriteLine(m12);
			var b12 = new Matrix(3, 1)
			{
				[0, 0] = 2,
				[1, 0] = 3,
				[2, 0] = 4
			};
			var y12 = m12.ForwardSubstitution(b12);
			Console.WriteLine(y12);
			Console.WriteLine(m12.BackSubstitution(y12));
			#endregion

			#region Task_LUP

			var m13 = new Matrix(3, 3)
			{
				[0, 0] = 1,
				[0, 1] = 1,
				[0, 2] = 1,
				[1, 0] = 1,
				[1, 1] = 1,
				[1, 2] = 3,
				[2, 0] = 1,
				[2, 1] = 3,
				[2, 2] = 3
			};
			try
			{
				m13.LowerUpper();
				Console.WriteLine(m13);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			m13.LowerUpper(true);
			Console.WriteLine(m13);

			var b13 = new Matrix(3, 1)
			{
				[0, 0] = 1.0 / 2,
				[1, 0] = 1,
				[2, 0] = 2
			};
			var y13 = m13.ForwardSubstitution(b13);
			Console.WriteLine(y13);
			Console.WriteLine(m13.BackSubstitution(y13));

			#endregion

			#region Task_Inverse

			var m14 = new Matrix(3, 3)
			{
				[0, 0] = 3,
				[0, 1] = 0,
				[0, 2] = 2,
				[1, 0] = 2,
				[1, 1] = 0,
				[1, 2] = -2,
				[2, 0] = 0,
				[2, 1] = 1,
				[2, 2] = 1
				//	[0, 0] = 1,
				//[0, 1] = 2,
				//[0, 2] = 3,
				//[1, 0] = 4,
				//[1, 1] = 5,
				//[1, 2] = 6,
				//[2, 0] = 7,
				//[2, 1] = 8,
				//[2, 2] = 9
			};
			Console.WriteLine(m14.Inverse());
			Console.WriteLine(m14.Determinant());
			#endregion

			Console.ReadLine();

		}


	}
}
