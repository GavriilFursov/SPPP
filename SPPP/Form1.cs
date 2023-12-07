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

namespace SPPP
{
    public partial class MainForm : Form
    {
        bool isConnected = false; // Флаг проверки подключения

        public MainForm()
        {
            InitializeComponent();
        }

        // Кнопка подключиться
        private void buttonConnect_Click(object sender, EventArgs e)
        {
             if(!isConnected) //Проверка флага, если false, подключаемся к плате управления, если true отключаемся
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

        // Функция отключения от платы управления
        private void disconnectFromArduino()
        {
            if(isConnected)
            {
                isConnected = false; // Меняем флаг
                conectPort.Close(); // Закрываем порт
                buttonConnect.Text = "Подключиться"; // Меняем текст кнопки
            }
        }

        // Функция подключения / отключения от платы управления
        private void connectOrDisconnectToArduino()
        {
            if(!isConnected) 
            {
                string arduinoPort = detectArduinoPort(); // Записываем порт, к которому подключена плата упраления
                if(!string.IsNullOrEmpty(arduinoPort)) // Если плата управления обнаружена, то подлючаемся
                {
                    isConnected = true; // Меняем флаг подключения
                    conectPort.PortName = arduinoPort; // Присваем название порта
                    conectPort.DataReceived += new SerialDataReceivedEventHandler(dataReceivedHandler);
                    conectPort.Open(); // Открываем порт
                    buttonConnect.Text = "Отключиться"; // Меняем текст кнопки
                }
                // Если плата управления не обнаружена
                else
                {
                    MessageBox.Show("Плата управления не обнаружена. Убедитесь, что она подключена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // Если флаг подключения true, отключаемся
            else
            {
                disconnectFromArduino(); // Функция для отключения от платы управления
            }
        }

        private void dataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

        }

        // Функция для обнаружения платы управления
        private string detectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);
            try
            {
                foreach(ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Arduino"))
                    {
                        return deviceId;
                    }
                }
            }
            catch (ManagementException) 
            {

            }
            return null;
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

        // Функция для установки состояния видимости кнопок и их активации, в зависимости от флага.
        private void condition()
        {
            if (isConnected)
            {
                buttonCondition.Text = "Успешное подключение"; // Меняем текст кнопки состояния
                buttonCondition.BackColor = Color.Green; // Меняем цвет кнопки состояния
                buttonLoad.Visible = true; // Активируем отображение кнопки загрузка
                buttonLoad.Enabled = true; // Активируем нажатие на кнопку загрузка
                buttonGenerateReport.Visible = true; // Активируем отображение кнопки сформировать отчет
                buttonGenerateReport.Enabled = true; // Активируем нажатие на кнопку сформировать отчет
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
            }
        }
    }
}
