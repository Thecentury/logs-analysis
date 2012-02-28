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
		/// <param name="comparer"> </param>
		public static void QuicksortSequential<T>( IList<T> arr, IComparer<T> comparer )
		{
			QuicksortSequential( arr, 0, arr.Count - 1, comparer );
		}

		/// <summary>
		/// Parallel quicksort
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr"></param>
		/// <param name="comparer"> </param>
		public static void QuicksortParallel<T>( IList<T> arr, IComparer<T> comparer )
		{
			QuicksortParallel( arr, 0, arr.Count - 1, comparer );
		}

		#endregion

		#region Private Static Methods

		private static void QuicksortSequential<T>( IList<T> arr, int left, int right, IComparer<T> comparer )
		{
			if ( right > left )
			{
				int pivot = Partition( arr, left, right, comparer );
				QuicksortSequential( arr, left, pivot - 1, comparer );
				QuicksortSequential( arr, pivot + 1, right, comparer );
			}
		}

		private static void QuicksortParallel<T>( IList<T> arr, int left, int right, IComparer<T> comparer )
		{
			const int sequentialThreshold = 2048;
			if ( right > left )
			{
				if ( right - left < sequentialThreshold )
				{
					QuicksortSequential( arr, left, right, comparer );
				}
				else
				{
					int pivot = Partition( arr, left, right, comparer );
					Parallel.Invoke(
						() => QuicksortParallel( arr, left, pivot - 1, comparer ),
						() => QuicksortParallel( arr, pivot + 1, right, comparer ) );
				}
			}
		}

		private static void Swap<T>( IList<T> arr, int i, int j )
		{
			T tmp = arr[i];
			arr[i] = arr[j];
			arr[j] = tmp;
		}

		private static int Partition<T>( IList<T> arr, int low, int high, IComparer<T> comparer )
		{
			// Simple partitioning implementation
			int pivotPos = (high + low) / 2;
			T pivot = arr[pivotPos];
			Swap( arr, low, pivotPos );

			int left = low;
			for ( int i = low + 1; i <= high; i++ )
			{
				if ( comparer.Compare( arr[i], pivot ) < 0 )
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
