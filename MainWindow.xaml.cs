using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace ViewboxImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            string[] files;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = (string[])e.Data.GetData(DataFormats.FileDrop);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                files = new string[] { (string)e.Data.GetData(DataFormats.Text) };
            }
            else
            {
                return;
            }
            if (files != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string file in files)
                {
                    this.HandleFileOpen(new FileInfo(new Uri(file).LocalPath), sb);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                this.Output = sb.ToString();
            }
        }

        /// <summary>
        /// <?xml version="1.0" encoding="UTF-8"?>
        /// <!-- users2 icon from the IconExperience.com O-Collection.Copyright by INCORS GmbH - www.incors.com -->
        /// <Viewbox xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation" Width="128" Height="128">
        ///   <Canvas Width = "10240" Height="10240">
        /// 	 <Path Data = "M9600 6922c0,544 -1152,758 -1920,758 -768,0 -1920,-214 -1920,-757 0,-1099 304,-1331 871,-1795 301,196 662,311 1049,311 387,0 748,-115 1049,-311 568,465 871,694 871,1794z" Fill="#4D82B8"/>
        /// 	 <Path Data = "M7680 1933c884,0 1600,714 1600,1594 0,880 -716,1593 -1600,1593 -884,0 -1600,-713 -1600,-1593 0,-880 716,-1594 1600,-1594z" Fill="#4D82B8"/>
        /// 	 <Path Data = "M4480 6922c0,544 -1152,758 -1920,758 -768,0 -1920,-214 -1920,-757 0,-1099 304,-1331 871,-1795 301,196 662,311 1049,311 387,0 748,-115 1049,-311 568,465 871,694 871,1794z" Fill="#4D82B8"/>
        /// 	 <Path Data = "M2560 1933c884,0 1600,714 1600,1594 0,880 -716,1593 -1600,1593 -884,0 -1600,-713 -1600,-1593 0,-880 716,-1594 1600,-1594z" Fill="#4D82B8"/>
        ///   </Canvas>
        /// </Viewbox>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="sb"></param>
        private void HandleFileOpen(FileInfo file, StringBuilder sb)
        {
            XDocument xDoc = XDocument.Load(file.OpenRead());
            XElement viewBox = xDoc.Root;
            Contract.Assert(viewBox != null);
            XName canvasName = XName.Get("Canvas", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            XName pathName = XName.Get("Path", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            XElement canvas = viewBox.Element(canvasName);
            Contract.Assert(canvas != null);
            int width = (int)canvas.Attribute("Width");
            int height = (int)canvas.Attribute("Height");

            sb.AppendLine($"<DrawingImage x:Key=\"{Path.GetFileNameWithoutExtension(file.Name)}\">");
            sb.AppendLine($"    <DrawingImage.Drawing>");
            sb.AppendLine($"        <DrawingGroup>");
            sb.AppendLine($"            <GeometryDrawing>");
            sb.AppendLine($"                <GeometryDrawing.Geometry>");
            sb.AppendLine($"                    <RectangleGeometry Rect=\"0,0,{width},{height}\"/>");
            sb.AppendLine($"                </GeometryDrawing.Geometry>");
            sb.AppendLine($"            </GeometryDrawing>");

            foreach (XElement path in canvas.Elements(pathName))
            {
                sb.AppendLine($"            <GeometryDrawing Brush=\"{path.Attribute("Fill")?.Value}\" Geometry=\"{path.Attribute("Data")?.Value}\"/>");
            }
            sb.AppendLine($"        </DrawingGroup>");
            sb.AppendLine($"    </DrawingImage.Drawing>");
            sb.AppendLine($"</DrawingImage>");
            //<DrawingImage x:Key="ImageUser2">
            //    <DrawingImage.Drawing>
            //        <DrawingGroup>
            //            <GeometryDrawing>
            //                <GeometryDrawing.Geometry>
            //                    <RectangleGeometry Rect="0,0,10240,10240"/>
            //                </GeometryDrawing.Geometry>
            //            </GeometryDrawing>

            //            <GeometryDrawing Brush="#4D82B8"
            //                             Geometry="M9600 6922c0,544 -1152,758 -1920,758 -768,0 -1920,-214 -1920,-757 0,-1099 304,-1331 871,-1795 301,196 662,311 1049,311 387,0 748,-115 1049,-311 568,465 871,694 871,1794z"/>
            //            <GeometryDrawing Brush="#4D82B8"
            //                             Geometry="M7680 1933c884,0 1600,714 1600,1594 0,880 -716,1593 -1600,1593 -884,0 -1600,-713 -1600,-1593 0,-880 716,-1594 1600,-1594z"/>
            //            <GeometryDrawing Brush="#4D82B8"
            //                             Geometry="M4480 6922c0,544 -1152,758 -1920,758 -768,0 -1920,-214 -1920,-757 0,-1099 304,-1331 871,-1795 301,196 662,311 1049,311 387,0 748,-115 1049,-311 568,465 871,694 871,1794z"/>
            //            <GeometryDrawing Brush="#4D82B8"
            //                             Geometry="M2560 1933c884,0 1600,714 1600,1594 0,880 -716,1593 -1600,1593 -884,0 -1600,-713 -1600,-1593 0,-880 716,-1594 1600,-1594z"/>
            //        </DrawingGroup>
            //    </DrawingImage.Drawing>
            //</DrawingImage>
        }

        public string Output
        {
            get { return (string)this.GetValue(OutputProperty); }
            set { this.SetValue(OutputProperty, value); }
        }

        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register(
            @"Output", typeof(string), typeof(MainWindow), new PropertyMetadata());

        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainWindow_OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}