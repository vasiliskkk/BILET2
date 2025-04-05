using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    MessageBox.Show("Количество поставщиков и потребителей должно быть положительным числом.");
                }
            }
            else
            {
                MessageBox.Show("Введите корректные числа для количества поставщиков и потребителей.");
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
    }
}