using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using SuperbEdit.Base;
using SuperbEdit.Base.Scripting;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock;
using System.IO;
using Tomers.WPF.Themes.Skins;

namespace SuperbEdit
{
    public class AppBootstrapper : BootstrapperBase
    {
        private CompositionContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            //TODO: implement pure
            IEnumerable<Assembly> assemblies = AssemblyListComposer.GetAssemblyList(Environment.GetCommandLineArgs().Contains("-pure"));
            assemblies = assemblies.Concat(ScriptListComposer.GetAssemblyList(false));
            return assemblies;
        }

        protected override void Configure()
        { 


            MessageBinder.SpecialValues.Add("$documentcontext", context =>
            {
                LayoutDocument doc = null;
                if (context.EventArgs is DocumentClosingEventArgs)
                {
                    var args = context.EventArgs as DocumentClosingEventArgs;
                    doc = args.Document;
                }
                else if (context.EventArgs is DocumentClosedEventArgs)
                {
                    var args = context.EventArgs as DocumentClosedEventArgs;
                    doc = args.Document;
                }
                return doc.Content;
            });

            container = new CompositionContainer(
                new AggregateCatalog(
                    AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()),
                true
                );

            var batch = new CompositionBatch();


            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        protected override object GetInstance(Type service, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(service) : key;
            IEnumerable<object> exports = container.GetExportedValues<object>(contract);
            if (exports.Any())
                return exports.First();
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(service));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            IConfig config = IoC.Get<IConfig>();
            string themeName = config.RetrieveConfigValue<string>("theme", "ExpressionDark.dll");

            string themePath = "";
            if (Directory.GetFiles(Folders.UserFolder).Select(f => Path.GetFileName(f)).Contains(themeName))
            {
                themePath = Folders.UserFolder;
            }
            else
            {
                themePath = Folders.DefaultThemesFolder;
            }

            themePath = Path.Combine(themePath, themeName);

			Skin skinLoad = null;
			string extension = Path.GetExtension(themeName);

			switch (extension)
			{
				case ".dll":
					skinLoad = new DirectAssemblySkin(themeName, themePath);
					break;
				case ".xaml":
					skinLoad = new LooseXamlSkin(themeName, new Uri(themePath));
					break;
				default:
					//TODO: Exception
					break;
			}



            skinLoad.Load();
            DisplayRootViewFor<IShell>();
        }
    }
}