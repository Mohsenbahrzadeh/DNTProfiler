﻿using System.Collections.Specialized;
using System.ComponentModel;
using DNTProfiler.Common.Converters;
using DNTProfiler.Common.JsonToolkit;
using DNTProfiler.Common.Models;
using DNTProfiler.Common.Mvvm;
using DNTProfiler.ContextInMultipleThreads.Core;
using DNTProfiler.Infrastructure.ViewModels;
using DNTProfiler.PluginsBase;

namespace DNTProfiler.ContextInMultipleThreads.ViewModels
{
    public class MainViewModel : MainViewModelBase
    {
        private readonly CallbacksManager _callbacksManager;

        public MainViewModel(ProfilerPluginBase pluginContext)
            : base(pluginContext)
        {
            if (Designer.IsInDesignModeStatic)
                return;

            setActions();
            setGuiModel();
            _callbacksManager = new CallbacksManager(PluginContext, GuiModelData);
            setEvenets();
        }

        private void GuiModelData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedApplicationIdentity":
                    _callbacksManager.ShowSelectedApplicationIdentityLocalCommands();
                    break;
                case "SelectedExecutedCommand":
                    _callbacksManager.ShowSelectedCommandRelatedStackTraces();
                    break;
            }
        }

        private void setActions()
        {
            PluginContext.Reset = () =>
            {
                ResetAll();
            };

            PluginContext.GetResults = () =>
            {
                return GuiModelData.RelatedCommands.ToFormattedJson();
            };
        }

        private void setEvenets()
        {
            PluginContext.ProfilerData.Commands.CollectionChanged += Commands_CollectionChanged;
        }

        void Commands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Command item in e.NewItems)
                    {
                        _callbacksManager.CheckObjectContextsInMultipleThreads(item);
                    }
                    break;
            }
        }

        private void setGuiModel()
        {
            GuiModelData.PropertyChanged += GuiModelData_PropertyChanged;
        }
    }
}