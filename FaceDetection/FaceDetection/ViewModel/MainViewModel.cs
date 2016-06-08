﻿using Emgu.CV;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using FrameDetection.Model;
using GalaSoft.MvvmLight.Command;
using PostSharp.Patterns.Threading;

namespace FrameDetection.ViewModel
{
    class MainViewModel: ViewModelBase
    {
        private Bitmap _image;
        private int _selectedCam;
        private List<Camera> _availableCameras;
        private readonly CameraHandler _cameraHandler;
        private int _fps;
        private bool _progressBarShown;

        public Bitmap Image
        {
            get
            {
                return _image;
            }

            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        public int SelectedCam
        {
            get
            {
                return _selectedCam;
            }

            set
            {
                _selectedCam = value;
                RaisePropertyChanged(nameof(SelectedCam));
            }
        }

        public List<Camera> AvailableCameras
        {
            get
            {
                return _availableCameras;
            }

            set
            {
                _availableCameras = value;
                RaisePropertyChanged(nameof(AvailableCameras));
            }
        }

        public int Fps
        {
            get { return _fps; }

            set
            {
                _fps = value;
                RaisePropertyChanged(nameof(Fps));
            }
        }

        public bool ProgressBarShown
        {
            get { return _progressBarShown; }
            set
            {
                _progressBarShown = value;
                RaisePropertyChanged(nameof(ProgressBarShown));
            }
        }

        public MainViewModel()
        {
            SelectedCam = 0;
            Fps = 0;
            ProgressBarShown = true;

            _cameraHandler = new CameraHandler();
            
            RefreshCameras();

            RunCamViewer();
        }

        public RelayCommand RefreshCamerasCommand => new RelayCommand(RefreshCameras);

        [Background]
        private void RunCamViewer()
        {
            while (true)
            {
                var cam = SelectedCam;
                var frames = 0;
                var timestamp = DateTime.Now;

                Capture capture = null;
                Image = null;
                Fps = 0;

                if (cam != -1)
                {
                    ProgressBarShown = true;
                    try
                    {
                        capture = new Capture(SelectedCam);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Couldn't read from camera input: " + SelectedCam);
                    }
                }
                
                while (cam == SelectedCam)
                {
                    if(cam != -1)
                    {
                        try
                        {
                            ProgressBarShown = false;
                            var frame = capture?.QueryFrame();
                            Image = frame?.Bitmap;

                            frames++;

                            if ((DateTime.Now.Subtract(timestamp).Ticks / TimeSpan.TicksPerMillisecond) > 1000)
                            {
                                Fps = frames;
                                frames = 0;
                                timestamp = DateTime.Now;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error reading frame: " + ex);
                        }
                    }

                    Thread.Sleep(20);
                }

                capture?.Dispose();

                Debug.WriteLine("Camera selection changed: " + SelectedCam);
            }
        }

        private void RefreshCameras()
        {
            AvailableCameras = _cameraHandler.GetAllCameras();
        }
    }
}
