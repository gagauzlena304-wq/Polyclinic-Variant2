using System;
using System.Windows;
using System.Windows.Controls;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;
using Polyclinic.Data;

namespace Polyclinic.UI;

/// <summary>
/// Окно просмотра истории посещений и диагнозов пациента (Модуль 1 и 2).
/// </summary>
public partial class AppointmentHistoryWindow : Window
{
    private readonly PolyclinicRepository _repository;
    private readonly int _patientId;
    private Patient? _patient;

    public AppointmentHistoryWindow(PolyclinicRepository repository, int patientId)
    {
        InitializeComponent();
        _repository = repository;
        _patientId = patientId;

        // Загрузка информации о пациенте и истории приёмов
        LoadPatientAndAppointments();
    }

    /// <summary>
    /// Логика загрузки данных пациента и его приёмов из базы данных.
    /// </summary>
    private void LoadPatientAndAppointments()
    {
        try
        {
            _patient = _repository.GetPatientById(_patientId);
            if (_patient == null)
            {
                MessageBox.Show("Пациент не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            Title = $"История приёмов - {_patient.FullName}";
            TitleTextBlock.Text = $"История приёмов: {_patient.FullName}";
            PatientInfoTextBlock.Text = $"{_patient.FullName} (Возраст: {_patient.Age} лет, { _patient.GenderName})";
            PatientMetaTextBlock.Text = $"Полис ОМС: {_patient.PolicyNumber} | Телефон: {_patient.Phone} | Email: {_patient.Email}";

            // Вычисляем визиты за 12 месяцев и индивидуальную скидку
            int visits = _patient.VisitsLast12Months;
            int discount = _patient.CurrentDiscountPercent;

            VisitsCountTextBlock.Text = $"{visits} визит(ов)";
            DiscountTextBlock.Text = $"{discount}%";

            // Загружаем приемы
            var appointments = _repository.GetAppointmentsByPatientId(_patientId);
            AppointmentsGrid.ItemsSource = appointments;
        }
        catch (Exception ex)
        {
            // Обработка исключений при загрузке данных
            MessageBox.Show($"Ошибка при загрузке истории приёмов: {ex.Message}", "Сбой загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (_patient == null) return;

        var addWin = new AddAppointmentWindow(_repository, _patientId, _patient.CurrentDiscountPercent)
        {
            Owner = this
        };

        if (addWin.ShowDialog() == true)
        {
            // Обновляем список приемов и пересчитываем скидку
            LoadPatientAndAppointments();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadPatientAndAppointments();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AppointmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
}
