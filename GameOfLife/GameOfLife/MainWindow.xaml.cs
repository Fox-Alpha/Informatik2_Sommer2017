﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace GameOfLife
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public DispatcherTimer updateTimer, runtimeTimer;
        private Random randomizer = new Random();

        const int anzahlZellenBreit = 30;
        const int anzahlZellenHoch = 30;

        private int maxAnzahlFelder = 0;
        public int MaxAnzahlFelder { get => maxAnzahlFelder;
            set
            {
                maxAnzahlFelder = value;
                this.NotifyPropertyChanged("MaxAnzahlFelder");
            }
        }

        Rectangle[,] felder = new Rectangle[anzahlZellenHoch, anzahlZellenBreit];

        private int maxGenerationCount;
        public int MaxGenerationCount { get { return maxGenerationCount;  }
            set
            {
                maxGenerationCount = value;
                this.NotifyPropertyChanged("MaxGenerationCount");
            }
        }

        private bool enableOptions;
        public bool EnableOptions { get => enableOptions;
            set {
                    enableOptions = value;
                this.NotifyPropertyChanged("enableOptions");
            }
        }

        private Brush entityDead = Brushes.Cyan;
        private Brush entityAlive = Brushes.Green;

        public event PropertyChangedEventHandler PropertyChanged;

        private int countAliveEntity;
        public int CountAliveEntity {
            get
            {
                return countAliveEntity;
            }
            set
            {
                countAliveEntity = value;
                this.NotifyPropertyChanged("CountAliveEntity");
            }
        }

        private int countDeadEntity;
        public int CountDeadEntity
        {
            get
            {
                return countDeadEntity;
            }
            set
            {
                countDeadEntity = value;
                this.NotifyPropertyChanged("CountDeadEntity");
            }
        }

        private int currentGHenerationTurn;        
        public int CurrentGHenerationTurn
        {
            get { return currentGHenerationTurn; }
            set {
                currentGHenerationTurn = value;
                this.NotifyPropertyChanged("CurrentGHenerationTurn");
            }
        }

        private double RuntimeStart;
        private TimeSpan runtime;
        public TimeSpan Runtime { get => runtime; set { runtime = value; this.NotifyPropertyChanged("Runtime"); } }

        public MainWindow()
        {
            InitializeComponent();

            updateTimer = new DispatcherTimer();
            runtimeTimer = new DispatcherTimer();

            updateTimer.Interval = TimeSpan.FromSeconds(0.033);
            updateTimer.Tick += Timer_Tick;

            runtimeTimer.Interval = TimeSpan.FromSeconds(1);
            runtimeTimer.Tick += RunTimer_Tick;

            this.DataContext = this;

            CountDeadEntity = 0;
            CountAliveEntity = 0;
            CurrentGHenerationTurn = 0;
            MaxAnzahlFelder = anzahlZellenBreit * anzahlZellenHoch;

            EnableOptions = true;
        }

        private void RunTimer_Tick(object sender, EventArgs e)
        {
            Runtime = TimeSpan.FromMilliseconds(GetUnixTimeStampMilliseconds()- RuntimeStart);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //RandomizeField();
            InitFields();

        }
        private void InitFields()
        {
            for (int i = 0; i < anzahlZellenHoch; i++)
            {
                for (int j = 0; j < anzahlZellenBreit; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = zeichenfläche.ActualWidth / anzahlZellenBreit - 2.0;
                    r.Height = zeichenfläche.ActualHeight / anzahlZellenHoch - 2.0;

                    r.Fill = Brushes.Aqua;
                    zeichenfläche.Children.Add(r);
                    Canvas.SetLeft(r, j * zeichenfläche.ActualWidth / anzahlZellenBreit);
                    Canvas.SetTop(r, i * zeichenfläche.ActualHeight / anzahlZellenHoch);
                    r.MouseDown += R_MouseDown;
                }
            }
        }

        private void RandomizeField()
        {
            zeichenfläche.Children.Clear();

            double unixTimestamp = (double)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            randomizer = new Random((int)unixTimestamp);

            for (int i = 0; i < anzahlZellenHoch; i++)
            {
                for (int j = 0; j < anzahlZellenBreit; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = zeichenfläche.ActualWidth / anzahlZellenBreit - 2.0;
                    r.Height = zeichenfläche.ActualHeight / anzahlZellenHoch - 2.0;

                    int rnd = randomizer.Next(0, 100);

                    if (rnd <= 45)
                    {
                        r.Fill = entityDead;
                        CountDeadEntity++;
                    }
                    else if (rnd > 75)
                    {
                        r.Fill = entityAlive;
                        CountAliveEntity++;
                    }
                    else
                    {
                        r.Fill = (randomizer.Next(0, 2) == 1) ? entityDead : entityAlive;

                        if (r.Fill == entityDead)
                        {
                            CountDeadEntity++;
                        }
                        else
                            CountAliveEntity++;
                    }

                    zeichenfläche.Children.Add(r);
                    Canvas.SetLeft(r, j * zeichenfläche.ActualWidth / anzahlZellenBreit);
                    Canvas.SetTop(r, i * zeichenfläche.ActualHeight / anzahlZellenHoch);
                    r.MouseDown += R_MouseDown;

                    felder[i, j] = r;
                }
            }
        }

        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle r = sender as Rectangle;
                if (r.Fill == Brushes.Green)
                {
                    r.Fill = entityDead;
                }
                else
                    r.Fill = entityAlive;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int[,] anzahlNachbarn = new int[anzahlZellenHoch, anzahlZellenBreit];
            for (int i = 0; i < anzahlZellenHoch; i++)
            {
                for (int j = 0; j < anzahlZellenBreit; j++)
                {
                    CountNeightbors(ref anzahlNachbarn, i, j);
                }
            }

            CountAliveEntity = 0;
            CountDeadEntity = 0;

            for (int i = 0; i < anzahlZellenHoch; i++)
            {
                for (int j = 0; j < anzahlZellenBreit; j++)
                {
                    if(anzahlNachbarn[i, j] < 2 || anzahlNachbarn[i, j] > 3)
                    {
                        felder[i, j].Fill = entityDead;
                        CountDeadEntity++;
                    }
                    else if (anzahlNachbarn[i, j] == 3)
                    {
                        felder[i, j].Fill = entityAlive;
                        CountAliveEntity++;
                    }
                    else
                    {
                        if (felder[i, j].Fill == entityDead)
                        {
                            CountDeadEntity++;
                        }
                        else
                        {
                            CountAliveEntity++;
                        }
                    }
                }
            }

            CurrentGHenerationTurn++;

            //SeriesCollection[0].Values.Add((double)CountAliveEntity);
            //SeriesCollection[1].Values.Add((double)CountDeadEntity);

            if (cbMaxGen.IsChecked == true && CurrentGHenerationTurn >= MaxGenerationCount)
            {
                EnableTimer();
            }
        }

        private void CountNeightbors(ref int[,] anzahlNachbarn, int X, int Y)
        {
            int iDarüber = X - 1;
            if (iDarüber < 0)
            { iDarüber = anzahlZellenHoch - 1; }

            int iDarunter = X + 1;
            if (iDarunter >= anzahlZellenHoch)
            { iDarunter = 0; }

            int jLinks = Y - 1;
            if (jLinks < 0)
            { jLinks = anzahlZellenBreit - 1; }

            int jRechts = Y + 1;
            if (jRechts >= anzahlZellenBreit)
            { jRechts = 0; }

            int nachbarn = 0;

            if (felder[iDarüber, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarüber, Y].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarüber, jRechts].Fill == entityAlive)
            { nachbarn++; }

            if (felder[X, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[X, jRechts].Fill == entityAlive)
            { nachbarn++; }

            if (felder[iDarunter, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarunter, Y].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarunter, jRechts].Fill == entityAlive)
            { nachbarn++; }

            anzahlNachbarn[X, Y] = nachbarn;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!updateTimer.IsEnabled)
            {
                CurrentGHenerationTurn = 0;
                RandomizeField();

                RuntimeStart = GetUnixTimeStampMilliseconds();
                Runtime = TimeSpan.FromMilliseconds(GetUnixTimeStampMilliseconds() - RuntimeStart);
            }

            EnableTimer();
        }

        private static double GetUnixTimeStampMilliseconds()
        {
            return (double)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        private void EnableTimer(bool an = true)
        {
            if (updateTimer.IsEnabled)
            {
                updateTimer.Stop();
                buttonStartStop.Content = "Starte Animation!";
            }
            else
            {
                updateTimer.Start();
                buttonStartStop.Content = "Stoppe Animation!";
            }

            EnableOptions = !updateTimer.IsEnabled;
            runtimeTimer.IsEnabled = updateTimer.IsEnabled;
        }
        
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void RandomField_Click(object sender, RoutedEventArgs e)
        {
            CountDeadEntity = 0;
            CountAliveEntity = 0;
            CurrentGHenerationTurn = 0;
            RandomizeField();
        }
    }
}
