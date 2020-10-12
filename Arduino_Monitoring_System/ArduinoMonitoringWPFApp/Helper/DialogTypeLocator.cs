using MvvmDialogs.DialogTypeLocators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMonitoringWPFApp.Helper
{
    public class DialogTypeLocator : IDialogTypeLocator
    {
        /// <summary>
        ///  특정 뷰모델에 Dialog 타입을 위치시키는 메서드
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Type Locate(INotifyPropertyChanged viewModel)
        {
            var viewModelType = viewModel.GetType();

            string dialogFullName = viewModelType.FullName;
            dialogFullName = dialogFullName.Substring(0, dialogFullName.Length - "Model".Length);

            return viewModelType.Assembly.GetType(dialogFullName);
        }
    }
}
