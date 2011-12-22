using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer.Collections
{
	/// <summary>
	/// Parallel quicksort algorithm.
	/// <remarks>
	/// <para/>
	/// http://stackoverflow.com/questions/1897458/parallel-sort-algorithm
	/// </remarks>
	/// </summary>
	internal static class ParallelSort
	{
		#region Public Static Methods

		/// <summary>
		/// Sequential quicksort.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr"></param>
		public static void QuicksortSequential<T>( IList<T> arr ) where T : IComparable<T>
		{
			QuicksortSequential( arr, 0, arr.Count - 1 );
		}

		/// <summary>
		/// Parallel quicksort
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr"></param>
		public static void QuicksortParallel<T>( IList<T> arr ) where T : IComparable<T>
		{
			QuicksortParallel( arr, 0, arr.Count - 1 );
		}

		#endregion

		#region Private Static Methods

		private static void QuicksortSequential<T>( IList<T> arr, int left, int right )
			where T : IComparable<T>
		{
			if ( right > left )
			{
				int pivot = Partition( arr, left, right );
				QuicksortSequential( arr, left, pivot - 1 );
				QuicksortSequential( arr, pivot + 1, right );
			}
		}

		private static void QuicksortParallel<T>( IList<T> arr, int left, int right )
			where T : IComparable<T>
		{
			const int SEQUENTIAL_THRESHOLD = 2048;
			if ( right > left )
			{
				if ( right - left < SEQUENTIAL_THRESHOLD )
				{
					QuicksortSequential( arr, left, right );
				}
				else
				{
					int pivot = Partition( arr, left, right );
					Parallel.Invoke(
						() => QuicksortParallel( arr, left, pivot - 1 ),
						() => QuicksortParallel( arr, pivot + 1, right )
												   );
				}
			}
		}

		private static void Swap<T>( IList<T> arr, int i, int j )
		{
			T tmp = arr[i];
			arr[i] = arr[j];
			arr[j] = tmp;
		}

		private static int Partition<T>( IList<T> arr, int low, int high )
			where T : IComparable<T>
		{
			// Simple partitioning implementation
			int pivotPos = (high + low) / 2;
			T pivot = arr[pivotPos];
			Swap( arr, low, pivotPos );

			int left = low;
			for ( int i = low + 1; i <= high; i++ )
			{
				if ( arr[i].CompareTo( pivot ) < 0 )
				{
					left++;
					Swap( arr, i, left );
				}
			}

			Swap( arr, low, left );
			return left;
		}

		#endregion
	}
}
