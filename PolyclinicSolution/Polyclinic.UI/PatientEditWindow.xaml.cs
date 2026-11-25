using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;
using Polyclinic.Data;

namespace Polyclinic.UI;

/// <summary>
/// Окно добавления и редактирования информации о пациенте (Модуль 3).
/// </summary>
public partial class PatientEditWindow : Window
{
    private readonly PolyclinicRepository _repository;
    private readonly int? _patientId;
    private Patient? _currentPatient;

    public bool IsSaved { get; private set; }

    public PatientEditWindow(PolyclinicRepository repository, int? patientId = null)
    {
        InitializeComponent();
        _repository = repository;
        _patientId = patientId;

        // Загрузка справочных данных (список полов) и данных пациента при редактировании
        LoadInitialData();
    }

    /// <summary>
    /// Логика первичной загрузки данных из базы данных.
    /// Заполняет выпадающие списки и загружает карточку пациента при режиме редактирования.
    /// </summary>
    private void LoadInitialData()
    {
        try
        {
            // Загрузка справочника полов из БД
            var genders = _repository.GetGenders();
            GenderComboBox.ItemsSource = genders;

            if (_patientId.HasValue)
            {
                // Режим редактирования: подгрузка данных пациента по ID из БД
                Title = "Редактирование пациента";
                HeaderTextBlock.Text = "Редактирование пациента";

                _currentPatient = _repository.GetPatientById(_patientId.Value);
                if (_currentPatient != null)
                {
                    FullNameTextBox.Text = _currentPatient.FullName;
                    BirthDatePicker.SelectedDate = _currentPatient.BirthDate;
                    GenderComboBox.SelectedValue = _currentPatient.GenderId;
                    PolicyNumberTextBox.Text = _currentPatient.PolicyNumber;
                    PhoneTextBox.Text = _currentPatient.Phone;
                    EmailTextBox.Text = _currentPatient.Email;
                    AddressTextBox.Text = _currentPatient.Address;
                }
                else
                {
                    MessageBox.Show("Пациент с указанным ID не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            else
            {
                // Режим создания нового пациента
                Title = "Добавление пациента";
                HeaderTextBlock.Text = "Добавление пациента";
                BirthDatePicker.SelectedDate = DateTime.Today.AddYears(-25);
                if (genders.Count > 0)
                {
                    GenderComboBox.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            // Обработка исключений при сбое соединения с базой данных
            MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Сбой загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region Валидация в реальном времени и визуальные подсказки

    private void PolicyNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = PolicyNumberTextBox.Text.Trim();
        if (text.Length == 16 && Regex.IsMatch(text, @"^\d{16}$"))
        {
            PolicyHint.Text = "✓ Номер полиса корректен (16 цифр)";
            PolicyHint.Foreground = new SolidColorBrush(Colors.Green);
            PolicyNumberTextBox.BorderBrush = new SolidColorBrush(Colors.Green);
        }
        else
        {
            PolicyHint.Text = $"Осталось ввести цифр: {Math.Max(0, 16 - text.Length)} из 16";
            PolicyHint.Foreground = new SolidColorBrush(Color.FromRgb(217, 119, 6)); // Warning
            PolicyNumberTextBox.BorderBrush = new SolidColorBrush(Color.FromRgb(217, 119, 6));
        }
    }

    private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = PhoneTextBox.Text.Trim();
        if (Regex.IsMatch(text, @"^\+?[0-9\s\-\(\)]{10,20}$"))
        {
            PhoneHint.Text = "✓ Формат телефона корректен";
            PhoneHint.Foreground = new SolidColorBrush(Colors.Green);
            PhoneTextBox.BorderBrush = new SolidColorBrush(Colors.Green);
        }
        else
        {
            PhoneHint.Text = "Введите номер в формате: +7 (999) 000-00-00";
            PhoneHint.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            PhoneTextBox.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        }
    }

    private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = EmailTextBox.Text.Trim();
        if (Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            EmailHint.Text = "✓ Email корректен";
            EmailHint.Foreground = new SolidColorBrush(Colors.Green);
            EmailTextBox.BorderBrush = new SolidColorBrush(Colors.Green);
        }
        else
        {
            EmailHint.Text = "Ожидается формат: ivanov@domain.ru";
            EmailHint.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            EmailTextBox.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        }
    }

    private void BirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BirthDatePicker.SelectedDate.HasValue)
        {
            if (BirthDatePicker.SelectedDate.Value > DateTime.Today)
            {
                BirthDateHint.Text = "⚠ Дата рождения не может быть в будущем!";
                BirthDateHint.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            }
            else
            {
                var age = DateTime.Today.Year - BirthDatePicker.SelectedDate.Value.Year;
                BirthDateHint.Text = $"✓ Возраст пациента: {age} лет";
                BirthDateHint.Foreground = new SolidColorBrush(Colors.Green);
            }
        }
    }

    private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    private void Input_TextChanged(object sender, TextChangedEventArgs e) { }

    #endregion

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var patient = _currentPatient ?? new Patient();
        patient.FullName = FullNameTextBox.Text.Trim();
        patient.BirthDate = BirthDatePicker.SelectedDate ?? DateTime.MinValue;
        patient.GenderId = GenderComboBox.SelectedValue is int gid ? gid : 0;
        patient.PolicyNumber = PolicyNumberTextBox.Text.Trim();
        patient.Phone = PhoneTextBox.Text.Trim();
        patient.Email = EmailTextBox.Text.Trim();
        patient.Address = AddressTextBox.Text.Trim();

        // Валидация введенных данных
        var errors = PatientValidator.Validate(patient);
        if (errors.Count > 0)
        {
            var errorMessage = string.Join(Environment.NewLine + "• ", errors);
            MessageBox.Show(
                $"Обнаружены ошибки при заполнении данных пациента:\n\n• {errorMessage}",
                "Ошибка валидации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Сохранение информации в базу данных
            if (_patientId.HasValue)
            {
                _repository.UpdatePatient(patient);
                MessageBox.Show("Данные пациента успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _repository.AddPatient(patient);
                MessageBox.Show("Пациент успешно добавлен в систему!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            IsSaved = true;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            // Обработка исключений СУБД (например, дубликат полиса ОМС или разрыв связи)
            if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("policy_number"))
            {
                MessageBox.Show(
                    "Пациент с таким номером полиса ОМС уже зарегистрирован в базе данных!",
                    "Дубликат полиса",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    $"Не удалось сохранить данные пациента:\n{ex.Message}",
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
