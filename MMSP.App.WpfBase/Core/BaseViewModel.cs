using System;
using System.Collections.Generic;
using System.Text;

namespace MMSP.App.WpfBase.Core
{  /// <summary>
   /// 导航基类
   /// </summary>
    // IConfirmNavigationRequest接口用于确认导航请求
    // INavigationAware接口用于导航接收参数
    // 以上两个接口，继承同一个父接口 IRegionAware。
    // IConfirmNavigationRequest接口功能比INavigationAware强。IConfirmNavigationRequest接口已经包含INavigationAware接口中所有功能，又添加ConfirmNavigationRequest()

    public class BaseViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// 服务
        /// </summary>
        //protected readonly IServiceManaged Service;

        /// <summary>
        /// 事件聚合器
        /// </summary>

        protected readonly IEventAggregator EventAggregatorBase;

        /// <summary>
        /// 区域管理器
        /// </summary>
        protected readonly IRegionManager RegionManager;

        /// <summary>
        /// 容器管理
        /// </summary>
        protected readonly IContainerExtension ContainerExt;

        public BaseViewModel(IRegionManager region, IContainerExtension container, IEventAggregator eventAggregator)
        {
            RegionManager = region;
            ContainerExt = container;
            EventAggregatorBase = eventAggregator;

            //Service = container.Resolve<ServiceManaged>();
        }

        /// <summary>
        /// 导航被执行触发
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            return;
        }

        /// <summary>
        /// 是否重用导航对象
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 导航切换时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            return;
        }
    }
}
