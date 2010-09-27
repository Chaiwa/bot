﻿using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PixelLab.SL.Demo.Core;

namespace PixelLab.SL.Demo {
  public partial class DemoHost : UserControl {
    public DemoHost() {
      InitializeComponent();


      var catalog = new AssemblyCatalog(typeof(App).Assembly);
      var items = (from part in catalog.Parts
                   from definition in part.ExportDefinitions
                   where definition.ContractName == DemoMetadataAttribute.DemoContractName
                   select new DemoMetadata(definition.Metadata["Name"] as string, definition, part)).ToList();

      var welcome = items.Where(tuple => tuple.Name.Equals("Welcome", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
      if (welcome != null) {
        items.Remove(welcome);
        items.Insert(0, welcome);
      }

      m_items.ItemsSource = items;

      m_items.SelectionChanged += (sender, args) => {
        var item = m_items.SelectedItem as DemoMetadata;
        if (item != null) {

          var element = item.PartDefinition.CreatePart().GetExportedValue(item.ExportDefinition) as FrameworkElement;
          if (element == null) {
            m_container.Child = new TextBlock() { Text = "Error loading component" };
          }
          else {
            m_container.Child = element;
          }

        }

      };

      m_items.SelectedIndex = 0;
    }
  }
}
