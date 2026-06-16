
//############################################//

//звичайний потік
Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//2
//асинхроний потік
await Task.Run(() =>// await - затримує закінчення програми поко не закінчеться цій потік
{
    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//5
});

Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//5

await Task.Delay(1000);// затримка на 1 секунду

//############################################//
var thread = new Thread(() =>//створення нового потоку
{
    Console.WriteLine("Hello World!");// його функціонал
});
thread.Start();//запуск потіка

//############################################//
//  гонка потоків
//  thread1 та thread2 виконуються неравномірно
var thread1 = new Thread(() => {// 1 потік
    while (true)
    {
        Thread.Sleep(500);
        Console.WriteLine("1");
    }
});
var thread2 = new Thread(() => {// 2 потік
    while (true)
    {
        Thread.Sleep(500);
        Console.WriteLine("2");
    }
});
thread1.IsBackground = true;// потік - фоновий
thread2.IsBackground = true;
thread1.Start(); // старт
thread2.Start();
Thread.Sleep(20000);//затримка основного потоку

//############################################//
//коли з різних потоків іде звернення до одного ресурсу
// не горонтується правельний результат

int number = 0;

var thread3 = new Thread(() => { for (int i = 0; i < 1000000; i++) { number++; } });
var thread4 = new Thread(() => { for (int i = 0; i < 1000000; i++) { number++; } });
var thread5 = new Thread(() => { for (int i = 0; i < 1000000; i++) { number++; } });

thread3.Start();// дожно додати 1000000
thread4.Start();
thread5.Start();

Console.WriteLine(number); // довжно = 3000000 але ні
// требо робити сінхронізацію

//############################################//
//сінхронізація
// Interlocked.Increment(ref number1) - сінхронезований інкримент
int number1 = 0;

var thread6 = new Thread(() => { for (int i = 0; i < 1000000; i++) Interlocked.Increment(ref number1); });
var thread7 = new Thread(() => { for (int i = 0; i < 1000000; i++) Interlocked.Increment(ref number1); });
var thread8 = new Thread(() => { for (int i = 0; i < 1000000; i++) Interlocked.Increment(ref number1); });

thread6.Start();// додає 1000000
thread7.Start();
thread8.Start();

Thread.Sleep(10000);
Console.WriteLine(number1); //  = 3000000


//############################################//
var cancellationToken = new CancellationToken();// дозволяє відмінити задачу

// як правельно:
var source = new CancellationTokenSource();//cтвор CancellationToken
source.CancelAfter(1000);//задаю властивості ( відміна через 1 сек)
var cancellationToken1 = source.Token;// створення самого токіна
// використання токіна:
//завантажую юзерів, якщо встигну за 1 с.
var user = await Users.AsNoTrecking().ToListAsync(cancellationToken);