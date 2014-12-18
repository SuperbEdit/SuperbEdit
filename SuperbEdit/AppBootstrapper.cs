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
using Custom.Windows;

namespace SuperbEdit
{
    public class AppBootstrapper : BootstrapperBase
    {
        private CompositionContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        private static void OnStartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            MessageBox.Show(string.Format("Another application was launched with args:\n{0}", string.Join("\n", e.Args)));

            //Mattias: I know I should have used MEF but this seemed the only way to do this
            //If anybody knows how to improve this feel free tu submit a PR.

            IShell shell = IoC.Get<IShell>();
            shell.ExecuteCommandLine(e.Args);
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
            var app = Application as InstanceAwareApplication;
            if (app == null || app.IsFirstInstance.GetValueOrDefault())
                DisplayRootViewFor<IShell>();
            else
                app.Shutdown();
        }

        protected override void PrepareApplication()
        {
            base.PrepareApplication();
            var application = (InstanceAwareApplication)Application;
            application.StartupNextInstance += OnStartupNextInstance;
        }
    }
}