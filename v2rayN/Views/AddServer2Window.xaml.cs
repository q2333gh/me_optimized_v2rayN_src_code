﻿using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace v2rayN.Views
{
    public partial class AddServer2Window
    {
        public AddServer2Window(ProfileItem profileItem)
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
            this.Loaded += Window_Loaded;
            ViewModel = new AddServer2ViewModel(profileItem, UpdateViewHandler);

            foreach (ECoreType it in Enum.GetValues(typeof(ECoreType)))
            {
                if (it == ECoreType.v2rayN)
                    continue; 
                //TODO
                if (it is ECoreType.clash or ECoreType.clash_meta or ECoreType.SagerNet)
                    continue;
                cmbCoreType.Items.Add(it.ToString());
            }
            cmbCoreType.Items.Add(string.Empty);

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.SelectedSource.remarks, v => v.txtRemarks.Text).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedSource.address, v => v.txtAddress.Text).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.CoreType, v => v.cmbCoreType.Text).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedSource.displayLog, v => v.togDisplayLog.IsChecked).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedSource.preSocksPort, v => v.txtPreSocksPort.Text).DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.BrowseServerCmd, v => v.btnBrowse).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.EditServerCmd, v => v.btnEdit).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.SaveServerCmd, v => v.btnSave).DisposeWith(disposables);
            });
            WindowsUtils.SetDarkBorder(this, LazyConfig.Instance.Config.uiItem.followSystemTheme ? !WindowsUtils.IsLightTheme() : LazyConfig.Instance.Config.uiItem.colorModeDark);
        }

        private async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
        {
            switch (action)
            {
                case EViewAction.CloseWindow:
                    this.DialogResult = true;
                    break;

                case EViewAction.BrowseServer:
                    if (UI.OpenFileDialog(out string fileName, "Config|*.json|YAML|*.yaml;*.yml|All|*.*") != true)
                    {
                        return false;
                    }
                    ViewModel?.BrowseServer(fileName);
                    break;
            }

            return await Task.FromResult(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtRemarks.Focus();
        }
    }
}