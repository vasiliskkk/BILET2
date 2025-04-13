using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

namespace BILET2
{
    public partial class MainWindow : Window
    {
        public class InputItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            CreateInputTables(3, 4);
        }

        private void BtnCreateTable_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbSuppliers.Text, out int suppliers) &&
                int.TryParse(tbConsumers.Text, out int consumers))
            {
                if (suppliers > 0 && consumers > 0)
                {
                    CreateInputTables(suppliers, consumers);
                }
                else
                {
                    MessageBox.Show("Количество поставщиков и потребителей должно быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректные числа для количества поставщиков и потребителей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateInputTables(int suppliers, int consumers)
        {
            // Создаем поля для ввода запасов
            var supplyItems = new List<InputItem>();
            for (int i = 0; i < suppliers; i++)
            {
                supplyItems.Add(new InputItem { Name = $"Поставщик {i + 1}", Value = 0 });
            }
            supplyInputs.ItemsSource = supplyItems;

            // Создаем поля для ввода потребностей
            var demandItems = new List<InputItem>();
            for (int j = 0; j < consumers; j++)
            {
                demandItems.Add(new InputItem { Name = $"Потребитель {j + 1}", Value = 0 });
            }
            demandInputs.ItemsSource = demandItems;

            // Создаем матрицу затрат
            costMatrix.Columns.Clear();
            for (int j = 0; j < consumers; j++)
            {
                costMatrix.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Потр. {j + 1}",
                    Binding = new Binding($"[{j}]")
                });
            }

            var costData = new List<int[]>();
            for (int i = 0; i < suppliers; i++)
            {
                costData.Add(new int[consumers]);
            }
            costMatrix.ItemsSource = costData;
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateInputData())
                    return;

                // Получаем данные из интерфейса
                var supply = supplyInputs.ItemsSource.Cast<InputItem>().Select(x => x.Value).ToArray();
                var demand = demandInputs.ItemsSource.Cast<InputItem>().Select(x => x.Value).ToArray();
                var costData = costMatrix.ItemsSource.Cast<int[]>().ToList();
                var costs = new int[supply.Length, demand.Length];

                for (int i = 0; i < supply.Length; i++)
                {
                    for (int j = 0; j < demand.Length; j++)
                    {
                        costs[i, j] = costData[i][j];
                    }
                }

                // Решаем задачу
                var solver = new TransportSolver();
                var result = solver.Solve(supply, demand, costs);

                // Отображаем решение
                DisplaySolution(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputData()
        {
            // Проверка запасов
            var supplyItems = supplyInputs.ItemsSource.Cast<InputItem>().ToList();
            foreach (var item in supplyItems)
            {
                if (item.Value < 0)
                {
                    MessageBox.Show($"Запасы не могут быть отрицательными. Проверьте поставщика {item.Name}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            // Проверка потребностей
            var demandItems = demandInputs.ItemsSource.Cast<InputItem>().ToList();
            foreach (var item in demandItems)
            {
                if (item.Value < 0)
                {
                    MessageBox.Show($"Потребности не могут быть отрицательными. Проверьте потребителя {item.Name}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            // Проверка матрицы затрат
            var costData = costMatrix.ItemsSource.Cast<int[]>().ToList();
            for (int i = 0; i < costData.Count; i++)
            {
                for (int j = 0; j < costData[i].Length; j++)
                {
                    if (costData[i][j] < 0)
                    {
                        MessageBox.Show($"Затраты не могут быть отрицательными. Проверьте ячейку [{i + 1},{j + 1}].", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private void DisplaySolution(TransportProblem result)
        {
            // Отображаем опорный план
            solutionGrid.Columns.Clear();
            for (int j = 0; j < result.Demand.Length; j++)
            {
                solutionGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Потр. {j + 1}",
                    Binding = new Binding($"[{j}]")
                });
            }

            var solutionData = new List<int[]>();
            for (int i = 0; i < result.Supply.Length; i++)
            {
                var row = new int[result.Demand.Length];
                for (int j = 0; j < result.Demand.Length; j++)
                {
                    row[j] = result.Solution[i, j];
                }
                solutionData.Add(row);
            }
            solutionGrid.ItemsSource = solutionData;

            // Формируем текстовый отчет
            string report = "=== Решение транспортной задачи ===\n\n";
            report += "Исходные данные:\n";

            report += "Запасы:\n";
            for (int i = 0; i < result.Supply.Length; i++)
            {
                report += $"Поставщик {i + 1}: {result.Supply[i]}\n";
            }

            report += "\nПотребности:\n";
            for (int j = 0; j < result.Demand.Length; j++)
            {
                report += $"Потребитель {j + 1}: {result.Demand[j]}\n";
            }

            report += "\nМатрица затрат:\n";
            for (int i = 0; i < result.Supply.Length; i++)
            {
                for (int j = 0; j < result.Demand.Length; j++)
                {
                    report += $"{result.Costs[i, j],4}";
                }
                report += "\n";
            }

            report += "\nОпорный план:\n";
            for (int i = 0; i < result.Supply.Length; i++)
            {
                for (int j = 0; j < result.Demand.Length; j++)
                {
                    report += $"{result.Solution[i, j],4}";
                }
                report += "\n";
            }

            report += $"\nОбщая стоимость перевозок: {result.TotalCost}\n";

            txtResult.Text = report;
        }

        private void BtnSaveSolution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtResult.Text))
                {
                    MessageBox.Show("Нет данных для сохранения. Сначала решите задачу.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    DefaultExt = ".txt",
                    FileName = "Решение транспортной задачи.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, txtResult.Text);
                    MessageBox.Show("Решение успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Очищаем поля ввода
                tbSuppliers.Text = "3";
                tbConsumers.Text = "4";

                // Создаем пустые таблицы
                CreateInputTables(3, 4);

                // Очищаем результаты
                solutionGrid.ItemsSource = null;
                solutionGrid.Columns.Clear();
                txtResult.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при очистке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}