using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;


namespace ArduinoMonitoringWPFApp.ViewModels
{
    public class InfoViewModel : Conductor<object>, IHaveDisplayName
    {
        #region 속성영역
        string productName;
        public string ProductName
        {
            get => productName;
            set
            {
                productName = value;
            }
        }

        string version;
        public string Version
        {
            get => version;
            set
            {
                version = value;
            }
        }

        string copyright;
        public string Copyright
        {
            get => copyright;
            set
            {
                copyright = value;
            }
        }

        string companyName;
        public string CompanyName
        {
            get => companyName;
            set
            {
                companyName = value;
            }
        }
        
        string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
            }
        }
        #endregion

        public InfoViewModel()
        {
            this.ProductName = AssemblyProduct;
            this.Version = String.Format("버전 {0}", AssemblyVersion);
            this.Copyright = AssemblyCopyright;
            this.CompanyName = AssemblyCompany;
            this.Description = AssemblyDescription;
        }

        #region 어셈블리 특성 접근자
        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
    }
}
