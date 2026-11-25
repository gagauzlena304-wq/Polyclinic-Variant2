using System;
using System.Windows;
using Polyclinic.Core.Models;
using Polyclinic.Data;

namespace Polyclinic.UI;

public partial class DeliveryAddWindow : Window
{
    private readonly PolyclinicRepository _repository;
    public bool IsSaved { get; private set; }

    public DeliveryAddWindow(PolyclinicRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        DeliveryDatePicker.SelectedDate = DateTime.Today;
        LoadFormData();
    }

    /// <summary>
    /// Загрузка справочников партнеров и типов материалов.
    /// </summary>
    private void LoadFormData()
    {
        try
        {
            PartnerComboBox.ItemsSource = _repository.GetPartners();
            MaterialTypeComboBox.ItemsSource = _repository.GetMaterialTypes();

            if (PartnerComboBox.Items.Count > 0) PartnerComboBox.SelectedIndex = 0;
            if (MaterialTypeComboBox.Items.Count > 0) MaterialTypeComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (PartnerComboBox.SelectedValue is not int partnerId || partnerId <= 0)
        {
            MessageBox.Show("Выберите поставщика.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MaterialTypeComboBox.SelectedValue is not int materialTypeId || materialTypeId <= 0)
        {
            MessageBox.Show("Выберите тип препарата.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(MedicineNameTextBox.Text))
        {
            MessageBox.Show("Укажите наименование препарата.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(QuantityTextBox.Text.Trim(), out int quantity) || quantity <= 0)
        {
            MessageBox.Show("Укажите корректное положительное количество.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DeliveryDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Выберите дату поставки.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var batch = string.IsNullOrWhiteSpace(BatchNumberTextBox.Text) 
            ? $"SER-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}" 
            : BatchNumberTextBox.Text.Trim();

        var delivery = new MedicineDelivery
        {
            PartnerId = partnerId,
            MaterialTypeId = materialTypeId,
            MedicineName = MedicineNameTextBox.Text.Trim(),
            Quantity = quantity,
            DeliveryDate = DeliveryDatePicker.SelectedDate.Value,
            BatchNumber = batch
        };

        try
        {
            _repository.AddMedicineDelivery(delivery);
            MessageBox.Show("Поставка успешно сохранена в журнале!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            IsSaved = true;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении поставки: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
