using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.ViewModels;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 用户页。未登录时登录，登录后按角色显示用户信息。管理员可新增、编辑、删除。
    /// </summary>
    public partial class UserView : UserControl
    {
        private UserViewModel? _model;

        /// <summary>
        /// 创建用户页。
        /// </summary>
        public UserView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        /// <summary>
        /// 双击行时打开编辑弹窗。
        /// </summary>
        private void OnUserRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow { DataContext: UserRow row } && DataContext is UserViewModel model)
                model.EditCommand.Execute(row);
        }

        /// <summary>
        /// 订上当前视图模型。
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_model is not null)
                _model.PropertyChanged -= OnModelPropertyChanged;

            _model = e.NewValue as UserViewModel;
            if (_model is not null)
            {
                _model.PropertyChanged += OnModelPropertyChanged;
                FocusLoginName();
            }
        }

        /// <summary>
        /// 页面打开后，未登录则聚焦登录名。
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusLoginName();
        }

        /// <summary>
        /// 弹窗打开时聚焦编辑登录名。回到登录区时聚焦登录名。密码由 PasswordService 绑定。
        /// </summary>
        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_model is null)
                return;

            if (e.PropertyName == nameof(UserViewModel.IsEditorOpen) && _model.IsEditorOpen)
                Dispatcher.BeginInvoke(new Action(() => EditorLoginBox.Focus()));

            if (e.PropertyName == nameof(UserViewModel.ShowLogin))
                FocusLoginName();
        }

        /// <summary>
        /// 未登录时把键盘焦点放到登录名。
        /// </summary>
        private void FocusLoginName()
        {
            if (_model is { ShowLogin: true })
                Dispatcher.BeginInvoke(new Action(() => LoginNameBox.Focus()));
        }
    }
}
