using Microsoft.Practices.ServiceLocation;
using Prism.DryIoc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TapeTracker
{
    internal class Bootstrapper : DryIocBootstrapper
    {
        protected Application CurrentApplication => Application.Current;
        protected BindableBase ShellBase => (BindableBase)CurrentApplication.MainWindow.DataContext;

        protected override DependencyObject CreateShell()
            => ServiceLocator.Current.GetInstance<MainWindow>();

        protected override void InitializeShell()
        {
            //PageContainer.RegisterAssembly(Assembly.GetExecutingAssembly());

            //ShellBase.CollectPages();

            CurrentApplication.MainWindow = (MainWindow)Shell;
            CurrentApplication.MainWindow.Show();
        }

    }
}
