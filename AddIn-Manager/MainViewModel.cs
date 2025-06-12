using Autodesk.AutoCAD.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace AddIn_Manager
{
    public class AssemblyLoader : MarshalByRefObject
    {

        private Assembly _assembly;

        public void Load(string assemblyPath)
        {
            _assembly = Assembly.Load(assemblyPath);
        }

        public IEnumerable<MethodInfo> GetMethods(string typeName, Expression<Func<MethodInfo, bool>> filter)
        {
            if (_assembly != null && filter != null)
            {
                var methods = _assembly.GetType(typeName).GetMethods();
                return methods.Where(filter.Compile()).ToList();
            }
            return default;
        }

        public void Execte(string typeName,string methodName, object[] args)
        {
            if (_assembly != null )
            {
                var type = _assembly.GetType(typeName);
                var method = type.GetMethod(methodName);
                if (method.IsStatic)
                {
                    method.Invoke(null, args);
                }
                else
                {
                    var instance = Activator.CreateInstance(type);
                    method.Invoke(instance, args);
                }
            }
        }
    }

    internal class MainViewModel : NotificationObject
    {

        private System.Windows.Window _window;
        private const string _configureFileName = "AddInManager.json";

        public MainViewModel(System.Windows.Window hostedWindow)
        {
            _window = hostedWindow;

            LoadConfigCommand = new DelegateCommand(LoadConfigMethod);
            SaveConfigCommand = new DelegateCommand(SaveConfigMethod);

            LoadAssemblyCommand = new DelegateCommand(LoadAssemblyMethod);
            ReloadAssemblyCommand = new DelegateCommand<CmdAssembly>(ReloadAssemblyMethhod);

            RemoveAssemblyCommand = new DelegateCommand<object>(RemoveAssemblyMethod);
            SelectItemChangedCommand = new DelegateCommand(() => { ExecuteMethodCommand.RaiseCanExecuteChanged(); });
            ExecuteMethodCommand = new DelegateCommand<object>(ExecuteMethodMethod, CanMethodExecute);
        }

        public DelegateCommand SelectItemChangedCommand { get; }

        private ObservableCollection<CmdAssembly> assemblies = new ObservableCollection<CmdAssembly>();
        public ObservableCollection<CmdAssembly> Assemblies
        {
            get { return assemblies; }
            set
            {
                assemblies = value;
                RaisePropertyChanged(nameof(Assemblies));
            }
        }


        public DelegateCommand LoadConfigCommand { get; }
        private void LoadConfigMethod()
        {
            var filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), _configureFileName);
            if (System.IO.File.Exists(filePath))
            {
                var text = System.IO.File.ReadAllText(filePath);

                var result = JsonConvert.DeserializeObject<List<CmdAssembly>>(text);
                foreach (var item in result)
                {
                    foreach (var cmdMethod in item.CommandMethods)
                    {
                        cmdMethod.CmdAssembly = item;
                    }
                }
                Assemblies = new ObservableCollection<CmdAssembly>(result);
            }
        }

        public DelegateCommand SaveConfigCommand { get; }
        private void SaveConfigMethod()
        {
            var filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), _configureFileName);

            var result = JsonConvert.SerializeObject(this.Assemblies,new JsonSerializerSettings() {  ReferenceLoopHandling=ReferenceLoopHandling.Ignore});
            System.IO.File.WriteAllText(filePath, result);
        }

        public DelegateCommand LoadAssemblyCommand { get; }
        private void LoadAssemblyMethod()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "dll|*.dll",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                var assembly=new CmdAssembly(dialog.FileName);
                
                var oldAssembly = Assemblies.FirstOrDefault(x => x.FilePath == dialog.FileName);
                if (oldAssembly != null)
                {
                    var index = Assemblies.IndexOf(oldAssembly);
                    Assemblies[index] = assembly;
                }
                else
                {
                    Assemblies.Add(assembly);
                }
            }
        }

        public DelegateCommand<CmdAssembly> ReloadAssemblyCommand { get; }
        private void ReloadAssemblyMethhod(CmdAssembly cmdAssembly)
        {
            if (cmdAssembly != null)
            {
                var index = Assemblies.IndexOf(cmdAssembly);
                var newAssembly = new CmdAssembly(cmdAssembly.FilePath);
                Assemblies[index] = newAssembly;
            }
        }

        public DelegateCommand<object> RemoveAssemblyCommand { get; }
        private void RemoveAssemblyMethod(object obj)
        {
            if (obj != null && obj is CmdAssembly)
            {
                Assemblies.Remove(obj as CmdAssembly);
            }
        }


        public DelegateCommand<object> ExecuteMethodCommand { get; }

        private void ExecuteMethodMethod(object obj)
        {
            if (obj is CmdMethod cmdMethod)
            {
                _window?.Hide();
                try
                {
                    var dir = System.IO.Path.GetDirectoryName(cmdMethod.CmdAssembly.FilePath);
                    if (!ExtApp.WorkDir.Contains(dir))
                    {
                        ExtApp.WorkDir.Add(dir);
                    }

                    var assembly = Assembly.Load(System.IO.File.ReadAllBytes(cmdMethod.CmdAssembly.FilePath));

                    var t = assembly.GetType(cmdMethod.TypeName);
                    var method = t.GetMethod(cmdMethod.MethodName);

                    object instance = null;
                    if (!method.IsStatic)
                    {
                        instance = assembly.CreateInstance(cmdMethod.TypeName);
                    }
                    method.Invoke(instance, Type.EmptyTypes);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                _window?.Show();
            }
        }

        private bool CanMethodExecute(object obj)
        {
            if (obj != null && obj is CmdMethod)
            {
                return true;
            }
            return false;
        }


    }


    public class CmdAssembly
    {
        public CmdAssembly()
        {

        }
        public CmdAssembly(string filePath)
        {
            this.FilePath = filePath;
            this.FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            var assembly = Assembly.Load(System.IO.File.ReadAllBytes(filePath));
            this.CommandMethods = GetCmds(assembly);
        }


        public CmdAssembly(Assembly assembly)
        {
            this.FilePath = assembly.Location;
            this.FileName = System.IO.Path.GetFileNameWithoutExtension(assembly.Location);
            this.CommandMethods = GetCmds(assembly);
        }


        private IList<CmdMethod> GetCmds(Assembly assembly)
        {
            var methods = new List<CmdMethod>();

            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(x => x != null).ToList();
            }

            foreach (var type in types)
            {
                var cmdMethods = type.GetMethods().Where(x => x.GetCustomAttribute<CommandMethodAttribute>() != null);
                if (cmdMethods.Any())
                {
                    foreach (var method in cmdMethods)
                    {
                        methods.Add(new CmdMethod(method)
                        {
                            CmdAssembly = this
                        });
                    }
                }
            }
            return methods;
        }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IList<CmdMethod> CommandMethods { get; set; }
    }

    public class CmdMethod
    {
        public CmdMethod()
        {

        }
        public CmdMethod(MethodInfo methodInfo)
        {
            var cmdName = methodInfo.GetCustomAttribute<CommandMethodAttribute>();
            this.CmdName = cmdName.GlobalName;

            this.MethodName = methodInfo.Name;
            this.TypeName = methodInfo.DeclaringType.FullName;
        }
        [JsonIgnore]
        public string DisplayName => $"{TypeName}.{MethodName}";

        public string TypeName { get; set; }

        public string MethodName { get; set; }

        public string CmdName { get; set; }

        public CmdAssembly CmdAssembly { get; internal set; }
    }


}
