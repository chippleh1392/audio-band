﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using AudioBand.Models;
using AutoMapper;
using FastMember;
using NLog;

namespace AudioBand.ViewModels
{
    /// <summary>
    /// Base class for a viewmodel with a model.
    /// </summary>
    /// <typeparam name="TModel">Type of the model</typeparam>
    public abstract class ViewModelBase<TModel> : ViewModelBase
        where TModel : ModelBase, new()
    {
        // Mapping from a model and model property to the viewmodel property name
        private readonly Dictionary<(object model, string modelPropName), string> _modelToPropertyName = new Dictionary<(object model, string modelPropName), string>();
        private readonly Dictionary<object, ObjectAccessor> _modelToAccessor = new Dictionary<object, ObjectAccessor>();
        private readonly MapperConfiguration _mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TModel>());
        private TModel _backup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{TModel}"/> class
        /// with the model and hooks up change notifications.
        /// </summary>
        /// <param name="model">The model.</param>
        protected ViewModelBase(TModel model)
        {
            Model = model;
            SetupModelBindings(Model);
        }

        /// <summary>
        /// Gets the model associated with this view model.
        /// </summary>
        protected TModel Model { get; }

        /// <summary>
        /// Get the model representation of this viewmodel. By default it returns <see cref="Model"/>.
        /// </summary>
        /// <returns>A model representation</returns>
        public virtual TModel GetModel()
        {
            return Model;
        }

        /// <summary>
        /// Sets the property in the <see cref="Model"/>.
        /// </summary>
        /// <typeparam name="TValue">Type of the property to set.</typeparam>
        /// <param name="modelPropertyName">Name of the property to set.</param>
        /// <param name="newValue">New value to set.</param>
        /// <param name="propertyName">Name of the property to notify with.</param>
        /// <returns>Returns true if new value was set</returns>
        protected bool SetProperty<TValue>(string modelPropertyName, TValue newValue, [CallerMemberName] string propertyName = null)
        {
            // Try and set the new value
            _modelToAccessor[Model][modelPropertyName] = newValue;

            // See if the value has changed
            var currentValue = (TValue)_modelToAccessor[Model][modelPropertyName];
            return EqualityComparer<TValue>.Default.Equals(currentValue, newValue);
        }

        /// <summary>
        /// Performs setup to recieve change notifications from <paramref name="model"/> .
        /// </summary>
        /// <param name="model">Model to subscribe to for <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <typeparam name="T">The type of the model.</typeparam>
        protected void SetupModelBindings<T>(T model)
            where T : ModelBase
        {
            _modelToAccessor.Add(model, ObjectAccessor.Create(model));

            var members = Accessor.GetMembers().Where(m => m.IsDefined(typeof(PropertyChangeBindingAttribute)));
            foreach (var member in members)
            {
                var bindingAttr = (PropertyChangeBindingAttribute)member.GetAttribute(typeof(PropertyChangeBindingAttribute), true);
                _modelToPropertyName.Add((model, bindingAttr.PropertyName), member.Name);
            }

            model.PropertyChanged += ModelOnPropertyChanged;
        }

        /// <summary>
        /// Resets the <see cref="Model"/> to a state as if it was just instantiated.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            ResetObject(Model);
        }

        /// <summary>
        /// Cancels edits to the <see cref="Model"/>.
        /// </summary>
        protected override void OnCancelEdit()
        {
            base.OnCancelEdit();
            if (_backup == null)
            {
                Logger.Warn("Cancelling edit but backup is null. Begin edit wasn't called.");
            }

            _mapperConfiguration.CreateMapper().Map(_backup, Model);
            _backup = null;
        }

        /// <summary>
        /// Accepts edits to the <see cref="Model"/>.
        /// </summary>
        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            if (_backup == null)
            {
                Logger.Warn("Ending edit but backup is null. Begin edit wasn't called.");
            }

            _backup = null;
        }

        /// <summary>
        /// Starts tracking changes to the <see cref="Model"/>.
        /// </summary>
        protected override void OnBeginEdit()
        {
            base.OnBeginEdit();
            if (_backup != null)
            {
                return;
            }

            Logger.Debug("Starting edit");

            _backup = new TModel();
            _mapperConfiguration.CreateMapper().Map(Model, _backup);
        }

        /// <summary>
        /// Try to load image from path or default image if invalid file.
        /// </summary>
        /// <param name="path">Path to load image from.</param>
        /// <param name="defaultImage">Default image to use if unable to laod file.</param>
        /// <returns>The loaded image or default image.</returns>
        protected Image LoadImage(string path, Image defaultImage)
        {
            try
            {
                return string.IsNullOrEmpty(path) ? defaultImage : Image.FromFile(path);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error loading image from {path}", path);
                return defaultImage;
            }
        }

        /// <summary>
        /// Called when a model property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnModelPropertyChanged(string propertyName) { }

        /// <summary>
        /// When a model property changes, raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for the corresponding property marked with <see cref="PropertyChangeBindingAttribute"/>
        /// and also properties in <see cref="AlsoNotifyAttribute"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyChangedEventArgs">The event args.</param>
        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!_modelToPropertyName.TryGetValue((sender, propertyChangedEventArgs.PropertyName), out var propertyName))
            {
                return;
            }

            RaisePropertyChanged(propertyName);

            if (!AlsoNotifyMap.TryGetValue(propertyName, out var alsoNotfify))
            {
                return;
            }

            foreach (var name in alsoNotfify)
            {
                RaisePropertyChanged(name);
            }

            OnModelPropertyChanged(propertyName);
        }
    }
}
