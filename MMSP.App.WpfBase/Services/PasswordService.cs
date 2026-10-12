using System.Windows;
using System.Windows.Controls;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 用附加属性把 PasswordBox 和视图模型上的字符串做双向绑定。密码不写日志。
    /// </summary>
    public static class PasswordService
    {
        /// <summary>
        /// 与视图模型同步的密码。
        /// </summary>
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordService),
                new FrameworkPropertyMetadata(
                    "",
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundPasswordChanged));

        /// <summary>
        /// 是否启用密码绑定。
        /// </summary>
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordService),
                new PropertyMetadata(false, OnBindPasswordChanged));

        /// <summary>
        /// 正在把密码写回密码框时为 true，用来挡住 PasswordChanged 回写。
        /// </summary>
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(PasswordService),
                new PropertyMetadata(false));

        /// <summary>
        /// 读取绑定的密码。
        /// </summary>
        public static string GetBoundPassword(DependencyObject element)
        {
            return (string)element.GetValue(BoundPasswordProperty) ?? "";
        }

        /// <summary>
        /// 写入绑定的密码。
        /// </summary>
        public static void SetBoundPassword(DependencyObject element, string value)
        {
            element.SetValue(BoundPasswordProperty, value ?? "");
        }

        /// <summary>
        /// 读取是否启用密码绑定。
        /// </summary>
        public static bool GetBindPassword(DependencyObject element)
        {
            return (bool)element.GetValue(BindPasswordProperty);
        }

        /// <summary>
        /// 设置是否启用密码绑定。
        /// </summary>
        public static void SetBindPassword(DependencyObject element, bool value)
        {
            element.SetValue(BindPasswordProperty, value);
        }

        /// <summary>
        /// 读取是否正在写回密码框。
        /// </summary>
        private static bool GetIsUpdating(DependencyObject element)
        {
            return (bool)element.GetValue(IsUpdatingProperty);
        }

        /// <summary>
        /// 设置写回标志。
        /// </summary>
        private static void SetIsUpdating(DependencyObject element, bool value)
        {
            element.SetValue(IsUpdatingProperty, value);
        }

        /// <summary>
        /// 视图模型密码变化时写进密码框。
        /// </summary>
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox passwordBox || !GetBindPassword(passwordBox) || GetIsUpdating(passwordBox))
                return;

            ApplyBoundPassword(passwordBox);
        }

        /// <summary>
        /// 打开或关闭密码框到绑定属性的同步。
        /// </summary>
        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox passwordBox)
                return;

            var wasBound = e.OldValue is true;
            var bind = e.NewValue is true;
            if (wasBound == bind)
                return;

            if (wasBound)
            {
                passwordBox.PasswordChanged -= OnPasswordChanged;
                passwordBox.IsVisibleChanged -= OnVisibleChanged;
            }

            if (bind)
            {
                passwordBox.PasswordChanged += OnPasswordChanged;
                passwordBox.IsVisibleChanged += OnVisibleChanged;
                ApplyBoundPassword(passwordBox);
            }
        }

        /// <summary>
        /// 弹窗再次显示时，按绑定值重写密码框。空密码也要写，避免上次的圆点留着。
        /// </summary>
        private static void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not true || sender is not PasswordBox passwordBox || !GetBindPassword(passwordBox))
                return;

            ApplyBoundPassword(passwordBox);
        }

        /// <summary>
        /// 密码框输入变化时写回绑定属性。
        /// </summary>
        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordBox || GetIsUpdating(passwordBox))
                return;

            SetIsUpdating(passwordBox, true);
            SetBoundPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }

        /// <summary>
        /// 把绑定值写入密码框。空密码且框里看起来已空时，仍先写一个不同的值再清空，强迫圆点重绘。
        /// </summary>
        private static void ApplyBoundPassword(PasswordBox passwordBox)
        {
            if (GetIsUpdating(passwordBox))
                return;

            var password = GetBoundPassword(passwordBox);
            var same = passwordBox.Password == password;
            if (same && password.Length > 0)
                return;

            SetIsUpdating(passwordBox, true);
            try
            {
                if (same)
                    passwordBox.Password = " ";

                passwordBox.Password = password;
            }
            finally
            {
                SetIsUpdating(passwordBox, false);
            }
        }
    }
}
