using System.Collections;
using System.Threading.Tasks;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public abstract class AsyncOverview : BindingObject, IOverviewViewModel
	{
		protected abstract IEnumerable UpdateOverviews();

		private Task<IEnumerable> _populationTask;
		private bool _overviewsPopulated;
		private IEnumerable _items = new object[0];

		private void SetOverviews( IEnumerable items )
		{
			_items = items;
			RaisePropertyChanged( "Items" );
		}

		protected void Update()
		{
			UpdateItems();
		}

		public IEnumerable Items
		{
			get
			{
				if ( !_overviewsPopulated && _populationTask == null )
				{
					UpdateItems();
				}
				return _items;
			}
		}

		public string Icon
		{
			get
			{
				string fullPath = PackUriHelper.MakePackUri( "/Resources/" + GetIcon() );
				return fullPath;
			}
		}

		protected abstract string GetIcon();

		public abstract string Tooltip { get; }

		private void UpdateItems()
		{
			_populationTask = new Task<IEnumerable>( UpdateOverviews );
			_populationTask.ContinueWith( t =>
											{
												_overviewsPopulated = true;
												_populationTask = null;

												SetOverviews( t.Result );
											} );
			_populationTask.Start();
		}
	}
}