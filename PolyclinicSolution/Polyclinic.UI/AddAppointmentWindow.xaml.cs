using System;
using System.Windows;
using System.Windows.Controls;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;
using Polyclinic.Data;

namespace Polyclinic.UI;

/// <summary>
/// Окно регистрации нового приёма пациента с автоматическим расчетом скидки.
/// </summary>
public partial class AddAppointmentWindow : Window
{
    private readonly PolyclinicRepository _repository;
    private readonly int _patientId;
    private readonly int _discountPercent;

    public bool IsSaved { get; private set; }

    public AddAppointmentWindow(PolyclinicRepository repository, int patientId, int discountPercent)
    {
        InitializeComponent();
        _repository = repository;
        _patientId = patientId;
        _discountPercent = discountPercent;

        DiscountLabel.Text = $"{_discountPercent}%";
        AppointmentDatePicker.SelectedDate = DateTime.Today;

        LoadDoctors();
        RecalculateCost();
    }

    /// <summary>
    /// Загрузка списка врачей из базы данных.
    /// </summary>
    private void LoadDoctors()
    {
        try
        {
            var doctors = _repository.GetDoctors();
            DoctorComboBox.ItemsSource = doctors;
            if (doctors.Count > 0)
            {
                DoctorComboBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке врачей: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BaseCostTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RecalculateCost();
    }

    private void RecalculateCost()
    {
        if (FinalCostLabel == null) return;

        if (decimal.TryParse(BaseCostTextBox.Text.Trim(), out decimal baseCost) && baseCost >= 0)
        {
            var finalCost = DiscountCalculator.CalculateFinalCost(baseCost, _discountPercent);
            FinalCostLabel.Text = $"{finalCost:N2} ₽";
        }
        else
        {
            FinalCostLabel.Text = "Неверная цена";
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DoctorComboBox.SelectedValue is not int doctorId || doctorId <= 0)
        {
            MessageBox.Show("Пожалуйста, выберите лечащего врача.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!AppointmentDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Пожалуйста, выберите дату приёма.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(DiagnosisTextBox.Text))
        {
            MessageBox.Show("Пожалуйста, укажите предварительный или окончательный диагноз.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(BaseCostTextBox.Text.Trim(), out decimal baseCost) || baseCost <= 0)
        {
            MessageBox.Show("Укажите корректную базовую стоимость приёма (положительное число).", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var finalCost = DiscountCalculator.CalculateFinalCost(baseCost, _discountPercent);

        var app = new Appointment
        {
            PatientId = _patientId,
            DoctorId = doctorId,
            AppointmentDate = AppointmentDatePicker.SelectedDate.Value.Date.Add(DateTime.Now.TimeOfDay),
            Status = "Завершен",
            Complaints = ComplaintsTextBox.Text.Trim(),
            Diagnosis = DiagnosisTextBox.Text.Trim(),
            Recommendations = RecommendationsTextBox.Text.Trim(),
            BaseCost = baseCost,
            DiscountPercent = _discountPercent,
            FinalCost = finalCost
        };

        try
        {
            // Сохранение записи приёма в базу данных
            _repository.AddAppointment(app);
            MessageBox.Show("Приём успешно зарегистрирован в базе данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            IsSaved = true;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            // Обработка исключений при записи в БД
            MessageBox.Show($"Не удалось сохранить приём: {ex.Message}", "Ошибка СУБД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
