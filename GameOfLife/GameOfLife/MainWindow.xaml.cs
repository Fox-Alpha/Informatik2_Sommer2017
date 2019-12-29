using System;
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

namespace GameOfLife
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        DispatcherTimer timer;
        private Random randomizer = new Random(197702);
        const int anzahlZellenBreit = 30;
        const int anzahlZellenHoch = 30;
        Rectangle[,] felder = new Rectangle[anzahlZellenHoch, anzahlZellenBreit];
        private int maxGenerationCount;
        public int MaxGenerationCount { get { return maxGenerationCount;  }
            set
            {
                maxGenerationCount = value;
                this.NotifyPropertyChanged("MaxGenerationCount");
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

        private int countTurn;

        public int CountTurn
        {
            get { return countTurn; }
            set {
                countTurn = value;
                this.NotifyPropertyChanged("CountTurn");
            }
        }

       

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += Timer_Tick;

            this.DataContext = this;

            CountDeadEntity = 0;
            CountAliveEntity = 0;
            CountTurn = 0;
            //MaxGenerationCount = 100;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RandomizeField();
        }

        private void RandomizeField()
        {
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
                }
            }

            CountTurn++;

            if (cbMaxGen.IsChecked == true && CountTurn >= MaxGenerationCount)
            {
                EnableTimer();
            }
        }

        private void CountNeightbors(ref int[,] anzahlNachbarn, int i, int j)
        {
            int iDarüber = i - 1;
            if (iDarüber < 0)
            { iDarüber = anzahlZellenHoch - 1; }
            int iDarunter = i + 1;
            if (iDarunter >= anzahlZellenHoch)
            { iDarunter = 0; }
            int jLinks = j - 1;
            if (jLinks < 0)
            { jLinks = anzahlZellenBreit - 1; }
            int jRechts = j + 1;
            if (jRechts >= anzahlZellenBreit)
            { jRechts = 0; }

            int nachbarn = 0;

            if (felder[iDarüber, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarüber, j].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarüber, jRechts].Fill == entityAlive)
            { nachbarn++; }
            if (felder[i, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[i, jRechts].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarunter, jLinks].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarunter, j].Fill == entityAlive)
            { nachbarn++; }
            if (felder[iDarunter, jRechts].Fill == entityAlive)
            { nachbarn++; }

            anzahlNachbarn[i, j] = nachbarn;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CountTurn = 0;
            EnableTimer();
        }

        private void EnableTimer(bool an = true)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                buttonStartStop.Content = "Starte Animation!";
            }
            else
            {
                timer.Start();
                buttonStartStop.Content = "Stoppe Animation!";
            }
        }


        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
