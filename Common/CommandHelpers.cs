﻿using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PixelLab.Common
{
    public class CommandHelpers
    {
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyPropHelper.RegisterAttached<CommandHelpers, FrameworkElement, object>("CommandParameter");

        public static object GetCommandParameter(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return element.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(FrameworkElement element, object value)
        {
            Contract.Requires(element != null);
            element.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandStringProperty =
            DependencyPropHelper.RegisterAttached<CommandHelpers, FrameworkElement, string>(
                "CommandString",
                element_commandStringChanged);

        public static string GetCommandString(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return (string)element.GetValue(CommandStringProperty);
        }

        public static void SetCommandString(FrameworkElement element, string value)
        {
            element.SetValue(CommandStringProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyPropHelper.RegisterAttached<CommandHelpers, FrameworkElement, ICommand>(
                "Command",
                element_commandChanged);

        public static ICommand GetCommand(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static void SetCommand(FrameworkElement element, ICommand value)
        {
            Contract.Requires(element != null);
            element.SetValue(CommandProperty, value);
        }

        #region impl

        private static void element_commandStringChanged(FrameworkElement source, string newValue, string oldValue)
        {
            Debug.Assert(source != null);
            Debug.Assert(!newValue.IsNullOrWhiteSpace());
            Debug.Assert(oldValue == null);

            source.Loaded += commandStringElement_loaded;
        }

        private static void commandStringElement_loaded(object sender, EventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Loaded -= commandStringElement_loaded;
            commandStringElement_loaded(element);
        }

        private static void commandStringElement_loaded(FrameworkElement element)
        {
            Contract.Requires(element != null);
            var commandName = GetCommandString(element);
            var value = (from anscestor in element.GetAncestors().OfType<ICommandProxy>()
                         let command = TryGetCommand(anscestor, element, commandName)
                         where command != null
                         select command).FirstOrDefault();

            if (value == null)
            {
                DebugTrace.WriteLine("Could not bind command string '{0}' to {1}", commandName, element);
                value = DeadCommand.Instance;
            }
            SetCommand(element, value);
        }

        private static ICommand TryGetCommand(ICommandProxy proxy, DependencyObject source, string name)
        {
            Contract.Requires(proxy != null);
            Contract.Requires(source != null);
            Contract.Requires(!name.IsNullOrWhiteSpace());

            var realSource = proxy.GetCommandOwner(source);
            if (realSource != null)
            {
                var propInfo = (from property in realSource.GetType().GetProperties()
                                where property.CanRead
                                where property.Name == name
                                where typeof(ICommand).IsAssignableFrom(property.PropertyType)
                                select property).SingleOrDefault();

                if (propInfo != null)
                {
                    return (ICommand)propInfo.GetGetMethod().Invoke(realSource, new object[0]);
                }
            }
            return null;
        }

        private static void element_commandChanged(FrameworkElement source, ICommand newValue, ICommand oldValue)
        {
            if (source is ButtonBase)
            {
                var button = (ButtonBase)source;
                Debug.Assert(button.Command == oldValue);
                button.Command = newValue;
            }
            else
            {
                // TODO: Do we want to ponder how to handle enabled changed? Perhaps disable the element? ...should ponder
                if (oldValue != null)
                {
                    source.MouseLeftButtonDown -= source_MouseLeftButtonDown;
                    if (elementLookup.ContainsKey(oldValue))
                    {
                        elementLookup[oldValue].Remove(source);
                    }

                    oldValue.CanExecuteChanged -= Command_CanExecuteChanged;
                }
                if (newValue != null)
                {
                    source.MouseLeftButtonDown += source_MouseLeftButtonDown;
                    if (!elementLookup.ContainsKey(newValue))
                    {
                        elementLookup.Add(newValue, new List<FrameworkElement>());
                    }

                    elementLookup[newValue].Add(source);
                    newValue.CanExecuteChanged += Command_CanExecuteChanged;
                }
            }
        }

        private static void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            var command = sender as ICommand;
            if ((command != null) && (elementLookup.ContainsKey(command)))
            {
                var elements = elementLookup[command];
                var controls = from el in elements
                               where el is Control
                               select (Control)el;
                foreach (var control in controls)
                {
                    var param = GetCommandParameter(control);
                    bool canExecute = command.CanExecute(param);
                    control.IsEnabled = canExecute;
                }
            }
        }

        private static void source_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                var element = (FrameworkElement)sender;
                Debug.Assert(!(element is ButtonBase));
                var command = GetCommand(element);
                Debug.Assert(command != null);
                var param = GetCommandParameter(element) ?? element.DataContext;

                if (command.CanExecute(param))
                {
                    e.Handled = true;
                    command.Execute(param);
                }
            }
        }

        private static readonly Dictionary<ICommand, List<FrameworkElement>> elementLookup = new Dictionary<ICommand, List<FrameworkElement>>();

        #endregion
    }
}
