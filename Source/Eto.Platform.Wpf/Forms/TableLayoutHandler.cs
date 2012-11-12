﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using swc = System.Windows.Controls;
using sw = System.Windows;
using swd = System.Windows.Data;

namespace Eto.Platform.Wpf.Forms
{
	public class TableLayoutHandler : WpfLayout<swc.Grid, TableLayout>, ITableLayout
	{
		Eto.Drawing.Size spacing;
		bool[] columnScale;
		bool lastColumnScale;
		bool[] rowScale;
		bool lastRowScale;

		public void CreateControl (int cols, int rows)
		{
			columnScale = new bool[cols];
			rowScale = new bool[rows];
			lastColumnScale = true;
			lastRowScale = true;
			Control = new swc.Grid {
				SnapsToDevicePixels = true
			};
			for (int i = 0; i < cols; i++) Control.ColumnDefinitions.Add (new swc.ColumnDefinition {
				Width = GetColumnWidth(i)
			});
			for (int i = 0; i < rows; i++) Control.RowDefinitions.Add (new swc.RowDefinition {
				Height = GetRowHeight(i)
			});
			Spacing = TableLayout.DefaultSpacing;
			Padding = TableLayout.DefaultPadding;
			Control.SizeChanged += delegate {
				SetSizes ();
			};
		}

		void SetSizes ()
		{
			if (!Widget.Loaded) return;
			for (int x = 0; x < Control.ColumnDefinitions.Count; x++) {

				var max = Control.ColumnDefinitions[x].ActualWidth;
				foreach (var child in ColumnControls (x)) {
					if (!double.IsNaN(child.Width))
						child.Width = Math.Max(0, max - child.Margin.Left - child.Margin.Right);
				}
			}
			for (int y = 0; y < Control.RowDefinitions.Count; y++) {
				var max = Control.RowDefinitions[y].ActualHeight;
				foreach (var child in RowControls (y)) {
					if (!double.IsNaN (child.Height))
						child.Height = Math.Max(0, max - child.Margin.Top - child.Margin.Bottom);
				}
			}
		}

		void SetSizes (sw.FrameworkElement control, int col, int row)
		{
			if (!Widget.Loaded) return;
			var maxWidth = double.IsNaN (control.Width) ? 0 : control.Width;
			var maxHeight = double.IsNaN (control.Height) ? 0 : control.Height;
			for (int x = 0; x < Control.ColumnDefinitions.Count; x++) {

				var max = Control.ColumnDefinitions[x].ActualWidth;
				if (x == col && max < maxWidth) max = maxWidth;
				foreach (var child in ColumnControls (x)) {
					if (!double.IsNaN (child.Width))
						child.Width = Math.Max(0, max - child.Margin.Left - child.Margin.Right);
				}
			}
			for (int y = 0; y < Control.RowDefinitions.Count; y++) {
				var max = Control.RowDefinitions[y].ActualHeight;
				if (y == row && max < maxHeight) max = maxHeight;
				foreach (var child in RowControls (y)) {
					if (!double.IsNaN (child.Height))
						child.Height = Math.Max(0, max - child.Margin.Top - child.Margin.Bottom);
				}
			}
		}

		void SetMargins ()
		{
			foreach (var child in Control.Children.OfType<sw.FrameworkElement> ()) {
				var x = swc.Grid.GetColumn (child);
				var y = swc.Grid.GetRow (child);
				SetMargins (child, x, y);
			}
		}

		sw.GridLength GetColumnWidth (int column)
		{
			var scale = columnScale[column];
			if (column == columnScale.Length - 1)
				scale |= lastColumnScale;
			return new System.Windows.GridLength (1, scale ? sw.GridUnitType.Star : sw.GridUnitType.Auto);
		}

		sw.GridLength GetRowHeight (int row)
		{
			var scale = rowScale[row];
			if (row == rowScale.Length - 1)
				scale |= lastRowScale;
			return new System.Windows.GridLength (1, scale ? sw.GridUnitType.Star : sw.GridUnitType.Auto);
		}

		public void SetColumnScale (int column, bool scale)
		{
			columnScale[column] = scale;
			var lastScale = columnScale.Length == 1 || columnScale.Take (columnScale.Length - 1).All (r => !r);
			Control.ColumnDefinitions[column].Width = GetColumnWidth (column);
			if (lastScale != lastColumnScale)
			{
				lastColumnScale = lastScale;
				Control.ColumnDefinitions[columnScale.Length - 1].Width = GetColumnWidth (columnScale.Length - 1);
			}
			SetSizes ();
		}

		public bool GetColumnScale (int column)
		{
			return columnScale[column];
		}

		public void SetRowScale (int row, bool scale)
		{
			rowScale[row] = scale;
			var lastScale = rowScale.Length == 1 || rowScale.Take (rowScale.Length - 1).All (r => !r);
			Control.RowDefinitions[row].Height = GetRowHeight (row);
			if (lastScale != lastRowScale)
			{
				lastRowScale = lastScale;
				Control.RowDefinitions[rowScale.Length - 1].Height = GetRowHeight (rowScale.Length - 1);
			}
			SetSizes ();
		}

		public bool GetRowScale (int row)
		{
			return rowScale[row];
		}

		public Eto.Drawing.Size Spacing
		{
			get { return spacing; }
			set
			{
				spacing = value;
				SetMargins ();
			}
		}

		IEnumerable<sw.FrameworkElement> ColumnControls (int x)
		{
			return Control.Children.OfType<sw.FrameworkElement> ().Where (r => swc.Grid.GetColumn (r) == x);
		}

		IEnumerable<sw.FrameworkElement> RowControls (int y)
		{
			return Control.Children.OfType<sw.FrameworkElement> ().Where (r => swc.Grid.GetRow (r) == y);
		}

		void SetMargins (sw.FrameworkElement c, int x, int y)
		{
			var margin = new sw.Thickness ();
			if (x > 0) margin.Left = spacing.Width / 2;
			if (x < Control.ColumnDefinitions.Count - 1) margin.Right = spacing.Width / 2;
			if (y > 0) margin.Top = spacing.Height / 2;
			if (y < Control.RowDefinitions.Count - 1) margin.Bottom = spacing.Height / 2;
			c.HorizontalAlignment = sw.HorizontalAlignment.Stretch;
			c.VerticalAlignment = sw.VerticalAlignment.Stretch;

			c.Margin = margin;
		}

		public Eto.Drawing.Padding Padding
		{
			get { return Generator.Convert (Control.Margin); }
			set { Control.Margin = Generator.Convert (value); }
		}

		public void Add (Control child, int x, int y)
		{
			if (child == null) {
				foreach (sw.UIElement element in Control.Children) {
					var col = swc.Grid.GetColumn (element);
					if (x != col) continue;
					var row = swc.Grid.GetRow (element);
					if (y != row) continue;
					Control.Children.Remove (element);
					break;
				}
			}
			else {
				var control = (sw.FrameworkElement)child.ControlObject;
				control.SetValue (swc.Grid.ColumnProperty, x);
				control.SetValue (swc.Grid.RowProperty, y);
				SetMargins (control, x, y);
				Control.Children.Add (control);
				SetSizes (control, x, y);
			}
		}

		public void Move (Control child, int x, int y)
		{
			var control = (sw.FrameworkElement)child.ControlObject;
			control.SetValue (swc.Grid.ColumnProperty, x);
			control.SetValue (swc.Grid.RowProperty, y);
			SetMargins (control, x, y);
			SetSizes (control, x, y);
		}

		public void Remove (Control child)
		{
			var control = (System.Windows.UIElement)child.ControlObject;
			Control.Children.Remove (control);
			SetSizes ();
		}
	}
}
