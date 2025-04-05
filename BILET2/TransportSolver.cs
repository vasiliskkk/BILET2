using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BILET2
{
    public class TransportSolver
    {
        public TransportProblem Solve(int[] supply, int[] demand, int[,] costs)
        {
            // Проверка на сбалансированность задачи
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            if (totalSupply != totalDemand)
            {
                throw new ArgumentException("Задача не сбалансирована. Сумма предложения должна равняться сумме спроса.");
            }

            int rows = supply.Length;
            int cols = demand.Length;

            int[,] solution = new int[rows, cols];
            int[] remainingSupply = (int[])supply.Clone();
            int[] remainingDemand = (int[])demand.Clone();

            while (true)
            {
                // Находим минимальный элемент в матрице затрат
                int minCost = int.MaxValue;
                int minRow = -1;
                int minCol = -1;

                for (int i = 0; i < rows; i++)
                {
                    if (remainingSupply[i] == 0) continue;

                    for (int j = 0; j < cols; j++)
                    {
                        if (remainingDemand[j] == 0) continue;

                        if (costs[i, j] < minCost)
                        {
                            minCost = costs[i, j];
                            minRow = i;
                            minCol = j;
                        }
                    }
                }

                // Если все потребности и запасы удовлетворены
                if (minRow == -1 || minCol == -1)
                    break;

                // Назначаем перевозку
                int amount = Math.Min(remainingSupply[minRow], remainingDemand[minCol]);
                solution[minRow, minCol] = amount;
                remainingSupply[minRow] -= amount;
                remainingDemand[minCol] -= amount;
            }

            // Вычисляем общую стоимость
            int totalCost = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    totalCost += solution[i, j] * costs[i, j];
                }
            }

            return new TransportProblem
            {
                Supply = supply,
                Demand = demand,
                Costs = costs,
                Solution = solution,
                TotalCost = totalCost
            };
        }
    }
}