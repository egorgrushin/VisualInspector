using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using AForge.Video.FFMPEG;
using VisualInspector.ViewModels;
using NLog;

namespace VisualInspector.Models
{
    public enum WarningLevels
    {
        Normal, Middle, High
    }

    public class Event
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly int FramesForPreview = 5;


        #region Properties

        public WarningLevels WarningLevel { get; set; }
        public int Lock { get; set; }
        public int Sensor { get; set; }
        public int Room { get; set; }
        public DateTime DateTime { get; set; }
        public int AccessLevel { get; set; }
        public string VideoFileName { get; set; }


		public List<BitmapImage> InitFramesList(object sender, DoWorkEventArgs e)
        {
			logger.Debug("Strated InitFramesList for {0}", this);
			var worker = sender as BackgroundWorker;
            var framesList = new List<BitmapImage>();

			if (File.Exists(VideoFileName))
			{
				var videoReader = new VideoFileReader();
				videoReader.Open(VideoFileName);
				var framesCount = videoReader.FrameCount;
				framesCount = 250;
				var multiplicity = (int)(framesCount / FramesForPreview);
				for (int i = 0; i < framesCount; i++)
				{
					if(worker.CancellationPending)
					{
						e.Cancel = true;
						logger.Debug("Canceled InitFramesList for {0} at {1} step", this, i);
						return null;
					}
					var nextFrame = videoReader.ReadVideoFrame();
					if (i % multiplicity == 0)
					{
						using (var memory = new MemoryStream())
						{
							nextFrame.Save(memory, ImageFormat.Png);
							memory.Position = 0;
							var bitmapImage = new BitmapImage();
							bitmapImage.BeginInit();
							bitmapImage.StreamSource = memory;
							bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
							bitmapImage.EndInit();
							bitmapImage.Freeze();
                          	framesList.Add(bitmapImage);
						}
					}
					worker.ReportProgress((int)(((double)i / (double)framesCount) * 100d));
					nextFrame.Dispose();
				}
			}
			else
			{
				logger.Error("File {0} does not exist! Can't read frames for event {1}", VideoFileName, this);
			}

			worker.ReportProgress(100);
			logger.Debug("Successfully finished InitFramesList for {0}", this);
			return framesList;
        }

        #endregion

		public override string ToString()
		{
			return string.Format("Warning: {0}\r\nLock: {1}\r\nSensor: {2}\r\nAccess: {3}\r\nRoom: {4}\r\nDateTime: {5}",
				this.WarningLevel,
				this.Lock,
				Enum.GetName(typeof(Sensors), this.Sensor),
				Enum.GetName(typeof(AccessLevels), this.AccessLevel),
				this.Room + 1,
				string.Format("{0:dd.MM.yyyy hh:mm:ss}", this.DateTime));
		}
    }
}
