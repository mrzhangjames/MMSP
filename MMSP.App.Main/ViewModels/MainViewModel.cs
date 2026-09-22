using MMSP.App.WpfBase.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMSP.App.Main.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(
            IRegionManager region,
            IContainerExtension container,
            IEventAggregator eventAggregator
        )
            : base(region, container, eventAggregator) { }
    }
}
