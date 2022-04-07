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
using AForge.Video;
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
        public Guid Id { get; set; }
        public WarningLevels WarningLevel { get; set; }
        public int Lock { get; set; }
        public int Sensor { get; set; }
        public int Room { get; set; }
        public DateTime DateTime { get; set; }
        public int AccessLevel { get; set; }
        public string VideoFileName 
		{ 
			get
			{
				return DateTime.Second % 2 == 0 ? "test1.mp4" : "test1.mp4";
			}
		}

        #endregion

		public Event()
		{
			Id = Guid.NewGuid();
		}
		public List<BitmapImage> InitFramesList(object sender, DoWorkEventArgs e)
		{
			logger.Debug("Strated InitFramesList for {0}", Id);
			var worker = sender as BackgroundWorker;
			var framesList = new List<BitmapImage>();
			try
			{
				if(File.Exists(VideoFileName))
				{
					var videoReader = new VideoFileReader();
					videoReader.Open(VideoFileName);
					var framesCount = videoReader.FrameCount;
					framesCount = 250;
					var multiplicity = (int)(framesCount / FramesForPreview);
					for(int i = 0; i < framesCount; i++)
					{
						if(worker.CancellationPending)
						{
							videoReader.Close();
							e.Cancel = true;
							logger.Debug("Canceled InitFramesList for {0} at {1} step", Id, i);
							return null;
						}
						var nextFrame = videoReader.ReadVideoFrame();
						if(i % multiplicity == 0)
						{
							using(var memory = new MemoryStream())
							{
								nextFrame.Save(memory, ImageFormat.Png);
								memory.Position = 0;
								var bitmapImage = new BitmapImage();
								bitmapImage.BeginInit();
								bitmapImage.StreamSource = memory;
								bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
								bitmapImage.EndInit();
								bitmapImage.Freeze();
								if(framesList.Count < FramesForPreview)
									framesList.Add(bitmapImage);
							}
						}
						logger.Trace("InitFramesList for {0} now at step {1}", Id, i);
						nextFrame.Dispose();
					}
					videoReader.Close();
				}
				else
				{
					logger.Error("File {0} does not exist! Can't read frames for event {1}", VideoFileName, Id);
				}
			}
			catch(VideoException ex)
			{
				logger.Warn("VideoException {0} occured while initialisig frames list for {1}", ex.Message, Id);
				return InitFramesList(sender, e);
			}

			logger.Debug("Successfully finished InitFramesList for {0}", Id);
			return framesList;
		}
		
		public void ParseWarningLevel(int toss)
		{
			var result = WarningLevels.Normal;
			if(toss == 200)
				result = WarningLevels.High;
			else if(toss >= 186)
				result = WarningLevels.Middle;
			//result = (WarningLevels)Enum.GetValues(typeof(WarningLevels)).GetValue(rd.Next(Enum.GetValues(typeof(WarningLevels)).GetLength(0)));
			WarningLevel = result;
		}

		public override string ToString()
		{
			return string.Format("Warning: {0}\r\nLock: {1}\r\n Sensor: {2}\r\nAccess: {3}\r\nRoom: {4}\r\nDateTime: {5:dd.MM.yyyy hh:mm tt}",
				WarningLevel,
				Lock,
				Enum.GetName(typeof(Sensors), Sensor),
				Enum.GetName(typeof(AccessLevels), AccessLevel),
				Room + 1,
				DateTime);
		}
    }
}
