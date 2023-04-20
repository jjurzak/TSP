

namespace TSP
{
    class City
    {
        // Properties.
        public string Name { get; set; }
        public double[] Distances { get; set; }
        // Konstruktor.
        public City(string name, double[] distances)
        {
            Name = name;
            Distances = distances;
        }
    }
    // Implementacja algorytmu genetycznego.
    class GeneticAlgorithm
    {
        private readonly Random _random;
        private readonly List<City> _cities;
        private List<List<int>> _population;
        private readonly int _populationSize;
        private readonly double _mutationProbability;
        private readonly int _maxGenerations;
        // Konstruktor dla klasy GeneticAlgorithm.
        public GeneticAlgorithm(List<City> cities, int populationSize, double mutationProbability, int maxGenerations)
        {
            _random = new Random();
            _cities = cities;
            _populationSize = populationSize;
            _mutationProbability = mutationProbability;
            _maxGenerations = maxGenerations;
            _population = CreateInitialPopulation();

        }
        // Metoda rozwiązująca problem.
        public List<int> Solve()
        {
            var population = CreateInitialPopulation();
            var bestGeneration = 0;
            var bestDistance = double.MaxValue;
            for (var i = 0; i < _maxGenerations; i++)
            {
                var fitnessScores = CalculateFitnessScores(population);
                var fittestIndividual = population[fitnessScores.IndexOf(fitnessScores.Max())];
                if (i % 1 == 0)
                {
                    var distance = CalculateDistance(fittestIndividual);
                    if (distance < bestDistance)
                    {
                        bestGeneration = i + 1;
                        bestDistance = distance;
                    }
                }
                if (fitnessScores.Max() == 0) return fittestIndividual;
                population = CreateNewPopulation(fitnessScores);
                
            }
            // zrobic ilosc generowanych dzieci rowna ilosci osobnikow w populacji i zwracac najlepszego osobnika. 90% dzieci przechodzi i pozostale 10% zastapic rodzicami
            var bestRoute = population[CalculateFitnessScores(population).IndexOf(CalculateFitnessScores(population).Max())];
    Console.WriteLine($"Best generation: {bestGeneration},\nBest distance: {bestDistance}, \nBest route: {string.Join("-", bestRoute)} | Indeksy miast w pliku ");
    return bestRoute;
        }
// zrobic ilosc generowanych dzieci rowna ilosci osobnikow w populacji i zwracac najlepszego osobnika. 90% dzieci przechodzi i pozostale 10% zastapic rodzicami
        // Tworzy początkową populację.
        private List<List<int>> CreateInitialPopulation()
        {
            var population = new List<List<int>>();
            for (var i = 0; i < _populationSize; i++)
            {
                var individual = Enumerable.Range(1, _cities.Count - 1).ToList();
                Shuffle(individual);
                individual.Insert(0, 0); // add the first city as the starting point
                population.Add(individual);
            }

            return population;
        }
        // Oblicza wartość funkcji fitness.
        private List<double> CalculateFitnessScores(List<List<int>> population)
        {
            var fitnessScores = new List<double>();
            var optimalOrder = Enumerable.Range(0, _cities.Count).ToList();
            foreach (var individual in population)
            {
                var distance = CalculateDistance(individual);
                var orderDeviation = individual.Zip(optimalOrder, (a, b) => a == b ? 0 : 1).Sum();
                var fitness = 1 / (distance + orderDeviation);
                fitnessScores.Add(fitness);
            }

            return fitnessScores;
        }
        // Tworzy nową populację wybierajac dwojga rodziców i tworząc z nich dziecko ktore mutuje.
        private List<List<int>> CreateNewPopulation(List<double> fitnessScores)
        {
            var newPopulation = new List<List<int>>();
            var bestIndividual = _population[fitnessScores.IndexOf(fitnessScores.Max())];
            newPopulation.Add(bestIndividual);
            for (var i = 1; i < _populationSize; i++)
            {
                List<int> parent1 = SelectParent(fitnessScores);
                List<int> parent2 = SelectParent(fitnessScores);
                List<int> child = Crossover(parent1, parent2);
                Mutate(child);
                newPopulation.Add(child);
            }
            _population = newPopulation;
            return _population;
        }

        private List<int> SelectParent(List<double> fitnessScores)
        {
            var totalFitness = fitnessScores.Sum();
            var randomValue = _random.NextDouble() * totalFitness;
            var fitnessSum = 0.1;
            for (var i = 0; i < _populationSize; i++)
            {
                fitnessSum += fitnessScores[i];
                if (fitnessSum >= randomValue)
                {
                    return _population[i];
                }
            }
            return _population[_populationSize - 1];
        }
        // Metoda ta wybiera losowo pojedynczego osobnika z populacji im większa wartość funkcji fitness tym większe prawdopodobieństwo wylosowania tego osobnika
        private List<int> SelectIndividual(List<List<int>> population, List<double> fitnessScores)
        {
            var totalFitness = fitnessScores.Sum();
            var randomValue = _random.NextDouble() * totalFitness;
            for (var i = 0; i < population.Count; i++)
            {
                randomValue -= fitnessScores[i];
                if (randomValue <= 0) return population[i];
            }

            return population.Last();
        }
        // Metoda ta tworzy dziecko z dwóch rodziców losow wybierając fragment z jednego rodzica i resztę z drugiego rodzica
        private List<int> Crossover(List<int> parent1, List<int> parent2)
        {
            var child = new List<int>();
            var start = _random.Next(parent1.Count);
            var end = _random.Next(start, parent1.Count);
            for (var i = start; i < end; i++)
            {
                child.Add(parent1[i]);
            }

            foreach (var t in parent2.Where(t => !child.Contains(t)))
            {
                child.Add(t);
            }

            return child;
        }
        // Mutacja polega na zamianie miejscami dwóch genów (okreslone prawdopodobienstwo) w osobniku.
        private void Mutate(List<int> individual)
        {
            for (var i = 0; i < individual.Count; i++)
            {
                if (_random.NextDouble() < _mutationProbability)
                {
                    var j = _random.Next(individual.Count);
                    (individual[i], individual[j]) = (individual[j], individual[i]);
                }
            }
        }
        // obliczanie dlugosci trasy jednego osobnika.
        private double CalculateDistance(List<int> individual)
        {
            var distance = 0.0;
            for (var i = 0; i < individual.Count - 1; i++)
            {
                distance += _cities[individual[i]].Distances[individual[i + 1]];
            }

            distance += _cities[individual.Last()].Distances[individual.First()];
            return distance;
        }
        // Metoda ta losowo przemiesza kolejność elementów w liście. 
        private void Shuffle(List<int> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _random.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    abstract class Program
    {
        static void Main()
        {
            // stworzenie listy miast i wczytanie danych z pliku.
            var cities = new List<City>();
            string filePath = "cities.txt";
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                string cityName = parts[0];
                double[] distances = new double[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                {
                    if (double.TryParse(parts[i], out var distance))
                    {
                        distances[i - 1] = distance;
                    }
                }
                cities.Add(new City(cityName, distances));
            }
            // stworzenie obiektu klasy GeneticAlgorithm i wywołanie metody Solve.
            var geneticAlgorithm = new GeneticAlgorithm(cities, 100, 0.1,  200);
            var solution = geneticAlgorithm.Solve();

            Console.Write("Optimal route: ");
            foreach (var index in solution)
            {
                Console.Write(cities[index].Name + " -> ");
            }
            // Jakub Jurzak
            Console.Write(cities[solution.First()].Name);
            
            }
        }
    }
