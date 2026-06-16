using HW_05.Async.Core;
using HW_05.Async.Models;
using HW_05.Async.Models;

var service = new FileScannerService();
var state = new TaskState();

while (true)
{
    Console.Clear();

    Console.WriteLine("1 - Пошук слова");
    Console.WriteLine("2 - Копіювання з заміною");
    Console.WriteLine("3 - Аналіз класів та інтерфейсів");
    Console.WriteLine("0 - Вихід");

    Console.Write("\nВаш вибір: ");

    if (!int.TryParse(Console.ReadLine(), out int choice))
        continue;

    switch (choice)
    {
        case 1:
            await RunSearchWord();
            break;

        case 2:
            await RunSearchFiles();
            break;

        case 3:
            await RunFindClassesAndInterfaces();
            break;

        case 0:
            return;
    }
}

async Task RunSearchWord()
{
    ResetState();

    var cts = new CancellationTokenSource();

    var searchTask = service.SearchWordAsync(state, cts.Token, "task1");

    var progressTask = DisplayProgressAsync(state, cts.Token);

    _ = WaitForCancelAsync(cts);

    try
    {
        await searchTask;
        await progressTask;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Пошук скасовано.");
    }

    Console.WriteLine($"\nЗнайдено файлів: {state.FoundFiles}");
    Console.WriteLine($"Знайдено входжень: {state.FoundWords}");
    Console.WriteLine("\nНатисніть будь-яку клавішу...");
    Console.ReadKey();
}

async Task RunSearchFiles()
{
    ResetState();

    var cts = new CancellationTokenSource();

    var searchTask = service.SearchWordAsync(state, cts.Token, "task2");

    var progressTask = DisplayProgressAsync(state, cts.Token);

    _ = WaitForCancelAsync(cts);

    try
    {
        await searchTask;
        await progressTask;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Пошук скасовано.");
    }

    Console.WriteLine($"\nСкопійовано файлів: {state.CopiedFiles}");
    Console.WriteLine($"Змінено входжень: {state.ReplacedWords}");

    Console.WriteLine("\nНатисніть будь-яку клавішу...");
    Console.ReadKey();
}
async Task RunFindClassesAndInterfaces()
{
    ResetState();

    var cts = new CancellationTokenSource();

    var searchTask = service.SearchWordAsync(state, cts.Token, "task3");

    var progressTask = DisplayProgressAsync(state, cts.Token);

    _ = WaitForCancelAsync(cts);

    try
    {
        await searchTask;
        await progressTask;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Пошук скасовано.");
    }

    Console.WriteLine($"\nЗнайдено класів: {state.ClassesFound}");
    Console.WriteLine($"Знайдено інтерфейсів: {state.InterfacesFound}");

    Console.WriteLine("\n=== КЛАСИ ===");

    foreach (var className in state.Classes)
    {
        Console.WriteLine(className);
    }

    Console.WriteLine("\n=== ІНТЕРФЕЙСИ ===");

    foreach (var interfaceName in state.Interfaces)
    {
        Console.WriteLine(interfaceName);
    }

    Console.WriteLine("\nНатисніть будь-яку клавішу...");
    Console.ReadKey();
}
//онулюю стартові значення
void ResetState()
{
    state.Adress = "";
    state.SearchWord = "";
    state.NewWord = "";
    state.TotalFiles = 0;
    state.ProcessedFiles = 0;
    state.FoundFiles = 0;
    state.FoundWords = 0;
    state.CopiedFiles = 0;
    state.ReplacedWords = 0;
    state.ClassesFound = 0;
    state.InterfacesFound = 0;
    state.IsCompleted = false;
}

static async Task WaitForCancelAsync(CancellationTokenSource cts)
{
    await Task.Run(() =>
    {
        Console.ReadLine();
        cts.Cancel();
    });
}

static async Task DisplayProgressAsync(TaskState state, CancellationToken token)
{
   
    while (!token.IsCancellationRequested && !state.IsCompleted)
    {
        
        Console.Clear();

        double progress = state.TotalFiles == 0 ? 0 : (double)state.ProcessedFiles / state.TotalFiles * 100;

        Console.WriteLine($"Оброблено {state.ProcessedFiles} із {state.TotalFiles} файлів");
        Console.WriteLine($"Прогрес: {progress:F1}%");

        Console.WriteLine("\nНатисніть Enter для зупинки...");

        await Task.Delay(1000, token);
        
        Console.Clear();
        Console.WriteLine($"Оброблено {state.ProcessedFiles} із {state.TotalFiles} файлів");
        Console.WriteLine("Прогрес: 100%");
    }
}