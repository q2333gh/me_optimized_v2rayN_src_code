using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using ServiceLib.Handlers;

namespace ServiceLib.ViewModels
{
    public class MainWindowViewModel : MyReactiveObject
    {
        #region private prop

        private CoreHandler _coreHandler;
        private bool _isAdministrator { get; set; }

        #endregion private prop

        #region ObservableCollection

        private IObservableCollection<RoutingItem> _routingItems = new ObservableCollectionExtended<RoutingItem>();
        public IObservableCollection<RoutingItem> RoutingItems => _routingItems;

        private IObservableCollection<ComboItem> _servers = new ObservableCollectionExtended<ComboItem>();
        public IObservableCollection<ComboItem> Servers => _servers;

        [Reactive]
        public RoutingItem SelectedRouting { get; set; }

        [Reactive]
        public ComboItem SelectedServer { get; set; }

        [Reactive]
        public bool BlServers { get; set; }

        #endregion ObservableCollection

        #region Menu

        //servers
        public ReactiveCommand<Unit, Unit> AddVmessServerCmd { get; }

        public ReactiveCommand<Unit, Unit> AddVlessServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddShadowsocksServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddSocksServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddHttpServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddTrojanServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddHysteria2ServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddTuicServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddWireguardServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddCustomServerCmd { get; }
        public ReactiveCommand<Unit, Unit> AddServerViaClipboardCmd { get; }
        public ReactiveCommand<Unit, Unit> AddServerViaScanCmd { get; }

        //Subscription
        public ReactiveCommand<Unit, Unit> SubSettingCmd { get; }

        public ReactiveCommand<Unit, Unit> SubUpdateCmd { get; }
        public ReactiveCommand<Unit, Unit> SubUpdateViaProxyCmd { get; }
        public ReactiveCommand<Unit, Unit> SubGroupUpdateCmd { get; }
        public ReactiveCommand<Unit, Unit> SubGroupUpdateViaProxyCmd { get; }

        //Setting
        public ReactiveCommand<Unit, Unit> OptionSettingCmd { get; }

        public ReactiveCommand<Unit, Unit> RoutingSettingCmd { get; }
        public ReactiveCommand<Unit, Unit> DNSSettingCmd { get; }
        public ReactiveCommand<Unit, Unit> GlobalHotkeySettingCmd { get; }
        public ReactiveCommand<Unit, Unit> RebootAsAdminCmd { get; }
        public ReactiveCommand<Unit, Unit> ClearServerStatisticsCmd { get; }
        public ReactiveCommand<Unit, Unit> OpenTheFileLocationCmd { get; }

        public ReactiveCommand<Unit, Unit> ReloadCmd { get; }

        [Reactive]
        public bool BlReloadEnabled { get; set; }

        public ReactiveCommand<Unit, Unit> NotifyLeftClickCmd { get; }

        #endregion Menu

        #region System Proxy

        [Reactive]
        public bool BlSystemProxyClear { get; set; }

        [Reactive]
        public bool BlSystemProxySet { get; set; }

        [Reactive]
        public bool BlSystemProxyNothing { get; set; }

        [Reactive]
        public bool BlSystemProxyPac { get; set; }

        public ReactiveCommand<Unit, Unit> SystemProxyClearCmd { get; }
        public ReactiveCommand<Unit, Unit> SystemProxySetCmd { get; }
        public ReactiveCommand<Unit, Unit> SystemProxyNothingCmd { get; }
        public ReactiveCommand<Unit, Unit> SystemProxyPacCmd { get; }

        [Reactive]
        public bool BlRouting { get; set; }

        [Reactive]
        public int SystemProxySelected { get; set; }

        #endregion System Proxy

        #region UI

        [Reactive]
        public string InboundDisplay { get; set; }

        [Reactive]
        public string InboundLanDisplay { get; set; }

        [Reactive]
        public string RunningServerDisplay { get; set; }

        [Reactive]
        public string RunningServerToolTipText { get; set; }

        [Reactive]
        public string RunningInfoDisplay { get; set; }

        [Reactive]
        public string SpeedProxyDisplay { get; set; }

        [Reactive]
        public string SpeedDirectDisplay { get; set; }

        [Reactive]
        public bool EnableTun { get; set; }

        [Reactive]
        public bool ShowClashUI { get; set; }

        [Reactive]
        public int TabMainSelectedIndex { get; set; }

        #endregion UI

        #region Init

        public MainWindowViewModel(bool isAdministrator, Func<EViewAction, object?, Task<bool>>? updateView)
        {
            _config = LazyConfig.Instance.Config;
            _noticeHandler = Locator.Current.GetService<NoticeHandler>();
            _updateView = updateView;
            _isAdministrator = isAdministrator;

            MessageBus.Current.Listen<string>(Global.CommandRefreshProfiles).Subscribe(async x => await _updateView?.Invoke(EViewAction.DispatcherRefreshServersBiz, null));

            SelectedRouting = new();
            SelectedServer = new();

            Init();

            _config.uiItem.showInTaskbar = true;
            if (_config.tunModeItem.enableTun && _isAdministrator)
            {
                EnableTun = true;
            }
            else
            {
                _config.tunModeItem.enableTun = EnableTun = false;
            }

            #region WhenAnyValue && ReactiveCommand

            this.WhenAnyValue(
                x => x.SelectedRouting,
                y => y != null && !y.remarks.IsNullOrEmpty())
                    .Subscribe(c => RoutingSelectedChangedAsync(c));

            this.WhenAnyValue(
              x => x.SelectedServer,
              y => y != null && !y.Text.IsNullOrEmpty())
                  .Subscribe(c => ServerSelectedChanged(c));

            SystemProxySelected = (int)_config.systemProxyItem.sysProxyType;
            this.WhenAnyValue(
              x => x.SystemProxySelected,
              y => y >= 0)
                  .Subscribe(c => DoSystemProxySelected(c));

            this.WhenAnyValue(
              x => x.EnableTun,
               y => y == true)
                  .Subscribe(c => DoEnableTun(c));

            //servers
            AddVmessServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.VMess);
            });
            AddVlessServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.VLESS);
            });
            AddShadowsocksServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.Shadowsocks);
            });
            AddSocksServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.SOCKS);
            });
            AddHttpServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.HTTP);
            });
            AddTrojanServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.Trojan);
            });
            AddHysteria2ServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.Hysteria2);
            });
            AddTuicServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.TUIC);
            });
            AddWireguardServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.WireGuard);
            });
            AddCustomServerCmd = ReactiveCommand.Create(() =>
            {
                AddServerAsync(true, EConfigType.Custom);
            });
            AddServerViaClipboardCmd = ReactiveCommand.Create(() =>
            {
                AddServerViaClipboardAsync(null);
            });
            AddServerViaScanCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await _updateView?.Invoke(EViewAction.ScanScreenTask, null);
            });

            //Subscription
            SubSettingCmd = ReactiveCommand.Create(() =>
            {
                SubSettingAsync();
            });

            SubUpdateCmd = ReactiveCommand.Create(() =>
            {
                UpdateSubscriptionProcess("", false);
            });
            SubUpdateViaProxyCmd = ReactiveCommand.Create(() =>
            {
                UpdateSubscriptionProcess("", true);
            });
            SubGroupUpdateCmd = ReactiveCommand.Create(() =>
            {
                UpdateSubscriptionProcess(_config.subIndexId, false);
            });
            SubGroupUpdateViaProxyCmd = ReactiveCommand.Create(() =>
            {
                UpdateSubscriptionProcess(_config.subIndexId, true);
            });

            //Setting
            OptionSettingCmd = ReactiveCommand.Create(() =>
            {
                OptionSettingAsync();
            });
            RoutingSettingCmd = ReactiveCommand.Create(() =>
            {
                RoutingSettingAsync();
            });
            DNSSettingCmd = ReactiveCommand.Create(() =>
            {
                DNSSettingAsync();
            });
            GlobalHotkeySettingCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await _updateView?.Invoke(EViewAction.GlobalHotkeySettingWindow, null) == true)
                {
                    _noticeHandler?.Enqueue(ResUI.OperationSuccess);
                }
            });
            RebootAsAdminCmd = ReactiveCommand.Create(() =>
            {
                RebootAsAdmin();
            });
            ClearServerStatisticsCmd = ReactiveCommand.Create(() =>
            {
                StatisticsHandler.Instance.ClearAllServerStatistics();
                RefreshServers();
            });
            OpenTheFileLocationCmd = ReactiveCommand.Create(() =>
            {
                Utils.ProcessStart("Explorer", $"/select,{Utils.GetConfigPath()}");
            });

            ReloadCmd = ReactiveCommand.Create(() =>
            {
                Reload();
            });

            NotifyLeftClickCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await _updateView?.Invoke(EViewAction.ShowHideWindow, null);
            });

            //System proxy
            SystemProxyClearCmd = ReactiveCommand.Create(() =>
            {
                SetListenerType(ESysProxyType.ForcedClear);
            });
            SystemProxySetCmd = ReactiveCommand.Create(() =>
            {
                SetListenerType(ESysProxyType.ForcedChange);
            });
            SystemProxyNothingCmd = ReactiveCommand.Create(() =>
            {
                SetListenerType(ESysProxyType.Unchanged);
            });
            SystemProxyPacCmd = ReactiveCommand.Create(() =>
            {
                SetListenerType(ESysProxyType.Pac);
            });

            #endregion WhenAnyValue && ReactiveCommand

            AutoHideStartup();
        }

        private void Init()
        {
            ConfigHandler.InitBuiltinRouting(_config);
            ConfigHandler.InitBuiltinDNS(_config);
            _coreHandler = new CoreHandler(_config, UpdateHandler);
            Locator.CurrentMutable.RegisterLazySingleton(() => _coreHandler, typeof(CoreHandler));

            if (_config.guiItem.enableStatistics)
            {
                StatisticsHandler.Instance.Init(_config, UpdateStatisticsHandler);
            }

            TaskHandler.Instance.RegUpdateTask(_config, UpdateTaskHandler);
            RefreshRoutingsMenu();
            //RefreshServers();

            Reload();
            ChangeSystemProxyStatusAsync(_config.systemProxyItem.sysProxyType, true);

            // 初始化定时任务
            InitCronTasks();
        }

        private DateTime _startTime = DateTime.Now;
        private int _retryCount = 0;
        private int _currentInterval = 10; // 初始间隔10秒
        private void InitCronTasks()
        {
            var taskHandler = PeriodicTaskHandler.Instance;
            var updateHandler = new UpdateHandler();
            
            // 检测连接状态
            _ = taskHandler.AddTask("ConnectivityCheckTask", TimeSpan.FromMinutes(1), async () =>
            {
                _noticeHandler?.SendMessage("正在执行心跳检查...");
                
                // 计算运行时间
                var runningTime = DateTime.Now - _startTime;
                var runningMinutes = (int)runningTime.TotalMinutes;
                
                // 直接获取延迟时间
                var time = await updateHandler.RunAvailabilityCheck();
                if (time == -1)  // -1 表示连接失败
                {
                    _retryCount++;
                    if (_retryCount >= 5)
                    {
                        _noticeHandler?.SendMessage($"服务运行{runningMinutes}分钟后连接检测已失败5次,停止重试");
                        return;
                    }

                    // 加倍重试间隔
                    _currentInterval *= 2;
                    _noticeHandler?.SendMessage($"服务运行{runningMinutes}分钟后检测到连接失败,{_currentInterval}秒后进行第{_retryCount + 1}次重试...");
                    
                    // 更新任务间隔
                    await taskHandler.AddTask("ConnectivityCheckTask", 
                        TimeSpan.FromSeconds(_currentInterval),
                        async () => await updateHandler.RunAvailabilityCheck(),
                        persistent: true);
                        
                    Reload();
                }
                else
                {
                    // 连接成功后重置计数和间隔
                    _retryCount = 0;
                    _currentInterval = 10;
                    
                    if (time < 200) // 延迟小于200ms
                    {
                        _noticeHandler?.SendMessage($"服务运行{runningMinutes}分钟,连接健康，延迟: {time}ms");
                    }
                    else if (time < 500) // 延迟在200-500ms之间
                    {
                        _noticeHandler?.SendMessage($"服务运行{runningMinutes}分钟,连接正常，延迟: {time}ms");
                    }
                    else // 延迟大于500ms
                    {
                        _noticeHandler?.SendMessage($"服务运行{runningMinutes}分钟,连接较慢，延迟: {time}ms");
                    }
                }
            }, persistent: true);
        }
        #endregion Init

        #region Actions

        private void UpdateHandler(bool notify, string msg)
        {
            _noticeHandler?.SendMessage(msg);
            if (notify)
            {
                _noticeHandler?.Enqueue(msg);
            }
        }

        private void UpdateTaskHandler(bool success, string msg)
        {
            _noticeHandler?.SendMessageEx(msg);
            if (success)
            {
                var indexIdOld = _config.indexId;
                RefreshServers();
                if (indexIdOld != _config.indexId)
                {
                    Reload();
                }
                if (_config.uiItem.enableAutoAdjustMainLvColWidth)
                {
                    _updateView?.Invoke(EViewAction.AdjustMainLvColWidth, null);
                }
            }
        }

        private void UpdateStatisticsHandler(ServerSpeedItem update)
        {
            if (!_config.uiItem.showInTaskbar)
            {
                return;
            }
            _updateView?.Invoke(EViewAction.DispatcherStatistics, update);
        }

        public void SetStatisticsResult(ServerSpeedItem update)
        {
            try
            {
                SpeedProxyDisplay = string.Format(ResUI.SpeedDisplayText, Global.ProxyTag, Utils.HumanFy(update.proxyUp), Utils.HumanFy(update.proxyDown));
                SpeedDirectDisplay = string.Format(ResUI.SpeedDisplayText, Global.DirectTag, Utils.HumanFy(update.directUp), Utils.HumanFy(update.directDown));

                if ((update.proxyUp + update.proxyDown) > 0 && DateTime.Now.Second % 3 == 0)
                {
                    Locator.Current.GetService<ProfilesViewModel>()?.UpdateStatistics(update);
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
        }

        public async Task MyAppExitAsync(bool blWindowsShutDown)
        {
            try
            {
                Logging.SaveLog("MyAppExit Begin");
                //if (blWindowsShutDown)
                await _updateView?.Invoke(EViewAction.UpdateSysProxy, true);

                ConfigHandler.SaveConfig(_config);
                ProfileExHandler.Instance.SaveTo();
                StatisticsHandler.Instance.SaveTo();
                StatisticsHandler.Instance.Close();
                _coreHandler.CoreStop();

                // 停止定时任务
                await PeriodicTaskHandler.Instance.Stop();

                Logging.SaveLog("MyAppExit End");
            }
            catch { }
            finally
            {
                await _updateView?.Invoke(EViewAction.Shutdown, null);
            }
        }

        public async Task V2rayUpgrade(string fileName)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "v2rayUpgrade",
                    Arguments = fileName.AppendQuotes(),
                    WorkingDirectory = Utils.StartupPath()
                }
            };
            process.Start();
            if (process.Id > 0)
            {
                await MyAppExitAsync(false);
            }
        }

        #endregion Actions

        #region Servers && Groups

        private void RefreshServers()
        {
            MessageBus.Current.SendMessage("", Global.CommandRefreshProfiles);
        }

        public void RefreshServersBiz()
        {
            RefreshServersMenu();

            //display running server
            var running = ConfigHandler.GetDefaultServer(_config);
            if (running != null)
            {
                RunningServerDisplay =
                RunningServerToolTipText = running.GetSummary();
            }
            else
            {
                RunningServerDisplay =
                RunningServerToolTipText = ResUI.CheckServerSettings;
            }
        }

        private void RefreshServersMenu()
        {
            var lstModel = LazyConfig.Instance.ProfileItems(_config.subIndexId, "");

            _servers.Clear();
            if (lstModel.Count > _config.guiItem.trayMenuServersLimit)
            {
                BlServers = false;
                return;
            }

            BlServers = true;
            for (int k = 0; k < lstModel.Count; k++)
            {
                ProfileItem it = lstModel[k];
                string name = it.GetSummary();

                var item = new ComboItem() { ID = it.indexId, Text = name };
                _servers.Add(item);
                if (_config.indexId == it.indexId)
                {
                    SelectedServer = item;
                }
            }
        }

        private void RefreshSubscriptions()
        {
            Locator.Current.GetService<ProfilesViewModel>()?.RefreshSubscriptions();
        }

        #endregion Servers && Groups

        #region Add Servers

        public async Task AddServerAsync(bool blNew, EConfigType eConfigType)
        {
            ProfileItem item = new()
            {
                subid = _config.subIndexId,
                configType = eConfigType,
                isSub = false,
            };

            bool? ret = false;
            if (eConfigType == EConfigType.Custom)
            {
                ret = await _updateView?.Invoke(EViewAction.AddServer2Window, item);
            }
            else
            {
                ret = await _updateView?.Invoke(EViewAction.AddServerWindow, item);
            }
            if (ret == true)
            {
                RefreshServers();
                if (item.indexId == _config.indexId)
                {
                    Reload();
                }
            }
        }

        public async Task AddServerViaClipboardAsync(string? clipboardData)
        {
            if (clipboardData == null)
            {
                await _updateView?.Invoke(EViewAction.AddServerViaClipboard, null);
                return;
            }
            int ret = ConfigHandler.AddBatchServers(_config, clipboardData, _config.subIndexId, false);
            if (ret > 0)
            {
                RefreshSubscriptions();
                RefreshServers();
                _noticeHandler?.Enqueue(string.Format(ResUI.SuccessfullyImportedServerViaClipboard, ret));
            }
        }

        public void ScanScreenTaskAsync(string result)
        {
            if (Utils.IsNullOrEmpty(result))
            {
                _noticeHandler?.Enqueue(ResUI.NoValidQRcodeFound);
            }
            else
            {
                int ret = ConfigHandler.AddBatchServers(_config, result, _config.subIndexId, false);
                if (ret > 0)
                {
                    RefreshSubscriptions();
                    RefreshServers();
                    _noticeHandler?.Enqueue(ResUI.SuccessfullyImportedServerViaScan);
                }
            }
        }

        private void SetDefaultServer(string indexId)
        {
            if (Utils.IsNullOrEmpty(indexId))
            {
                return;
            }
            if (indexId == _config.indexId)
            {
                return;
            }
            var item = LazyConfig.Instance.GetProfileItem(indexId);
            if (item is null)
            {
                _noticeHandler?.Enqueue(ResUI.PleaseSelectServer);
                return;
            }

            if (ConfigHandler.SetDefaultServerIndex(_config, indexId) == 0)
            {
                RefreshServers();
                Reload();
            }
        }

        private void ServerSelectedChanged(bool c)
        {
            if (!c)
            {
                return;
            }
            if (SelectedServer == null)
            {
                return;
            }
            if (Utils.IsNullOrEmpty(SelectedServer.ID))
            {
                return;
            }
            SetDefaultServer(SelectedServer.ID);
        }

        public async Task TestServerAvailability()
        {
            var item = ConfigHandler.GetDefaultServer(_config);
            if (item == null)
            {
                return;
            }
            await (new UpdateHandler()).RunAvailabilityCheck(async (bool success, string msg) =>
            {
                _noticeHandler?.SendMessageEx(msg);
                await _updateView?.Invoke(EViewAction.DispatcherServerAvailability, msg);
            });
        }

        public void TestServerAvailabilityResult(string msg)
        {
            RunningInfoDisplay = msg;
        }

        #endregion Add Servers

        #region Subscription

        private async Task SubSettingAsync()
        {
            if (await _updateView?.Invoke(EViewAction.SubSettingWindow, null) == true)
            {
                RefreshSubscriptions();
            }
        }

        public void UpdateSubscriptionProcess(string subId, bool blProxy)
        {
            (new UpdateHandler()).UpdateSubscriptionProcess(_config, subId, blProxy, UpdateTaskHandler);
        }

        #endregion Subscription

        #region Setting

        private async Task OptionSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.OptionSettingWindow, null);
            if (ret == true)
            {
                //RefreshServers();
                Reload();
            }
        }

        private async Task RoutingSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.RoutingSettingWindow, null);
            if (ret == true)
            {
                ConfigHandler.InitBuiltinRouting(_config);
                RefreshRoutingsMenu();
                //RefreshServers();
                Reload();
            }
        }

        private async Task DNSSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.DNSSettingWindow, null);
            if (ret == true)
            {
                Reload();
            }
        }

        private void RebootAsAdmin()
        {
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                Arguments = Global.RebootAs,
                WorkingDirectory = Utils.StartupPath(),
                FileName = Utils.GetExePath().AppendQuotes(),
                Verb = "runas",
            };
            try
            {
                Process.Start(startInfo);
                MyAppExitAsync(false);
            }
            catch { }
        }

        #endregion Setting

        #region core job

        /// <summary>
        /// 重启核心服务的主要方法
        /// </summary>
        public void Reload()
        {
            // 1. 禁用重启按钮，防止重复操作
            BlReloadEnabled = false;

            // 2. 加载核心服务，完成后执行后续操作
            LoadCore().ContinueWith(async task =>
            {
                // 3. 测试当前选中服务器的可用性
                await TestServerAvailability();

                // 4. 触发UI更新
                await _updateView?.Invoke(EViewAction.DispatcherReload, null);
            });
        }

  

        /// <summary>
        /// UI更新完成后的回调方法
        /// </summary>
        public void ReloadResult()
        {
            // 1. 根据当前配置更新系统代理状态
            ChangeSystemProxyStatusAsync(_config.systemProxyItem.sysProxyType, false);
            
            // 2. 重新启用重启按钮
            BlReloadEnabled = true;
            
            // 3. 检查是否使用Clash核心
            ShowClashUI = _config.IsRunningCore(ECoreType.clash);
            if (ShowClashUI)
            {
                // 如果是Clash核心，重新加载代理列表
                Locator.Current.GetService<ClashProxiesViewModel>()?.ProxiesReload();
            }
            else 
            { 
                // 非Clash核心，切换到第一个标签页
                TabMainSelectedIndex = 0; 
            }
        }

        /// <summary>
        /// 加载核心服务的具体实现
        /// </summary>
        private async Task LoadCore()
        {
            await Task.Run(() =>
            {
                // 获取当前默认的服务器配置
                var node = ConfigHandler.GetDefaultServer(_config);
                // 使用新的配置��新加载核心服务
                _coreHandler.LoadCore(node);
            });
        }

        public void CloseCore()
        {
            ConfigHandler.SaveConfig(_config, false);

            ChangeSystemProxyStatusAsync(ESysProxyType.ForcedClear, false);

            _coreHandler.CoreStop();
        }

        #endregion core job

        #region System proxy and Routings

        public void SetListenerType(ESysProxyType type)
        {
            if (_config.systemProxyItem.sysProxyType == type)
            {
                return;
            }
            _config.systemProxyItem.sysProxyType = type;
            ChangeSystemProxyStatusAsync(type, true);

            SystemProxySelected = (int)_config.systemProxyItem.sysProxyType;
            ConfigHandler.SaveConfig(_config, false);
        }

        private async Task ChangeSystemProxyStatusAsync(ESysProxyType type, bool blChange)
        {
            //await _updateView?.Invoke(EViewAction.UpdateSysProxy, _config.tunModeItem.enableTun ? true : false);
            await _updateView?.Invoke(EViewAction.UpdateSysProxy, false);
            _noticeHandler?.SendMessageEx($"{ResUI.TipChangeSystemProxy} - {_config.systemProxyItem.sysProxyType.ToString()}");

            BlSystemProxyClear = (type == ESysProxyType.ForcedClear);
            BlSystemProxySet = (type == ESysProxyType.ForcedChange);
            BlSystemProxyNothing = (type == ESysProxyType.Unchanged);
            BlSystemProxyPac = (type == ESysProxyType.Pac);

            InboundDisplayStaus();

            if (blChange)
            {
                await _updateView?.Invoke(EViewAction.DispatcherRefreshIcon, null);
            }
        }

        private void RefreshRoutingsMenu()
        {
            _routingItems.Clear();
            if (!_config.routingBasicItem.enableRoutingAdvanced)
            {
                BlRouting = false;
                return;
            }

            BlRouting = true;
            var routings = LazyConfig.Instance.RoutingItems();
            foreach (var item in routings)
            {
                _routingItems.Add(item);
                if (item.id == _config.routingBasicItem.routingIndexId)
                {
                    SelectedRouting = item;
                }
            }
        }

        private async Task RoutingSelectedChangedAsync(bool c)
        {
            if (!c)
            {
                return;
            }

            if (SelectedRouting == null)
            {
                return;
            }

            var item = LazyConfig.Instance.GetRoutingItem(SelectedRouting?.id);
            if (item is null)
            {
                return;
            }
            if (_config.routingBasicItem.routingIndexId == item.id)
            {
                return;
            }

            if (ConfigHandler.SetDefaultRouting(_config, item) == 0)
            {
                _noticeHandler?.SendMessageEx(ResUI.TipChangeRouting);
                Reload();
                await _updateView?.Invoke(EViewAction.DispatcherRefreshIcon, null);
            }
        }

        private void DoSystemProxySelected(bool c)
        {
            if (!c)
            {
                return;
            }
            if (_config.systemProxyItem.sysProxyType == (ESysProxyType)SystemProxySelected)
            {
                return;
            }
            SetListenerType((ESysProxyType)SystemProxySelected);
        }

        private void DoEnableTun(bool c)
        {
            if (_config.tunModeItem.enableTun != EnableTun)
            {
                _config.tunModeItem.enableTun = EnableTun;
                // When running as a non-administrator, reboot to administrator mode
                if (EnableTun && !_isAdministrator)
                {
                    _config.tunModeItem.enableTun = false;
                    RebootAsAdmin();
                    return;
                }
                ConfigHandler.SaveConfig(_config);
                Reload();
            }
        }

        #endregion System proxy and Routings

        #region UI

        public void InboundDisplayStaus()
        {
            StringBuilder sb = new();
            sb.Append($"[{EInboundProtocol.socks}:{LazyConfig.Instance.GetLocalPort(EInboundProtocol.socks)}]");
            sb.Append(" | ");
            //if (_config.systemProxyItem.sysProxyType == ESysProxyType.ForcedChange)
            //{
            //    sb.Append($"[{Global.InboundHttp}({ResUI.SystemProxy}):{LazyConfig.Instance.GetLocalPort(Global.InboundHttp)}]");
            //}
            //else
            //{
            sb.Append($"[{EInboundProtocol.http}:{LazyConfig.Instance.GetLocalPort(EInboundProtocol.http)}]");
            //}
            InboundDisplay = $"{ResUI.LabLocal}:{sb}";

            if (_config.inbound[0].allowLANConn)
            {
                if (_config.inbound[0].newPort4LAN)
                {
                    StringBuilder sb2 = new();
                    sb2.Append($"[{EInboundProtocol.socks}:{LazyConfig.Instance.GetLocalPort(EInboundProtocol.socks2)}]");
                    sb2.Append(" | ");
                    sb2.Append($"[{EInboundProtocol.http}:{LazyConfig.Instance.GetLocalPort(EInboundProtocol.http2)}]");
                    InboundLanDisplay = $"{ResUI.LabLAN}:{sb2}";
                }
                else
                {
                    InboundLanDisplay = $"{ResUI.LabLAN}:{sb}";
                }
            }
            else
            {
                InboundLanDisplay = $"{ResUI.LabLAN}:None";
            }
        }

        private void AutoHideStartup()
        {
            if (_config.uiItem.autoHideStartup)
            {
                Observable.Range(1, 1)
                 .Delay(TimeSpan.FromSeconds(1))
                 .Subscribe(async x =>
                 {
                     await _updateView?.Invoke(EViewAction.ShowHideWindow, false);
                 });
            }
        }

        #endregion UI
    }
}