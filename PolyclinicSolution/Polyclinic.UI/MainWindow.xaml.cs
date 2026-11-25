using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;
using Polyclinic.Data;

namespace Polyclinic.UI;

/// <summary>
/// Главное окно медицинской информационной системы «Поликлиника».
/// Интегрирует Модули 1, 2, 3 и 4.
/// </summary>
public partial class MainWindow : Window
{
    private readonly PolyclinicRepository _repository;
    private List<TreatmentType> _treatmentTypes = new();
    private List<MaterialType> _materialTypes = new();

    public MainWindow()
    {
        InitializeComponent();

        // Инициализация репозитория доступа к локальной базе данных
        _repository = new PolyclinicRepository();

        // Первичная загрузка всех разделов приложения
        LoadAllData();
    }

    /// <summary>
    /// Логика начальной загрузки и наполнения интерфейса данными.
    /// Загружает списки пациентов, врачей, партнеров, поставок и параметров калькулятора.
    /// </summary>
    private void LoadAllData()
    {
        try
        {
            LoadPatients();
            LoadDoctors();
            LoadPartnersAndDeliveries();
            LoadCalculatorData();
            StatusTextBlock.Text = $"Подключение к БД успешно. Загружено пациентов: {PatientsGrid.Items.Count}";
        }
        catch (Exception ex)
        {
            // Обработка исключений при стартовой загрузке
            MessageBox.Show(
                $"Ошибка инициализации базы данных:\n{ex.Message}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusTextBlock.Text = "Ошибка подключения к базе данных.";
        }
    }

    #region Пациенты (Модуль 2 и 3)

    /// <summary>
    /// Загрузка и обновление списка пациентов из БД без перезапуска приложения.
    /// </summary>
    private void LoadPatients()
    {
        try
        {
            var search = SearchTextBox.Text.Trim();
            var list = _repository.GetPatients(string.IsNullOrWhiteSpace(search) ? null : search);
            PatientsGrid.ItemsSource = list;
            StatusTextBlock.Text = $"Пациентов в списке: {list.Count} (поиск: '{(string.IsNullOrEmpty(search) ? "все" : search)}')";
        }
        catch (Exception ex)
        {
            // Обработка исключений при чтении списка пациентов
            MessageBox.Show($"Ошибка при загрузке списка пациентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadPatients();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadPatients();
    }

    private void AddPatientButton_Click(object sender, RoutedEventArgs e)
    {
        // Открытие диалогового окна создания нового пациента
        var win = new PatientEditWindow(_repository)
        {
            Owner = this
        };

        if (win.ShowDialog() == true)
        {
            // Модуль 3: список пациентов обновляется без перезапуска
            LoadPatients();
        }
    }

    private void EditPatientButton_Click(object sender, RoutedEventArgs e)
    {
        if (PatientsGrid.SelectedItem is not Patient selected)
        {
            MessageBox.Show(
                "Пожалуйста, выберите пациента из таблицы для редактирования.",
                "Пациент не выбран",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Открытие диалога редактирования выбранного пациента
        var win = new PatientEditWindow(_repository, selected.PatientId)
        {
            Owner = this
        };

        if (win.ShowDialog() == true)
        {
            // Обновление списка пациентов без перезапуска
            LoadPatients();
        }
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (PatientsGrid.SelectedItem is not Patient selected)
        {
            MessageBox.Show(
                "Пожалуйста, выберите пациента для просмотра истории приёмов.",
                "Пациент не выбран",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var historyWin = new AppointmentHistoryWindow(_repository, selected.PatientId)
        {
            Owner = this
        };
        historyWin.ShowDialog();

        // Перезагружаем пациента, так как могли быть добавлены приемы и измениться скидка
        LoadPatients();
    }

    private void DeletePatientButton_Click(object sender, RoutedEventArgs e)
    {
        if (PatientsGrid.SelectedItem is not Patient selected)
        {
            MessageBox.Show("Выберите пациента для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Вы действительно хотите удалить пациента «{selected.FullName}» и всю историю его посещений?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _repository.DeletePatient(selected.PatientId);
                LoadPatients();
            }
            catch (Exception ex)
            {
                // Обработка исключений при удалении записи
                MessageBox.Show($"Не удалось удалить пациента: {ex.Message}", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void PatientsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (PatientsGrid.SelectedItem is Patient)
        {
            HistoryButton_Click(sender, e);
        }
    }

    private void PatientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    #endregion

    #region Врачи (Модуль 1)

    private void LoadDoctors()
    {
        try
        {
            var doctors = _repository.GetDoctors();
            DoctorsGrid.ItemsSource = doctors;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке врачей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Поставки партнеров (Модуль 4)

    private void LoadPartnersAndDeliveries()
    {
        try
        {
            var partners = _repository.GetPartners();
            var filterList = new List<Partner>
            {
                new Partner { PartnerId = 0, PartnerName = "Все партнеры" }
            };
            filterList.AddRange(partners);

            PartnerFilterComboBox.ItemsSource = filterList;
            PartnerFilterComboBox.SelectedIndex = 0;

            LoadDeliveries();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке данных о поставках: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadDeliveries()
    {
        try
        {
            int? partnerId = null;
            if (PartnerFilterComboBox.SelectedValue is int pid && pid > 0)
            {
                partnerId = pid;
            }

            var deliveries = _repository.GetMedicineDeliveries(partnerId);
            DeliveriesGrid.ItemsSource = deliveries;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке поставок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PartnerFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadDeliveries();
    }

    private void RefreshDeliveriesButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDeliveries();
    }

    private void AddDeliveryButton_Click(object sender, RoutedEventArgs e)
    {
        var win = new DeliveryAddWindow(_repository)
        {
            Owner = this
        };

        if (win.ShowDialog() == true)
        {
            LoadDeliveries();
        }
    }

    #endregion

    #region Калькулятор медикаментов (Модуль 4)

    private void LoadCalculatorData()
    {
        try
        {
            _treatmentTypes = _repository.GetTreatmentTypes();
            _materialTypes = _repository.GetMaterialTypes();

            CalcTreatmentComboBox.ItemsSource = _treatmentTypes;
            CalcMaterialComboBox.ItemsSource = _materialTypes;

            if (_treatmentTypes.Count > 1) CalcTreatmentComboBox.SelectedIndex = 1; // Инфузионная терапия
            if (_materialTypes.Count > 1) CalcMaterialComboBox.SelectedIndex = 1;  // Физраствор

            ExecuteCalculation();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации калькулятора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CalcInputs_Changed(object sender, RoutedEventArgs e)
    {
        ExecuteCalculation();
    }

    private void CalculateRequirementButton_Click(object sender, RoutedEventArgs e)
    {
        ExecuteCalculation();
    }

    /// <summary>
    /// Вызов расчетного метода по спецификации задания Модуля 4.
    /// </summary>
    private void ExecuteCalculation()
    {
        if (CalcResultNumber == null || CalcTreatmentComboBox == null || CalcMaterialComboBox == null) return;

        if (CalcTreatmentComboBox.SelectedValue is not int productTypeId ||
            CalcMaterialComboBox.SelectedValue is not int materialTypeId)
        {
            return;
        }

        if (!int.TryParse(CalcQuantityTextBox.Text.Trim(), out int quantity) ||
            !double.TryParse(CalcParam1TextBox.Text.Trim(), out double param1) ||
            !double.TryParse(CalcParam2TextBox.Text.Trim(), out double param2))
        {
            CalcResultNumber.Text = "Ошибка ввода";
            CalcCoeffDetails.Text = "Укажите корректные положительные числовые параметры.";
            return;
        }

        // Построение словарей коэффициентов из базы данных
        var treatmentDict = _treatmentTypes.ToDictionary(t => t.TreatmentTypeId, t => t.TreatmentCoefficient);
        var defectDict = _materialTypes.ToDictionary(m => m.MaterialTypeId, m => m.DefectRate);

        // Вызов метода расчета согласно спецификации задания
        int result = MedicineRequirementCalculator.CalculateMedicineRequirement(
            productTypeId,
            materialTypeId,
            quantity,
            param1,
            param2,
            treatmentDict,
            defectDict);

        if (result == -1)
        {
            CalcResultNumber.Text = "-1 (Неверные данные)";
            CalcCoeffDetails.Text = "Метод вернул -1: недопустимые параметры или неизвестный тип лечения/препарата.";
        }
        else
        {
            treatmentDict.TryGetValue(productTypeId, out double coeff);
            defectDict.TryGetValue(materialTypeId, out double defect);

            CalcResultNumber.Text = $"{result:N0} ед.";
            CalcCoeffDetails.Text = $"• Коэффициент типа лечения (treatment_coefficient): {coeff:F3}\n" +
                                   $"• Процент потерь / брака (defect_rate): {defect:P1}\n" +
                                   $"• Расход на 1 пациента: {param1 * param2 * coeff:N2} ед.\n" +
                                   $"• Количество пациентов: {quantity}";
        }
    }

    #endregion
}
