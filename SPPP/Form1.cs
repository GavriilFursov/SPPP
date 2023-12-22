﻿using System;
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
using System.Net.NetworkInformation;

namespace SPPP
{
    public partial class MainForm : Form
    {
        public string startWord = "Start"; // Стартовое слово, для инициализации платы и определения типа стенда
        public string typeOfStand; // Запоминаем тип стенда, для выбора конкретного шаблона
        bool isConnected = false; // Флаг проверки соединения
        private bool isLoadData; // Флаг загрузки данных    
        private bool isReadingData; // Флаг чтения данных
        private string[] sensorData = new string[2]; // Инициализация массива байтов
        private string startMarker = "$"; // Стартовый маркер для приема данных 
        private string endMarker = "%"; // Конечный маркер для приема данных

        public MainForm()
        {
            InitializeComponent(); // Инициализация
            timer1 = new System.Windows.Forms.Timer(); // Инициализация таймера
            timer1.Interval = 2000; // Время срабатывани
            timer1.Tick += timerTick; // Тикаем до времени срабатывания
            textBoxForCondition.Enabled = false; // Делаем текстбокс недоступным
        }

        // Функция для запуска таймера
        private void timerTick(object sender, EventArgs e)
        {
            timer1.Stop(); // Стоп таймер
            isLoadData = false; // Обнуляем флаг приема данных
            isReadingData = false; // Обнуляем флаг чтения данных
            disconnectFromArduino(); // Отключаемся от платы
            condition(); // Возвращаемся в начальное меню
            DialogResult dialogResult = MessageBox.Show("Проверьте подключение стенда, выполните перезагрузку устройства.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.OK) 
            {
                System.Windows.Forms.Application.Restart(); // Перезагружаем интерфейс, если жмем ок
            }
        }

        // Кнопка подключиться
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected) // Проверка флага, если false, подключаемся к плате управления, если true отключаемся
            {
                connectorDisconnectToArduino(); // Функция подключения / отключения от платы управления
                condition(); // Функция изменения цвета кнопки состояние подключения
            }
            else
            {
                disconnectFromArduino(); // Функция отключения от платы управления
                condition(); // Функция изменения цвета кнопки состояние подключения
            }
        }

        // Функция подключения / отключения от платы управления
        private void connectorDisconnectToArduino() 
        {
            if (!isConnected) // Проверка флага на отсутствующее подключение
            {
                string arduinoPort = findPortAndDetermineTypeOfStand(); // Определяем порт и тип стенда
                if (!string.IsNullOrEmpty(arduinoPort))
                {
                    isConnected = true; // Флаг - соединение установлено
                    connectPort.PortName = arduinoPort; // Присваиваем в название порта, найденный порт
                    connectPort.DataReceived += new SerialDataReceivedEventHandler(dataReceivedHandler); // Делегат
                    connectPort.Open(); // Открываем порт   
                    timer1.Stop(); // Останавливаем таймер
                    buttonConnect.Text = "Отключиться"; // Меняем статус кнопки на "отключиться"
                }
                else
                {
                    MessageBox.Show("Проверьте подключение стенда, выполните перезагрузку устройства.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                disconnectFromArduino(); // Отлючаемся от платы, если были подключены
            }
        }

        // Функция отключения от платы управления
        private void disconnectFromArduino()
        {
            if (isConnected) // Проверяем флаг
            {
                isConnected = false; // Меняем флаг
                connectPort.Close(); // Закрываем порт
                buttonConnect.Text = "Подключиться"; // Меняем текст кнопки
                textBox1.Text = null; // Обнуляем текст бокс
                textBox2.Text = null; // Обнуляем текст бокс
                textBox3.Text = null; // Обнуляем текст бокс
            }
        }
        // Функция для получения названия порта и определение типа стенда
        private string findPortAndDetermineTypeOfStand()
        {
            List<string> listPorts = SerialPort.GetPortNames().ToList(); // Заполяем массив листов доступными портами
            try
            {
                foreach (string port in listPorts)
                {
                    connectPort.PortName = port; // Присваем в название порта доступной порт
                    connectPort.Open(); // Открываем порт
                    if (port != "COM1")
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            connectPort.WriteLine(startWord); // Отправляем стартовое слово на МК
                            Thread.Sleep(200);
                            timer1.Start();
                            if (connectPort.BytesToRead > 0) // Если получили в ответ хоть что-то, то заходим
                            {
                                string flag = connectPort.ReadLine(); // Считываем принятое значение
                                typeOfStand = flag.Trim(); // Присваем тип стенда в глобальную переменную
                                if (flag.Trim().Equals("0")) // СРПП
                                {
                                    label1.Visible = true; // Включаем отображение
                                    label2.Visible = true; // Включаем отображение
                                    label3.Visible = true; // Включаем отображение
                                    label1.Text = "Серийный номер изделия:"; // Меняем текст лейбл1
                                    label2.Text = "Продолжительность испытания:"; // Меняем текст лейбл2
                                    label3.Text = "Количество циклов испытания:"; // Меняем текст лейбл3
                                    textBox1.Visible = true; // Включаем отображение
                                    textBox2.Visible = true; // Включаем отображение
                                    textBox3.Visible = true; // Включаем отображение
                                    labelTypeOfStand.Text = "Подключенный стенд: СРПП"; // Отображаем тип стенда
                                }
                                else if (flag.Trim().Equals("1")) // СППП
                                {
                                    label1.Visible = true; // Включаем отображение
                                    label2.Visible = true; // Включаем отображение
                                    label3.Visible = true; // Включаем отображение
                                    label1.Text = "Серийный номер изделия:"; // Меняем текст лейбл1
                                    label2.Text = "Продолжительность испытания:"; // Меняем текст лейбл2 
                                    label3.Text = "Количество циклов испытания:"; // Меняем текст лейбл3
                                    textBox1.Visible = true; // Включаем отображение
                                    textBox2.Visible = true; // Включаем отображение
                                    textBox3.Visible = true; // Включаем отображение
                                    labelTypeOfStand.Text = "Подключенный стенд: СППП"; // Отображаем тип стенда
                                }
                                else if (flag.Trim().Equals("2")) // СРСП
                                {
                                    label1.Visible = true; // Включаем отображение
                                    label2.Visible = true; // Включаем отображение
                                    label3.Visible = true; // Включаем отображение
                                    label1.Text = "Серийный номер изделия:"; // Меняем текст
                                    label2.Text = "Продолжительность испытания:"; // Меняем текст
                                    label3.Text = "Количество циклов испытания:"; // Меняем текст
                                    textBox1.Visible = true; // Включаем отображение
                                    textBox2.Visible = true; // Включаем отображение
                                    textBox3.Visible = true; // Включаем отображение
                                    labelTypeOfStand.Text = "Подключенный стенд: СРСП"; // Отображаем тип стенда
                                }
                                else if (flag.Trim().Equals("3")) // СПСП
                                {
                                    label1.Visible = true; // Включаем отображение
                                    label2.Visible = true; // Включаем отображение
                                    label3.Visible = true; // Включаем отображение
                                    label1.Text = "Серийный номер изделия:"; // Меняем текст
                                    label2.Text = "Продолжительность испытания:"; // Меняем текст
                                    label3.Text = "Количество циклов испытания:"; // Меняем текст
                                    textBox1.Visible = true; // Включаем отображение
                                    textBox2.Visible = true; // Включаем отображение
                                    textBox3.Visible = true; // Включаем отображение
                                    labelTypeOfStand.Text = "Подключенный стенд: СПСП"; // Отображаем тип стенда
                                }
                                connectPort.Close(); // Закрываем порт
                                return port; // Возвращаем значение порта
                            }
                            else
                            {
                                continue; // Продолжаем поиск
                            }
                        }
                        return null;
                    }
                    connectPort.Close(); // Закрываем порт
                }
            }
            catch (Exception)
            {
                connectPort.Close(); // При возникновении ошибки, закрываем порт
            }
            return null; 
        }

        // Добавляем нули в поле время.
        private string addZero(int input)
        {
            String output = "";
            if (input >= 10)
            {
                output += input;
            }
            else
            {
                output += "0";
                output += input;
            }
            return output;
        }


        // Функция для установки состояния видимости кнопок и их активации, в зависимости от флага.
        private void condition()
        {
            if (isConnected) // Если подключены
            {
                textBoxForCondition.Text = "Успешное подключение"; // Меняем текст кнопки состояния
                buttonLoad.Visible = true; // Активируем отображение кнопки загрузка
                buttonLoad.Enabled = true; // Активируем нажатие на кнопку загрузка
                //buttonGenerateReport.Visible = true; // Активируем отображение кнопки сформировать отчет
                //buttonGenerateReport.Enabled = true; // Активируем нажатие на кнопку сформировать отчет
                buttonInformation.Visible = false; // Деактивируем визуальное отображение кнопки информация
                buttonInformation.Enabled = false; // Деактивируем нажатие на кнопку информация
                labelTypeOfStand.Visible = true; // Включаем отображение типа стенда
                textBox2.Enabled = false; // Закрываем доступ к редактированию
                textBox3.Enabled = false; // Закрываем доступ к редактированию
            }
            else
            {
                textBoxForCondition.Text = "Нет подключения"; // Меняем текст кнопки состояния
                buttonLoad.Visible = false; // Активируем отображение кнопки загрузка 
                buttonLoad.Enabled = false; // Активируем нажатие на кнопку загрузка
                buttonGenerateReport.Visible = false; // Активируем отображение кнопки сформировать отчет
                buttonGenerateReport.Enabled = false; // Активируем нажатие на кнопку сформировать отчет
                buttonInformation.Visible = true; // Деактивируем визуальное отображение кнопки информация
                buttonInformation.Enabled = true; // Деактивируем нажатие на кнопку информация
                label1.Visible = false; // Выкл. отображения
                label2.Visible = false; // Выкл. отображения
                label3.Visible = false; // Выкл. отображения
                textBox1.Visible = false; // Выкл. отображения
                textBox2.Visible = false; // Выкл. отображения
                textBox3.Visible = false; // Выкл. отображения
                labelTypeOfStand.Visible = false; // Выкл. отображение типа стенда
            }
        }
        //System.InvalidOperationException
        private void dataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (isLoadData) // При значение флага Тру, заходим
            {
                try
                {
                    while (connectPort.BytesToRead > 0) // Пока данные получаем
                    {
                        string receivedByte = connectPort.ReadExisting(); // Считываем байты из потока

                        if (!isReadingData && receivedByte.Trim().Equals(startMarker)) // Если данные не получены и получен стартовый флаг
                        {
                            isReadingData = true; // Меняем флаг чтения данных

                            for (int i = 0; i < 2; i++)
                            {
                                Thread.Sleep(100); // Выдержка 100 мс.
                                sensorData[i] = connectPort.ReadExisting(); // Считываем пакет
                            }
                        }
                        else if (isReadingData && receivedByte.Trim().Equals(endMarker)) // Если данные получены и отправлен конечный маркер
                        {
                            timer1.Stop(); // Останавливаем таймер.
                            string one = sensorData[0].ToString(); // Парсим в стринги
                            string two = sensorData[1].ToString(); // Парсим в стринги
                            int time = int.Parse(one); // Парсим в инты
                            int hour = time / 3600; // Получаем часы
                            int minute = (time % 3600) / 60; // Получаем минуты
                            int seconds = time % 60; // Получаем секунды

                            // Проверка значений на нули
                            if (one.Trim().Equals("0") || two.Trim().Equals("0"))
                            {
                                MessageBox.Show("Отсутствует информация о выполненной процедуре.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                isReadingData = false; // Меняем флаг полученния данных
                                isLoadData = false; // Меняем флаг загрузки данных
                                return; // Принудительно останавливаем.
                            }

                            // Выводим ошибку на экран
                            MessageBox.Show("Данные успешно получены", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                            // Поток для вывода данных
                            textBox1.Invoke(new System.Action(() =>
                            {
                                textBoxForCondition.Text = "Данные получены"; // Меняем текст
                                buttonLoad.Visible = true; 
                                buttonLoad.Enabled = true;
                                textBox2.Text = addZero(hour) + ":" + addZero(minute) + ":" + addZero(seconds); // Выводим время
                                textBox3.Text = two; // Выводим циклы
                                buttonGenerateReport.Visible = true; // Отображаем
                                buttonGenerateReport.Enabled = true; // Отображаем
                            }));
                            isReadingData = false; // Меняем флаг получения данных
                            isLoadData = false; // Меняем флаг загрузки данных
                        }
                        // Если данные получены, но конечный маркер отсутствует, то выдаем ошибку
                        else if (isReadingData && receivedByte.Trim().Equals(endMarker))
                        {
                            // Выводим ошибку на экран
                            MessageBox.Show("Данные повреждены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (InvalidOperationException) // Исключение при отсутствии подключения
                {
                    // Выводим ошибку на экран
                    MessageBox.Show("Потеряно соединение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Кнопка информация
        private void buttonInformation_Click(object sender, EventArgs e)
        {
            string message = "Прикладное программное обеспечение\n" +
                "для формирования отчетной документации\n" +
                "Версия ППО: 1.0 от 20.12.2023\n" +
                "Разработчик: ЮЗГУ НИЛ 'МиР'\n" +
                "Номер телефона: +7(4712)22-26-26\n" +
                "Email: lab.swsu@gmail.com";

            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        // Функция для полученния данных с МК
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (!isLoadData) // Проверяем флаг
            {
                if (!connectPort.IsOpen) // Проверяем порт, что он открыт и доступен
                {
                    // Выводим ошибку на экран
                    MessageBox.Show("Проверьте подключения стенда.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    disconnectFromArduino(); // Отключаемся от МК
                    condition(); // Возвращаемся к стартовому виду интерфейса
                }
                else
                {
                    connectPort.WriteLine("Load"); // Отпрляем флаг на МК
                    isLoadData = true; //  Меняем флаг
                    timer1.Start(); // Запускаем таймер
                }
            }
        }

        private void buttonGenerateReport_Click(object sender, EventArgs e)
        {
            if (!connectPort.IsOpen) // Если порт закрыт и недоступен
            {
                // Выводим ошибку на экран
                MessageBox.Show("Проверьте подключения стенда.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                disconnectFromArduino(); // Отключаемся от МК
                condition(); // Возвращаемся к стартовому виду интерфейса
            }
            else if (string.IsNullOrEmpty(textBox1.Text)) // Если поле серийный номер = null
            {
                // Выводим ошибку на экран
                MessageBox.Show("Введите серийный номер изделия.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                saveFileDialog1 = new SaveFileDialog(); // Инициализация сохранения файла
                saveFileDialog1.Filter = "Word document|*.docx";  // Фильтр для ворд файлов
                saveFileDialog1.Title = "Выберите путь сохранения"; // Пусть сохранения указываем
                if (DialogResult.OK == saveFileDialog1.ShowDialog()) // Показываем меню, для выбора пути сохранения файла
                {
                    string location = saveFileDialog1.FileName;  // Присваиваем имя файла
                    WordHelper helper = new WordHelper(); // Инициализация класса
                    if (helper.WordTemplate(DateTime.Now.ToString(), textBox1.Text, textBox2.Text, textBox3.Text, location, typeOfStand))
                    {
                        // Выводим уведомление, что все прошло успешно
                        MessageBox.Show("Отчёт сформирован", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        textBoxForCondition.Text = "Отчет сформирован"; // Выводим статус.
                    }
                    else
                    {
                        // Выводим ошибку, если появилась ошибки при сохранении, создании или редактирование файла.
                        MessageBox.Show("Что-то пошло не так. Повторите попытку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}