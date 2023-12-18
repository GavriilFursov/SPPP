using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;
using System.Threading;
using Microsoft.Office.Interop.Word;
using System.Timers;

namespace SPPP
{
    public partial class MainForm : Form
    {
        private List<string> listPorts; //Лист портов
        public string startWord = "Start"; // Стартовое слово, для инициализации платы и определения типа стенда
        public string typeOfStand; // Запоминаем тип стенда, для выбора конкретного шаблона
        bool isConnected = false; // Флаг проверки соединения
        private bool isLoadData;
        private bool isReadingData;
        private byte[] sensorData = new byte[2];
        private byte startMarker = 0xA0;
        private byte endMarker = 0xC0;

        public MainForm()
        {
            InitializeComponent();
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 2000;
            timer1.Tick += timerTick;
        }

        private void timerTick(object sender, EventArgs e)
        {
            timer1.Stop();
            isLoadData = false;
            isReadingData = false;
            MessageBox.Show("Превышено время ожидания пакета, проверьте соединение со стендом");
        }

        // Кнопка подключиться
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected) // Проверка флага, если false, подключаемся к плате управления, если true отключаемся
            {
                connectOrDisconnectToArduino(); // Функция подключения / отключения от платы управления
                condition(); // Функция изменения цвета кнопки состояние подключения
            }
            else
            {
                disconnectFromArduino(); // Функция отключения от платы управления
                condition(); // Функция изменения цвета кнопки состояние подключения
            }
        }

        // Функция подключения / отключения от платы управления
        private void connectOrDisconnectToArduino()
        {
            if (!isConnected)
            {
                string arduinoPort = findPortAndDeterminningTypeOfStand(); // Записываем порт, к которому подключена плата упраления
                if (!string.IsNullOrEmpty(arduinoPort)) // Если плата управления обнаружена, то подлючаемся
                {
                    isConnected = true; // Меняем флаг подключения
                    connectPort.PortName = arduinoPort; // Присваем название порта
                    connectPort.DataReceived += new SerialDataReceivedEventHandler(dataReceivedHandler); // Делегат
                    connectPort.Open(); // Открываем порт
                    buttonConnect.Text = "Отключиться"; // Меняем текст кнопки
                }
                // Если плата управления не обнаружена
                else
                {
                    MessageBox.Show("Плата управления не обнаружена. Убедитесь, что она подключена.");
                }
            }
            // Если флаг подключения true, отключаемся
            else
            {
                disconnectFromArduino(); // Функция для отключения от платы управления
            }
        }

        // Функция отключения от платы управления
        private void disconnectFromArduino()
        {
            if (isConnected)
            {
                isConnected = false; // Меняем флаг
                connectPort.Close(); // Закрываем порт
                buttonConnect.Text = "Подключиться"; // Меняем текст кнопки
                textBox1.Text = null; // Обнуляем текст бокс
                textBox2.Text = null; // Обнуляем текст бокс
                textBox3.Text = null; // Обнуляем текст бокс
            }
        }

        private string findPortAndDeterminningTypeOfStand()
        {
            listPorts = SerialPort.GetPortNames().ToList();
            try
            {
                foreach (string port in listPorts)
                {
                    connectPort.PortName = port;
                    connectPort.Open();
                    if (port != "COM1")
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            connectPort.WriteLine(startWord);
                            if (connectPort.BytesToRead > 0)
                            {
                                string flag = connectPort.ReadLine();
                                typeOfStand = flag;
                                if (flag.Trim().Equals("0"))
                                {
                                    label1.Visible = true;
                                    label2.Visible = true;
                                    label3.Visible = true;
                                    label1.Text = "Серийный номер изделия:";
                                    label2.Text = "Продолжительность испытания:";
                                    label3.Text = "Количество циклов испытания:";
                                    textBox1.Visible = true;
                                    textBox2.Visible = true;
                                    textBox3.Visible = true;
                                }
                                else if (flag.Trim().Equals("1"))
                                {
                                    label1.Visible = true;
                                    label2.Visible = true;
                                    label3.Visible = true;
                                    label1.Text = "Zalupa";
                                    label2.Text = "Anus";
                                    label3.Text = "Chlen";
                                    textBox1.Visible = true;
                                    textBox2.Visible = true;
                                    textBox3.Visible = true;
                                }
                                else if (flag.Trim().Equals("2"))
                                {
                                    label1.Visible = true;
                                    label2.Visible = true;
                                    label3.Visible = true;
                                    label1.Text = "Zalupa";
                                    label2.Text = "Anus";
                                    label3.Text = "Chlen";
                                    textBox1.Visible = true;
                                    textBox2.Visible = true;
                                    textBox3.Visible = true;
                                }
                                else if (flag.Trim().Equals("3"))
                                {
                                    label1.Visible = true;
                                    label2.Visible = true;
                                    label3.Visible = true;
                                    label1.Text = "Zalupa";
                                    label2.Text = "Anus";
                                    label3.Text = "Chlen";
                                    textBox1.Visible = true;
                                    textBox2.Visible = true;
                                    textBox3.Visible = true;
                                }
                                connectPort.Close();
                                return port;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        return null;
                    }
                    connectPort.Close();
                }
            }
            catch
            {

            }
            return null;
        }

        // Функция для установки состояния видимости кнопок и их активации, в зависимости от флага.
        private void condition()
        {
            if (isConnected)
            {
                buttonCondition.Text = "Успешное подключение"; // Меняем текст кнопки состояния
                buttonCondition.BackColor = Color.Green; // Меняем цвет кнопки состояния
                buttonLoad.Visible = true; // Активируем отображение кнопки загрузка
                buttonLoad.Enabled = true; // Активируем нажатие на кнопку загрузка
                //buttonGenerateReport.Visible = true; // Активируем отображение кнопки сформировать отчет
                //buttonGenerateReport.Enabled = true; // Активируем нажатие на кнопку сформировать отчет
                buttonInformation.Visible = false; // Деактивируем визуальное отображение кнопки информация
                buttonInformation.Enabled = false; // Деактивируем нажатие на кнопку информация
            }
            else
            {
                buttonCondition.Text = "Нет подключения"; // Меняем текст кнопки состояния
                buttonCondition.BackColor = Color.Red; // Меняем цвет кнопки состояния
                buttonLoad.Visible = false; // Активируем отображение кнопки загрузка 
                buttonLoad.Enabled = false; // Активируем нажатие на кнопку загрузка
                buttonGenerateReport.Visible = false; // Активируем отображение кнопки сформировать отчет
                buttonGenerateReport.Enabled = false; // Активируем нажатие на кнопку сформировать отчет
                buttonInformation.Visible = true; // Деактивируем визуальное отображение кнопки информация
                buttonInformation.Enabled = true; // Деактивируем нажатие на кнопку информация
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                textBox1.Visible = false;
                textBox2.Visible = false;
                textBox3.Visible = false;
            }
        }
        //System.InvalidOperationException
        private void dataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (isLoadData)
            {
                try
                {
                    while (connectPort.BytesToRead > 0)
                    {
                        byte receivedByte = (byte)connectPort.ReadByte();

                        if (!isReadingData && receivedByte == startMarker)
                        {
                            isReadingData = true;

                            for (int i = 0; i < 2; i++)
                            {
                                sensorData[i] = (byte)connectPort.ReadByte();
                            }
                            string one = sensorData[0].ToString();
                            string two = sensorData[1].ToString();

                            if (two == "0")
                            {
                                MessageBox.Show("Калибровка не выполнена");
                                isReadingData = false;
                                isLoadData = false;
                            }
                            if (one == "0")
                            {
                                MessageBox.Show("Калибровка не выполнена");
                                isReadingData = false;
                                isLoadData = false;
                            }
                        }
                        else if (isReadingData && receivedByte == endMarker)
                        {
                            timer1.Stop();
                            MessageBox.Show("Данные успешно получены");

                            string one = sensorData[0].ToString();
                            string two = sensorData[1].ToString();

                            textBox1.Invoke(new System.Action(() =>
                            {
                                buttonCondition.Text = "Данные получены";
                                buttonLoad.Visible = true;
                                buttonLoad.Enabled = true;
                                textBox2.Text = one;
                                textBox3.Text = two;
                                buttonGenerateReport.Visible = true;
                                buttonGenerateReport.Enabled = true;
                            }));
                            isReadingData = false;
                            isLoadData = false;
                        }
                        else if (isReadingData && receivedByte != endMarker)
                        {
                            MessageBox.Show("Данные повреждены.");
                        }
                    }
                }
                catch(InvalidOperationException)
                {
                    MessageBox.Show("Потеряно соединение.");
                }
            }
        }

        // Кнопка информация
        private void buttonInformation_Click(object sender, EventArgs e)
        {
            string message = "Прикладное программное обеспечение СППП\n" + 
                "Версия ППО: 1.0 от 11.12.2023\n" +
                "Разработчик: ЮЗГУ НИЛ 'МиР'\n" +
                "Номер телефона: +7(4712)22-26-26\n" +
                "Email: lab.swsu@gmail.com";

            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (!isLoadData)
            {
                if (!connectPort.IsOpen)
                {
                    MessageBox.Show("Проверьте подключения стенда.");
                    disconnectFromArduino();
                    condition();
                }
                else
                {
                    connectPort.WriteLine("Load");
                    isLoadData = true;
                    timer1.Start();
                }
            }
        }

        private void buttonGenerateReport_Click(object sender, EventArgs e)
        {
            if (!connectPort.IsOpen)
            {
                MessageBox.Show("Проверьте подключения стенда.");
                disconnectFromArduino();
                condition();
            }
            else if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Введите серийный номер изделия.");
            }
            else
            {
                saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Word document|*.docx";
                saveFileDialog1.Title = "Выберите путь сохранения";
                if (DialogResult.OK == saveFileDialog1.ShowDialog())
                {
                    string location = saveFileDialog1.FileName;
                    WordHelper helper = new WordHelper();
                    if (helper.WordTemplate(DateTime.Now.ToString(), textBox1.Text, textBox2.Text, textBox3.Text, location, typeOfStand))
                    {
                        MessageBox.Show("Отчёт сформирован");
                    }
                    else
                    {
                        MessageBox.Show("Что-то пошло не так. Повторите попытку.");
                    }
                }
            }
        }
    }
}
