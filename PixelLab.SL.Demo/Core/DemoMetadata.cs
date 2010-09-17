﻿using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace PixelLab.SL.Demo.Core {

  [MetadataAttribute]
  [AttributeUsage(AttributeTargets.Class)]
  public class DemoMetadataAttribute : ExportAttribute, IDemoMetadata {
    public DemoMetadataAttribute(string name) : this(name, string.Empty) { }
    public DemoMetadataAttribute(string name, string description)
      : base(ContractName, typeof(FrameworkElement)) {
      Name = name;
      Description = description;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }

    public const string ContractName = "PixelLab.SL.Demo";
  }

  public interface IDemoMetadata {
    string Name { get; }
    string Description { get; }
  }
}
