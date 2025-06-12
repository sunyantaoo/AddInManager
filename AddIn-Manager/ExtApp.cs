using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[assembly: ExtensionApplication(typeof(AddIn_Manager.ExtApp))]
namespace AddIn_Manager
{
    internal class ExtApp : IExtensionApplication
    {
        public static IList<string> WorkDir
        {
            get;
        }

        static ExtApp()
        {
            WorkDir = new List<string> { System.IO.Path.GetDirectoryName(typeof(ExtApp).Assembly.Location) };

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var assemblyName = new AssemblyName(e.Name);
                foreach (var dir in WorkDir)
                {
                    var file = $"{System.IO.Path.Combine(dir, assemblyName.Name)}.dll";
                    if (System.IO.File.Exists(file))
                    {
                        return Assembly.Load(System.IO.File.ReadAllBytes(file));
                    }
                }
                return e.RequestingAssembly;
            };
        }


        public void Initialize()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.Idle += CADApplication_Idle;  
        }

        private void CADApplication_Idle(object sender, EventArgs e)
        {
            RibbonTab manageTab = ComponentManager.Ribbon.FindTab("ACAD.ID_TabManage");
            if (manageTab == null || manageTab.Panels.Count <= 0) return;

            var panelSource = new RibbonPanelSource()
            {
                Id = "AddIn_Manager.RibbonPanel",
                Name = "AddIn_Manager.RibbonPanel",
                Title = "AddIn Manager",
            };
            var button = new RibbonButton()
            {
                Id = "AddIn_Manager.ShowView",
                Name = "AddIn_Manager.ShowView",
                Text = "AddIn Manager",
                Image = BitmapToImageSource(Properties.Resources.Amitjakhu_Drip_Gear_16),
                LargeImage = BitmapToImageSource(Properties.Resources.Amitjakhu_Drip_Gear_32),
                CommandHandler = new ShowAddInManagerCommand(),
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical
            };
            panelSource.Items.Add(button);

            var panel = new RibbonPanel()
            {
                Source = panelSource,
            };

            var tab = ComponentManager.Ribbon.FindTab("ACAD.RBN_00012112");
            if (tab == null)
            {
                tab = new RibbonTab()
                {
                    Id = "AddIn_Manager.TabId",
                    Name = "AddIn Manager",
                    Title = "AddIn Manager"
                };
                tab.Panels.Add(panel);
                tab.IsVisible = true;

                ComponentManager.Ribbon.Tabs.Add(tab);
            }
            else
            {
                tab.Panels.Add(panel);
                tab.IsVisible = true;
            }
            Autodesk.AutoCAD.ApplicationServices.Application.Idle -= CADApplication_Idle;
        }

        public void Terminate()
        {

        }

        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }

    public class ShowAddInManagerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        private static Window _instance;

        public void Execute(object parameter)
        {
            if (_instance == null)
            {
                _instance = new MainView();
                _instance.Closed += (sender, e) =>
                {
                    _instance = null;
                };
            }
            _instance.Show();
            if (!_instance.IsActive) { _instance.Activate(); }
        }
    }
}