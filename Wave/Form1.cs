using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Un4seen.Bass;

namespace Wave
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        int n;
        bool f;
        string FileNAme = "";
        int chan = 0;//номер потока
        Single[] fft = null;//массив данных спектра
        private void button1_Click(object sender, EventArgs e)
        {
            n = Bass.BASS_GetDevice();//получаем устройство по умолчанию
            if (Bass.BASS_Init(n, 44100, 0, IntPtr.Zero) == false) //попытка инициализации
            {
                MessageBox.Show("BASS_Init failed");
                return;
            }
            if (FileNAme != "") // Если подобрали свой файл
            {
                chan = Bass.BASS_StreamCreateFile(@FileNAme,
                    0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_LOOP);//создаем поток в режиме FLOAT
            }
            else // Иначе откроем файл для визуализации по умолчанию.
            {
                chan = Bass.BASS_StreamCreateFile(@"C:\MP3\file.mp3",
                     0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_SAMPLE_LOOP);//создаем поток в режиме FLOAT
            }
    
            if (chan == 0)
            {
                MessageBox.Show("BASS_StreamCreateFile failed");
                return;
            }

            f = Bass.BASS_ChannelPlay(chan, false);//воспроизводим поток
            if (f == false)
            {
                MessageBox.Show("BASS_ChannelPlay failed");
                return;
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (fft == null) return;

            PointF p1, p2;
            float max_y = 0;//максимальное значение амплитуды
            float min_y = Single.MaxValue;//минимальное значение амплитуды
            float max_x = fft.Length - 1;//максимальная частота, для которой амплитуда ненулевая

            int i = 0;
            max_x = 0;
            foreach (float f in fft)//находим максимальные и минимальные значения
            {
                if (f > max_y) max_y = f;
                if (f < min_y) min_y = f;

                if (f > 0.001f) max_x = (float)i;
                i++;
            }

            /*меняем направление оси Y, чтобы она смотрела вверх*/
            e.Graphics.ScaleTransform(1.0f, -1.0f);
            e.Graphics.TranslateTransform(0.0f, -1.0f * panel1.Height);

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            float y;
            p1 = new PointF(0.0f, 0.0f);//начальная точка

            for (float x = 1; x <= max_x; x++)
            {
                y = fft[(int)x];

                /*вычисляем координату следующей точки по относительной амплитуде*/
                p2 = new PointF((x / max_x) * panel1.Width, panel1.Height * (y - min_y) / (max_y - min_y));

                path.AddLine(p1, p2);//добавляем линию в график
                p1 = p2;
            }
            e.Graphics.DrawPath(Pens.Black, path);
        }

        private void timer1_Tick(object sender, EventArgs e)//запускаем каждые 500 мс
        {
            if (chan == 0) return;
            if (Bass.BASS_ChannelIsActive(chan) != BASSActive.BASS_ACTIVE_PLAYING) return;

            fft = new Single[2048];//выделяем массив для данных            
            Bass.BASS_ChannelGetData(chan, fft, (int)BASSData.BASS_DATA_FFT4096);//получаем спектр потока
            fft[0] = 0.0f;//избавляемся от постоянной составляющей            

            panel1.Refresh();//перерисовка графика
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            Bass.BASS_ChannelStop(chan); //останавливаем музяку
            Bass.BASS_Free();// выгружаем устройства потоки итд.
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd1 = new OpenFileDialog();
            if(fd1.ShowDialog().Equals(DialogResult.OK))
            {
                FileNAme = fd1.FileName;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
