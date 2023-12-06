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
                ConnectOrDisconnectToArduino(); // Функция подключения / отключения от платы управления
            }
            else
            {
                disconnectFromArduino(); // Функция отключения от платы управления
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
        private void ConnectOrDisconnectToArduino()
        {
            if(!isConnected) 
            {
                string arduinoPort = DetectArduinoPort(); // Записываем порт, к которому подключена плата упраления
                if(!string.IsNullOrEmpty(arduinoPort)) // Если плата управления обнаружена, то подлючаемся
                {
                    isConnected = true; // Меняем флаг подключения
                    conectPort.PortName = arduinoPort; // Присваем название порта
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

        // Функция для обнаружения платы управления
        private string DetectArduinoPort()
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
    }
}
