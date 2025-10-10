using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.UI.WPF.Controls
{
    /// <summary>
    /// Toast视图模型接口
    /// </summary>
    public interface IToastViewModel
    {
        WindowToastManager? ToastManager { get; set; }
    }
}
